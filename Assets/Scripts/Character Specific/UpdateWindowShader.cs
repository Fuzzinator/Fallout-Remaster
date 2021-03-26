using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class UpdateWindowShader : MonoBehaviour
{
    [SerializeField]
    private Camera _targetCam;
    [SerializeField]
    private List<Material> _targetMats = new List<Material>();
    
    private static readonly int PlayerScreenPos = Shader.PropertyToID("_PlayerScreenPos");
    private static readonly int PlayerWorldPos = Shader.PropertyToID("_PlayerWorldPos");
    
    // Start is called before the first frame update
    private void Start()
    {
        UpdateShaders();
    }

    public void UpdateShaders()
    {
        var tPos = transform.position;
        var screenPos = _targetCam.WorldToViewportPoint(tPos);
        
        foreach (var mat in _targetMats)
        {
            mat.SetVector(PlayerScreenPos, screenPos);
            mat.SetVector(PlayerWorldPos, tPos);
        }
    }
}
