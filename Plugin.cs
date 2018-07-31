using System;
using System.Globalization;
using System.Linq;
using IllusionPlugin;
using UnityEngine;
using UnityEngine.SceneManagement;
using WebSocketSharp.Server;

namespace SongStatus
{
    public class Broadcast : WebSocketBehavior
    {
        public Broadcast() : base()
        {
            this.IgnoreExtensions = true;
        }

        protected override void OnOpen()
        {
            base.OnOpen();
            Logger.Log("WebSocket Client Connected!");
        }
    }

    public class Plugin : IPlugin
    {
        private const string MenuSceneName = "Menu";
        private const string GameSceneName = "StandardLevel";

        private MainGameSceneSetupData _mainSetupData;
        private bool _init;
        private StatusWriter _writer;

        private WebSocketServer _wss;
        private bool _wsEnabled = false;

        public string Name { get => "Song Status"; }
        public string Version { get => "v1.4"; }

        public void UpdateTemplate()
        {
            Logger.Log("Reading Template");

            TemplateReader.EnsureTemplateExists();
            Template template = TemplateReader.ReadTemplate();

            if (_wsEnabled)
                _writer = new StatusWriter(template, _wss);
            else
                _writer = new StatusWriter(template);
        }

        public void OnApplicationStart()
        {
            if (_init) return;
            _init = true;

            SceneManager.activeSceneChanged += SceneManagerOnActiveSceneChanged;

            int wsPort = PortManager.FindAvailablePort(3333);
            if (wsPort == 0)
            {
                // Couldn't find free port
                Logger.Log("Could not find free port");
            }
            else
            {
                Logger.Log("Starting WebSocket Server on port " + wsPort);
                _wss = new WebSocketServer(wsPort);
                _wss.AddWebSocketService<Broadcast>("/");
                _wss.Start();

                _wsEnabled = true;
            }

            UpdateTemplate();
            _writer.WriteEmpty();
        }

        public void OnApplicationQuit()
        {
            SceneManager.activeSceneChanged -= SceneManagerOnActiveSceneChanged;
            _writer.WriteEmpty();
            _wss.Stop();
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

            // Update template
            UpdateTemplate();

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
