using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace ipscan
{


    class Program
    {
        static int scanned = 0, LineTop, LineLeft;
        static int threads = 200;
        static int timeOut = 1000;
        static int successed = 0, failed = 0;
        static int totalRangesCount;
        static  string file = @"c://ip.txt";
        static List<int> ports = new List<int>();
        private static List<string> goods = new List<string>();
        static void Main(string[] args)
        {
            Console.BackgroundColor = ConsoleColor.Black;
            if (args.Length==-1)
            {
                LineTop = 2;
                LineLeft = 5;
                Console.ForegroundColor = ConsoleColor.Red;
                WriteLine("No params!");
                Console.ReadLine();
                Environment.Exit(0);
            }
            ports.Add(3389);
           // 3389,3390,3391
            Console.CursorVisible = false;
            Console.Clear();
            for (int i = 0;i<args.Length;i++)
            {
                switch(args[i])
                {
                    case "-t":
                        threads = Convert.ToInt32(args[i + 1].Trim());
                        break;
                    case "-o":
                        timeOut = Convert.ToInt32(args[i + 1].Trim());
                        break;
                    case "-f":
                        file = args[i + 1].Trim();
                        break;
                    case "-p":
                        foreach (string port in args[i + 1].Trim().Split(','))
                        {
                            ports.Add(Convert.ToInt32(port));
                        }
                        break;

                }
            }
     
            List<Range> ranges= LoadRangesFromFile(file);
            totalRangesCount = ranges.Count;
            Scanner scanner = new Scanner();
            System.Timers.Timer timer = new System.Timers.Timer();
            timer.Interval = 1000;

            string strPorts = "|";
            foreach (var port in ports)
                strPorts += port+"|";

      

            timer.Elapsed += (source, e) =>
            {
                SaveGoodHosts();

                LineTop = 2;
                LineLeft = 5;
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                WriteLine(string.Format("================== Total ==================", (scanner.to - scanner.from)));
                WriteLine(string.Format("Threads:        {0}", scanner.workers.Count));
                WriteLine(string.Format("Ports:          {0}", strPorts));
                Console.ForegroundColor = ConsoleColor.DarkYellow;
                WriteLine(string.Format("TimeOut:        {0}", timeOut));
                Console.ForegroundColor = ConsoleColor.DarkMagenta;
                WriteLine(string.Format("Ranges:         {0}", totalRangesCount));
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                WriteLine(string.Format("Scanned Ranges: {0}", totalRangesCount - ranges.Count));
                WriteLine(string.Format("Scanned Ips:    {0}", scanned));
                Console.ForegroundColor = ConsoleColor.DarkGreen;    
                WriteLine(string.Format("Good Hosts:     {0}", successed));
                Console.ForegroundColor = ConsoleColor.DarkRed;
                WriteLine(string.Format("Bad Hosts:      {0}", failed));
                Console.ForegroundColor = ConsoleColor.DarkCyan;
                WriteLine(string.Format(""));
                WriteLine(string.Format("============== Current Range =============="));
                if (ranges.Count != 0)
                { 
                  WriteLine(string.Format("From:    {0}", ranges[0].from));
                  WriteLine(string.Format("To:      {0}", ranges[0].to));
                }
                
                WriteLine(string.Format("Ips:     {0}", scanner.to - scanner.from));
                WriteLine(string.Format("scanned: {0}", scanner.current - scanner.from));
                Console.ForegroundColor = ConsoleColor.DarkGreen;
                if (finished)
                {
                    WriteLine("");
                    WriteLine("Finished [!]");
                }
                
                WriteLine("                                          ");
                WriteLine("                                          ");
            };
            timer.Start();

            scanner.OnRespnse += (ip, success) =>
            {
      
                if (success)
                {
                    goods.Add(ip);
                    successed++;
                }
                else failed++;
                scanned++;
            };
            scanner.OnFinished += () =>
            {
                Debug.Print("finished");
                if (!finished) finished = true;  
            };
            scanner.TimeOut = timeOut;
            scanner.Ranges = ranges;
            scanner.Threads = threads;
            scanner.Ports = ports;
            scanner.Start();

            Console.ReadLine();
        }

        private static void SaveGoodHosts()
        {
            string lines = null;
            while (goods.Count != 0)
            {

                lines += goods[0] + Environment.NewLine;
                goods.RemoveAt(0);
            }
            if (lines != null)
            using (StreamWriter w = File.AppendText("good.txt"))
            {
                w.Write(lines);
                w.Flush();
            }
        }

        static bool finished = false;
        public static void WriteLine(string[] lines)
        {
            foreach (var line in lines)
            {
                WriteLine(line);
            }
        }
        public static void WriteLine(string lines)
        {
            Console.SetCursorPosition(LineLeft, LineTop);
            Console.Write(lines);
            LineTop++;
        }

        public static void ClearCurrentConsoleLine()
        {
    
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(0, Console.CursorTop);
        }

        private static List<Range> LoadRangesFromFile(string path)
        {
        
            List<Range> ranges = new List<Range>();
            string text = File.ReadAllText(path, Encoding.UTF8);
            string[] strRanges = text.Split('\n');
            foreach (string range in strRanges)
            {
                string from = range.Substring(0, range.IndexOf("-"));
                string to = range.Substring(range.IndexOf("-") + 1);
                ranges.Add(new Range(from, to));
            }
            return ranges;
        }

    }
}
