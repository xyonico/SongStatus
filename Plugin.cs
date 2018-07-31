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

        private MainGameSceneSetupData _mainSetupData;
        private bool _init;
        private StatusWriter _writer;

        public string Name { get => "Song Status"; }
        public string Version { get => "v1.4"; }

        public void UpdateTemplate()
        {
            Logger.Log("Reading Template");

            TemplateReader.EnsureTemplateExists();
            Template template = TemplateReader.ReadTemplate();

            _writer = new StatusWriter(template);
        }

        public void OnApplicationStart()
        {
            if (_init) return;
            _init = true;

            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;

            UpdateTemplate();
            _writer.WriteEmpty();
        }

        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
            _writer.WriteEmpty();
        }

        private void SceneManagerOnActiveSceneChanged(Scene oldScene, Scene newScene)
        {
            if (newScene.name == MenuSceneName)
            {
                // Menu scene loaded
                _writer.WriteEmpty();
                return;
            }

            if (newScene.name != GameSceneName)
                return;

            // Only execute when main game scene is loaded
            _mainSetupData = Resources.FindObjectsOfTypeAll<MainGameSceneSetupData>().FirstOrDefault();
            if (_mainSetupData == null)
            {
                Logger.Log("Error finding the scriptable objects required to update presence.");
                return;
            }

            var diff = _mainSetupData.difficultyLevel;
            var song = diff.level;

            string isNoFail = _mainSetupData.gameplayOptions.noEnergy ? "No Fail" : string.Empty;
            string isMirrored = _mainSetupData.gameplayOptions.mirror ? "Mirrored" : string.Empty;

            string beatsPerMinute = song.beatsPerMinute.ToString(CultureInfo.InvariantCulture);
            string notesCount = diff.beatmapData.notesCount.ToString(CultureInfo.InvariantCulture);
            string obstaclesCount = diff.beatmapData.obstaclesCount.ToString(CultureInfo.InvariantCulture);

            _writer.ReplaceKeyword("songName", song.songName);
            _writer.ReplaceKeyword("songSubName", song.songSubName);
            _writer.ReplaceKeyword("authorName", song.songAuthorName);
            _writer.ReplaceKeyword("gamemode", GetGameplayModeName(_mainSetupData.gameplayMode));
            _writer.ReplaceKeyword("difficulty", diff.difficulty.Name());
            _writer.ReplaceKeyword("isNoFail", isNoFail);
            _writer.ReplaceKeyword("isMirrored", isMirrored);
            _writer.ReplaceKeyword("beatsPerMinute", beatsPerMinute);
            _writer.ReplaceKeyword("notesCount", notesCount);
            _writer.ReplaceKeyword("obstaclesCount", obstaclesCount);
            _writer.ReplaceKeyword("environmentName", song.environmentSceneInfo.sceneName);

            _writer.Write();
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
