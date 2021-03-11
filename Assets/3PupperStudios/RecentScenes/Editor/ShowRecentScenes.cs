using System.Collections.Generic;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;
using StringComparison = System.StringComparison;

namespace ThreePupperStudios.RecentSceneManagement
{
    internal class ShowRecentScenes : EditorWindow, IHasCustomMenu
    {
        private static ShowRecentScenes _window;

        internal static ShowRecentScenes Window => _window;

        private static SettingsWindow _settingsWindow;
        private static PreviewWindow _previewWindow;
        private static InfoWindow _infoWindow;

        private static Texture2D _toolIcon;

        private static Texture2D ToolIcon
        {
            get
            {
                if (_toolIcon == null)
                {
                    _toolIcon = Resources.Load(TOOLICON) as Texture2D;
                }

                return _toolIcon;
            }
        }
        
        private static Texture2D _toolIconSmall;
        private static Texture2D _toolIconSmallDarkMode;
        private static Texture2D ToolIconSmall
        {
            get
            {
                if (EditorGUIUtility.isProSkin)
                {
                    if (_toolIconSmallDarkMode == null)
                    {
                        _toolIconSmallDarkMode = Resources.Load(TOOLICONSMALLDARKMODE) as Texture2D;
                    }

                    return _toolIconSmallDarkMode;
                }
                else
                {
                    if (_toolIconSmall == null)
                    {
                        _toolIconSmall = Resources.Load(TOOLICONSMALL) as Texture2D;
                    }

                    return _toolIconSmall;
                }
            }
        }

        private static readonly Dictionary<string, bool> _foldoutSelected = new Dictionary<string, bool>();

        #region Const strings

        private const string ADD = "Load Additive";
        private const string SINGLE = "Load Single";
        private const string RECENT = "Recent";

        private const string RECENTTOOLTIP =
            "View your previous scenes and select to load them individually or load them additively.";

        private const string TOOLICON = "RecentScenesIcon";
        private const string TOOLICONSMALL = "RecentScenesIconSmall";
        private const string TOOLICONSMALLDARKMODE = "RecentScenesIconSmallDarkMode";
        
        private const string BOX = "box";
        private const string FILENAMEDIVIDER = ".";

        private const string SCENEOPENTITLE = "Scene Currently Open.";
        private const string SCENEOPENMESSAGE = "The selected scene is already open. Would you like to reopen it?";

        private const string CLEARRECENT = "Clear Recent Scenes";

        private const string RECENTSCENES = "RecentScenes";

        private const string FAVORITESCENES = "FavoriteScenes";

        private const string NORECENT = "There are no recent scenes";

        private const string NOFAVORITES = "There are no favorited scenes";

        private const string HELPICON = "_Help";
        private const string HELPTOOLTIP = "Opens Recent Scenes Manager Info";

        private const string RECENTBUTTONTOOLTIP = "View Recently Opened Scenes.";
        
        private const string FAVORITES = "Favorites";
        private const string FAVORITESBUTTONTOOLTIP = "View favorited scenes.";

        private const string CLEARFAVORITES = "Clear Favorites";
        private const string CLEARFAVTOOLTIP = "Remove all scenes from your favorites.";

        private const string CLEARFAVCONFIRM = "Clear All Favorites?";
        private const string CLEARFAVMSG = "Are you sure you would like to clear all favorited scenes?";

        private const string MOREDETAILS = "More Details";

        private const string OPENFORDETAILS = "Please open the scene at least once to view more information.";

        private const string LASTEDITEDBY = "Last Edited By:";
        private const string LASTEDITEDBYTOOLTIP =
            "The name and email address of the last user to make changes to the scene. Note this works best if the user is signed in to their Unity account in Unity.";

        private const string LASTEDITEDDATE = "Last Edited:";
        private const string LASTEDITEDDATETOOLTIP =
            "The date of the last time this scene was changed. Before tracking this will be retrieved by the systems date. After tracking it uses an internal tracker.";

        private const string LASTEDITEDDATETOOLTIP2 = "The date of the last time this scene was changed.";

        private const string GAMEOBJCOUNT = "GameObject Count:";
        private const string GAMEOBJCOUNTTOOLTIP = "The number of GameObjects in the scene.";
        
        private const string LOCATION = "Location:";
        private const string LOCTOOLTIP = "The location of the scene in your project.";

        private const string REFRESHICON = "RotateTool";
        private const string REFRESHTOOLTIP = "Refresh the GameObject count.";

        private const string SCENELOADED = "Scene Loaded:";

        private const string FAVORITEON = "Favorite On";
        private const string FAVORITEONTOOLTIP = "Un-favorite Scene.";
        private const string FAVORITEOFF = "Favorite Off";
        private const string FAVORITEOFFTOOLTIP = "Favorite Scene";
        
        #endregion

        private static readonly Vector2 WindowMin = new Vector2(360, 200);

        private static Vector2 _scrollPos = Vector2.zero;

        private static Texture2D _refreshBttn;
        private static Texture2D _favBttnOn;
        private static Texture2D _favBttnOff;
        private static Texture2D _settingsBttn;
        private static Texture2D _helpBttn;

        internal static void Init()
        {
            _window = GetWindow<ShowRecentScenes>();
            _window.titleContent.text = RECENT;
            _window.titleContent.tooltip = RECENTTOOLTIP;

            _window.titleContent.image = ToolIconSmall;


            _window.minSize = WindowMin;
            _window.Show();
        }

        internal void RefreshGUI()
        {
            _window.Repaint();
        }

        private void OnGUI()
        {
            if (_window == null)
            {
                _window = this;

                _window.titleContent.image = ToolIcon;
            }

            EditorGUILayout.BeginVertical(BOX);

            GUILayout.Space(2);

            #region Help Button

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();

            if (_helpBttn == null)
            {
                _helpBttn = EditorGUIUtility.Load(HELPICON) as Texture2D;
            }

            var helpStyle = new GUIStyle(GUIStyle.none)
            {
                fontStyle = FontStyle.Bold
            };
            var helpContent = new GUIContent(string.Empty, _helpBttn, HELPTOOLTIP);

            if (GUILayout.Button(helpContent, helpStyle, GUILayout.Width(15)))
            {
                if (_infoWindow == null)
                {
                    _infoWindow = InfoWindow.InfoInit();
                }
            }

            GUILayout.Space(3);
            EditorGUILayout.EndHorizontal();

            #endregion

            GUILayout.Space(1);

            #region Fav or Recent Tab

            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.Space();
            var tabSkin = new GUIStyle(GUI.skin.button)
            {
                alignment = TextAnchor.MiddleCenter,
                fontSize = 12,
                fontStyle = FontStyle.Bold
            };

            var recentBttn = new GUIContent(RECENT, RECENTBUTTONTOOLTIP);
            var favoriteBttn = new GUIContent(FAVORITES, FAVORITESBUTTONTOOLTIP);
            RecentSceneManager.recentOrFav = GUILayout.SelectionGrid(RecentSceneManager.recentOrFav,
                new[] {recentBttn, favoriteBttn}, 2, tabSkin, GUILayout.MaxWidth(200));
            EditorGUILayout.Space();
            EditorGUILayout.EndHorizontal();

            #endregion
            
            GUILayout.Label("", GUI.skin.horizontalSlider);
            
            _scrollPos = GUILayout.BeginScrollView(_scrollPos);
            if (RecentSceneManager.recentOrFav == 0)
            {
                var labelWidth = 0f;
                foreach (var scene in RecentSceneManager.recentScenes)
                {
                    var textDimensions = GUI.skin.label.CalcSize(new GUIContent(scene));
                    if (textDimensions.x > labelWidth) labelWidth = textDimensions.x;
                }

                for (var i = 0; i < RecentSceneManager.recentScenes.Count; i++)
                {
                    i = DisplayObject(i, RecentSceneManager.recentScenes);
                }

                if (RecentSceneManager.recentScenes.Count > 0)
                {
                    var spaceSize = RecentSceneManager.keepTrackOfRecentScenes + 1 -
                                    RecentSceneManager.recentScenes.Count;
                    spaceSize *= 28;
                    GUILayout.Space(pixels: spaceSize);

                    EditorGUILayout.EndScrollView();
                    if (GUILayout.Button(CLEARRECENT))
                    {
                        for (var j = 0; j < RecentSceneManager.recentScenes.Count; j++)
                        {
                            RecentSceneManager.recentScenes[j] = null;
                            InitializeSceneManager.RemoveKey(Application.productName + RECENTSCENES + j);
                        }
                    }
                }
                else
                {
                    EditorGUILayout.EndScrollView();
                    GUILayout.Space(20);
                    var centerBoldLabel = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold
                    };
                    GUILayout.Label(NORECENT, centerBoldLabel);
                    GUILayout.Space(130);

                    if (GUILayout.Button(RecentSceneManager.CLOSE))
                    {
                        _window.Close();
                    }
                }
            }
            else
            {
                var labelWidth = 0f;
                foreach (var scene in FavoriteScenesManager.favoritedScenes)
                {
                    var textDimensions = GUI.skin.label.CalcSize(new GUIContent(scene));
                    if (textDimensions.x > labelWidth) labelWidth = textDimensions.x;
                }

                for (var i = 0; i < FavoriteScenesManager.favoritedScenes.Count; i++)
                {
                    i = DisplayObject(i, FavoriteScenesManager.favoritedScenes);
                }

                if (FavoriteScenesManager.favoritedScenes.Count > 0)
                {
                    var spaceSize = RecentSceneManager.keepTrackOfRecentScenes + 1 -
                                    RecentSceneManager.recentScenes.Count;
                    spaceSize *= 28;
                    GUILayout.Space(pixels: spaceSize);

                    EditorGUILayout.EndScrollView();
                    var clearFavBttn = new GUIContent(CLEARFAVORITES, CLEARFAVTOOLTIP);
                    if (GUILayout.Button(clearFavBttn))
                    {
                        if (EditorUtility.DisplayDialog(CLEARFAVCONFIRM,CLEARFAVMSG,
                            RecentSceneManager.YES, RecentSceneManager.NO))
                        {
                            for (var j = 0; j < FavoriteScenesManager.favoritedScenes.Count; j++)
                            {
                                FavoriteScenesManager.favoritedScenes[j] = null;
                                InitializeSceneManager.RemoveKey(Application.productName + FAVORITESCENES + j);
                            }

                            FavoriteScenesManager.favoritedScenes.Clear();
                        }
                    }
                }
                else
                {
                    EditorGUILayout.EndScrollView();
                    GUILayout.Space(20);
                    var centerBoldLabel = new GUIStyle(EditorStyles.label)
                    {
                        alignment = TextAnchor.MiddleCenter,
                        fontStyle = FontStyle.Bold
                    };
                    GUILayout.Label(NOFAVORITES, centerBoldLabel);
                    GUILayout.Space(130);

                    if (GUILayout.Button(RecentSceneManager.CLOSE))
                    {
                        _window.Close();
                    }
                }
            }

            EditorGUILayout.EndVertical();
        }

        private static int DisplayObject(int i, IList<string> myList)
        {
            var thisScene = myList[i];
            if (string.IsNullOrWhiteSpace(thisScene))
            {
                myList.Remove(thisScene);
                i--;
                return i;
            }

            if (!System.IO.File.Exists($"{Application.dataPath}{thisScene.Substring(thisScene.IndexOf('/'))}"))
            {
                myList.Remove(thisScene);
                FavoriteScenesManager.favoritedScenes.Remove(thisScene);
                
                if (RecentSceneManager.pathAndScene.ContainsKey(thisScene))
                {
                    var recentScene = RecentSceneManager.pathAndScene[thisScene];
                    recentScene.ClearImage();
                    RecentSceneManager.pathAndScene.Remove(thisScene);
                }

                RecentSceneManager.WriteToEditorPrefs();
                
                i--;
                return i;
            }

            var currentEvent = Event.current;
            var eventModifiers = currentEvent?.modifiers ?? 0;

            var split = thisScene.Split('/');
            var sceneName = split[split.Length - 1];
            if (sceneName.LastIndexOf(FILENAMEDIVIDER, StringComparison.Ordinal) >= 0)
            {
                sceneName = sceneName.Substring(0,
                    sceneName.LastIndexOf(FILENAMEDIVIDER, StringComparison.Ordinal));
            }

            EditorGUILayout.BeginVertical(BOX);
            var box = EditorGUILayout.BeginHorizontal();
            if (!_foldoutSelected.ContainsKey(sceneName))
            {
                _foldoutSelected[sceneName] = false;
            }

            var noneStyle = new GUIStyle(GUIStyle.none)
            {
                padding = new RectOffset(3, 2, 4, 0),
                fontStyle = FontStyle.Bold
            };
            GUILayout.Label($"{i + 1}:", noneStyle, GUILayout.MaxWidth(15f));
            
            var foldoutRect = new Rect(box.position.x + 15, box.position.y + (box.height * .1f) + 2, 10,
                10);
            var content = new GUIContent(string.Empty, MOREDETAILS);
            _foldoutSelected[sceneName] =
                EditorGUI.Foldout(foldoutRect, _foldoutSelected[sceneName], content, true);

            GUILayout.Space(15);
            

            var sceneNameStyle = new GUIStyle(GUI.skin.label)
            {
                fontStyle = FontStyle.Bold
            };
            sceneNameStyle.padding = new RectOffset(sceneNameStyle.padding.left, sceneNameStyle.padding.right,
                sceneNameStyle.padding.top + 1, sceneNameStyle.padding.bottom);

            if ((GUILayout.Button(sceneName, sceneNameStyle)))
            {
                if ((eventModifiers & EventModifiers.Alt) != 0)
                {
                    SelectScene(thisScene);
                }
                else if ((eventModifiers & EventModifiers.Command) != 0 ||
                         (eventModifiers & EventModifiers.Control) != 0)
                {
                    SelectScene(thisScene, true);
                }
                else
                {
                    PingScene(thisScene);
                }
            }

            if (GUILayout.Button(SINGLE, GUILayout.MaxWidth(90)))
            {
                if (thisScene != SceneManager.GetActiveScene().path)
                {
                    LoadNewScene(thisScene);
                }
                else
                {
                    if (EditorUtility.DisplayDialog(SCENEOPENTITLE, SCENEOPENMESSAGE, RecentSceneManager.YES,
                        RecentSceneManager.NO))
                    {
                        LoadNewScene(thisScene);
                    }
                }
            }

            if (GUILayout.Button(ADD, GUILayout.MaxWidth(90)))
            {
                var count = SceneManager.sceneCount;
                var canAdd = true;
                for (var x = 0; x < count; x++)
                {
                    if (thisScene != SceneManager.GetSceneAt(x).path)
                    {
                        continue;
                    }

                    if (EditorUtility.DisplayDialog(SCENEOPENTITLE, SCENEOPENMESSAGE, RecentSceneManager.YES,
                        RecentSceneManager.NO))
                    {
                        canAdd = true;
                        break;
                    }

                    canAdd = false;
                }

                if (canAdd)
                {
                    AddScene(thisScene);
                }
            }

            EditorGUILayout.EndHorizontal();
            GUILayout.Space(5);

            if (_foldoutSelected[sceneName])
            {
                DisplayInfo(thisScene);
            }

            EditorGUILayout.EndVertical();
            return i;
        }

        private new void Close()
        {
            if (_settingsWindow != null)
            {
                _settingsWindow.Close();
            }

            if (_previewWindow != null)
            {
                _previewWindow.Close();
            }

            if (_infoWindow != null)
            {
                _infoWindow.Close();
            }

            base.Close();
        }

        private static void DisplayInfo(string path)
        {
            var recentScene = new RecentScene();
            if (RecentSceneManager.pathAndScene.ContainsKey(path))
            {
                recentScene = RecentSceneManager.pathAndScene[path];
            }

            if (string.IsNullOrWhiteSpace(recentScene.name))
            {
                EditorGUILayout.Space();
                var bold = new GUIStyle(GUI.skin.label)
                {
                    fontStyle = FontStyle.Bold,
                    wordWrap = true,
                    alignment = TextAnchor.MiddleCenter
                };
                EditorGUILayout.LabelField(OPENFORDETAILS, bold);
                EditorGUILayout.Space();
            }


            var scene = SceneManager.GetSceneByPath(path);

            EditorGUILayout.BeginHorizontal();
            if (!string.IsNullOrWhiteSpace(recentScene.lastOpened))
            {
                EditorGUILayout.LabelField("Last Opened:", recentScene.lastOpened);
            }

            if (RecentSceneManager.trackScenePreview && recentScene.ScenePreview != null)
            {
                if (GUILayout.Button("Preview", EditorStyles.miniButton, GUILayout.MaxWidth(80)))
                {
                    var tex = recentScene.ScenePreview;
                    if (tex != null)
                    {
                        PreviewWindow.Init(recentScene.name, tex);
                    }
                }
            }

            EditorGUILayout.EndHorizontal();

            var lastEdit = System.IO.File.GetLastWriteTime(path);
            if (RecentSceneManager.trackEditedBy && recentScene.data != null)
            {
                var editedBy = new GUIContent(LASTEDITEDBY, LASTEDITEDBYTOOLTIP);
                EditorGUILayout.LabelField(editedBy,
                    recentScene.data ? recentScene.data.lastEditedBy : string.Empty);
                
                var editedDate = new GUIContent(LASTEDITEDDATE, LASTEDITEDDATETOOLTIP);
                EditorGUILayout.LabelField(editedDate,
                    recentScene.data
                        ? recentScene.data.lastEditedDate
                        : $"{lastEdit.ToShortDateString()} at {lastEdit.ToShortTimeString()}");
            }
            else
            {
                var editedDate = new GUIContent(LASTEDITEDDATE, LASTEDITEDDATETOOLTIP);
                var actualEditedDate = new GUIContent($"{lastEdit.ToShortDateString()} at {lastEdit.ToShortTimeString()}");
                EditorGUILayout.LabelField(editedDate, actualEditedDate);
            }

            if (string.IsNullOrWhiteSpace(recentScene.path))
            {
                recentScene.path = path;
            }

            var locationTitle = new GUIContent(LOCATION, LOCTOOLTIP);
            var locationContent = new GUIContent(recentScene.path, recentScene.path);
            EditorGUILayout.LabelField(locationTitle, locationContent);
            EditorGUILayout.BeginHorizontal();
            if (!string.IsNullOrWhiteSpace(recentScene.objCount))
            {
                var objCount = new GUIContent(GAMEOBJCOUNT, GAMEOBJCOUNTTOOLTIP);
                var actualCount = new GUIContent(recentScene.objCount);
                EditorGUILayout.LabelField(objCount, actualCount);
            }

            if (scene.IsValid() && !scene.isDirty)
            {
                if (_refreshBttn == null)
                {
                    _refreshBttn = EditorGUIUtility.Load(REFRESHICON) as Texture2D;
                }

                var refreshContent = new GUIContent(_refreshBttn, REFRESHTOOLTIP);
                if (GUILayout.Button(refreshContent, GUIStyle.none, GUILayout.MaxWidth(25)))
                {
                    recentScene.objCount = RecentScene.GetSceneObjCount(scene);
                    RecentSceneManager.pathAndScene[path] = recentScene;
                }
            }

            EditorGUILayout.EndHorizontal();
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(SCENELOADED, (scene.IsValid() && scene.isLoaded).ToString());
            EditorGUILayout.Space();

            if (FavoriteScenesManager.favoritedScenes.Contains(path))
            {
                if (_favBttnOn == null)
                {
                    _favBttnOn = Resources.Load(FAVORITEON) as Texture2D;
                }

                var favContent = new GUIContent(_favBttnOn, FAVORITEONTOOLTIP);

                if (GUILayout.Button(favContent, GUIStyle.none, GUILayout.MaxHeight(20), GUILayout.MaxWidth(20)))
                {
                    if (EditorUtility.DisplayDialog($"Unfavorite {recentScene.name}?",
                        $"Are you sure you would like to remove {recentScene.name} from the favorited scenes?", 
                        RecentSceneManager.YES, RecentSceneManager.NO))
                    {
                        FavoriteScenesManager.RemoveFromFavorites(path);
                    }
                }
            }
            else
            {
                if (_favBttnOff == null)
                {
                    _favBttnOff = Resources.Load(FAVORITEOFF) as Texture2D;
                }

                var favContent = new GUIContent(_favBttnOff, FAVORITEOFFTOOLTIP);

                if (GUILayout.Button(favContent, GUIStyle.none, GUILayout.MaxHeight(20), GUILayout.MaxWidth(20),
                    GUILayout.Width(20), GUILayout.Height(20)))
                {
                    FavoriteScenesManager.AddToFavorites(path);
                }
            }


            EditorGUILayout.EndHorizontal();
        }

        private static void PingScene(string sceneName)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath(sceneName, typeof(SceneAsset));
            if (sceneAsset != null)
            {
                EditorGUIUtility.PingObject(sceneAsset);
            }
        }

        private static void SelectScene(string sceneName, bool additive = false)
        {
            var sceneAsset = AssetDatabase.LoadAssetAtPath(sceneName, typeof(SceneAsset));
            if (sceneAsset == null)
            {
                return;
            }

            if (additive)
            {
                var selection = new List<Object>();
                selection.AddRange(Selection.objects);
                if (!selection.Contains(sceneAsset))
                {
                    selection.Add(sceneAsset);
                }
                else
                {
                    selection.Remove(sceneAsset);
                }

                Selection.objects = selection.ToArray();
            }
            else
            {
                Selection.activeObject = sceneAsset;
            }
        }

        private static void LoadNewScene(string path)
        {
            var split = path.Split('/');
            var sceneName = split[split.Length - 1];
            sceneName = sceneName.Substring(0, sceneName.IndexOf(".", StringComparison.Ordinal));
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
                var option = EditorUtility.DisplayDialogComplex($"Open {sceneName}",
                    RecentSceneManager.OPENSCENEMESSAGE,
                    RecentSceneManager.SAVEANDOPEN, RecentSceneManager.CANCEL, RecentSceneManager.OPENDONTSAVE);
                switch (option)
                {
                    case (0):
                        if (SceneManager.sceneCount > 1 &&
                            EditorUtility.DisplayDialog(RecentSceneManager.SAVEALLTITLE,
                                RecentSceneManager.SAVEALLMESSAGE,
                                RecentSceneManager.YES, RecentSceneManager.NO))
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

                        EditorSceneManager.OpenScene(path);
                        if (RecentSceneManager.closeWindowOnLoad)
                        {
                            _window.Close();
                        }

                        break;
                    case (1):
                        break;
                    case (2):
                        EditorSceneManager.OpenScene(path);
                        if (RecentSceneManager.closeWindowOnLoad)
                        {
                            _window.Close();
                        }

                        break;
                }
            }
            else
            {
                EditorSceneManager.OpenScene(path);
                if (RecentSceneManager.closeWindowOnLoad)
                {
                    _window.Close();
                }
            }
        }

        private static void AddScene(string path)
        {
            EditorSceneManager.OpenScene(path, OpenSceneMode.Additive);
            if (RecentSceneManager.closeWindowOnLoad)
            {
                _window.Close();
            }
        }

        private void ShowButton(Rect pos)
        {
            if (_settingsBttn == null)
            {
                _settingsBttn = EditorGUIUtility.Load("SettingsIcon") as Texture2D;
            }

            var settingsContent = new GUIContent(string.Empty, _settingsBttn, "Opens small settings window.");

            var settingsPos = pos;
            settingsPos.y += 2;
            if (GUI.Button(settingsPos, settingsContent, GUIStyle.none))
            {
                if (_settingsWindow == null)
                {
                    _settingsWindow = SettingsWindow.SettingsInit();
                }
            }
        }

        /// <summary>
        /// Adds buttons to the window context menu.
        /// </summary>
        /// <param name="menu"></param>
        public void AddItemsToMenu(GenericMenu menu)
        {
            menu.AddSeparator("");
            menu.AddItem(new GUIContent("Settings"), false, data => SettingsWindow.SettingsInit(), this);
        }

        private static Vector2 GetCenter(Vector2 size)
        {
            var x = (Screen.currentResolution.width - size.x) * .5f;
            var y = (Screen.currentResolution.height - size.y) * .5f;
            return new Vector2(x, y);
        }

        private class SettingsWindow : EditorWindow
        {
            internal static SettingsWindow SettingsInit()
            {
                _settingsWindow = GetWindow(typeof(SettingsWindow), true, "Recent Scenes Settings") as SettingsWindow;
                if (_settingsWindow == null)
                {
                    return null;
                }

                _settingsWindow.titleContent.image = ToolIconSmall;
                _settingsWindow.titleContent.text = "Recent Scenes Settings";
                _settingsWindow.titleContent.tooltip =
                    "Allows you to change a few settings for the Recent Scenes Manager";

                _settingsWindow.minSize = WindowMin;
                _settingsWindow.Show();
                return _settingsWindow;
            }

            private void OnGUI()
            {
                EditorGUILayout.BeginVertical(BOX);
                GUILayout.Space(1);

                #region Help Button

                EditorGUILayout.BeginHorizontal();
                var iconStyle = new GUIStyle(GUIStyle.none)
                {
                    padding = new RectOffset(-1, 0, -2, 0)
                };
                GUILayout.Label(ToolIconSmall, iconStyle, GUILayout.Height(17), GUILayout.Width(17));
                EditorGUILayout.Space();

                if (_helpBttn == null)
                {
                    _helpBttn = EditorGUIUtility.Load("_Help") as Texture2D;
                }

                var helpStyle = new GUIStyle(GUIStyle.none)
                {
                    fontStyle = FontStyle.Bold
                };
                var helpContent = new GUIContent(string.Empty, _helpBttn, "Opens Recent Scenes Manager Info");

                if (GUILayout.Button(helpContent, helpStyle, GUILayout.Width(15)))
                {
                    if (_infoWindow == null)
                    {
                        _infoWindow = InfoWindow.InfoInit();
                    }
                }

                GUILayout.Space(1);
                EditorGUILayout.EndHorizontal();

                #endregion

                GUILayout.Space(3);

                var content = new GUIContent("Max Tracked Scenes");
                var wasTrack = RecentSceneManager.keepTrackOfRecentScenes;
                RecentSceneManager.keepTrackOfRecentScenes =
                    EditorGUILayout.IntSlider(content, RecentSceneManager.keepTrackOfRecentScenes, 2, 20);
                if (wasTrack != RecentSceneManager.keepTrackOfRecentScenes)
                {
                    InitializeSceneManager.SetEditorPrefInt(InitializeSceneManager.TRACKEDCOUNT,
                        RecentSceneManager.keepTrackOfRecentScenes);
                    while (RecentSceneManager.recentScenes.Count > RecentSceneManager.keepTrackOfRecentScenes)
                    {
                        var last = RecentSceneManager.recentScenes[RecentSceneManager.recentScenes.Count - 1];
                        if (RecentSceneManager.pathAndScene.ContainsKey(last))
                        {
                            var scene = RecentSceneManager.pathAndScene[last];
                            RecentSceneManager.pathAndScene.Remove(last);
                            scene.ClearImage();
                        }

                        InitializeSceneManager.SetEditorPrefString(
                            $"{Application.productName}{RECENTSCENES}{RecentSceneManager.recentScenes.Count - 1}",
                            string.Empty);

                        RecentSceneManager.recentScenes.Remove(last);
                    }

                    _window.OnGUI();
                }

                EditorGUILayout.Space();

                var trackScenes = RecentSceneManager.trackScenePreview;
                RecentSceneManager.trackScenePreview =
                    EditorGUILayout.Toggle("Track Scene Previews", RecentSceneManager.trackScenePreview);
                if (trackScenes != RecentSceneManager.trackScenePreview)
                {
                    InitializeSceneManager.SetEditorPrefBool(InitializeSceneManager.TRACKSCENEPREVIEW,
                        RecentSceneManager.trackScenePreview);
                }

                var trackEdited = RecentSceneManager.trackEditedBy;
                RecentSceneManager.trackEditedBy =
                    EditorGUILayout.Toggle("Track Edited By", RecentSceneManager.trackEditedBy);
                if (trackEdited != RecentSceneManager.trackEditedBy)
                {
                    InitializeSceneManager.SetEditorPrefBool(InitializeSceneManager.TRACKEDITEDBY,
                        RecentSceneManager.trackEditedBy);
                }

                EditorGUILayout.Space();

                var close = RecentSceneManager.closeWindowOnLoad;
                RecentSceneManager.closeWindowOnLoad =
                    EditorGUILayout.ToggleLeft("Close Window On Load", RecentSceneManager.closeWindowOnLoad);
                if (close != RecentSceneManager.closeWindowOnLoad)
                {
                    InitializeSceneManager.SetEditorPrefBool(InitializeSceneManager.CLOSEWINDOWONLOAD,
                        RecentSceneManager.closeWindowOnLoad);
                }

                EditorGUILayout.Space();
                EditorGUI.BeginDisabledGroup(RecentSceneManager.recentScenes.Count == 0);

                if (GUILayout.Button(CLEARRECENT))
                {
                    for (var j = 0; j < RecentSceneManager.recentScenes.Count; j++)
                    {
                        RecentSceneManager.recentScenes[j] = null;
                        InitializeSceneManager.RemoveKey(Application.productName + RECENTSCENES + j);
                    }
                }

                EditorGUI.EndDisabledGroup();

                EditorGUILayout.EndVertical();
            }
        }

        private class PreviewWindow : EditorWindow
        {
            private static Texture2D _preview;
            private const int _minSize = 256;

            internal static void Init(string title, Texture2D preview)
            {
                _previewWindow = GetWindow<PreviewWindow>(true, title);
                if (_previewWindow == null)
                {
                    return;
                }

                _previewWindow.titleContent.image = preview;
                _previewWindow.titleContent.text = title;
                _previewWindow.titleContent.tooltip = $"Preview of the {title} scene.";

                _preview = preview;

                var size = new Vector2(preview.width, preview.height) * .5f;
                _previewWindow.minSize = new Vector2(size.x < _minSize ? _minSize : size.x,
                    size.y < _minSize ? _minSize : size.y);
                var center = GetCenter(_previewWindow.minSize);
                _previewWindow.position = new Rect(center, _previewWindow.minSize);
                _previewWindow.Show();
            }

            private void OnGUI()
            {
                if (_preview == null)
                {
                    Close();
                }

                var box = EditorGUILayout.BeginVertical();

                var rect = new Rect(box.position, position.size);

                GUI.DrawTexture(rect, _preview, ScaleMode.ScaleToFit);

                EditorGUILayout.EndVertical();
            }
        }

        private class InfoWindow : EditorWindow
        {
            #region Const Strings

            private const string TITLE = "About Recent Scenes Manager";
            private const string THANKYOU = "Thank you for purchasing The Three Pupper Studios Recent Scene Manager!";
            private const string FUNCTIONHEADER = "To take full advantage of this tool:";
            private const string MESSAGE0 = "■ The scenes are listed and numbered in order of most recently used.";
            private const string MESSAGE1 = "■ Click the name of the scene to ping the scene asset.";
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
            private const string MESSAGE2 = "■ Alt + click the name of the scene to select the scene asset.";
#elif UNITY_STANDALONE_OSX
            private const string MESSAGE2 = "■ Option + click the name of the scene to select the scene asset.";

#endif
#if UNITY_STANDALONE_WIN || UNITY_STANDALONE_LINUX
            private const string MESSAGE3 = "■ Ctrl + click the name of the scene to select the scene asset.";
#elif UNITY_STANDALONE_OSX
            private const string MESSAGE3 = "■ Command + click the name of the scene to select the scene asset.";

#endif
            private const string MESSAGE4 =
                "■ Click the \"Load Single\" button to remove all currently open scenes and open the selected scene. ";

            private const string MESSAGE5 =
                "■ Click the \"Load Additive\" button to additively add the selected scene.";

            private const string MESSAGE6 =
                "■ Click the ► icon to open additional information about the selected scene.";

            private const string MESSAGE7 =
                "■ Click the \"Preview\" button in the additional information area to load a preview of the selected scene.(This button will only show if the \"Track Scene Previews\" option is enabled in the settings menu)";

            private const string MESSAGE8 =
                "■ Click the Refresh icon button in the additional information area to load a preview of the selected scene.(This button will only show if the selected scene is currently loaded)";

            private const string MESSAGE9 =
                "■ Click the Gear/Settings icon in the top right to open the settings menu.";

            private const string MESSAGE10 =
                "■ Adjust the \"Max Tracked Scenes\" slider in the settings menu to change the number of scenes the Recent Scenes Manager will keep track of.";

            private const string MESSAGE11 =
                "■ Enabling/Disabling the \"Track Scene Previews\" option in the settings menu will determine if the Recent Scenes Manager will capture scene previews and be able to display them.";

            private const string MESSAGE12 =
                "■ Enabling/Disabling the \"Track Edited By\" option in the settings menu will determine if the Recent Scenes Manager will store the EditedBy asset in your project in order to record and display the name of the account last used to make changes to the scene.";

            private const string MESSAGE13 =
                "■ Enabling/Disabling the \"Close Window On Load\" option in the settings menu will determine if the Recent Scenes Manager windows will automatically close or not upon loading a scene from the manager.";

            private const string MESSAGE14 =
                "■ Click the \"Clear Recent Scenes\" button to reset the Recent Scenes Manager clearing the tracked settings.";

            private const string FINALMESSAGE =
                "© 2020 ThreePupperStudios\nThe Recent Scenes Manager is developed by Ben Ricks";

            #endregion

            private static readonly Vector2 windowSize = new Vector2(395, 695);
            private static readonly Vector2 maxWindowSize = new Vector2(395, 1000);

            internal static InfoWindow InfoInit()
            {
                var window = GetWindow<InfoWindow>(true, TITLE, true);
                window.titleContent.image = ToolIcon;
                window.maxSize = maxWindowSize;
                window.minSize = windowSize;
                window.position = new Rect(GetCenter(windowSize), windowSize);

                return window;
            }

            private void OnGUI()
            {
                EditorGUILayout.BeginVertical(BOX);

                EditorGUILayout.BeginHorizontal();
                GUILayout.Label(ToolIcon, GUILayout.Height(45), GUILayout.Width(45));
                var titleStyle = new GUIStyle(GUIStyle.none)
                {
                    padding = new RectOffset(0, 0, 10, 0),
                    fontSize = 12,
                    fontStyle = FontStyle.Bold,
                    wordWrap = true
                };

                GUILayout.Label(THANKYOU, titleStyle);
                EditorGUILayout.EndHorizontal();
                EditorGUILayout.Separator();
                var headerStyle = new GUIStyle(GUIStyle.none)
                {
                    padding = new RectOffset(3, 0, 3, 0),
                    fontStyle = FontStyle.Bold,
                    alignment = TextAnchor.MiddleCenter,
                    wordWrap = true
                };
                GUILayout.Label(FUNCTIONHEADER, headerStyle);
                var messageStyle = new GUIStyle(EditorStyles.label)
                {
                    padding = new RectOffset(3, 0, 5, 0),
                    wordWrap = true,
                };
                GUILayout.Space(3);
                GUILayout.Label(MESSAGE0, messageStyle);
                GUILayout.Label(MESSAGE1, messageStyle);
                GUILayout.Label(MESSAGE2, messageStyle);
                GUILayout.Label(MESSAGE3, messageStyle);
                GUILayout.Label(MESSAGE4, messageStyle);
                GUILayout.Label(MESSAGE5, messageStyle);
                GUILayout.Label(MESSAGE6, messageStyle);
                GUILayout.Label(MESSAGE7, messageStyle);
                GUILayout.Label(MESSAGE8, messageStyle);
                GUILayout.Label(MESSAGE9, messageStyle);
                GUILayout.Label(MESSAGE10, messageStyle);
                GUILayout.Label(MESSAGE11, messageStyle);
                GUILayout.Label(MESSAGE12, messageStyle);
                GUILayout.Label(MESSAGE13, messageStyle);
                GUILayout.Label(MESSAGE14, messageStyle);


                var finalMessageStyle = new GUIStyle(EditorStyles.label)
                {
                    padding = new RectOffset(3, 0, 5, 0),
                    wordWrap = true,
                    fontSize = 10,
                    fontStyle = FontStyle.Italic,
                    alignment = TextAnchor.LowerCenter,
                };

                EditorGUILayout.LabelField(FINALMESSAGE, finalMessageStyle);//*/

                EditorGUILayout.EndVertical();
            }
        }
    }
}