using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;

namespace ipscan
{
    class Scanner
    {
        public delegate void OnRespnseDelegate(string ip, bool success);
        public event OnRespnseDelegate OnRespnse;
        public delegate void OnFinishedDelegate();
        public event OnFinishedDelegate OnFinished;
        public long from, to, current;
        public List<Worker> workers = new List<Worker>();
        private Object ThreadLock = new object();
        private Range CurrentRange { get; set; }
        private List<Range> ranges;
        public List<Range> Ranges
        {
            get
            {
                return ranges;
            }
            
            set
            {
                ranges = value;
                CurrentRange = ranges[0];
                from = IP2Long(CurrentRange.from);
                to = IP2Long(CurrentRange.to);
                current = from;
            }
        }
  
        public int Threads { get; set; }
        public List<int> Ports { get; set; }
        public int TimeOut { get; set; }
        private Timer timer = new Timer();
        public Scanner()
        {
            timer.Interval = 500;
            timer.Elapsed += Timer_Elapsed;
            timer.Start();
        }

        private bool started;
       
        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (workers.Count == Threads)
            {
                //start workers
                if (!started) StartWorkers();
                //dispose workers
                DisposeWorkers();           
            }
        }

        private void DisposeWorkers()
        {
            int count = 0;
            foreach (Worker worker in workers)
                if (worker.ShutDown) count++;
            if (count == Threads)
            {
                timer.Stop();
                OnFinished?.Invoke();
            }
        }

        private void StartWorkers()
        {
            int count = 0;
            started = true;
            foreach (Worker worker in workers)
                if (!worker.Started) count++;
            if (count == Threads)
            {
                foreach (var worker in workers)
                {
                    Debug.Print("new worker: " + worker.Id);
                    worker.Start(GetIp(), Ports, TimeOut);
                }
            }
        }

        public void Start()
        { 
            //create workers       
            for (int i = 0; i < Threads; i++)
            {
                Worker worker = new Worker();
                workers.Add(worker);
                worker.OnRespnse += (sender, success) =>
                {
                    Debug.Print("response " + sender.ip);
                    OnRespnse?.Invoke(sender.ip, success);
                    string ip = GetIp();   
                    worker.Start(ip, Ports, TimeOut);
                };
            }
  
        }

        public string GetIp()
        {
            if (current <= to)
            {
                current++;
                return LongToIP(current);  
            }
            else
            {
                if (Ranges.Count != 0) Ranges.RemoveAt(0);
                if (Ranges.Count != 0)
                {
                    CurrentRange = Ranges[0];
                    from = IP2Long(CurrentRange.from);
                    to = IP2Long(CurrentRange.to);
                    current = from;
                    return LongToIP(current);
                }
            
            }
            return null;
        }
        public long IP2Long(string ip)
        {
            string[] ipBytes;
            double num = 0;
            if (!string.IsNullOrEmpty(ip))
            {
                ipBytes = ip.Split('.');
                for (int i = ipBytes.Length - 1; i >= 0; i--)
                {
                    num += ((int.Parse(ipBytes[i]) % 256) * Math.Pow(256, (3 - i)));
                }
            }
            return (long)num;
        }

        static public string LongToIP(long longIP)
        {
            string ip = string.Empty;
            for (int i = 0; i < 4; i++)
            {
                int num = (int)(longIP / Math.Pow(256, (3 - i)));
                longIP = longIP - (long)(num * Math.Pow(256, (3 - i)));
                if (i == 0)
                    ip = num.ToString();
                else
                    ip = ip + "." + num.ToString();
            }
            return ip;
        }
    }
}
