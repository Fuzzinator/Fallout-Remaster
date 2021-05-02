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

    [SerializeField, Range(0, 360)]
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
        if (!verts.Contains(bot))
        {
            verts.Add(bot);
        }

        if (!verts.Contains(top))
        {
            verts.Add(top);
        }

        var minBot = bot + (minDegree * _viewDistance);
        var minTop = minBot + Vector3.up;

        if (!verts.Contains(minBot))
        {
            verts.Add(minBot);
        }

        if (!verts.Contains(minTop))
        {
            verts.Add(minTop);
        }

        var midBot = bot + (transform.forward * _viewDistance);
        var midTop = midBot + Vector3.up;

        if (!verts.Contains(midBot))
        {
            verts.Add(midBot);
        }

        if (!verts.Contains(midTop))
        {
            verts.Add(midTop);
        }

        var points = Mathf.RoundToInt(_viewDistance * .5f * Mathf.Max(_fov / 90, 1));
        var angleIncs = 0f;
        if (points > 0)
        {
            angleIncs = _fov / points;
        }

        for (var i = 1; i < points; i++)
        {
            var angle = angleIncs * i;
            var pointRad = angle * (Mathf.PI / 360);
            var pointDeg = new Vector3(Mathf.Sin(pointRad), 0, -Mathf.Cos(pointRad));
            var minPointDeg = new Vector3(Mathf.Sin(-pointRad), 0, -Mathf.Cos(-pointRad));
            var minPoint1 = bot + (pointDeg * _viewDistance);
            var maxPoint1 = minPoint1 + Vector3.up;

            var minPoint2 = bot + (minPointDeg * _viewDistance);
            var maxPoint2 = minPoint2 + Vector3.up;

            if (!verts.Contains(minPoint1))
            {
                verts.Add(minPoint1);
            }

            if (!verts.Contains(maxPoint1))
            {
                verts.Add(maxPoint1);
            }

            if (!verts.Contains(minPoint2))
            {
                verts.Add(minPoint2);
            }

            if (!verts.Contains(maxPoint2))
            {
                verts.Add(maxPoint2);
            }
        }


        var maxBot = bot + (maxDegree * _viewDistance);
        var maxTop = maxBot + Vector3.up;

        if (!verts.Contains(maxBot))
        {
            verts.Add(maxBot);
        }

        if (!verts.Contains(maxTop))
        {
            verts.Add(maxTop);
        }

        return verts.ToArray();
    }
}