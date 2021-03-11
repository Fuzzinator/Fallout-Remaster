using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace ThreePupperStudios.RecentSceneManagement
{
    [InitializeOnLoad]
    internal class InitializeSceneManager
    {
        #region Const Strings

        internal const string TRACKEDCOUNT = "TrackedScenesCount";
        internal const string TRACKSCENEPREVIEW = "TrackScenePreview?";
        internal const string TRACKEDITEDBY = "TrackEditedBy?";
        internal const string CLOSEWINDOWONLOAD = "CloseWindowOnLoad?";

        #endregion

        static InitializeSceneManager()
        {
            if (RecentSceneManager.instance == null)
            {
                RecentSceneManager.instance = ScriptableObject.CreateInstance<RecentSceneData>();
            }

            SetEditorPrefs();

            RecentSceneManager.recentScenes.Capacity = RecentSceneManager.keepTrackOfRecentScenes;

#if UNITY_2019_4_OR_NEWER //Unity finally fixed the bug that makes this not work but prior to 2019.4 or 2020.2 it doesn't work.
            EditorSceneManager.sceneClosed += RecentSceneManager.SceneClosed;
#endif
            EditorSceneManager.sceneOpened += RecentSceneManager.OpenedScene;
            EditorSceneManager.newSceneCreated += RecentSceneManager.NewSceneCreated;
            EditorSceneManager.sceneSaved += RecentSceneManager.SceneSaved;
        }

        private static void SetEditorPrefs()
        {
            RecentSceneManager.keepTrackOfRecentScenes =
                GetEditorPrefInt(TRACKEDCOUNT, RecentSceneManager.keepTrackOfRecentScenes);
            
            RecentSceneManager.trackScenePreview = GetEditorPrefBool(TRACKSCENEPREVIEW, true);
            RecentSceneManager.trackEditedBy = GetEditorPrefBool(TRACKEDITEDBY, true);
            RecentSceneManager.closeWindowOnLoad = GetEditorPrefBool(CLOSEWINDOWONLOAD, false);


            for (var i = 0; i < RecentSceneManager.keepTrackOfRecentScenes; i++)
            {
                if (i < RecentSceneManager.recentScenes.Count)
                {
                    RecentSceneManager.recentScenes[i] =
                        GetEditorPrefString($"{Application.productName}{RecentSceneManager.RECENTSCENES}{i}");

                    var sceneJson =
                        GetEditorPrefString($"{Application.productName}{RecentSceneManager.ACTUALRECENTSCENES}{i}");
                    if (!string.IsNullOrWhiteSpace(sceneJson))
                    {
                        if (!System.IO.File.Exists($"{Application.dataPath}{sceneJson.Substring(sceneJson.IndexOf('/'))}"))
                        {
                            continue;
                        }
                        var recentScene = JsonUtility.FromJson<RecentScene>(sceneJson);
                        if (!string.IsNullOrWhiteSpace(recentScene.name))
                        {
                            recentScene.data = recentScene.GetDataObj();
                        }
                        RecentSceneManager.pathAndScene[RecentSceneManager.recentScenes[i]] = recentScene;
                    }
                }
                else
                {
                    var scenePath =
                        GetEditorPrefString($"{Application.productName}{RecentSceneManager.RECENTSCENES}{i}");
                    RecentSceneManager.recentScenes.Add(scenePath);


                    var sceneJson =
                        GetEditorPrefString($"{Application.productName}{RecentSceneManager.ACTUALRECENTSCENES}{i}");
                    if (!string.IsNullOrWhiteSpace(sceneJson))
                    {
                        if (!System.IO.File.Exists($"{Application.dataPath}{sceneJson.Substring(sceneJson.IndexOf('/'))}"))
                        {
                            continue;
                        }
                        var recentScene = JsonUtility.FromJson<RecentScene>(sceneJson);
                        if (!string.IsNullOrWhiteSpace(recentScene.name))
                        {
                            recentScene.data = recentScene.GetDataObj();
                        }
                        RecentSceneManager.pathAndScene[scenePath] = recentScene;
                    }
                }

                if (i >= RecentSceneManager.recentScenes.Count)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(RecentSceneManager.recentScenes[i]))
                {
                    RecentSceneManager.recentScenes.RemoveAt(i);
                }
            }
            
            
            var prev = RecentSceneManager.PreviousScene;
            
            var favoritesCount = 
                GetEditorPrefInt($"{Application.productName}{FavoriteScenesManager.FAVORITEDSCENESCOUNT}");
            for (var i = 0; i < favoritesCount; i++)
            {
                var scenePath =
                    GetEditorPrefString($"{Application.productName}{FavoriteScenesManager.FAVORITESCENES}{i}");
                var sceneJson =
                    GetEditorPrefString($"{Application.productName}{FavoriteScenesManager.ACTUALFAVORITEDSCENES}{i}");
                
                if (!string.IsNullOrWhiteSpace(sceneJson))
                {
                    if (!System.IO.File.Exists($"{Application.dataPath}{sceneJson.Substring(sceneJson.IndexOf('/'))}"))
                    {
                        continue;
                    }
                    var recentScene = JsonUtility.FromJson<RecentScene>(sceneJson);
                    if (!string.IsNullOrWhiteSpace(recentScene.name))
                    {
                        recentScene.data = recentScene.GetDataObj();
                    }
                    RecentSceneManager.pathAndScene[scenePath] = recentScene;
                }
                
                if (i < FavoriteScenesManager.favoritedScenes.Count)
                {
                    FavoriteScenesManager.favoritedScenes[i] = scenePath;

                   if (!string.IsNullOrWhiteSpace(sceneJson))
                    {
                        if (!System.IO.File.Exists($"{Application.dataPath}{sceneJson.Substring(sceneJson.IndexOf('/'))}"))
                        {
                            continue;
                        }
                        var favoriteScene = JsonUtility.FromJson<RecentScene>(sceneJson);
                        if (!string.IsNullOrWhiteSpace(favoriteScene.name))
                        {
                            favoriteScene.data = favoriteScene.GetDataObj();
                        }
                        RecentSceneManager.pathAndScene[FavoriteScenesManager.favoritedScenes[i]] = favoriteScene;
                    }
                }
                else
                {
                    FavoriteScenesManager.favoritedScenes.Add(scenePath);
                }

                if (i >= FavoriteScenesManager.favoritedScenes.Count)
                {
                    continue;
                }

                if (string.IsNullOrWhiteSpace(FavoriteScenesManager.favoritedScenes[i]))
                {
                    FavoriteScenesManager.favoritedScenes.RemoveAt(i);
                } 
            }
        }

        private static int GetEditorPrefInt(string key, int defaultValue = 0)
        {
            key = $"{PlayerSettings.companyName}.{PlayerSettings.productName}.{key}";
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetInt(key) : defaultValue;
        }

        internal static void SetEditorPrefInt(string key, int value)
        {
            key = $"{PlayerSettings.companyName}.{PlayerSettings.productName}.{key}";
            if (!EditorPrefs.HasKey(key) || value != EditorPrefs.GetInt(key))
            {
                EditorPrefs.SetInt(key, value);
            }
        }

        internal static string GetEditorPrefString(string key, string defaultValue = "")
        {
            key = $"{PlayerSettings.companyName}.{PlayerSettings.productName}.{key}";
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetString(key) : defaultValue;
        }

        internal static void SetEditorPrefString(string key, string value)
        {
            key = $"{PlayerSettings.companyName}.{PlayerSettings.productName}.{key}";
            if (string.IsNullOrWhiteSpace(value))
            {
                EditorPrefs.DeleteKey(key);
            }
            else if (!EditorPrefs.HasKey(key) || value != EditorPrefs.GetString(key))
            {
                EditorPrefs.SetString(key, value);
            }
        }

        private static bool GetEditorPrefBool(string key, bool defaultValue = true)
        {
            key = $"{PlayerSettings.companyName}.{PlayerSettings.productName}.{key}";
            return EditorPrefs.HasKey(key) ? EditorPrefs.GetBool(key) : defaultValue;
        }

        internal static void SetEditorPrefBool(string key, bool value)
        {
            key = $"{PlayerSettings.companyName}.{PlayerSettings.productName}.{key}";
            if (!EditorPrefs.HasKey(key) || value != EditorPrefs.GetBool(key))
            {
                EditorPrefs.SetBool(key, value);
            }
        }

        internal static void RemoveKey(string key)
        {
            key = $"{PlayerSettings.companyName}.{PlayerSettings.productName}.{key}";
            
            if (EditorPrefs.HasKey(key)) EditorPrefs.DeleteKey(key);
        }
    }
}