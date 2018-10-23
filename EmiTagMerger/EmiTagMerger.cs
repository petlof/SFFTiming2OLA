using EMITController;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace EmiTagMerger
{
    public partial class EmiTagMerger : Form
    {
        private SerialPort sourcePort;
        private SerialPort targetPort;
        int m_ecuCode = 0;

        public EmiTagMerger()
        {
            InitializeComponent();
            btnRefreshPorts_Click(null, new EventArgs());
        }

        private void listBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            
        }

        private void btnRefreshPorts_Click(object sender, EventArgs e)
        {
            comboBox1.DataSource = SerialPortUtilities.GetSerialPorts(); 
            comboBox1.DisplayMember = "Name";
            comboBox2.DataSource = SerialPortUtilities.GetSerialPorts(); 
            comboBox2.DisplayMember = "Name";
        }

        private void btnConnectSource_Click(object sender, EventArgs e)
        {
            if (sourcePort != null)
            {
                sourcePort.DataReceived -= serialDataReceived;
                sourcePort.ErrorReceived -= sp_ErrorReceived;
                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    sourcePort.Close();
                    sourcePort.Dispose();
                    sourcePort = null;
                    btnConnectSource.Invoke(new MethodInvoker(() =>
                    {
                        btnConnectSource.Text = "Connect";
                    }));
                });
            }
            else
            {
                sourcePort = new SerialPort((comboBox1.SelectedItem as SerialPortUtilities.COMPortInfo).PortName, 115200);
                sourcePort.WriteBufferSize = 10;
                sourcePort.ReceivedBytesThreshold = 1;
                sourcePort.DataReceived += serialDataReceived;
                sourcePort.ErrorReceived += sp_ErrorReceived;
                sourcePort.Open();

                btnConnectSource.Text = "Disconnect";
            }
        }

        private List<byte> curBuf = new List<byte>();
        void serialDataReceived(object o, object args)
        {
            try
            {
                int numRead = 0;
                int toRead = sourcePort.BytesToRead;
                byte[] data = new byte[toRead];
                while (numRead < toRead)
                {
                    int nr = sourcePort.Read(data, numRead, toRead - numRead);
                    numRead += nr;
                }
                curBuf.AddRange(data);

                while (curBuf.Contains(0x03))
                {
                    int etxPos = curBuf.IndexOf(0x03);
                    string tmp = System.Text.Encoding.ASCII.GetString(curBuf.Take(etxPos).Where(x => x != 0x02 && x != '\n' && x != '\r').ToArray());
                    curBuf.RemoveRange(0, etxPos + 1);
                    //Logit(tmp);
                    ParseMessage(tmp);
                }
            }
            catch (Exception ee)
            {
                if (sourcePort != null)
                {
                    MessageBox.Show("Error: " + ee.Message);
                }
            }
        }

        void ParseMessage(string msg)
        {
            var parts = msg.Split('\t');

            TagReadOut ro = new TagReadOut();
            string messageTime = "";
            string hardwareId = "";
            int punchCode = 0;
            string unitSerialNo = "";

            foreach (var p in parts)
            {
                if (p.StartsWith("I"))
                {
                    hardwareId = p.Substring(1);
                }
                else if (p.StartsWith("W"))
                {
                    //m_unitClocktime = p.Substring(1);
                    messageTime = p.Substring(1);
                }
                else if (p.StartsWith("C"))
                {
                    m_ecuCode = int.Parse(p.Substring(1));
                }
                else if (p.StartsWith("Y"))
                {
                    unitSerialNo = p.Substring(1);
                }
                else if (p.StartsWith("N"))
                {
                    ro.TagNo = int.Parse(p.Substring(1));
                }
                else if (p.StartsWith("R"))
                {
                    ro.TagText = p.Substring(1);
                }
                else if (p.StartsWith("P"))
                {
                    string[] passParts = p.Substring(1).Split('-');
                    PostPass pp = new PostPass();
                    pp.PostNo = int.Parse(passParts[0]);
                    pp.PostCode = int.Parse(passParts[1]);
                    var timeParts = passParts[2].Split(':');
                    var time = int.Parse(timeParts[0]) * 1000 * 3600 + int.Parse(timeParts[1]) * 1000 * 60 + Math.Round(double.Parse(timeParts[2], CultureInfo.InvariantCulture), 3) * 1000;
                    pp.Time = TimeSpan.FromMilliseconds(time);
                    ro.PostPasses.Add(pp);
                }
            }

            if (ro.TagNo >= 0 && ro.PostPasses.Count > 0 && ro.PostPasses.Last().PostCode == m_ecuCode)
            {
                ro.TimeOfReadout = DateTime.ParseExact(DateTime.Now.ToString("yyyy-MM-dd") + " " + messageTime, "yyyy-MM-dd HH:mm:ss.fff", CultureInfo.InvariantCulture);
                ListBox lst = ro.TagNo < 1000 ? lstCard1 : lstCard2;
                lst.Invoke(new MethodInvoker(delegate
                {
                    lst.Items.Clear();
                    foreach (var p in ro.PostPasses)
                    {
                        lst.Items.Add(p.PostNo.ToString().PadLeft(2) + "  " + p.PostCode.ToString().PadLeft(3) + "  " + p.Time);
                    }
                }));

               /* var tr = OnTagRead;
                if (tr != null)
                    tr(ro);*/
            }
            else
            {
                txtEcuTime.Invoke(new MethodInvoker(delegate
                {
                    txtEcuTime.Text = messageTime;
                }));
            }
        }

        void sp_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            MessageBox.Show("Got error: " + e.EventType);
            sourcePort.Close();
            sourcePort.Dispose();
            sourcePort = null;
            btnConnectSource.Invoke(new MethodInvoker(() =>
            {
                btnConnectSource.Text = "Connect";
            }));
        }
    }
}
