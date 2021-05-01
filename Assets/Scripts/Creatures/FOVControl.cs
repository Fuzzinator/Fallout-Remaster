using System;
using System.Collections;
using System.Collections.Generic;
using ThreePupperStudios.Lockable;
using UnityEngine;

public class FOVControl : MonoBehaviour
{
    [SerializeField, Lockable]
    private Creature _creature;
    [SerializeField]
    private float _viewDistance;
    [SerializeField, Range(0,360)]
    private float _fov;

    [Space]
    [SerializeField, Lockable]
    private Collider _collider;
    [SerializeField, Lockable]
    private Mesh _mesh;

    [SerializeField]
    private bool _shouldUpdate;

    private const float maxEndDist = 5f;
    private const float maxAngle = 360f;
    
    private void OnValidate()
    {
        if (_creature == null)
        {
            _creature = GetComponentInParent<Creature>();
        }
        if (_shouldUpdate)
        {
            UpdateFOVMesh();
        }
    }

    private void OnDrawGizmos()
    {
        var verts = GetNewVerticies();
        var inc = 1f / verts.Length;
        var lerpVal = 0f;
        foreach (var vert in verts)
        {
            Gizmos.color = Color.Lerp(Color.red, Color.green, lerpVal);
            lerpVal += inc;
            Gizmos.DrawSphere(vert, .5f);
        }
    }

    private void UpdateFOVMesh()
    {
        var mesh = new Mesh();
        mesh.vertices = GetNewVerticies();
    }

    private Vector3[] GetNewVerticies()
    {
        var verts = new List<Vector3>();

        var radians = _fov * (Mathf.PI / 360);
        var minRads = -radians;
        var minDegree = new Vector3(Mathf.Sin(minRads), 0, -Mathf.Cos(minRads));
        var maxDegree = new Vector3(Mathf.Sin(radians), 0, -Mathf.Cos(radians));
        
        var bot = transform.position;
        var top = bot + Vector3.up;
        
        verts.Add(bot);
        verts.Add(top);
        
        var minBot = bot + (minDegree*_viewDistance);
        var minTop = minBot + Vector3.up;
        
        verts.Add(minBot);
        verts.Add(minTop);

        var points = Mathf.Max(Mathf.RoundToInt(_viewDistance * .5f), 1);
        var angleIncs = _fov / (points+1);
        
        for (var i = 0; i < points; i++)
        {
            var angle = angleIncs * i;
            var pointRad = angle * (Mathf.PI / 360);
            var pointDeg = new Vector3(Mathf.Sin(pointRad), 0, -Mathf.Cos(pointRad));
            var minPoint = bot + (pointDeg*_viewDistance);
            var maxPoint = minPoint + Vector3.up;
            verts.Add(minPoint);
            verts.Add(maxPoint);
        }

        var maxBot = bot + (maxDegree*_viewDistance);
        var maxTop = maxBot + Vector3.up;
        
        verts.Add(maxBot);
        verts.Add(maxTop);
        
        return verts.ToArray();
    }
}
