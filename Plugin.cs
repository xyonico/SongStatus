using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace SongStatus
{
	public class Plugin : IPlugin
	{
		private const string MenuSceneName = "Menu";
		private const string GameSceneName = "StandardLevel";
		
		private readonly string _defaultStatusPath = Path.Combine(Environment.CurrentDirectory, "status.txt");
		private readonly string _defaultTemplate = string.Join(
			Environment.NewLine,
			"path=status.txt",
			"Playing: {songName}{ songSubName} - {authorName}",
			"{gamemode} | {difficulty} | BPM: {beatsPerMinute}{",
			"This line will only appear when there is a sub name: songSubName}",
			"{[isNoFail] }{[isMirrored] }");

		private MainGameSceneSetupData _mainSetupData;
		private bool _init;
		private readonly string _templatePath = Path.Combine(Environment.CurrentDirectory, "statusTemplate.txt");
		private string _statusPath;

		public string Name
		{
			get { return "Song Status"; }
		}

		public string Version
		{
			get { return "v1.3"; }
		}

		public void OnApplicationStart()
		{
			if (_init) return;
			_init = true;
			SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;
			
			_statusPath = _defaultStatusPath;
			
			if (!File.Exists(_templatePath))
			{
				File.WriteAllText(_templatePath, _defaultTemplate);
			}
		}

		public void OnApplicationQuit()
		{
			SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
			File.WriteAllText(_statusPath, string.Empty);
		}

		private void SceneManagerOnActiveSceneChanged(Scene oldScene, Scene newScene)
		{
			string templateText;
			if (!File.Exists(_templatePath))
			{
				templateText = _defaultTemplate;
				File.WriteAllText(_templatePath, templateText);
			}
			else
			{
				templateText = File.ReadAllText(_templatePath);
			}

			templateText = ReadStatusPath(templateText);
			
			if (newScene.name == MenuSceneName)
			{
				//Menu scene loaded
				File.WriteAllText(_statusPath, string.Empty);
			}
			else if (newScene.name == GameSceneName)
			{
				_mainSetupData = Resources.FindObjectsOfTypeAll<MainGameSceneSetupData>().FirstOrDefault();
				if (_mainSetupData == null)
				{
					Console.WriteLine("Song Status: Error finding the scriptable objects required to update presence.");
					return;
				}

				//Main game scene loaded

                var diff = _mainSetupData.difficultyLevel;
                var song = diff.level;

				var gameplayModeText = GetGameplayModeName(_mainSetupData.gameplayMode);
				var keywords = templateText.Split('{', '}');
				
				templateText = ReplaceKeyword("songName", song.songName, keywords, templateText);
				templateText = ReplaceKeyword("songSubName", song.songSubName, keywords, templateText);
				templateText = ReplaceKeyword("authorName", song.songAuthorName, keywords, templateText);
				templateText = ReplaceKeyword("gamemode", gameplayModeText, keywords, templateText);
				templateText = ReplaceKeyword("difficulty", diff.difficulty.Name(), keywords, templateText);
				templateText = ReplaceKeyword("isNoFail",
					_mainSetupData.gameplayOptions.noEnergy ? "No Fail" : string.Empty, keywords, templateText);
				templateText = ReplaceKeyword("isMirrored",
					_mainSetupData.gameplayOptions.mirror ? "Mirrored" : string.Empty, keywords, templateText);
				templateText = ReplaceKeyword("beatsPerMinute",
					song.beatsPerMinute.ToString(CultureInfo.InvariantCulture), keywords, templateText);
				templateText = ReplaceKeyword("notesCount",
					diff.beatmapData.notesCount.ToString(CultureInfo.InvariantCulture), keywords, templateText);
				templateText = ReplaceKeyword("obstaclesCount",
					diff.beatmapData.obstaclesCount.ToString(CultureInfo.InvariantCulture), keywords, templateText);
				templateText = ReplaceKeyword("environmentName", song.environmentSceneInfo.sceneName, keywords,
					templateText);

				File.WriteAllText(_statusPath, templateText);
			}
		}

		private string ReplaceKeyword(string keyword, string replaceKeyword, string[] keywords, string text)
		{
			if (!keywords.Any(x => x.Contains(keyword))) return text;
			var containingKeywords = keywords.Where(x => x.Contains(keyword));

			if (string.IsNullOrEmpty(replaceKeyword))
			{
				//If the replacement word is null or empty, we want to remove the whole bracket.
				foreach (var containingKeyword in containingKeywords)
				{
					text = text.Replace("{" + containingKeyword + "}", string.Empty);
				}

				return text;
			}

			foreach (var containingKeyword in containingKeywords)
			{
				text = text.Replace("{" + containingKeyword + "}", containingKeyword);
			}

			text = text.Replace(keyword, replaceKeyword);

			return text;
		}

		private string ReadStatusPath(string templateText)
		{
			var lines = Regex.Split(templateText, "\r\n|\r|\n");
			if (lines.Length == 0)
			{
				_statusPath = _defaultStatusPath;
				return templateText;
			}
			var sep = lines[0].Split('=');
			if (sep.Length <= 1)
			{
				_statusPath = _defaultStatusPath;
				return templateText;
			}

			if (!string.Equals(sep[0], "path", StringComparison.OrdinalIgnoreCase))
			{
				_statusPath = _defaultStatusPath;
				return templateText;
			}

			_statusPath = sep[1];
			return string.Join(Environment.NewLine, lines.Skip(1));
		}

		public void OnLevelWasLoaded(int level)
		{

		}

		public void OnLevelWasInitialized(int level)
		{

		}

		public void OnUpdate()
		{

		}

		public void OnFixedUpdate()
		{

		}

		public static string GetGameplayModeName(GameplayMode gameplayMode)
		{
			switch (gameplayMode)
			{
				case GameplayMode.SoloStandard:
					return "Solo Standard";
				case GameplayMode.SoloOneSaber:
					return "One Saber";
				case GameplayMode.SoloNoArrows:
					return "No Arrows";
				case GameplayMode.PartyStandard:
					return "Party";
				default:
					return "Solo Standard";
			}
		}
	}
}