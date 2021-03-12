using UnityEngine;

namespace ThreePupperStudios.RecentSceneManagement
{
    internal class RecentSceneData : ScriptableObject
    {
        [HideInInspector, SerializeField]
        internal string path;
        [HideInInspector, SerializeField]
        internal string lastEditedBy;
        [HideInInspector, SerializeField]
        internal string lastEditedDate;

        internal void Save()
        {
            UnityEditor.EditorUtility.SetDirty(this);
            UnityEditor.AssetDatabase.SaveAssets();
        }
    }
}