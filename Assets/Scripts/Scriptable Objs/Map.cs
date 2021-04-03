using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Scene = UnityEngine.SceneManagement.Scene;

public class Map : ScriptableObject
{
    [SerializeField]
    private string _mapName;
    public string MapName => _mapName;

    [SerializeField]
    private Scene _scene;
    public Scene SceneRef => _scene;
}
