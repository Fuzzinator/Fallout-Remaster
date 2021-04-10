using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Core
{
    public class FalloutSceneManager : MonoBehaviour
    {
        public static FalloutSceneManager Instance { get; private set; }

        /// <summary>
        /// Invoked when a scene is loaded. The scene is always the new scene, the first SceneType is the previous scene
        /// and the second scene type is the new scene
        /// </summary>
        public static Action<Scene, SceneType, SceneType> sceneLoaded;

        private static SceneType _currentSceneType;
        private static Location _currentLocation;
        public static Location CurrentLocation => _currentLocation;
        public static Map CurrentMap => _currentLocation?.ActiveMap;

        private void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(this);
            }
            else
            {
                Destroy(this);
            }
        }

        public static void SetLocation(Location location, Map map)
        {
            
        }

        public static void LoadNewScene(int buildIndex, SceneType sceneType, bool additive = false)
        {
            var currentScene = SceneManager.GetActiveScene();
            var currentType = _currentSceneType;
            _currentSceneType = sceneType;
            var param = additive ? LoadSceneMode.Additive : LoadSceneMode.Single;
            var loadScene = SceneManager.LoadSceneAsync(buildIndex, param);
            loadScene.completed += (op) =>
            {
                sceneLoaded?.Invoke(SceneManager.GetSceneAt(buildIndex), currentType, sceneType);
            };
        }

        public enum SceneType
        {
            None = 0,
            Menu = 1,
            Game = 2,
            Map = 3
        }
    }
}