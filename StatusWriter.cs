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
        public TemplateResponse _template;

        private string[] _keywords;

        public StatusWriter(TemplateResponse template)
        {
            _template = template;
            _keywords = _template.Text.Split('{', '}');
        }

        public void Write(string text)
        {
            File.WriteAllText(_template.StatusPath, text);
        }

        public void Write()
        {
            Write(_template.Text);
        }

        public void WriteEmpty()
        {
            Write(String.Empty);
        }

        public void ReplaceKeyword(string keyword, string replacement)
        {
            _template.Text = _template.Text.ReplaceKeyword(keyword, replacement, _keywords);
        }
    }
}
