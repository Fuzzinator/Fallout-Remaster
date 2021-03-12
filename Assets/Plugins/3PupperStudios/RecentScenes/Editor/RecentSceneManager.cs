using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEngine.SceneManagement;
using UnityEditor.SceneManagement;
using System.Reflection;

namespace ThreePupperStudios.RecentSceneManagement
{
    using Scene = UnityEngine.SceneManagement.Scene;
    internal static class RecentSceneManager
    {
        #region Const Strings

        internal const string RECENTSCENES = "RecentScenes";
        internal const string ACTUALRECENTSCENES = "ActualRecentScenes";

        internal const string OPENSCENEMESSAGE = "What would you like to do?";
        internal const string SAVEANDOPEN = "Save and Open";
        internal const string OPENDONTSAVE = "Open Don't Save";
        internal const string CANCEL = "Cancel";

        internal const string SAVEALLTITLE = "Save all scenes?";
        internal const string SAVEALLMESSAGE = "Would you like to save all open scenes?";

        internal const string YES = "Yes";
        internal const string NO = "No";
        internal const string CLOSE = "Close";

        #endregion

        internal static RecentSceneData instance;
        internal static int keepTrackOfRecentScenes = 5;
        internal static bool trackScenePreview = true;
        internal static bool trackEditedBy = true;
        internal static bool closeWindowOnLoad = false;
        internal static int recentOrFav = 0;

        internal static readonly List<string> recentScenes = new List<string>();

        private static string _previousScene = string.Empty;

        private static string _lastScene = string.Empty;

        internal static string PreviousScene
        {
            get
            {
                if (string.IsNullOrWhiteSpace(_previousScene))
                {
                    if (recentScenes.Count >= 2)
                    {
                        _previousScene = recentScenes[1];
                    }
                    else
                    {
                        _previousScene =
                            InitializeSceneManager.GetEditorPrefString($"{Application.productName}{RECENTSCENES}{1}",
                                string.Empty);
                    }
                }

                return _previousScene;
            }
            set { _previousScene = value; }
        }

        private static string newScene = string.Empty;

        private static bool _newSceneNotSaved = false;
        private static bool _comingFromNewScene = false;

        internal static readonly Dictionary<string, RecentScene> pathAndScene = new Dictionary<string, RecentScene>();

        internal static void NewSceneCreated(Scene myNewScene, NewSceneSetup setup, NewSceneMode mode)
        {
            _previousScene = _lastScene;
            _lastScene = newScene;
            _newSceneNotSaved = true;
            var now = System.DateTime.Now;
            var lastOpened = $"{now.ToShortDateString()} at {now.ToShortTimeString()}";
            var scene = new RecentScene(myNewScene, instance, lastOpened, true);
            if (scene.data != null)
            {
                var userName = GetUserName();
                scene.data.lastEditedBy = userName;
                scene.data.lastEditedDate = lastOpened;
                scene.data.Save();
            }

            pathAndScene[myNewScene.path] = scene;
            UpdatePreviousScenes();
        }

        internal static void SceneClosed(Scene closingScene)
        {
            if (!pathAndScene.ContainsKey(closingScene.path))
            {
                return;
            }

            if (instance == null)
            {
                instance = ScriptableObject.CreateInstance<RecentSceneData>();
            }

            var originalScene = pathAndScene.ContainsKey(closingScene.path)
                ? pathAndScene[closingScene.path]
                : new RecentScene(closingScene, instance);

            var scene = new RecentScene(closingScene, instance, originalScene.lastOpened);
            pathAndScene[closingScene.path] = scene;
        }

        internal static void SceneSaved(Scene savedScene)
        {
            if (pathAndScene.ContainsKey(savedScene.path))
            {
                if (instance == null)
                {
                    instance = ScriptableObject.CreateInstance<RecentSceneData>();
                }

                var originalScene = pathAndScene.ContainsKey(savedScene.path)
                    ? pathAndScene[savedScene.path]
                    : new RecentScene(savedScene, instance);

                var scene = new RecentScene(savedScene, instance, originalScene.lastOpened, true);
                if (scene.data != null)
                {
                    var now = System.DateTime.Now;
                    var lastEdited = $"{now.ToShortDateString()} at {now.ToShortTimeString()}";
                    var userName = GetUserName();
                    scene.data.lastEditedBy = userName;
                    scene.data.lastEditedDate = lastEdited;
                    scene.data.Save();
                }

                pathAndScene[savedScene.path] = scene;
            }

            if (!_newSceneNotSaved)
            {
                return;
            }

            newScene = savedScene.path;
            _newSceneNotSaved = false;
            _comingFromNewScene = true;
        }

        internal static void OpenedScene(Scene myNewScene, OpenSceneMode mode)
        {
            if (newScene == myNewScene.path && !_comingFromNewScene)
            {
                return;
            }

            _previousScene = _lastScene;
            _lastScene = myNewScene.path;

            var now = System.DateTime.Now;
            var lastOpened = $"{now.ToShortDateString()} at {now.ToShortTimeString()}";
            var recentScene = new RecentScene(myNewScene, instance, lastOpened);

            if (recentScene.data != null)
            {
                recentScene.data.Save();
            }


            pathAndScene[myNewScene.path] = recentScene;
            newScene = myNewScene.path;

            if (instance == null)
            {
                instance = ScriptableObject.CreateInstance<RecentSceneData>();
            }

            UpdatePreviousScenes();
        }

        private static void UpdatePreviousScenes()
        {
            if (!recentScenes.Contains(_lastScene))
            {
                if (recentScenes.Count == keepTrackOfRecentScenes)
                {
                    var last = recentScenes[recentScenes.Count - 1];
                    if (pathAndScene.ContainsKey(last))
                    {
                        var recentScene = pathAndScene[last];
                        recentScene.ClearImage();
                    }

                    recentScenes.Remove(last);
                }

                recentScenes.Insert(0, _lastScene);
            }
            else
            {
                recentScenes.RemoveAt(recentScenes.IndexOf(_lastScene));
                recentScenes.Insert(0, _lastScene);
            }

            WriteToEditorPrefs();

            if (ShowRecentScenes.Window != null)
            {
                ShowRecentScenes.Window.RefreshGUI();
            }
        }

        public static void WriteToEditorPrefs()
        {
            for (var i = 0; i < keepTrackOfRecentScenes; i++)
            {
                if (i >= recentScenes.Count || recentScenes[i] == null)
                {
                    continue;
                }

                InitializeSceneManager.SetEditorPrefString(
                    $"{Application.productName}{RECENTSCENES}{i}", recentScenes[i]);

                if (!pathAndScene.ContainsKey(recentScenes[i]))
                {
                    continue;
                }

                var sceneJson = EditorJsonUtility.ToJson(pathAndScene[recentScenes[i]]);
                if (!string.IsNullOrWhiteSpace(sceneJson))
                {
                    InitializeSceneManager.SetEditorPrefString(
                        $"{Application.productName}{ACTUALRECENTSCENES}{i}", sceneJson);
                }
            }
        }
        
        private static string GetUserName()
        {
            var assembly = Assembly.GetAssembly(typeof(EditorWindow));
            var uc = assembly.CreateInstance("UnityEditor.Connect.UnityConnect", false,
                BindingFlags.NonPublic | BindingFlags.Instance, null, null, null, null);
            
            // Cache type of UnityConnect.
            var t = uc?.GetType();
            
            // Get user info object from UnityConnect.
            var userInfo = t?.GetProperty("userInfo")?.GetValue(uc, null);
            
            // Retrieve userName and displayName from user info.
            var userInfoType = userInfo?.GetType();
            var displayName = userInfoType?.GetProperty("displayName")?.GetValue(userInfo, null) as string;
            var userName = userInfoType?.GetProperty("userName")?.GetValue(userInfo, null) as string;
            var name = $"{displayName} - {userName}";
            if (string.IsNullOrWhiteSpace(name))
            {
                name = System.Security.Principal.WindowsIdentity.GetCurrent().Name;
            }

            return name;
        }

        [MenuItem("File/Open Previous Scene", false, 151)]
        private static void ReopenLastScene()
        {
            if (PreviousScene == SceneManager.GetActiveScene().path || string.IsNullOrWhiteSpace(_previousScene))
            {
                return;
            }

            var split = _previousScene.Split('/');

            var sceneName = split[split.Length - 1];
            sceneName = sceneName.Substring(0, sceneName.IndexOf(".", System.StringComparison.Ordinal));
            var askToSave = false;
            for (var i = 0; i < SceneManager.sceneCount; i++)
            {
                var scene = SceneManager.GetSceneAt(i);
                if (!scene.isDirty)
                {
                    continue;
                }

                askToSave = true;
                break;
            }

            if (askToSave)
            {
                var option = EditorUtility.DisplayDialogComplex($"Open {sceneName}", OPENSCENEMESSAGE, SAVEANDOPEN,
                    CANCEL,
                    OPENDONTSAVE);
                switch (option)
                {
                    case (0):
                        if (SceneManager.sceneCount > 1 && EditorUtility.DisplayDialog(SAVEALLTITLE,
                            SAVEALLMESSAGE, YES, NO))
                        {
                            for (var i = 0; i > SceneManager.sceneCount; i++)
                            {
                                EditorSceneManager.SaveScene(SceneManager.GetSceneAt(i));
                            }
                        }
                        else
                        {
                            EditorSceneManager.SaveScene(SceneManager.GetActiveScene());
                        }

                        EditorSceneManager.OpenScene(_previousScene);
                        break;
                    case (1):
                        break;
                    case (2):
                        EditorSceneManager.OpenScene(_previousScene);
                        break;
                }
            }
            else
            {
                EditorSceneManager.OpenScene(_previousScene);
            }
        }

        [MenuItem("File/Recent Scenes", false, 152)]
        private static void OpenRecentScenes()
        {
            ShowRecentScenes.Init();
        }

        [MenuItem("File/Open Previous Scene", true)]
        private static bool ReopenLastSceneValidation()
        {
            return (!string.IsNullOrWhiteSpace(PreviousScene) && _previousScene != SceneManager.GetActiveScene().path);
        }

        [MenuItem("File/Recent Scenes", true)]
        private static bool OpenRecentScenesValidation()
        {
            return (recentScenes != null && recentScenes.Count > 0 && recentScenes[0] != null);
        }
    }

    [System.Serializable]
    internal struct RecentScene
    {
        [SerializeField] internal string path;
        [SerializeField] internal string name;
        [SerializeField] internal string objCount;
        [SerializeField] internal string lastOpened;

        [SerializeField] private Texture2D _scenePreview;

        internal RecentSceneData data;


        internal Texture2D ScenePreview
        {
            get
            {
                if (_scenePreview == null)
                {
                    _scenePreview = GetScenePreview(RecentSceneManager.instance, name);
                }

                return _scenePreview;
            }
            set { _scenePreview = value; }
        }


        internal bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(path) && !string.IsNullOrWhiteSpace(objCount);
        }

        internal RecentScene(Scene scene, ScriptableObject obj, string openDate = "", bool creatingNew = false)
        {
            path = scene.path;
            name = scene.name;
            objCount = GetSceneObjCount(scene);
            lastOpened = openDate;
            if (scene.isLoaded && RecentSceneManager.trackScenePreview)
            {
                _scenePreview = SetScenePreview(scene, obj);
            }
            else
            {
                _scenePreview = GetScenePreview(obj, scene.name);
            }

            data = null;
            
            if (RecentSceneManager.trackEditedBy && !string.IsNullOrWhiteSpace(name))
            {
                data = GetDataObj(creatingNew);
            }
        }

        internal static string GetSceneObjCount(Scene scene)
        {
            if (!scene.IsValid() || !scene.isLoaded)
            {
                return "Invalid";
            }

            var rootObjs = scene.GetRootGameObjects();
            var count = 0;
            foreach (var obj in rootObjs)
            {
                count += GetChildCount(obj.transform);
            }

            return count.ToString();
        }

        private static int GetChildCount(Transform parent, List<Transform> checkedObjs = null)
        {
            if (checkedObjs == null)
            {
                checkedObjs = new List<Transform>() {parent};
            }
            else if (!checkedObjs.Contains(parent))
            {
                checkedObjs.Add(parent);
            }
            else
            {
                return 0;
            }

            for (var i = 0; i < parent.childCount; i++)
            {
                var t = parent.GetChild(i);
                if (checkedObjs.Contains(t))
                {
                    continue;
                }

                GetChildCount(t, checkedObjs);
            }

            return checkedObjs.Count;
        }

        internal static Texture2D SetScenePreview(Scene scene, ScriptableObject obj)
        {
            if(string.IsNullOrWhiteSpace(scene.name))
            {
                return null;
            }
            var cam = GetObjInScene<Camera>(scene, "MainCamera");
            if (cam == null)
            {
                cam = GetObjInScene<Camera>(scene);
            }

            if (cam == null)
            {
                return null;
            }

            var imgFolder = GetImagePath(obj);

            var targetTex = cam.targetTexture;
            var currentRT = RenderTexture.active;

            var width = cam.pixelWidth;
            var height = cam.pixelHeight;
            var camRendnull = targetTex == null;
            if (camRendnull)
            {
                var rendTex = new RenderTexture(width, height, 16, RenderTextureFormat.Default);

                (cam.targetTexture = rendTex).Create();
                targetTex = rendTex;
            }

            RenderTexture.active = targetTex;
            cam.Render();
            if(camRendnull)
            {
                cam.targetTexture = null;
            }

            var image = new Texture2D(width, height);
            try
            {
                image.ReadPixels(new Rect(0, 0, width, height), 0, 0);
            }
            catch
            {
            }

            image.Apply();

            var bytes = image.EncodeToJPG();
            var path = $"{imgFolder}/{scene.name}.jpg";
            System.IO.File.WriteAllBytes(path, bytes);

            RenderTexture.active = currentRT;
            AssetDatabase.ImportAsset(path);
            return image;
        }

        private static Texture2D GetScenePreview(ScriptableObject obj, string name)
        {
            var imgFolder = GetImagePath(obj);

            var image = AssetDatabase.LoadAssetAtPath<Texture2D>($"{imgFolder}/{name}.jpg");
            return image;
        }

        internal static string GetImagePath(ScriptableObject obj)
        {
            var directory = GetMyPath(obj);
            var imgFolder = $"{directory}/Preview Images";
            if (!AssetDatabase.IsValidFolder(imgFolder))
            {
                AssetDatabase.CreateFolder(directory, "Preview Images");
            }

            return imgFolder;
        }

        internal static string GetSceneDataPath(ScriptableObject obj)
        {
            var directory = GetMyPath(obj);
            var dataFolder = $"{directory}/Scene Data";
            if (!AssetDatabase.IsValidFolder(dataFolder))
            {
                AssetDatabase.CreateFolder(directory, "Scene Data");
            }

            return dataFolder;
        }

        private static string GetMyPath(ScriptableObject obj)
        {
            if (obj == null)
            {
                obj = ScriptableObject.CreateInstance<RecentSceneData>();
            }

            var script = MonoScript.FromScriptableObject(obj);
            var scriptPath = AssetDatabase.GetAssetPath(script);
            var directory = scriptPath.Substring(0, scriptPath.LastIndexOf('/'));
            return directory;
        }

        internal static T GetObjInScene<T>(Scene scene, string tag = null, bool onlyEnabled = true) where T : Behaviour
        {
            T component = null;
            if (!scene.IsValid())
            {
                return null;
            }

            var rootObjs = scene.GetRootGameObjects();
            foreach (var obj in rootObjs)
            {
                component = GetObjInChildren<T>(obj.transform, tag);
                if (component != null)
                {
                    break;
                }
            }

            return component;
        }

        internal RecentSceneData GetDataObj(bool createNew = false)
        {
            var assetPath = GetSceneDataPath(RecentSceneManager.instance);
            var assetsAt = new List<Object>();
            assetsAt.AddRange(AssetDatabase.LoadAllAssetsAtPath($"{assetPath}/{name}.asset"));
            var scenePath = this.path;
            var asset =
                assetsAt.Find(i => i as RecentSceneData != null && ((RecentSceneData) i).path == scenePath) as
                    RecentSceneData;

            if (createNew && asset == null)
            {
                asset = ScriptableObject.CreateInstance<RecentSceneData>();
                asset.path = path;
                AssetDatabase.CreateAsset(asset, $"{assetPath}/{name}.asset");
                AssetDatabase.Refresh();
            }
            
            return asset;
        }

        private static T GetObjInChildren<T>(Transform parent, string tag = null, List<Transform> checkedObjs = null,
            bool onlyEnabled = true)
            where T : Behaviour
        {
            T component = null;
            if (checkedObjs == null)
            {
                checkedObjs = new List<Transform>() {parent};
            }
            else if (!checkedObjs.Contains(parent))
            {
                checkedObjs.Add(parent);
            }
            else
            {
                return null;
            }

            if (onlyEnabled && parent.gameObject.activeSelf)
            {
                if (!string.IsNullOrWhiteSpace(tag) && parent.CompareTag(tag))
                {
                    component = parent.GetComponent<T>();
                    if (component != null)
                    {
                        component = onlyEnabled ? component.enabled ? component : null : component;
                    }

                    if (component != null)
                    {
                        return component;
                    }
                }
            }


            for (var i = 0; i < parent.childCount; i++)
            {
                var t = parent.GetChild(i);
                if (checkedObjs.Contains(t))
                {
                    continue;
                }

                component = GetObjInChildren<T>(t, tag, checkedObjs, onlyEnabled);
                if (component != null)
                {
                    break;
                }
            }

            return component;
        }

        internal void ClearImage()
        {
            var imgPath = GetImagePath(RecentSceneManager.instance);
            if (!string.IsNullOrWhiteSpace(imgPath))
            {
                imgPath = $"{imgPath}/{name}.jpg";
                AssetDatabase.DeleteAsset(imgPath);
            }

            _scenePreview = null;
        }
    }
}