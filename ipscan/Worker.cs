using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ipscan
{
    class Worker
    {

        public delegate void OnRespnseDelegate(Worker sender, bool success);
        public event OnRespnseDelegate OnRespnse;

        public Worker()
        {
            Id = IdProvider.GetNewId();
            thread = new Thread(new ThreadStart(ExecuteAsync));
            thread.Start();
        }

        private List<int> ports;
        private Thread thread;
        public string ip;
        private int timeOut;
        public bool Idle { get; set; }
        public bool ShutDown { get; set; }
        public bool Started { get; set; }
        public int Id { get; private set; }
        public void Start(String ip, List<int> ports, int timeOut)
        {
            this.ip = ip;
            this.ports = ports;
            this.timeOut = timeOut;
            Idle = true;
            Started = true;
        }
 
        private void ExecuteAsync()
        {
            while (!ShutDown)
            {
                Thread.Sleep(100);
                if (Idle)
                {
                    Idle = false;
                    if (ip == null)
                    {
                        ShutDown = true;
                        return;
                    }       
                    foreach (var port in ports)
                        using (TcpClient client = new TcpClient())
                            if (client.ConnectAsync(ip, port).Wait(timeOut))
                            {
                                OnRespnse?.Invoke(this, true);
                                return;
                            }
                                
                    OnRespnse?.Invoke(this, false);
                }
            }
        }
    }
}
