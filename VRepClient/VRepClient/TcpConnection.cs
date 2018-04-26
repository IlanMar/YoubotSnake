//WinRos project by Sekou Diane (sekoudiane1990@gmail.com) and Alexey Novoselsky
//Moscow, MIREA, 2014-2015
//The aim of the project is to control a group of Ubuntu-driven robots (youBots) from Windows laptop

using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace VRepClient

{
    public class TcpConnection
    {
        public TcpClient tc;

        private StreamReader reader;
        private StreamWriter writer;

        public Thread tc_thread;

        private const string confirmation = "#ok#";
        private const string time_check = "#time_check#";

        public delegate void EventDel(string info);

        private EventDel onConnected, onDataReceived, onDisconnect;

        private System.Windows.Forms.Timer timer_keep_alive;

        public bool IsConnected()
        {
            if (tc == null || tc_thread == null) return false;

            bool res = (DateTime.Now - t_last_msg_from_server).Seconds < 60;

            return res;
        }

        private Form1 f1;
        int ID;

        public TcpConnection(int ID, Form1 f1, EventDel onConnected, EventDel onDataReceived, EventDel onDisconnect)
        {
            this.ID = ID;
            this.f1 = f1;
            this.onConnected = onConnected;
            this.onDataReceived = onDataReceived;
            this.onDisconnect = onDisconnect;
        }

        public void Dispose()
        {

            if (tc_thread != null)
            {
                tc_thread.Abort();
                tc_thread = null;
            }
            if (reader != null) { reader.Dispose(); reader = null; }
            if (writer != null) { writer.Dispose(); writer = null; }
        }

        public bool IsDisposed { get { return tc_thread==null; } }
        public void Connect(string ip, string port)
        {
            tc = new TcpClient();

            var serverEndPoint = new IPEndPoint(IPAddress.Parse(ip), int.Parse(port));

            if (tc.Client.RemoteEndPoint == null || tc.Client.RemoteEndPoint.ToString() != serverEndPoint.ToString())
            {
                try
                {
#warning ограничить длительность попытки подключения
                    tc.Connect(serverEndPoint);
                }
                catch (Exception)
                {
                    MessageBox.Show("Bad endpoint: ID = "+(ID+1));
                    tc = null;
                    return;
                }

                reader = new StreamReader(tc.GetStream());
                writer = new StreamWriter(tc.GetStream());

                tc_thread = new Thread(WaitTcpFromServer);
                tc_thread.Start();
            }

            t_last_msg_from_server = DateTime.Now;

            if (!IsConnected()) throw new Exception("Couldn't connect");
            if(onConnected!=null) onConnected("Connected!");
            Send("#start#");

            timer_keep_alive = new System.Windows.Forms.Timer();
            timer_keep_alive.Interval = 1000;
            timer_keep_alive.Enabled = true;
            timer_keep_alive.Tick += (s, e) =>
            {
                Send(confirmation);
                if (tcs == 0)
                {
                    hpt.Start();
                    Send(time_check);
                }
                tcs = (tcs + 1) % time_check_skipper;
            };
        }

        int time_check_skipper = 5, tcs=0;

        public void Disconnect(string reason, bool show_mb)
        {
            timer_keep_alive.Enabled = false;

            if (tc_thread != null && tc_thread.IsAlive)
            {
                try
                {
                    tc_thread.Abort();
                }
                catch
                {
                }
            }
            if (tc != null && tc.Connected)
            {
                tc.Close();
                reader.Close(); reader = null;
                writer.Close(); writer = null;
            }

            tc = null;
            tc_thread = null;

            if (onDisconnect != null) onDisconnect(reason);

            if (show_mb) MessageBox.Show("Disconnect: " + reason);
        }

        private object read_write_sem = 777;

        private DateTime t_last_msg_from_server;

        private void WaitTcpFromServer()
        {
            //read

            while (true)
            {
                if (!IsConnected())
                {
                    Disconnect("Connection timeout or smth else", true);
                    break;
                }

                string data = null;
                lock (read_write_sem)
                {
                    if (tc.Available > 0)
                    {
                        try
                        {
                            data = reader.ReadLine();
                        }
                        catch(Exception ex)
                        {
                            MessageBox.Show(ex.Message);
                        }
                        
                    }
                }
                if (data != null)
                {
                    // Строка, содержащая ответ от сервера
                    if (data == confirmation)
                    {
                        t_last_msg_from_server = DateTime.Now;
                    } 
                    else if (data == time_check)
                    {
                        hpt.Stop();
                        delay = 0.5f*delay+0.5f*(float)hpt.Duration;
                    }
                    else
                    {
                        if (onDataReceived != null) onDataReceived(data.Replace("^^^", "\r\n"));
                    }
                }
                else
                {
                    Thread.Sleep(30);
                }
            }
        }

        public float delay;
        HiPerfTimer hpt = new HiPerfTimer();

        private bool msg_box_shown = false;

        public void Send(string s)
        {
            if (tc == null)
            {
                msg_box_shown = true;
                if(!msg_box_shown) MessageBox.Show("Not connected");
                return;
            }

            lock (read_write_sem)
            {
                try
                {
                    s = s.Replace("\r\n", "\n");
                    writer.Write(s + "^^^");
                    writer.Flush();
                }
                catch
                {
                    Disconnect("Can't send (server stopped?): ID = "+(ID+1), true);
                }
            }
        }

    }
    #region HiPerfTimer
    public class HiPerfTimer
    {
        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceCounter(out long lpPerformanceCount);

        [DllImport("Kernel32.dll")]
        private static extern bool QueryPerformanceFrequency(out long lpFrequency);

        private long startTime, stopTime;
        private long freq;

        // Constructor
        public HiPerfTimer()
        {
            startTime = 0; stopTime = 0;
            if (QueryPerformanceFrequency(out freq) == false)
            {
                // high-performance counter not supported
                throw new System.ComponentModel.Win32Exception();
            }
        }

        // Start the timer
        public void Start()
        {
            // lets do the waiting threads their work
            Thread.Sleep(0);
            QueryPerformanceCounter(out startTime);
        }

        // Stop the timer
        public void Stop()
        {
            QueryPerformanceCounter(out stopTime);
        }

        // Returns the duration of the timer (in seconds)
        public double Duration
        { get { return (double)(stopTime - startTime) / (double)freq; } }
    }
    #endregion


    /*
        *       
           #region TCP
           public TcpConnection tcpcon;

           int conn_ttl = 0;

           void decrement_tcp_ttl()
           {
               if (tcpcon != null && tcpcon.IsConnected()) conn_ttl = 2;
               else conn_ttl--;

               if (conn_ttl < 0) tcpcon = null;
           }

           void close_tcp()
           {
               if (tcpcon != null)
               {
                   //пусть сервер разрывает соединене, т.к. если это сделает клиент, то на сервере могут от внезапности посыпаться ошибки

                   if (tcpcon.IsConnected()) tcpcon.Send("#end#");//tcpcon.Disconnect("User", false);

                   // обнуление уже сделано в callback-функции  //if(!tcpcon.IsDisposed) tcpcon.Dispose(); tcpcon = null; 
                   tcpcon.Disconnect("User", false);
               }
           }

           private void finish_disconnect()
           {
               Thread.Sleep(1000);
               if (tcpcon != null && !tcpcon.IsDisposed)
               {
                   tcpcon.Dispose();
                   tcpcon = null;
               }
               bt_conn.Invoke(new Action(() => bt_conn.Text = "Connect"));
           }

           private void open_tcp()
           {
               tcpcon = new TcpConnection(this,
                   s => bt_conn.Invoke(new Action(() =>
                   {
                       bt_conn.Text = "Disconn";
                       tcpcon.Send("#start#");
                   })),
                   s => bt_conn.Invoke(new Action(() =>
                   {
                       //rtb_tcp_recv.Text = s;
                       //str_callback(s);

                   })),
                   s => bt_conn.Invoke(new Action(() =>
                   {
                       var thread = new Thread(finish_disconnect);
                       thread.Start();
                   }))
                       );
               tcpcon.Connect(robot_endpoint, "7777");
           }
           //Connect
           private void bt_conn_Click(object sender, EventArgs e)
           {
               if (!IsConnected)
               {
                   open_tcp();
               }
               else
               {
                   close_tcp();
               }
           }

           //Send
           private void button5_Click(object sender, EventArgs e)
           {
               if (tcpcon == null)
               {
                   MessageBox.Show("No TCP connection!");
                   return;
               }
               tcpcon.Send(string.Format("LUA_UpdateNavigation({0}, {1}, {2})", interpoladed_pi.x_real, interpoladed_pi.y_real));

           }
           private bool IsConnected
           {
               get { return tcpcon != null && tcpcon.IsConnected(); }
           }

           #endregion
        * */
}