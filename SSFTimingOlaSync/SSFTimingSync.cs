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
        }
        private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
        {
            int ssfId = ((StartArgs)e.Argument).SSFTimingEventId;
            int olaId = ((StartArgs)e.Argument).OlaEventId;
            int raceId = ((StartArgs)e.Argument).OlaRaceId;
            bool startTime2 = ((StartArgs)e.Argument).StartTime2;
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
                StartTime2 = checkBox1.Checked,
                OlaRaceId = (cmbRace.SelectedItem as ListComp).Id

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
                int idxClub = Array.IndexOf(header, "FED");
                int idxLastName = Array.IndexOf(header, "Surname");
                int idxFirstName = Array.IndexOf(header, "First name");
                int idxBibNo = Array.IndexOf(header, "Chest No");
                int idxStartTime = Array.IndexOf(header, "Start Time");
                int idxHeat = Array.IndexOf(header, "Heat");

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
                            Class = eclass
                        });
                    }

                    using (var xtw = XmlWriter.Create(ofd.FileName + ".xml", new XmlWriterSettings() { Indent = true }))
                    {
                        xtw.WriteStartElement("EntryList");
                        xtw.WriteStartElement("IOFVersion");
                        xtw.WriteAttributeString("version", "2.0.3");
                        xtw.WriteEndElement();

                        int clubId = 1000001;
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

                                xtw.WriteStartElement("PersonName");
                                xtw.WriteElementString("Family", entry.LastName);

                                xtw.WriteStartElement("Given");
                                xtw.WriteAttributeString("sequence", 1.ToString());
                                xtw.WriteValue(entry.FirstName);
                                xtw.WriteEndElement();

                                xtw.WriteEndElement();

                                /*xtw.WriteStartElement("PersonId");
                                xtw.WriteAttributeString("idManager", "Sweden");
                                xtw.WriteAttributeString("type", "nat");
                                xtw.WriteValue(entry.EntryId.ToString());
                                xtw.WriteEndElement();*/

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
                                var startTime = importClass.StartDate.ToShortDateString() + " " + runner.StartTime + ":00";
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
                            o.name, epc.cardNumber, ec.name as classname
                            from entries e, results r, raceClasses rc, electronicPunchingCards epc,
                            persons p, organisations o, eventClasses ec
                            where r.raceClassid = rc.raceClassId and 
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
                                        (Convert.ToInt32(reader["cardNumber"]) + 1000));
                                }
                                reader.Close();
                            }
                            sw.Close();
                            /*foreach (var runner in runners)
                            {
                                var startTime = importClass.StartDate.ToShortDateString() + " " + runner.StartTime + ":00";
                                cmd.CommandText = "update results set bibNumber=" + runner.BibNumber + ", allocatedStartTime='" + startTime + "', startTime = '" + startTime + "' where entryid = (select entryId from entries where externalId = " + runner.CardNo + ")";
                                cmd.ExecuteNonQuery();
                            }*/
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
