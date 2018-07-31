using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace SongStatus
{
    public class Broadcast : WebSocketBehavior
    {
        protected override void OnOpen()
        {
            base.OnOpen();
            Logger.Log("WebSocket Client Connected!");
        }
    }

    class StatusWriter
    {
        public Template _template;

        private string[] _keywords;
        private string _statusText;

        private readonly bool _ws;
        private WebSocketServer _wss;

        public StatusWriter(Template template)
        {
            _template = template;
            _keywords = _template.Text.Split('{', '}');
            _statusText = _template.Text;

            if (Path.GetExtension(_template.StatusPath) == ".html")
            {
                int wsPort = GetAvailablePort(3333);
                if (wsPort == 0)
                {
                    // Couldn't find free port
                    Logger.Log("Could not find free port");
                    return;
                }

                Logger.Log("Starting WebSocket Server on port " + wsPort);

                _ws = true;
                _wss = new WebSocketServer(wsPort);
                _wss.AddWebSocketService<Broadcast>("/");
                _wss.Start();
            }
        }

        /// <summary>
        /// Write text to the output file
        /// </summary>
        /// <param name="text">Text to write</param>
        public void Write(string text)
        {
            File.WriteAllText(_template.StatusPath, text);
            _statusText = _template.Text;
        }

        /// <summary>
        /// Write status text to the output file
        /// </summary>
        public void Write()
        {
            Write(_statusText);
        }

        /// <summary>
        /// Remove all text in the output file
        /// </summary>
        public void WriteEmpty()
        {
            Write(String.Empty);
        }

        /// <summary>
        /// Replace a keyword from the template
        /// </summary>
        /// <param name="keyword">Keyword to replace</param>
        /// <param name="replacement">Text to replace with</param>
        public void ReplaceKeyword(string keyword, string replacement)
        {
            _statusText = _template.Text.ReplaceKeyword(keyword, replacement, _keywords);
        }

        /// <summary>
        /// Checks for used ports and retrieves the first free port
        /// </summary>
        /// <returns>the free port or 0 if it did not find a free port</returns>
        public static int GetAvailablePort(int startingPort)
        {
            IPEndPoint[] endPoints;
            List<int> portArray = new List<int>();

            IPGlobalProperties properties = IPGlobalProperties.GetIPGlobalProperties();

            //getting active connections
            TcpConnectionInformation[] connections = properties.GetActiveTcpConnections();
            portArray.AddRange(from n in connections
                               where n.LocalEndPoint.Port >= startingPort
                               select n.LocalEndPoint.Port);

            //getting active tcp listners - WCF service listening in tcp
            endPoints = properties.GetActiveTcpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            //getting active udp listeners
            endPoints = properties.GetActiveUdpListeners();
            portArray.AddRange(from n in endPoints
                               where n.Port >= startingPort
                               select n.Port);

            portArray.Sort();

            for (int i = startingPort; i < UInt16.MaxValue; i++)
                if (!portArray.Contains(i))
                    return i;

            return 0;
        }
    }
}
