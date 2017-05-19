using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace EMITController
{
    public partial class SetClockTime : Form
    {
        private DateTime _refTime;
        private Stopwatch _sw;
        public SetClockTime()
        {
            InitializeComponent();

            _sw = new Stopwatch();
            this.Shown += SetClockTime_Shown;
            comboBox1.DataSource = SerialPortUtilities.GetSerialPorts(); //SerialPort.GetPortNames();
            comboBox1.DisplayMember = "Name";
        }

        private void SetClockTime_Shown(object sender, EventArgs e)
        {
            button12_Click(null,new EventArgs());
        }

        private SerialPort sp;
        private void button3_Click(object sender, EventArgs e)
        {
            if (sp != null)
            {
                sp.DataReceived -= serialDataReceived;
                sp.ErrorReceived -= sp_ErrorReceived;
                ThreadPool.QueueUserWorkItem((obj) =>
                {
                    sp.Close();
                    sp.Dispose();
                    sp = null;
                    button3.Invoke(new MethodInvoker(() =>
                    {
                        button3.Text = "Connect";
                    }));
                });
            }
            else
            {
                sp = new SerialPort((comboBox1.SelectedItem as SerialPortUtilities.COMPortInfo).PortName, 115200);
                sp.WriteBufferSize = 10;
                sp.ReceivedBytesThreshold = 1;
                sp.DataReceived += serialDataReceived;
                sp.ErrorReceived += sp_ErrorReceived;
                sp.Open();

                button3.Text = "Disconnect";
            }

        }

        private List<byte> curBuf = new List<byte>();
        void serialDataReceived(object o, object args)
        {
            try
            {
                int numRead = 0;
                int toRead = sp.BytesToRead;
                byte[] data = new byte[toRead];
                while (numRead < toRead)
                {
                    int nr = sp.Read(data, numRead, toRead-numRead);
                    numRead += nr;
                } 
                curBuf.AddRange(data);

                while (curBuf.Contains(0x03))
                {
                    int etxPos = curBuf.IndexOf(0x03);
                    string tmp = System.Text.Encoding.ASCII.GetString(curBuf.Take(etxPos).Where(x=>x!=0x02).ToArray());
                    curBuf.RemoveRange(0, etxPos + 1);
                    Logit(tmp);
                }
            }
            catch (Exception ee)
            {
                if (sp != null)
                {
                    Logit("Error: " + ee.Message);
                }
            }
        }

        void sp_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Logit("Got error: " + e.EventType);
            sp.Close();
            sp.Dispose();
            sp = null;
            button3.Invoke(new MethodInvoker(() =>
            {
                button3.Text = "Connect";
            }));
        }


        private void Logit(string tmp)
        {
            listBox1.Invoke(new MethodInvoker(() =>
            {
                listBox1.Items.Insert(0, (_refTime + _sw.Elapsed).ToString("HH:mm:ss.fff") + ": " + tmp);
            }));
        }

        private void button1_Click(object sender, EventArgs e)
        {
            SetClock(0);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            SetClock(1);
        }

        private void button4_Click(object sender, EventArgs e)
        {
            SetClock(2);
        }

        private void button5_Click(object sender, EventArgs e)
        {
            SetClock(3);
        }

        private void button6_Click(object sender, EventArgs e)
        {
            SetClock(4);
        }

        private void button7_Click(object sender, EventArgs e)
        {
            SetClock(5);
        }

        private void button8_Click(object sender, EventArgs e)
        {
            SetClock(6);
        }

        private void SetClock(int offsetMinutes)
        {
            DateTime currentTime = _refTime + _sw.Elapsed;
            DateTime setTime = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour,
                currentTime.Minute, currentTime.Second);
            setTime = setTime.AddMinutes(offsetMinutes).AddSeconds(1);
            string msg = "SC" + setTime.ToString("HH:mm:ss");
            List<byte> bytes = new List<byte>();
            bytes.Add(0x2F);
            bytes.AddRange(System.Text.Encoding.ASCII.GetBytes(msg));
            bytes.Add(0x0D);
            bytes.Add(0x0A);
            byte[] bArr = bytes.ToArray();
            //Send message so that last byte (including / and CR LF) is received at next whole second.
            //int waitMs = 1000 - _refTime.AddTicks(_sw.ElapsedTicks).Millisecond;//- 5 * bArr.Length;
            //Thread.Sleep(waitMs);

            DateTime setTimeAtSec = new DateTime(currentTime.Year, currentTime.Month, currentTime.Day, currentTime.Hour,
                currentTime.Minute, currentTime.Second).AddSeconds(1).AddMilliseconds(-5 * bytes.Count).AddMilliseconds(-200);//.AddMilliseconds(500);

            while (_refTime + _sw.Elapsed < setTimeAtSec)
            {
                
            }
            sendmsg(bArr);
            Logit("Sent message: " + msg + " at time " + (_refTime + _sw.Elapsed).ToString("HH:mm:ss.fff"));
        }


        private void sendmsg(byte[] bytes)
        {
            var sw = new Stopwatch();
                sw.Start();
                foreach (var b in bytes)
                {
                    sp.BaseStream.WriteByte(b);
                    long el = sw.ElapsedMilliseconds;
                    while (sw.ElapsedMilliseconds < el + 5)
                    {
                        
                    }
                }
                sp.BaseStream.Flush();
                sw.Stop();
        }

       

        private void button9_Click(object sender, EventArgs e)
        {
            comboBox1.DataSource = SerialPort.GetPortNames();
        }

        private void button10_Click(object sender, EventArgs e)
        {
            SetClock(int.Parse(textBox1.Text));
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {

        }

        private void button11_Click(object sender, EventArgs e)
        {
            string msg = "CD" + textBox2.Text;
            List<byte> bytes = new List<byte>();
            bytes.Add(0x2F);
            bytes.AddRange(System.Text.Encoding.ASCII.GetBytes(msg));
            bytes.Add(0x0D);
            bytes.Add(0x0A);
            byte[] bArr = bytes.ToArray();
            sendmsg(bArr);
            Logit("Sent message: " + msg + " at time " + DateTime.Now.ToString("HH:mm:ss.fff"));
        }

        private void timer1_Tick(object sender, EventArgs e)
        {
            textBox3.Text = (_refTime + _sw.Elapsed).ToString("yyyy-MM-dd HH:mm:ss.fff");
        }

        private void button12_Click(object sender, EventArgs e)
        {
            button12.BackColor = Color.Red;
            _refTime = AtomicTime.Now;
            _sw.Reset();
            _sw.Start();
            button12.BackColor = Color.LightGreen;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            DateTime time = (_refTime + _sw.Elapsed).ToUniversalTime();
            SYSTEMTIME st = new SYSTEMTIME();
            st.wYear = (short)time.Year; // must be short
            st.wMonth = (short)time.Month;
            st.wDay = (short)time.Day;
            st.wHour = (short)time.Hour;
            st.wMinute = (short)time.Minute;
            st.wSecond = (short)time.Second;
            st.wMilliseconds = (short) time.Millisecond;

            SetSystemTime(ref st); // invoke this method.
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool SetSystemTime(ref SYSTEMTIME st);

        [StructLayout(LayoutKind.Sequential)]
        public struct SYSTEMTIME
        {
            public short wYear;
            public short wMonth;
            public short wDayOfWeek;
            public short wDay;
            public short wHour;
            public short wMinute;
            public short wSecond;
            public short wMilliseconds;
        }
    }
}
