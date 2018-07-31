using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SongStatus
{
    public struct Template
    {
        public readonly string Text;
        public readonly string StatusPath;

        public Template(string text, string statusPath)
        {
            Text = text;
            StatusPath = statusPath;
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

        /// <summary>
        /// Create the template file if it doesn't exist
        /// </summary>
        public static void EnsureTemplateExists()
        {
            if (!File.Exists(_templatePath))
                File.WriteAllText(_templatePath, _defaultTemplate);
        }

        /// <summary>
        /// Read the template file and parse it into text and path
        /// </summary>
        /// <returns>Template object</returns>
        public static Template ReadTemplate()
        {
            string templateText = File.ReadAllText(_templatePath);
            Template template = ParseTemplate(templateText);

            return template;
        }

        /// <summary>
        /// Parse the a template string into text and path
        /// </summary>
        /// <param name="templateText"></param>
        /// <returns>Template object</returns>
        public static Template ParseTemplate(string templateText)
        {
            Template defaultTemplate = new Template(templateText, _defaultStatusPath);

            var lines = Regex.Split(templateText, "\r\n|\r|\n");
            if (lines.Length == 0)
                return defaultTemplate;

            string[] sep = lines[0].Split('=');
            if (sep.Length <= 1)
                return defaultTemplate;

            if (!string.Equals(sep[0], "path", StringComparison.OrdinalIgnoreCase))
                return defaultTemplate;

            return new Template(
                string.Join(Environment.NewLine, lines.Skip(1)),
                sep[1]
            );
        }
    }
}
