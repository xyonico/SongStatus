using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SongStatus
{
    public struct TemplateResponse
    {
        public string Text;
        public string StatusPath;
        public bool IsDefault;

        public TemplateResponse(string text, string statusPath)
        {
            Text = text;
            StatusPath = statusPath;
            IsDefault = true;
        }

        public TemplateResponse(string text, string statusPath, bool isDefault)
        {
            Text = text;
            StatusPath = statusPath;
            IsDefault = isDefault;
        }
    }

    class TemplateReader
    {
        public static string _templatePath = Path.Combine(Environment.CurrentDirectory, "statusTemplate.txt");
        public static string _defaultStatusPath = Path.Combine(Environment.CurrentDirectory, "status.txt");

        public static string _defaultTemplate = string.Join(
            Environment.NewLine,
            "path=status.txt",
            "Playing: {songName}{ songSubName} - {authorName}",
            "{gamemode} | {difficulty} | BPM: {beatsPerMinute}{",
            "This line will only appear when there is a sub name: songSubName}",
            "{[isNoFail] }{[isMirrored] }");

        public static void EnsureTemplateExists()
        {
            if (!File.Exists(_templatePath))
            {
                File.WriteAllText(_templatePath, _defaultTemplate);
            }
        }

        public static TemplateResponse ReadTemplate()
        {
            string templateText = File.ReadAllText(_templatePath);
            TemplateResponse resp = ParseStatusPath(templateText);

            return resp;
        }

        public static TemplateResponse ParseStatusPath(string templateText)
        {
            TemplateResponse defaultResponse = new TemplateResponse(templateText, _defaultStatusPath);

            var lines = Regex.Split(templateText, "\r\n|\r|\n");
            if (lines.Length == 0)
                return defaultResponse;

            string[] sep = lines[0].Split('=');
            if (sep.Length <= 1)
                return defaultResponse;

            if (!string.Equals(sep[0], "path", StringComparison.OrdinalIgnoreCase))
                return defaultResponse;

            return new TemplateResponse(
                string.Join(Environment.NewLine, lines.Skip(1)),
                sep[1],
                false
            );
        }
    }
}
