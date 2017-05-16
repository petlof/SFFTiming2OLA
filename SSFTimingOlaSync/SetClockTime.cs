using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace SSFTimingOlaSync
{
    public partial class SetClockTime : Form
    {
        public SetClockTime()
        {
            InitializeComponent();

            comboBox1.DataSource = SerialPort.GetPortNames();
        }

        private SerialPort sp;
        private StreamReader sr;
        private void button3_Click(object sender, EventArgs e)
        {
            if (sp != null)
            {
                sp.Close();
                sp.Dispose();
                sp = null;
                button3.Text = "Connect";
            }
            else
            {
                sp = new SerialPort(comboBox1.SelectedItem as string, 115200);
                sp.DataReceived += (o, args) =>
                {
                    string tmp;
                    while ((tmp = sp.ReadLine()) != null)
                    {
                        Logit(tmp);
                    }
                };
                sp.ErrorReceived += sp_ErrorReceived;
                sp.Open();

                button3.Text = "Disconnect";
            }

        }

        void sp_ErrorReceived(object sender, SerialErrorReceivedEventArgs e)
        {
            Logit("Got error: " + e.EventType);
            sp.Close();
            sp.Dispose();
            sp = null;
            button3.Text = "Connect";
        }

        private void Logit(string tmp)
        {
            listBox1.Invoke(new MethodInvoker(() =>
            {
                listBox1.Items.Insert(0, tmp);
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
            string msg = "SC" + DateTime.Now.AddMinutes(offsetMinutes).AddSeconds(1).ToString("HH:mm:ss");
            List<byte> bytes = new List<byte>();
            bytes.Add(0x2F);
            bytes.AddRange(System.Text.Encoding.ASCII.GetBytes(msg));
            bytes.Add(0x0D);
            bytes.Add(0x0A);
            byte[] bArr = bytes.ToArray();
            //Send message so that last byte (including / and CR LF) is received at next whole second.
            int waitMs = 1000 - DateTime.Now.Millisecond - 5 * bArr.Length;
            Thread.Sleep(waitMs);
            sendmsg(bArr);
            Logit("Sent message: " + msg + " at time " + DateTime.Now.ToString("HH:mm:ss.fff"));
        }


        private void sendmsg(byte[] bytes)
        {
            foreach (var b in bytes)
            {
                sp.BaseStream.WriteByte(b);
                Thread.Sleep(5);
            }
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
    }
}
