using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using MySql.Data.MySqlClient;
using System.IO;
using System.Xml;

namespace SSFTimingOlaSync
{
    public partial class SSFTimingSync : Form
    {
        private IDbConnection m_sffTimingConnection;
        private IDbConnection m_olaConnection;
        public SSFTimingSync()
        {
            InitializeComponent();

            m_sffTimingConnection = new SqlConnection(ConfigurationManager.ConnectionStrings["ssftiming"].ConnectionString);
            logit("connecting to sfftiming: " + ConfigurationManager.ConnectionStrings["ssftiming"].ConnectionString);
            m_sffTimingConnection.Open();

            using (IDbCommand cmd = m_sffTimingConnection.CreateCommand())
            {
                cmd.CommandText = "select raceId, RaceName, raceDate from dbRaces";
                cmbSSF.Items.Clear();

                IDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var cmp = new ListComp();
                    cmp.Id = Convert.ToInt32(reader["raceId"].ToString());
                    cmp.Name = Convert.ToString(reader["raceName"]) + " [" + Convert.ToDateTime(reader["raceDate"]).ToString("yyyy-MM-dd") + "]";
                    cmbSSF.Items.Add(cmp);
                }
                if (cmbSSF.Items.Count > 0)
                    cmbSSF.SelectedIndex = 0;
                reader.Close();
                cmd.Dispose();
            }

            logit("connected");
            m_olaConnection = new MySqlConnection(ConfigurationManager.ConnectionStrings["ola"].ConnectionString);
            logit("connecting to ola: " + ConfigurationManager.ConnectionStrings["ola"].ConnectionString);
            m_olaConnection.Open();

            using (IDbCommand cmd = m_olaConnection.CreateCommand())
            {
                cmd.CommandText = "select eventid, name from Events";

                cmbOLA.Items.Clear();
                IDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var cmp = new ListComp();
                    cmp.Id = Convert.ToInt32(reader["eventid"].ToString());
                    cmp.Name = Convert.ToString(reader["name"]);
                    cmbOLA.Items.Add(cmp);
                }
                reader.Close();
                cmd.Dispose();

                if (cmbOLA.Items.Count > 0)
                    cmbOLA.SelectedIndex = 0;
            }
            logit("connected");
        }

        private class ListComp
        {
            public string Name;
            public int Id;

            public override string ToString()
            {
                return Name;
            }
        }

        void logit(string msg)
        {
            if (listBox1.InvokeRequired)
            {
                listBox1.Invoke(new MethodInvoker(() => logit(msg)));
            }
            else
            {
                listBox1.Items.Insert(0, msg);
            }
        }

        private void SSFTimingSync_FormClosing(object sender, FormClosingEventArgs e)
        {
            m_olaConnection.Close();
            m_sffTimingConnection.Close();
        }

        class StartArgs
        {
            public int SSFTimingEventId { get; set; }
            public int OlaEventId { get; set; }
            public bool StartTime2 { get; set; }
            public int OlaRaceId { get; set; }
            public bool IsRelay { get; set; }
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int ssfId = ((StartArgs)e.Argument).SSFTimingEventId;
            int olaId = ((StartArgs)e.Argument).OlaEventId;
            int raceId = ((StartArgs)e.Argument).OlaRaceId;
            bool startTime2 = ((StartArgs)e.Argument).StartTime2;
            bool isRelay = ((StartArgs)e.Argument).IsRelay;

            if (isRelay)
            {
                MonitorRelay((StartArgs)e.Argument);
            }
            else
            {

                string initialCommand = string.Format(@"select  dbName.FirstName, dbName.LastName,
 dbTeam.Name as teamname, dbclass.name as classname,
 " + (startTime2 ? "dbName.startTime2" : "dbName.startTime") + @" as allocatedStartTime, dbRuns.StartTime,dbRuns.FinishTime,dbRuns.RaceTime, dbName.Startno, dbRuns.Status
from dbName, dbTeam, dbclass, dbRuns
where dbName.raceId = {0}
and dbTeam.raceId = {0}
and dbclass.raceid={0}
and dbRuns.RaceID = {0}
and dbRuns.StartNo = dbName.Startno
and dbTeam.TeamID = dbName.teamid
and dbclass.classid = dbName.classid", ssfId);

                int lastLogId = -1;

                using (IDbCommand ssfCmd = m_sffTimingConnection.CreateCommand())
                {
                    ssfCmd.CommandText = initialCommand;

                    ParseReader(ssfCmd, olaId, raceId);

                    while (!backgroundWorker1.CancellationPending)
                    {
                        ssfCmd.CommandText = "select max(logid) from dbLog where raceid=" + ssfId;
                        object oval = ssfCmd.ExecuteScalar();
                        int maxLogId = -1;

                        if (oval != null && oval != DBNull.Value)
                        {
                            maxLogId = Convert.ToInt32(oval);
                        }

                        if (maxLogId > -1 && maxLogId > lastLogId)
                        {
                            ssfCmd.CommandText = initialCommand +
                                                     string.Format(
                                                         @" and dbName.Startno in (select distinct startno from dbLog where raceId={0} and logid > {1})", ssfId,
                                                         lastLogId);
                            ParseReader(ssfCmd, olaId, raceId);
                        }
                        lastLogId = maxLogId;

                        Thread.Sleep(100);
                    }
                }
            }
        }

        private void MonitorRelay(StartArgs args)
        {
            int ssfId = args.SSFTimingEventId;
            int olaId = args.OlaEventId;
            int raceId = args.OlaRaceId;
            bool startTime2 = args.StartTime2;
            bool isRelay = args.IsRelay;

            int lastLogId = -1;

            

            using (IDbCommand ssfCmd = m_sffTimingConnection.CreateCommand())
            {
                Dictionary<string, RelayLegInfo> relayLegInfo = GetRelayLegInfo(ssfCmd, ssfId);

                string initialCommand = string.Format(@"select  dbName.FirstName as teamname,dbRelay.Firstname, dbRelay.LastName, 
                             dbRelay.Leg, dbclass.name as classname,
                             dbName.startTime,dbRuns.Finishtime, dbName.Startno, dbRuns.Status
                            from dbName 
                            inner join dbclass on (dbClass.RaceId={0} and dbclass.classId=dbName.ClassID)
                            inner join dbRelay on (dbRelay.RaceID={0} and dbRelay.NameID=dbName.ID)
                            left outer join dbRuns on (dbRuns.RaceID = {0} and dbRuns.StartNo = dbName.Startno)
                            where dbName.raceId = {0} and leg=3", ssfId);

                string initialSplitCommand = string.Format(@"select dbName.FirstName as teamname, dbRelay.Firstname, dbRelay.LastName, dbRelay.Leg, 
                                dbclass.name as classname, dbName.Startno, dbITime.runtime, dbITime.IPos, dbItime.ITime as Finishtime
                                from dbName, dbclass, dbITime, dbRelay, dbITimeInfo
                                where dbName.raceId = {0}
                                and dbRelay.RaceID = {0}
                                and dbclass.raceid={0}
                                and dbITime.RaceID = {0}
                                and dbITimeInfo.RaceID={0}
                                and dbItime.StartNo = dbName.Startno
                                and dbclass.classid = dbName.classid
                                and dbRelay.NameID = dbName.ID
                                and dbclass.course = dbITimeInfo.Course
                                and dbITimeInfo.Ipos = dbITime.IPos
                                and dbITimeInfo.Leg=dbRelay.Leg and dbRelay.Leg != 3", ssfId);

                ssfCmd.CommandText = initialSplitCommand;
                ParseRelaySplitReader(ssfCmd, olaId, raceId, relayLegInfo);

                ssfCmd.CommandText = initialCommand;
                ParseRelayReader(ssfCmd, olaId, raceId);

                while (!backgroundWorker1.CancellationPending)
                {
                    ssfCmd.CommandText = "select max(logid) from dbLog where raceid=" + ssfId;
                    object oval = ssfCmd.ExecuteScalar();
                    int maxLogId = -1;

                    if (oval != null && oval != DBNull.Value)
                    {
                        maxLogId = Convert.ToInt32(oval);
                    }

                    if (maxLogId > -1 && maxLogId > lastLogId)
                    {
                        ssfCmd.CommandText = initialSplitCommand +
                                                 string.Format(
                                                     @" and dbName.Startno in (select distinct startno from dbLog where raceId={0} and logid > {1})", ssfId,
                                                     lastLogId);
                        ParseRelaySplitReader(ssfCmd, olaId, raceId, relayLegInfo);
                        
                        ssfCmd.CommandText = initialCommand +
                                                 string.Format(
                                                     @" and dbName.Startno in (select distinct startno from dbLog where raceId={0} and logid > {1})", ssfId,
                                                     lastLogId);
                        ParseRelayReader(ssfCmd, olaId, raceId);
                    }
                    lastLogId = maxLogId;

                    Thread.Sleep(100);
                }
            }
        }

        private Dictionary<string, RelayLegInfo> GetRelayLegInfo(IDbCommand cmd, int ssfEventId, bool isRelay = true)
        {
            cmd.CommandText =
                                "select dbclass.Name as className, dbITimeInfo.Name as intermediateName, dbITimeInfo.Ipos, dbITimeInfo.Leg from dbclass, dbITimeInfo where dbclass.RaceId = " +
                                ssfEventId + " and dbITimeInfo.RaceID = " + ssfEventId + " and dbITimeInfo.Course = dbclass.course  " +
                                " and dbiTimeInfo.PresInternet = 'Ja' order by dbclass.Name, ipos";

            Dictionary<string, int> classMaxLeg = new Dictionary<string, int>();
            List<IntermediateTime> intermediates = new List<IntermediateTime>();
            Dictionary<string, RelayLegInfo> relayLegs = new Dictionary<string, RelayLegInfo>();
            using (var reader = cmd.ExecuteReader())
            {

                while (reader.Read())
                {
                    string className = reader["className"] as string;
                    if (!string.IsNullOrEmpty(className))
                        className = className.Trim();

                    string cleanClassName = className;

                    if (isRelay)
                    {
                        if (!className.EndsWith("-"))
                            className += "-";

                        className += reader["leg"].ToString().Trim();
                    }


                    string intermediateName = reader["intermediateName"] as string;
                    if (isRelay)
                    {
                        if (intermediateName.Contains("-"))
                        {
                            intermediateName = intermediateName.Split('-')[1].Trim();
                        }
                    }
                    if (!string.IsNullOrEmpty(intermediateName))
                        intermediateName = intermediateName.Trim();
                    int position = Convert.ToInt32(reader["Ipos"]);

                    if (isRelay)
                    {
                        if (!relayLegs.ContainsKey(className))
                        {
                            relayLegs.Add(className, new RelayLegInfo
                            {
                                ITimeForFinish = position
                            });
                        }
                        else
                        {
                            if (position > relayLegs[className].ITimeForFinish)
                                relayLegs[className].ITimeForFinish = position;
                        }
                        if (!classMaxLeg.ContainsKey(cleanClassName))
                        {
                            classMaxLeg.Add(cleanClassName, Convert.ToInt32(reader["leg"]));
                        }
                        else
                        {
                            if (Convert.ToInt32(reader["leg"]) > classMaxLeg[cleanClassName])
                                classMaxLeg[cleanClassName] = Convert.ToInt32(reader["leg"]);
                        }
                    }


                    intermediates.Add(new IntermediateTime { ClassName = className, IntermediateName = intermediateName, Position = position });
                }
                reader.Close();
            }

            if (isRelay)
            {
                foreach (var kvp in classMaxLeg)
                {
                    var cName = kvp.Key;
                    if (!cName.EndsWith("-"))
                    {
                        cName += "-";
                    }
                    cName += kvp.Value;
                    relayLegs[cName].IsLastLeg = true;
                }

                foreach (var relayLeg in relayLegs)
                {
                    if (!relayLeg.Value.IsLastLeg)
                    {
                        var toDelete = intermediates.FirstOrDefault(x => x.ClassName == relayLeg.Key && x.Position == relayLeg.Value.ITimeForFinish);
                        if (toDelete != null)
                            intermediates.Remove(toDelete);
                    }
                }
            }
            return relayLegs;
        }

        private class RelayLegInfo
        {
            public int ITimeForFinish { get; set; }
            public bool IsLastLeg { get; set; }
        }

        private class IntermediateTime
        {
            public string ClassName { get; set; }
            public string IntermediateName { get; set; }
            public int Position { get; set; }
        }

        private void ParseReader(IDbCommand ssfCmd, int olaEventId, int olaRaceId)
        {
            using (var reader = ssfCmd.ExecuteReader())
            {
                using (var olaCmd = m_olaConnection.CreateCommand())
                {
                    while (reader.Read())
                    {
                        int startNumber = Convert.ToInt32(reader["startno"]);
                        DateTime startTime = DateTime.MinValue;
                        DateTime finishTime = DateTime.MinValue;
                        int time = -9;

                        if (reader["allocatedstarttime"] != null && reader["allocatedstarttime"] != DBNull.Value)
                        {
                            startTime = ParseDateTime(reader["allocatedstarttime"].ToString());
                        }
                        if (reader["starttime"] != null && reader["starttime"] != DBNull.Value)
                        {
                            startTime = ParseDateTime(reader["starttime"].ToString());
                            startTime = startTime.AddMilliseconds(-1 * startTime.Millisecond);
                        }

                        if (reader["finishTime"] != null && reader["finishTime"] != DBNull.Value)
                        {
                            finishTime = ParseDateTime(reader["finishTime"].ToString());
                            finishTime = finishTime.AddMilliseconds(-1 * finishTime.Millisecond);
                        }

                        if (reader["RaceTime"] != null && reader["RaceTime"] != DBNull.Value)
                        {
                            time = GetSSFRunTime(reader["RaceTime"].ToString());
                            time = ((int)(time / 100d)) * 100;
                        }

                        olaCmd.CommandText = "select results.resultId, startTime, finishTime, totalTime, "
                                             + "runnerStatus from results, entries, raceClasses where entries.entryId=results.entryId and results.RaceClassId=raceClasses.RaceClassId and raceClasses.eventRaceId=" + olaRaceId
                                             + " and entries.eventId=" + olaEventId + " and results.bibNumber=" + startNumber + " and raceClasses.raceClassStatus <> 'notUsed'";

                        DateTime olaStartTime = DateTime.MinValue;
                        DateTime olaFinishTime = DateTime.MinValue;
                        int olaTime = -9;
                        int olaResultId = -1;
                        bool currentOlaStatusIsNotActivated = false;
                        using (var olaReader = olaCmd.ExecuteReader())
                        {
                            if (olaReader.Read())
                            {
                                olaResultId = Convert.ToInt32(olaReader["resultId"]);
                                if (olaReader["starttime"] != null && olaReader["starttime"] != DBNull.Value)
                                {
                                    string tTime = Convert.ToDateTime(olaReader["starttime"]).ToString("yyyy-MM-dd HH:mm:ss.fff");
                                    olaStartTime = ParseDateTime(tTime);
                                }

                                if (olaReader["finishTime"] != null && olaReader["finishTime"] != DBNull.Value)
                                {
                                    string tTime = Convert.ToDateTime(olaReader["finishTime"]).ToString("yyyy-MM-dd HH:mm:ss.fff");
                                    olaFinishTime = ParseDateTime(tTime);
                                }

                                if (olaReader["runnerStatus"] as string == "notActivated")
                                {
                                    currentOlaStatusIsNotActivated = true;
                                }

                                if (olaReader["totaltime"] != null && olaReader["totaltime"] != DBNull.Value)
                                {
                                    olaTime = Convert.ToInt32(olaReader["totalTime"].ToString());
                                    if (olaTime <= 0)
                                        olaTime = -9;
                                }
                            }
                            olaReader.Close();
                        }

                        if (olaResultId >= 0)
                        {

                            if (olaStartTime != startTime
                                || olaTime != time || finishTime != olaFinishTime)
                            {

                                olaCmd.CommandText = "update results set ";

                                bool first = true;

                                if (olaStartTime != startTime)
                                {
                                    logit("Setting starttime for bib: " + startNumber + " to " + startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                                    olaCmd.CommandText += "startTime = '" + startTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' ";
                                    first = false;
                                }
                                if (olaFinishTime != finishTime)
                                {
                                    logit("Setting finishTime for bib: " + startNumber + " to " + finishTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));
                                    if (!first)
                                        olaCmd.CommandText += ",";
                                    olaCmd.CommandText += " finishTime='" + finishTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
                                    first = false;
                                }

                                if (olaTime != time)
                                {
                                    logit("Setting time for bib: " + startNumber + " to " + time +
                                          (currentOlaStatusIsNotActivated ? " [and status prel.]" : ""));
                                    if (!first)
                                        olaCmd.CommandText += ",";
                                    olaCmd.CommandText += " totaltime = " + time;
                                    if (currentOlaStatusIsNotActivated)
                                    {
                                        olaCmd.CommandText += ", runnerStatus='finishedTimeOk'";
                                    }
                                    first = false;
                                }
                                olaCmd.CommandText += ", modifyDate='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
                                olaCmd.CommandText += " where resultId=" + olaResultId;
                                olaCmd.ExecuteNonQuery();
                            }
                        }

                    }
                    reader.Close();
                }
            }
        }

        private void ParseRelayReader(IDbCommand ssfCmd, int olaEventId, int olaRaceId)
        {
            using (var reader = ssfCmd.ExecuteReader())
            {
                using (var olaCmd = m_olaConnection.CreateCommand())
                {
                    while (reader.Read())
                    {
                        int startNumber = Convert.ToInt32(reader["startno"]);
                        DateTime finishTime = DateTime.MinValue;

                        if (reader["finishTime"] != null && reader["finishTime"] != DBNull.Value)
                        {
                            finishTime = ParseDateTime(reader["finishTime"].ToString());
                            finishTime = finishTime.AddMilliseconds(-1 * finishTime.Millisecond);
                            int leg = Convert.ToInt32(reader["leg"]);

                            olaCmd.CommandText = "select results.resultId, startTime, finishTime, totalTime, "
                                                 + "runnerStatus from results, entries, raceClasses where entries.entryId=results.entryId and results.RaceClassId=raceClasses.RaceClassId and raceClasses.eventRaceId=" + olaRaceId
                                                 + " and entries.eventId=" + olaEventId + " and results.bibNumber=" + startNumber + " and raceClasses.raceClassStatus <> 'notUsed' and raceClasses.relayLeg=" + leg;

                            DateTime olaStartTime = DateTime.MinValue;
                            DateTime olaFinishTime = DateTime.MinValue;
                            int olaTime = -9;
                            int olaResultId = -1;
                            bool currentOlaStatusIsNotActivated = false;
                            using (var olaReader = olaCmd.ExecuteReader())
                            {
                                if (olaReader.Read())
                                {
                                    olaResultId = Convert.ToInt32(olaReader["resultId"]);
                                    if (olaReader["starttime"] != null && olaReader["starttime"] != DBNull.Value)
                                    {
                                        string tTime = Convert.ToDateTime(olaReader["starttime"]).ToString("yyyy-MM-dd HH:mm:ss.fff");
                                        olaStartTime = ParseDateTime(tTime);
                                    }

                                    if (olaReader["finishTime"] != null && olaReader["finishTime"] != DBNull.Value)
                                    {
                                        string tTime = Convert.ToDateTime(olaReader["finishTime"]).ToString("yyyy-MM-dd HH:mm:ss.fff");
                                        olaFinishTime = ParseDateTime(tTime);
                                    }

                                    if (olaReader["runnerStatus"] as string == "notActivated")
                                    {
                                        currentOlaStatusIsNotActivated = true;
                                    }

                                    if (olaReader["totaltime"] != null && olaReader["totaltime"] != DBNull.Value)
                                    {
                                        olaTime = Convert.ToInt32(olaReader["totalTime"].ToString());
                                        if (olaTime <= 0)
                                            olaTime = -9;
                                    }
                                }
                                olaReader.Close();
                            }



                            if (olaResultId >= 0)
                            {
                                //int time = (int)Math.Round((finishTime - olaStartTime).TotalSeconds * 100);
                                int time = (int)(finishTime - olaStartTime).TotalSeconds * 100;

                                if (olaTime != time || finishTime != olaFinishTime)
                                {

                                    olaCmd.CommandText = "update results set ";

                                    bool first = true;

                                    //if (olaStartTime != startTime)
                                    //{
                                    //    logit("Setting starttime for bib: " + startNumber + " to " + startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                                    //    olaCmd.CommandText += "startTime = '" + startTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' ";
                                    //    first = false;
                                    //}
                                    if (olaFinishTime != finishTime)
                                    {
                                        logit("Setting finishTime for bib: " + startNumber + ":" + leg + " to " + finishTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                        if (!first)
                                            olaCmd.CommandText += ",";
                                        olaCmd.CommandText += " finishTime='" + finishTime.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                        first = false;
                                    }

                                    if (olaTime != time)
                                    {
                                        logit("Setting time for bib: " + startNumber + ":" + leg + " to " + time +
                                              (currentOlaStatusIsNotActivated ? " [and status prel.]" : ""));
                                        if (!first)
                                            olaCmd.CommandText += ",";
                                        olaCmd.CommandText += " totaltime = " + time;
                                        if (currentOlaStatusIsNotActivated)
                                        {
                                            olaCmd.CommandText += ", runnerStatus='finishedTimeOk'";
                                        }
                                        first = false;
                                    }
                                    olaCmd.CommandText += ", modifyDate='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
                                    olaCmd.CommandText += " where resultId=" + olaResultId;
                                    olaCmd.ExecuteNonQuery();
                                }
                            }

                        }
                        
                    }
                    reader.Close();
                }
            }
        }

        private void ParseRelaySplitReader(IDbCommand ssfCmd, int olaEventId, int olaRaceId, Dictionary<string,RelayLegInfo> relayLegs)
        {
            using (var reader = ssfCmd.ExecuteReader())
            {
                using (var olaCmd = m_olaConnection.CreateCommand())
                {
                    while (reader.Read())
                    {
                        int startNumber = Convert.ToInt32(reader["startno"]);
                        DateTime finishTime = DateTime.MinValue;

                        if (reader["finishTime"] != null && reader["finishTime"] != DBNull.Value)
                        {
                            finishTime = ParseDateTime(reader["finishTime"].ToString());
                            finishTime = finishTime.AddMilliseconds(-1 * finishTime.Millisecond);
                            int leg = Convert.ToInt32(reader["leg"]);

                            string className = reader["className"] + "-" + leg;
                            if (Convert.ToInt32(reader["Ipos"]) != relayLegs[className].ITimeForFinish)
                                continue;

                            olaCmd.CommandText = "select results.resultId, startTime, finishTime, totalTime, "
                                                 + "runnerStatus from results, entries, raceClasses where entries.entryId=results.entryId and results.RaceClassId=raceClasses.RaceClassId and raceClasses.eventRaceId=" + olaRaceId
                                                 + " and entries.eventId=" + olaEventId + " and results.bibNumber=" + startNumber + " and raceClasses.raceClassStatus <> 'notUsed' and raceClasses.relayLeg=" + leg;

                            DateTime olaStartTime = DateTime.MinValue;
                            DateTime olaFinishTime = DateTime.MinValue;
                            int olaTime = -9;
                            int olaResultId = -1;
                            bool currentOlaStatusIsNotActivated = false;
                            using (var olaReader = olaCmd.ExecuteReader())
                            {
                                if (olaReader.Read())
                                {
                                    olaResultId = Convert.ToInt32(olaReader["resultId"]);
                                    if (olaReader["starttime"] != null && olaReader["starttime"] != DBNull.Value)
                                    {
                                        string tTime = Convert.ToDateTime(olaReader["starttime"]).ToString("yyyy-MM-dd HH:mm:ss.fff");
                                        olaStartTime = ParseDateTime(tTime);
                                    }

                                    if (olaReader["finishTime"] != null && olaReader["finishTime"] != DBNull.Value)
                                    {
                                        string tTime = Convert.ToDateTime(olaReader["finishTime"]).ToString("yyyy-MM-dd HH:mm:ss.fff");
                                        olaFinishTime = ParseDateTime(tTime);
                                    }

                                    if (olaReader["runnerStatus"] as string == "notActivated")
                                    {
                                        currentOlaStatusIsNotActivated = true;
                                    }

                                    if (olaReader["totaltime"] != null && olaReader["totaltime"] != DBNull.Value)
                                    {
                                        olaTime = Convert.ToInt32(olaReader["totalTime"].ToString());
                                        if (olaTime <= 0)
                                            olaTime = -9;
                                    }
                                }
                                olaReader.Close();
                            }



                            if (olaResultId >= 0)
                            {
                                int time = (int)(finishTime - olaStartTime).TotalSeconds * 100;
                                //int time = (int)Math.Round((finishTime - olaStartTime).TotalSeconds * 100);

                                if (olaTime != time || finishTime != olaFinishTime)
                                {

                                    olaCmd.CommandText = "update results set ";

                                    bool first = true;

                                    //if (olaStartTime != startTime)
                                    //{
                                    //    logit("Setting starttime for bib: " + startNumber + " to " + startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"));

                                    //    olaCmd.CommandText += "startTime = '" + startTime.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' ";
                                    //    first = false;
                                    //}
                                    if (olaFinishTime != finishTime)
                                    {
                                        logit("Setting finishTime for bib: " + startNumber + ":" + leg + " to " + finishTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                        if (!first)
                                            olaCmd.CommandText += ",";
                                        olaCmd.CommandText += " finishTime='" + finishTime.ToString("yyyy-MM-dd HH:mm:ss") + "'";
                                        first = false;
                                    }

                                    if (olaTime != time)
                                    {
                                        logit("Setting time for bib: " + startNumber + ":" + leg + " to " + time +
                                              (currentOlaStatusIsNotActivated ? " [and status prel.]" : ""));
                                        if (!first)
                                            olaCmd.CommandText += ",";
                                        olaCmd.CommandText += " totaltime = " + time;
                                        if (currentOlaStatusIsNotActivated)
                                        {
                                            olaCmd.CommandText += ", runnerStatus='finishedTimeOk'";
                                        }
                                        first = false;
                                    }
                                    olaCmd.CommandText += ", modifyDate='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "'";
                                    olaCmd.CommandText += " where resultId=" + olaResultId;
                                    olaCmd.ExecuteNonQuery();

                                    if (olaFinishTime != finishTime)
                                    {
                                        /*Sätt också start på nästa sträcka*/
                                        olaCmd.CommandText = "select results.resultId from results, entries, raceClasses where entries.entryId=results.entryId and results.RaceClassId=raceClasses.RaceClassId and raceClasses.eventRaceId=" + olaRaceId
                                               + " and entries.eventId=" + olaEventId + " and results.bibNumber=" + startNumber + " and raceClasses.raceClassStatus <> 'notUsed' and raceClasses.relayLeg=" + (leg+1);
                                        int resultId = Convert.ToInt32(olaCmd.ExecuteScalar());
                                        logit("Setting startTime for bib: " + startNumber + ":" + (leg+1) + " to " + finishTime.ToString("yyyy-MM-dd HH:mm:ss"));
                                        olaCmd.CommandText = "update results set startTime='" + finishTime.ToString("yyyy-MM-dd HH:mm:ss") + "', modifyDate='" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "' where resultId=" + resultId;
                                        olaCmd.ExecuteNonQuery();
                                    }
                                }
                            }

                        }

                    }
                    reader.Close();
                }
            }
        }


        private static int GetSSFRunTime(string runTime)
        {
            int factor = 1;
            if (runTime.StartsWith("-"))
            {
                factor = -1;
                runTime = runTime.Substring(1);
            }

            DateTime dt;
            if (!DateTime.TryParseExact(runTime, "HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
            {
                if (!DateTime.TryParseExact(runTime, "HH:mm:ss.ff", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                {
                    if (!DateTime.TryParseExact(runTime, "HH:mm:ss.f", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                    {
                        if (!DateTime.TryParseExact(runTime, "HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out dt))
                        {
                            throw new ApplicationException("Could not parse Time" + runTime);
                        }
                    }
                }
            }

            return (int)Math.Round(dt.TimeOfDay.TotalSeconds * 100 * factor);
        }

        private static DateTime ParseDateTime(string tTime)
        {
            if (!string.IsNullOrEmpty(tTime))
                tTime = tTime.Trim();
            DateTime startTime;
            if (!DateTime.TryParseExact(tTime, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
            {
                if (!DateTime.TryParseExact(tTime, "yyyy-MM-dd HH:mm:ss.f", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                {
                    if (!DateTime.TryParseExact(tTime, "yyyy-MM-dd HH:mm:ss.ff", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                    {
                        if (!DateTime.TryParseExact(tTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture, DateTimeStyles.None, out startTime))
                        {
                        }
                    }
                }
            }
            return startTime;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            backgroundWorker1.RunWorkerAsync(new StartArgs
            {
                OlaEventId = (cmbOLA.SelectedItem as ListComp).Id,
                SSFTimingEventId = (cmbSSF.SelectedItem as ListComp).Id,
                StartTime2 = checkBox1.Checked,
                OlaRaceId = (cmbRace.SelectedItem as ListComp).Id,
                IsRelay = checkBox2.Checked
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }

        private void createXMLimportFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Title = "Select IOF startlist to import";
            if (ofd.ShowDialog(this) == DialogResult.OK)
            {
                string[] lines = File.ReadAllLines(ofd.FileName, Encoding.Default);
                string[] header = lines[0].Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                lines = lines.Skip(1).ToArray();

                int idxChip = Array.IndexOf(header, "ID");
                if (idxChip == -1)
                    idxChip = Array.IndexOf(header, "Emit");
                int idxClub = Array.IndexOf(header, "FED");
                if (idxClub == -1)
                    idxClub = Array.IndexOf(header, "Country");
                int idxLastName = Array.IndexOf(header, "Surname");
                if (idxLastName == -1)
                    idxLastName = Array.IndexOf(header, "Last Name");
                int idxFirstName = Array.IndexOf(header, "First name");
                if (idxFirstName == -1)
                    idxFirstName = Array.IndexOf(header, "First Name");
                int idxBibNo = Array.IndexOf(header, "Chest No");
                if (idxBibNo == -1)
                    idxBibNo = Array.IndexOf(header, "BibNo");
                int idxStartTime = Array.IndexOf(header, "Start Time");
                if (idxStartTime == -1)
                    idxStartTime = Array.IndexOf(header, "Start");
                int idxHeat = Array.IndexOf(header, "Heat");
                int idxIofID = Array.IndexOf(header, "IOF-ID");
                if (idxIofID == -1)
                    idxIofID = Array.IndexOf(header, "IOF ID");

                FrmSelectClass importClass = new FrmSelectClass();
                if (importClass.ShowDialog(this) == DialogResult.OK)
                {
                    List<Runner> runners = new List<Runner>();
                    foreach (var line in lines)
                    {
                        string[] parts = line.Split(new string[] { ",", ";" }, StringSplitOptions.RemoveEmptyEntries);
                        int chip = int.Parse(parts[idxChip]);
                        string club = parts[idxClub];
                        string firstName = parts[idxFirstName];
                        string lastName = parts[idxLastName];
                        int bibNo = int.Parse(parts[idxBibNo]);
                        string startTime = parts[idxStartTime];
                        int id = Convert.ToInt32(parts[idxIofID]);

                        EventClass eclass = importClass.Class;
                        if (importClass.EventWithHeats)
                        {
                            if (parts[idxHeat] == "1")
                                eclass = importClass.Heat1;
                            else if (parts[idxHeat] == "2")
                                eclass = importClass.Heat2;
                            else if (parts[idxHeat] == "3")
                                eclass = importClass.Heat3;
                            else
                            {
                                MessageBox.Show("Unknown heat!");
                            }
                        }

                        runners.Add(new Runner
                        {
                            CardNo = chip,
                            Club = club,
                            Country = club,
                            FirstName = firstName,
                            LastName = lastName,
                            BibNumber = bibNo,
                            StartTime = startTime,
                            Class = eclass,
                            EntryId = id

                        });
                    }

                    using (var xtw = XmlWriter.Create(ofd.FileName + ".xml", new XmlWriterSettings() { Indent = true }))
                    {
                        xtw.WriteStartElement("EntryList");
                        xtw.WriteStartElement("IOFVersion");
                        xtw.WriteAttributeString("version", "2.0.3");
                        xtw.WriteEndElement();

                        int clubId = ofd.FileName.Contains("Men") ? 2000001 : 1000001 ;
                        foreach (var club in runners.GroupBy(x => x.Club))
                        {
                            xtw.WriteStartElement("ClubEntry");
                            xtw.WriteStartElement("Club");
                            xtw.WriteElementString("ClubId", clubId.ToString());
                            xtw.WriteElementString("Name", club.Key);
                            xtw.WriteElementString("ShortName", club.Key);
                            xtw.WriteEndElement();

                            foreach (var entry in club)
                            {
                                xtw.WriteStartElement("Entry");
                                xtw.WriteAttributeString("nonCompetitor", "N");
                                xtw.WriteElementString("EntryId", entry.CardNo.ToString());
                                xtw.WriteStartElement("Person");
                                xtw.WriteAttributeString("sex", entry.Class.Sex);

                                /*xtw.WriteStartElement("PersonName");
                                xtw.WriteElementString("Family", entry.LastName);

                                xtw.WriteStartElement("Given");
                                xtw.WriteAttributeString("sequence", 1.ToString());
                                xtw.WriteValue(entry.FirstName);
                                xtw.WriteEndElement();*/
                                xtw.WriteStartElement("PersonName");
                                xtw.WriteElementString("Family", entry.LastName);

                                xtw.WriteStartElement("Given");
                                xtw.WriteAttributeString("sequence", 1.ToString());
                                xtw.WriteValue(entry.FirstName);
                                xtw.WriteEndElement();

                                xtw.WriteEndElement();

                                xtw.WriteStartElement("PersonId");
                                xtw.WriteAttributeString("idManager", "IOF");
                                xtw.WriteAttributeString("type", "int");
                                xtw.WriteValue(entry.EntryId.ToString());
                                xtw.WriteEndElement();

                                xtw.WriteEndElement();

                                xtw.WriteStartElement("CCard");
                                xtw.WriteElementString("CCardId", entry.CardNo.ToString());
                                xtw.WriteStartElement("PunchingUnitType");
                                xtw.WriteAttributeString("value", "Emit");
                                xtw.WriteEndElement();
                                xtw.WriteEndElement();



                                xtw.WriteStartElement("EntryClass");
                                xtw.WriteAttributeString("sequence", "1");
                                string classId = entry.Class.Id.ToString();
                                xtw.WriteElementString("ClassId", classId);
                                xtw.WriteEndElement();


                                xtw.WriteEndElement();
                            }

                            clubId++;

                            xtw.WriteEndElement();
                        }

                        xtw.WriteEndElement();
                        xtw.Close();


                    }

                    if (MessageBox.Show("Do import", "Import generated XML-file in OLA och press OK to update with bibNumbers and StartTimes", MessageBoxButtons.OKCancel) == DialogResult.OK)
                    {
                        var olaConnection = new MySqlConnection(ConfigurationManager.ConnectionStrings["ola"].ConnectionString);
                        olaConnection.Open();
                        using (IDbCommand cmd = olaConnection.CreateCommand())
                        {
                            foreach (var runner in runners)
                            {
                                var startTime = importClass.StartDate.ToShortDateString() + " " + runner.StartTime;
                                cmd.CommandText = "update results set bibNumber=" + runner.BibNumber + ", allocatedStartTime='" + startTime + "', startTime = '" + startTime + "' where entryid = (select entryId from entries where externalId = " + runner.CardNo + ")";
                                cmd.ExecuteNonQuery();
                            }
                        }
                        olaConnection.Close();
                    }
                }
            }
        }

        private void createSSFTimingStartlistFromOLAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.csv|*.csv";
            sfd.Title = "Save startlist as";
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                using (var sw = new StreamWriter(sfd.FileName, false, Encoding.Default))
                {
                    sw.WriteLine("Förnamn;Efternamn;Klubb;Klass;Startnr;Starttid;Födelsedatum;Chipnr;Chipnr2");
                    using (var olaConnection = new MySqlConnection(ConfigurationManager.ConnectionStrings["ola"].ConnectionString))
                    {
                        olaConnection.Open();
                        using (IDbCommand cmd = olaConnection.CreateCommand())
                        {
                            string sql = @"select r.bibNumber, r.startTime, p.firstName, p.FamilyName,
                            o.name, epc.cardNumber, ec.name as classname, pi.externalId
                            from entries e, results r, raceClasses rc, electronicPunchingCards epc,
                            persons p, organisations o, eventClasses ec, personIds pi
                            where r.raceClassid = rc.raceClassId and 
                            pi.personId = p.personId and
                            ec.eventClassId=rc.eventClassId and
                            epc.cardId = r.electronicPunchingCardId and p.personId = e.competitorId 
                            and e.entryId = r.entryId and rc.eventRaceId=1 
                            and p.defaultOrganisationId=o.organisationId and rc.raceClassStatus <> 'notUsed' order by bibNumber";
                            cmd.CommandText = sql;

                            using (var reader = cmd.ExecuteReader())
                            {
                                while (reader.Read())
                                {
                                    sw.WriteLine(reader["firstName"] as string + ";" +
                                        reader["FamilyName"] as string + ";" +
                                        reader["Name"] as string + ";" +
                                        reader["classname"] as string + ";" +
                                        reader["bibNumber"] as string + ";" +
                                        reader["startTime"] as string + ";;" +
                                        reader["cardNumber"].ToString() + ";" +
                                        (Convert.ToInt32(reader["cardNumber"]) + 1000) + ";" +
                                        reader["externalId"] as string);
                                }
                                reader.Close();
                            }
                            sw.Close();
                        }

                        olaConnection.Close();
                    }
                }
            }
        }

        private void cmbOLA_SelectedIndexChanged(object sender, EventArgs e)
        {
            using (IDbCommand cmd = m_olaConnection.CreateCommand())
            {
                cmd.CommandText = "select eventraceId, name from EventRaces where eventId=" + (cmbOLA.SelectedItem as ListComp).Id;

                cmbRace.Items.Clear();
                IDataReader reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    var cmp = new ListComp();
                    cmp.Id = Convert.ToInt32(reader["eventRaceId"].ToString());
                    cmp.Name = Convert.ToString(reader["name"]);
                    cmbRace.Items.Add(cmp);
                }
                reader.Close();
                cmd.Dispose();

                if (cmbRace.Items.Count > 0)
                    cmbRace.SelectedIndex = 0;
            }
        }

        private void createREsultsFromOLAToolStripMenuItem_Click(object sender, EventArgs e)
        {
            SaveFileDialog sfd = new SaveFileDialog();
            sfd.Filter = "*.csv|*.csv";
            sfd.Title = "Save startlist as";
            if (sfd.ShowDialog(this) == DialogResult.OK)
            {
                using (var sw = new StreamWriter(sfd.FileName, false, Encoding.Default))
                {
                    sw.WriteLine("Place;Förnamn;Efternamn;Klubb;Klass;TotalTid");
                    using (var olaConnection = new MySqlConnection(ConfigurationManager.ConnectionStrings["ola"].ConnectionString))
                    {
                        olaConnection.Open();
                        using (IDbCommand cmd = olaConnection.CreateCommand())
                        {
                            string sql = @"select p.firstName, p.FamilyName,
                            o.name, ec.name as classname, r.totalTime, r.runnerStatus
                            from entries e, results r, raceClasses rc,
                            persons p, organisations o, eventClasses ec
                            where r.raceClassid = rc.raceClassId and 
                            ec.eventClassId=rc.eventClassId and
                            p.personId = e.competitorId 
                            and e.entryId = r.entryId and rc.eventRaceId=1
                            and p.defaultOrganisationId=o.organisationId and rc.raceClassStatus <> 'notUsed' order by className,runnerStatus desc, totalTime";
                            cmd.CommandText = sql;

                            using (var reader = cmd.ExecuteReader())
                            {
                                var lastTime = "";
                                int pl = 1;
                                int curPl = 1;
                                while (reader.Read())
                                {
                                    //if (reader["totalTime"] != null && reader["totalTime"] != DBNull.Value)
                                    {
                                        var time = (reader["totalTime"] != null && reader["totalTime"] != DBNull.Value) ? formatTime(Convert.ToInt32(reader["totalTime"]), reader["runnerStatus"] as string, true, true, false) : "OKÄND";
                                        var strPl = time == lastTime ? "=" : pl.ToString();

                                        sw.WriteLine(pl + ";" + reader["firstName"] as string + ";" +
                                            reader["FamilyName"] as string + ";" +
                                            reader["Name"] as string + ";" +
                                            reader["classname"] as string + ";" +
                                           time);

                                        lastTime = time;
                                        pl++;
                                    }
                                }
                                reader.Close();
                            }
                            sw.Close();
                        }

                        olaConnection.Close();
                    }
                }
            }
        }

         public string formatTime(int time, string status, bool showHours=false, bool padZeros=true, bool showTenthOfSecs = true) {
             Dictionary<string,string> runnerStatus = new Dictionary<string,string>();
            runnerStatus.Add("notStarted", "DNS");
            runnerStatus.Add("passed", "OK");
            runnerStatus.Add("notValid", "MP");
            runnerStatus.Add("notActivated", "Start");
            runnerStatus.Add("disqualified", "DSQ");
            int iminutes;
            int iseconds;
            int ihours;
            int tenth;
            if (status != "passed") {
                return runnerStatus[status];
            } else {
                if (showHours) {
                    ihours = (int)Math.Floor(time / 360000d);
                    iminutes = (int)Math.Floor((time - ihours * 360000) / 6000d);
                    iseconds = (int)Math.Floor((time - iminutes * 6000 - ihours * 360000d) / 100);
                    tenth = (int)Math.Floor((time - iminutes * 6000 - ihours * 360000 - iseconds * 100) / 10d);


                    if (true|| ihours > 0) {
                        return (padZeros ? strPad(ihours, 2) : ihours + "") + ":" + strPad(iminutes, 2) + ":" + strPad(iseconds, 2) + (showTenthOfSecs ? "." + tenth : "");
                    } else {
                        return (padZeros ? strPad(iminutes, 2) : iminutes + "") + ":" + strPad(iseconds, 2) + (showTenthOfSecs ? "." + tenth : "");
                    }

                } else {

                    iminutes = (int)Math.Floor(time / 6000d);
                    iseconds = (int)Math.Floor((time - iminutes * 6000d) / 100);
                    tenth = (int)Math.Floor((time - iminutes * 6000 - iseconds * 100) / 10d);
                    if (padZeros) {
                        return strPad(iminutes, 2) + ":" + strPad(iseconds, 2) + (showTenthOfSecs ? "." + tenth : "");
                    } else {
                        return iminutes + ":" + strPad(iseconds, 2) + (showTenthOfSecs ? "." + tenth : "");
                    }
                }
            }
        }
         public string strPad(int num, int length){

            var str = "" + num;
            while (str.Length < length) {
                str = '0' + str;
            }

            return str;
        }

         private void createSSFTimingRelaystartlistFromOLAToolStripMenuItem_Click(object sender, EventArgs e)
         {
             SaveFileDialog sfd = new SaveFileDialog();
             sfd.Filter = "*.csv|*.csv";
             sfd.Title = "Save startlist as";
             if (sfd.ShowDialog(this) == DialogResult.OK)
             {
                 using (var sw = new StreamWriter(sfd.FileName, false, Encoding.Default))
                 {
                     using (var sw2 = new StreamWriter(sfd.FileName + ".spec.txt", false, Encoding.Default))
                     {
                         sw.WriteLine("Lagnamn;Lagnummer;Klass;Startnr;Starttid;Country;Fiscode;Förnamn 1;Efternamn 1;Chip1-1;Chip2- 1;Fispunkter Sprint 1;Fispunkter Distans 1;Förnamn 2;Efternamn 2;Chip1-2;Chip2- 2;Fispunkter Sprint 2;Fispunkter Distans 2;Förnamn 3;Efternamn 3;Chip1-3;Chip2- 3;Fispunkter Sprint 3;Fispunkter Distans 3;Förnamn 4;Efternamn 4;Chip1-4;Chip2- 4;Fispunkter Sprint 4;Fispunkter Distans 4");
                         using (var olaConnection = new MySqlConnection(ConfigurationManager.ConnectionStrings["ola"].ConnectionString))
                         {
                             olaConnection.Open();
                             using (IDbCommand cmd = olaConnection.CreateCommand())
                             {
                                 string sql = @"select e.teamName, r.bibNumber, ec.name as classname, r.startTime, 
p.firstName as firstName1,p.familyName as familyName1,epc.cardNumber as card1,
(select externalId from personIds where personId = r.relayPersonId) as Fiscode1,

(select firstName from persons where personId = (select relayPersonId from results r2, raceClasses rc2 where rc2.raceClassId=r2.raceClassId and r2.entryId = r.entryId and rc2.relayLeg=2)) as firstName2,
(select familyName from persons where personId = (select relayPersonId from results r2, raceClasses rc2 where rc2.raceClassId=r2.raceClassId and r2.entryId = r.entryId and rc2.relayLeg=2)) as familyName2,
(select cardNumber from electronicPunchingCards where cardId = (select electronicPunchingCardId from results r2, raceClasses rc2 where rc2.raceClassId=r2.raceClassId and r2.entryId = r.entryId and rc2.relayLeg=2)) as card2,
(select externalId from personIds where personId = (select relayPersonId from results r2, raceClasses rc2 where rc2.raceClassId=r2.raceClassId and r2.entryId = r.entryId and rc2.relayLeg=2)) as Fiscode2,

(select firstName from persons where personId = (select relayPersonId from results r2, raceClasses rc2 where rc2.raceClassId=r2.raceClassId and r2.entryId = r.entryId and rc2.relayLeg=3)) as firstName3,
(select familyName from persons where personId = (select relayPersonId from results r2, raceClasses rc2 where rc2.raceClassId=r2.raceClassId and r2.entryId = r.entryId and rc2.relayLeg=3)) as familyName3,
(select cardNumber from electronicPunchingCards where cardId = (select electronicPunchingCardId from results r2, raceClasses rc2 where rc2.raceClassId=r2.raceClassId and r2.entryId = r.entryId and rc2.relayLeg=3)) as card3,
(select externalId from personIds where personId = (select relayPersonId from results r2, raceClasses rc2 where rc2.raceClassId=r2.raceClassId and r2.entryId = r.entryId and rc2.relayLeg=3)) as Fiscode3,

(select firstName from persons where personId = (select relayPersonId from results r2, raceClasses rc2 where rc2.raceClassId=r2.raceClassId and r2.entryId = r.entryId and rc2.relayLeg=4)) as firstName4,
(select familyName from persons where personId = (select relayPersonId from results r2, raceClasses rc2 where rc2.raceClassId=r2.raceClassId and r2.entryId = r.entryId and rc2.relayLeg=4)) as familyName4,
(select cardNumber from electronicPunchingCards where cardId = (select electronicPunchingCardId from results r2, raceClasses rc2 where rc2.raceClassId=r2.raceClassId and r2.entryId = r.entryId and rc2.relayLeg=4)) as card4,
(select externalId from personIds where personId = (select relayPersonId from results r2, raceClasses rc2 where rc2.raceClassId=r2.raceClassId and r2.entryId = r.entryId and rc2.relayLeg=4)) as Fiscode4


from entries e, results r, raceClasses rc, eventclasses ec, electronicPunchingCards epc,
 persons p 
where e.entryId=r.entryId and r.raceClassId=rc.raceClassId and ec.eventClassId=rc.eventClassId 
and r.relayPersonId=p.personId and r.electronicPunchingCardId=epc.cardid and rc.relayLeg=1 
and rc.raceClassStatus <> 'notUsed' order by cast(r.bibNumber as unsigned)";
                                 cmd.CommandText = sql;

                                 using (var reader = cmd.ExecuteReader())
                                 {
                                     while (reader.Read())
                                     {
                                         sw.WriteLine((reader["teamName"] as string).Replace(" 1", "") + ";;" +
                                             reader["ClassName"] as string + ";" +
                                             reader["bibNumber"] as string + ";" +
                                             reader["startTime"] as string + ";;;" +
                                             reader["firstName1"] as string + ";" +
                                             reader["familyName1"] as string + ";" +
                                             reader["card1"].ToString() + ";" +
                                             (Convert.ToInt32(reader["card1"]) + 1000) + ";" +
                                             reader["Fiscode1"] as string + ";;" +

                                             reader["firstName2"] as string + ";" +
                                             reader["familyName2"] as string + ";" +
                                             reader["card2"].ToString() + ";" +
                                             (Convert.ToInt32(reader["card2"]) + 1000) + ";" +
                                             reader["Fiscode2"] as string + ";;" +

                                             reader["firstName3"] as string + ";" +
                                             reader["familyName3"] as string + ";" +
                                             reader["card3"].ToString() + ";" +
                                             (Convert.ToInt32(reader["card3"]) + 1000) + ";" +
                                             reader["Fiscode3"] as string + ";;" 
                                             
                                            /* +

                                             reader["firstName4"] as string + ";" +
                                             reader["familyName4"] as string + ";" +
                                             reader["card4"].ToString() + ";" +
                                             (Convert.ToInt32(reader["card4"]) + 1000) + ";" +
                                             reader["Fiscode4"] as string + ";"*/
                                             );

                                         sw2.WriteLine(reader["bibNumber"] as string + ";" +
                                             (reader["teamName"] as string).Replace(" 1", ""));

                                         sw2.WriteLine(reader["firstName1"] as string + ";" +
                                             reader["familyName1"] as string + ";" +
                                             reader["card1"].ToString() + ";" +
                                             (Convert.ToInt32(reader["card1"]) + 1000) + ";" +
                                             reader["Fiscode1"] as string);

                                         sw2.WriteLine(reader["firstName2"] as string + ";" +
                                             reader["familyName2"] as string + ";" +
                                             reader["card2"].ToString() + ";" +
                                             (Convert.ToInt32(reader["card2"]) + 1000) + ";" +
                                             reader["Fiscode2"] as string);

                                         sw2.WriteLine(reader["firstName3"] as string + ";" +
                                        reader["familyName3"] as string + ";" +
                                        reader["card3"].ToString() + ";" +
                                        (Convert.ToInt32(reader["card3"]) + 1000) + ";" +
                                        reader["Fiscode3"] as string);

                                        /* sw2.WriteLine(reader["firstName4"] as string + ";" +
                                        reader["familyName4"] as string + ";" +
                                        reader["card4"].ToString() + ";" +
                                        (Convert.ToInt32(reader["card4"]) + 1000) + ";" +
                                        reader["Fiscode4"] as string
                                        );*/
                                         sw2.WriteLine();

                                     }
                                     reader.Close();
                                 }
                                 sw.Close();
                                 sw2.Close();
                             }

                             olaConnection.Close();
                         }
                     }
                 }
             }
         }

         private void assignCardstorelayPersonsToolStripMenuItem_Click(object sender, EventArgs e)
         {
             OpenFileDialog ofd = new OpenFileDialog();
             if (ofd.ShowDialog(this) == DialogResult.OK)
             {
                 Dictionary<int, int> tags = new Dictionary<int, int>();
                 using (var sr = new StreamReader(ofd.FileName))
                 {
                     string[] header = sr.ReadLine().Split(';');
                     int idxIofId = Array.IndexOf(header, "IOF ID");
                     int idxTag = Array.IndexOf(header, "emiTag");
                     string tmp;
                     while ((tmp = sr.ReadLine()) != null)
                     {
                         string[] parts = tmp.Split(';');
                         int iofId = Convert.ToInt32(parts[idxIofId]);
                         int tag = Convert.ToInt32(parts[idxTag]);
                         tags.Add(iofId, tag);
                     }
                     sr.Close();
                 }

                 using (var olaConnection = new MySqlConnection(ConfigurationManager.ConnectionStrings["ola"].ConnectionString))
                 {
                     olaConnection.Open();

                     Dictionary<int, int> toSet = new Dictionary<int, int>();

                     using (IDbCommand cmd = olaConnection.CreateCommand())
                     {
                         cmd.CommandText = "select externalId,resultId from personIds,results where results.relayPersonId=personIds.personId and results.electronicPunchingCardId is null";

                         using (var dr = cmd.ExecuteReader())
                         {
                             while (dr.Read())
                             {
                                 int iofId = Convert.ToInt32(dr["externalId"]);
                                 int tag = tags[iofId];
                                 toSet.Add(Convert.ToInt32(dr["resultId"]), tag);
                             }
                             dr.Close();
                         }

                         foreach (var kvp in toSet)
                         {
                             cmd.CommandText = "insert into electronicPunchingCards (cardNumber,electronicPunchingCardType,modifyDate) values(" + kvp.Value + ",'Emit','" + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff") + "')";
                             cmd.ExecuteNonQuery();
                             cmd.CommandText = "select cardId from electronicPunchingCards where cardNumber=" + kvp.Value;
                             int cardId = Convert.ToInt32(cmd.ExecuteScalar());
                             cmd.CommandText = "update results set electronicPunchingCardId=" + cardId + " where resultId=" + kvp.Key;
                             cmd.ExecuteNonQuery();
                         }
                     }
                 }
             }
         }
    }

    class Runner
    {
        public int EntryId;
        public int CardNo;
        public string FirstName;
        public string LastName;
        public string Club;
        public string Country;
        public int BibNumber;
        public string StartTime;
        public EventClass Class;

    }
}
