using System.Collections;
using System.Collections.Generic;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Serialization;

[ExecuteInEditMode]
public class Test : MonoBehaviour
{
    public Material mat;

    [FormerlySerializedAs("camera")] 
    public Camera cam;

    private static readonly int PlayerScreenPos = Shader.PropertyToID("_PlayerScreenPos");
    private static readonly int PlayerWorldPos = Shader.PropertyToID("_PlayerWorldPos");

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        var tPos = transform.position;
        var screenPos = cam.WorldToViewportPoint(tPos);
        mat.SetVector(PlayerScreenPos, screenPos);
        mat.SetVector(PlayerWorldPos, tPos);
    }
}
