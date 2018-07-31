using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace SongStatus
{
    class StatusWriter
    {
        public Template _template;

        private string[] _keywords;
        private string _statusText;

        private readonly bool _wsEnabled;
        private WebSocketServer _wss;

        public StatusWriter(Template template)
        {
            _template = template;
            _keywords = _template.Text.Split('{', '}');
            _wsEnabled = false;

            Reset();
        }

        public StatusWriter(Template template, WebSocketServer wss)
        {
            _template = template;
            _keywords = _template.Text.Split('{', '}');

            _wsEnabled = Path.GetExtension(_template.StatusPath) == ".html";
            _wss = wss;

            if (_wsEnabled)
                File.WriteAllText(_template.StatusPath, HTMLPage(_wss.Port));

            Reset();
        }

        public static string HTMLPage(int port)
        {
            return "<html><head><title>SongStatus</title><style>body { font-family: sans-serif; }</style></head>" +
                "<body><p id='text'></p><script>" +
                "let p = document.getElementById('text'); let ws = new WebSocket('ws://localhost:" + port + "'); " +
                "ws.onmessage = ev => { p.innerHTML = ev.data.replace(/\\n/g, '<br />') }; ws.onerror = ev => { console.error(ev) }" +
                "</script></body></html>";
        }

        /// <summary>
        /// Reset status text to template
        /// </summary>
        void Reset()
        {
            _statusText = _template.Text;
        }

        /// <summary>
        /// Write text to the output file
        /// </summary>
        /// <param name="text">Text to write</param>
        public void Write(string text)
        {
            if (_wsEnabled)
                _wss.WebSocketServices["/"].Sessions.Broadcast(text);
            else
                File.WriteAllText(_template.StatusPath, text);
        }

        /// <summary>
        /// Write status text to the output file
        /// </summary>
        public void Write()
        {
            Write(_statusText);
            Reset();
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
            _statusText = _statusText.ReplaceKeyword(keyword, replacement, _keywords);
        }
    }
}
