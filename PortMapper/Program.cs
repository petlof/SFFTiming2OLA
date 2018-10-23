using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace PortMapper
{
    class Program
    {
        private static SerialPort source;
        private static SerialPort target;
        private static TcpClient cli;
        private static string sourceName;
        private static string targetName;
        static void Main(string[] args)
        {
            sourceName = args[0];
            int sourceBaudRate = Convert.ToInt32(args[1]);
            
            
            Console.Write("Proxying data from " + args[0] + "(" + sourceBaudRate + ")");
            source = new SerialPort(args[0], sourceBaudRate);
            int targetBaudRate = 0;
            if (args.Length > 2)
            {
                targetBaudRate = Convert.ToInt32(args[3]);
                target = new SerialPort(args[2], targetBaudRate);
                targetName = args[2];
                Console.WriteLine("to " + args[1] + "(" + targetBaudRate + ")");
            }
            else
            {
                targetName = "EMIT GPRS SERVER";
                cli = new TcpClient("home.lofas.se",80);
                Console.WriteLine("to EMIT GPRS SERVER");
            }
            source.DataReceived += Source_DataReceived;

            if (args.Length > 2)
            {
                target.DataReceived += Target_DataReceived;
            }
            Console.WriteLine("Opening " + args[0] + " in " + sourceBaudRate + "bps");
            source.Open();

            if (args.Length > 2)
            {
                Console.WriteLine("Opening " + args[2] + " in " + targetBaudRate + "bps");
                target.Open();
            }
            Console.WriteLine("Press any key to exit");
            Console.Read();

        }

        private static void Source_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buf = new byte[source.BytesToRead];
            source.Read(buf, 0, buf.Length);
            if (target != null)
            {
                target.Write(buf, 0, buf.Length);
            }
            else if (cli != null)
            {
                var str = cli.GetStream();
                str.Write(buf, 0, buf.Length);
            }
            Console.WriteLine("Sent " + buf.Length + " bytes from " + sourceName + " => " + targetName );
        }
        private static void Target_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            byte[] buf = new byte[target.BytesToRead];
            target.Read(buf, 0, buf.Length);
            source.Write(buf, 0, buf.Length);
            Console.WriteLine("Sent " + buf.Length + " bytes from " + targetName + " => " + sourceName);
        }
}
}
