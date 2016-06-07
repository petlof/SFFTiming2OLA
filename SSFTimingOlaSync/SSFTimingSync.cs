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
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int ssfId = ((StartArgs) e.Argument).SSFTimingEventId;
            int olaId = ((StartArgs) e.Argument).OlaEventId;
            bool startTime2 = ((StartArgs)e.Argument).StartTime2;
            string initialCommand = string.Format(@"select  dbName.FirstName, dbName.LastName,
 dbTeam.Name as teamname, dbclass.name as classname,
 " + (startTime2 ? "dbName.startTime2" : "dbName.startTime")+ @" as allocatedStartTime, dbRuns.StartTime,dbRuns.FinishTime,dbRuns.RaceTime, dbName.Startno, dbRuns.Status
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

                ParseReader(ssfCmd, olaId);

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
                        ParseReader(ssfCmd, olaId);
                    }
                    lastLogId = maxLogId;

                    Thread.Sleep(100);
                }
            }
        }

        private void ParseReader(IDbCommand ssfCmd, int olaEventId)
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
                        }

                        if (reader["finishTime"] != null && reader["finishTime"] != DBNull.Value)
                        {
                            finishTime = ParseDateTime(reader["finishTime"].ToString());
                        }

                        if (reader["RaceTime"] != null && reader["RaceTime"] != DBNull.Value)
                        {
                            time = GetSSFRunTime(reader["RaceTime"].ToString());
                        }

                        olaCmd.CommandText = "select results.resultId, startTime, finishTime, totalTime, "
                                             + "runnerStatus from results, entries where entries.entryId=results.entryId "
                                             + " and entries.eventId=" + olaEventId + " and results.bibNumber=" + startNumber;
                        
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
                                        olaCmd.CommandText += ", runnerStatus='finished'";
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

            return (int)dt.TimeOfDay.TotalSeconds * 100 * factor;
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
                StartTime2 = checkBox1.Checked
            });
        }

        private void button2_Click(object sender, EventArgs e)
        {
            backgroundWorker1.CancelAsync();
        }
    }
}
