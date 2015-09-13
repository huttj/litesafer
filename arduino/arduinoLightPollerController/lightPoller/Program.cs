using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Threading;

using System.Web;
using System.Net;

using System.Text;

using System.IO.Ports;

namespace lightPoller
{
    class Program
    {
        public class WebServer
        {
            private readonly HttpListener _listener = new HttpListener();
            private readonly Func<HttpListenerRequest, string> _responderMethod;

            public WebServer(string[] prefixes, Func<HttpListenerRequest, string> method)
            {
                if (!HttpListener.IsSupported)
                    throw new NotSupportedException(
                        "Needs Windows XP SP2, Server 2003 or later.");

                // URI prefixes are required, for example 
                // "http://localhost:8080/index/".
                if (prefixes == null || prefixes.Length == 0)
                    throw new ArgumentException("prefixes");

                // A responder method is required
                if (method == null)
                    throw new ArgumentException("method");

                foreach (string s in prefixes)
                    _listener.Prefixes.Add(s);

                _responderMethod = method;
                _listener.Start();
            }

            public WebServer(Func<HttpListenerRequest, string> method, params string[] prefixes)
                : this(prefixes, method) { }

            public void Run()
            {
                ThreadPool.QueueUserWorkItem((o) =>
                {
                    Console.WriteLine("Webserver running...");
                    try
                    {
                        while (_listener.IsListening)
                        {
                            ThreadPool.QueueUserWorkItem((c) =>
                            {
                                var ctx = c as HttpListenerContext;
                                try
                                {
                                    string rstr = _responderMethod(ctx.Request);
                                    byte[] buf = Encoding.UTF8.GetBytes(rstr);
                                    ctx.Response.ContentLength64 = buf.Length;
                                    ctx.Response.OutputStream.Write(buf, 0, buf.Length);
                                }
                                catch { } // suppress any exceptions
                                finally
                                {
                                    // always close the stream
                                    ctx.Response.OutputStream.Close();
                                }
                            }, _listener.GetContext());
                        }
                    }
                    catch { } // suppress any exceptions
                });
            }

            public void Stop()
            {
                _listener.Stop();
                _listener.Close();
            }
        }

        public static string SendResponse(HttpListenerRequest request)
        {
            return string.Format("<HTML><BODY>My web page.<br>{0}</BODY></HTML>", DateTime.Now);
        }

        static int pollServer()
        {
            WebClient wc = new WebClient();
            string result = wc.DownloadString("http://www.hirekris.com:3333/light");
            if (result == "{\"light\":\"-1\"}")
                return -1;
            return (result.Substring(10, 1)[0] - '0');
        }

        static void Main(string[] args)
        {
            //WebServer ws = new WebServer(SendResponse, "http://localhost:8080/test/");
            //ws.Run();
            //ws.Stop();
            
            var p = new SerialPort("COM3", 9600, Parity.None);
            p.Open();

            while (true)
            {
                int status = pollServer();
                p.Write((status == -1) ? "-" : status.ToString());
                
                Thread.Sleep(1000);
                Console.WriteLine(status);
            }

            p.Close();
        }
    }
}
