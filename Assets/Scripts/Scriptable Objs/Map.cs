using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scene = UnityEngine.SceneManagement.Scene;

[CreateAssetMenu(order = 13, fileName = "New Map", menuName = "ScriptObjs/Map")]
public class Map : ScriptableObject
{
    [SerializeField]
    private string _mapName;
    public string MapName => _mapName;

    [SerializeField]
    private Scene _scene;

    [SerializeField]
    private int _index;
    
    public Scene Scene => _scene;
    
#if UNITY_EDITOR
    public UnityEditor.SceneAsset sceneAsset;
    public void OnValidate()
    {
        var path = UnityEditor.AssetDatabase.GetAssetPath(sceneAsset);
        _scene = UnityEngine.SceneManagement.SceneManager.GetSceneByPath(path);
        _mapName = _scene.name;
        _index = _scene.buildIndex;
    }
#endif
}
