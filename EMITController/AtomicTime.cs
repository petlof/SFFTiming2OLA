using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace EMITController
{
    public static class AtomicTime
    {
        private static DateTime _currentAtomicTime;
        private static bool _canConnectToServer = true;
        private static Stopwatch _timeSinceLastValue = new Stopwatch();
        private static readonly object Locker = new object();
        private static Countdown _countdown; //used to help ensure we get the fastest server

        private static void GetDateTimeFromServer(string server)
        {
            if (_currentAtomicTime ==  DateTime.MinValue)
            {
                try
                {
                    // Connect to the server (at port 13) and get the response
                    string serverResponse;
                    using (var reader = new StreamReader(new System.Net.Sockets.TcpClient(server, 13).GetStream()))
                        serverResponse = reader.ReadToEnd();

                    // If a response was received
                    if (!string.IsNullOrEmpty(serverResponse) || _currentAtomicTime != DateTime.MinValue)
                    {
                        // Split the response string ("55596 11-02-14 13:54:11 00 0 0 478.1 UTC(NIST) *")
                        //format is RFC-867, see example here: http://www.kloth.net/software/timesrv1.php
                        //some other examples of how to parse can be found in this: http://cosinekitty.com/nist/
                        string[] tokens = serverResponse.Replace("n", "").Split(' ');

                        // Check the number of tokens
                        if (tokens.Length >= 6)
                        {
                            // Check the health status
                            string health = tokens[5];
                            if (health == "0")
                            {
                                // Get date and time parts from the server response
                                string[] dateParts = tokens[1].Split('-');
                                string[] timeParts = tokens[2].Split(':');

                                // Create a DateTime instance
                                var utcDateTime = new DateTime(
                                    Convert.ToInt32(dateParts[0]) + 2000,
                                    Convert.ToInt32(dateParts[1]), Convert.ToInt32(dateParts[2]),
                                    Convert.ToInt32(timeParts[0]), Convert.ToInt32(timeParts[1]),
                                    Convert.ToInt32(timeParts[2]));

                                //subject milliseconds from it
                                if (Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator ==  "," && tokens[6].Contains("."))
                                tokens[6] = tokens[6].Replace(".", ",");
                                else if (Thread.CurrentThread.CurrentCulture.NumberFormat.NumberDecimalSeparator == "." && tokens[6].Contains(","))
                                tokens[6] = tokens[6].Replace(",", ".");

                                double millis;
                                double.TryParse(tokens[6], out millis);
                                utcDateTime = utcDateTime.AddMilliseconds(-millis);

                                // Convert received (UTC) DateTime value to the local timezone
                                if (_currentAtomicTime == DateTime.MinValue)
                                {
                                    _currentAtomicTime = utcDateTime.ToLocalTime();
                                    _timeSinceLastValue = new Stopwatch();
                                    _timeSinceLastValue.Start();
                                    _countdown.PulseAll(); //we got a valid time, move on and no need to worry about results from other threads
                                }
                            }
                        }
                    }
                }
                catch (Exception ee)
                {
                    Debug.WriteLine("Error with server: " + server);
                    // Ignore exception and try the next server
                }
            }

            //let CountdownEvent know that we're done here
            _countdown.Signal();
        }

        public static DateTime Now
        {
            get
            {
                //found part of this code here: http://www.datavoila.com/projects/internet/get-nist-atomic-clock-time.html

                //we have attempted to connect to the server and had no luck, no need to try again
                //if (_canConnectToServer == false)
                //return DateTime.MinValue;

                //keep track so we don't have to keep connecting to the servers
                if (_currentAtomicTime != DateTime.MinValue)
                {
                    _currentAtomicTime += _timeSinceLastValue.Elapsed;
                    _timeSinceLastValue.Reset();
                    _timeSinceLastValue.Start();
                }
                else
                {
                    //ensure we aren't doing this multiple times from multiple locations
                    lock (Locker)
                    {
                        if (_currentAtomicTime != DateTime.MinValue) //we got the time already, pass it along
                        {
                            _currentAtomicTime += _timeSinceLastValue.Elapsed;
                            _timeSinceLastValue.Reset();
                            _timeSinceLastValue.Start();
                        }
                        else
                        {
                            // Initialize the list of NIST time servers
                            // http://tf.nist.gov/tf-cgi/servers.cgi
                            var servers = new[]
                            {
                                "0.se.pool.ntp.org",
                                "1.se.pool.ntp.org",
                                "2.se.pool.ntp.org",
                                "3.se.pool.ntp.org",
                                "time-a.nist.gov",
                                "time-b.nist.gov",
                                "time-c.nist.gov",
                                "time-d.nist.gov",
                                //"time.google.com",
                                //"time2.google.com"
                            };

                            // Try 5 servers in random order to spread the load
                            var rnd = new Random();
                            _countdown = new Countdown(5);
                            foreach (string server in servers.OrderBy(s => rnd.NextDouble()).Take(5))
                            {
                                string server1 = server;
                                var t = new Thread(o => GetDateTimeFromServer(server1));
                                t.SetApartmentState(ApartmentState.STA);
                                t.Start();
                            }
                            _countdown.Wait();
                            if (_currentAtomicTime == DateTime.MinValue)
                                _canConnectToServer = false;
                        }
                    }
                }

                return _currentAtomicTime;
            }
        }
    }

    public class Countdown
    {
        readonly object _locker = new object();
        int _value;

        public Countdown() { }
        public Countdown(int initialCount) { _value = initialCount; }
        public void Signal() { AddCount(-1); }
        public void PulseAll()
        {
            lock (_locker)
            {
                _value = 0;
                Monitor.PulseAll(_locker);
            }
        }

        public void AddCount(int amount)
        {
            lock (_locker)
            {
                _value += amount;
                if (_value <= 0) Monitor.PulseAll(_locker);
            }
        }
        public void Wait()
        {
            lock (_locker)
                while (_value > 0)
                    Monitor.Wait(_locker);
        }


    }


}
