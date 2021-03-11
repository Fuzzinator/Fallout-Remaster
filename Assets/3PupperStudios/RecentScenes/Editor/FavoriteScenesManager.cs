using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace ThreePupperStudios.RecentSceneManagement
{
    internal static class FavoriteScenesManager
    {
        #region Const Strings

        internal const string FAVORITESCENES = "FavoriteScenes";
        internal const string ACTUALFAVORITEDSCENES = "ActualFavoritedScenes";
        internal const string FAVORITEDSCENESCOUNT = "FavoritedScenesCount";

        #endregion

        //internal static bool saveToProject; //I may use this in the future to allow favorites to be shared across multiple users/computers (like with git and such)
        internal static readonly List<string> favoritedScenes = new List<string>();

        internal static void AddToFavorites(string path)
        {
            if (favoritedScenes.Contains(path))
            {
                return;
            }

            favoritedScenes.Add(path);
            var saveName = $"{Application.productName}{FAVORITESCENES}{favoritedScenes.Count - 1}";
            InitializeSceneManager.SetEditorPrefString(saveName, path);
            InitializeSceneManager.SetEditorPrefInt($"{Application.productName}{FAVORITEDSCENESCOUNT}",
                favoritedScenes.Count);
            if (RecentSceneManager.pathAndScene.ContainsKey(path))
            {
                var sceneJson = EditorJsonUtility.ToJson(RecentSceneManager.pathAndScene[path]);
                if (!string.IsNullOrWhiteSpace(sceneJson))
                {
                    InitializeSceneManager.SetEditorPrefString(
                        $"{Application.productName}{ACTUALFAVORITEDSCENES}{favoritedScenes.Count - 1}", sceneJson);
                }
            }
            ShowRecentScenes.Window.RefreshGUI();
        }


        internal static void RemoveFromFavorites(string path)
        {
            var index = favoritedScenes.IndexOf(path);
            if (index < 0)
            {
                return;
            }

            favoritedScenes.Remove(path);
            for (var i = index; i < favoritedScenes.Count; i++)
            {
                var saveName = $"{Application.productName}{FAVORITESCENES}{i}";
                InitializeSceneManager.SetEditorPrefString(saveName, favoritedScenes[i]);
            }

            InitializeSceneManager.SetEditorPrefInt($"{Application.productName}{FAVORITEDSCENESCOUNT}",
                favoritedScenes.Count);
        }
    }

    internal static class ContextMenuStuff
    {
        [MenuItem("CONTEXT/SceneAsset/Favorite Scene", false, 10)]
        public static void ContextFavoriteScene(MenuCommand command)
        {
            FavoriteScene(command.context);
        }

        [MenuItem("Assets/Favorite Scene", false, 200)]
        public static void ProjectFavoriteScene()
        {
            FavoriteScene(Selection.activeObject);
        }

        private static void FavoriteScene(Object asset)
        {
            FavoriteScenesManager.AddToFavorites(AssetDatabase.GetAssetPath(asset));
        }

        #region ValidationFunctions

        [MenuItem("Assets/Favorite Scene", true, 200)]
        public static bool ProjectValidateFavoriteScene(MenuCommand command)
        {
            return Selection.activeObject is SceneAsset;
        }

        #endregion
    }
}