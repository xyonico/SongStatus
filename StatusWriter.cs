using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SongStatus
{
    class StatusWriter
    {
        public Template _template;

        private string[] _keywords;
        private string _statusText;

        public StatusWriter(Template template)
        {
            _template = template;
            _keywords = _template.Text.Split('{', '}');
            _statusText = _template.Text;
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
    }
}
