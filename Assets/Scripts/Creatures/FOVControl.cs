using System;
using System.Collections;
using System.Collections.Generic;
using ThreePupperStudios.Lockable;
using UnityEngine;

public class FOVControl : MonoBehaviour
{
    [Header("FOV Size and Generation")]
    [SerializeField]
    private float _viewDistance;

    [SerializeField, Range(0, 360)]
    private float _fov;

    [Space]
    [SerializeField, Lockable]
    private MeshCollider _collider;

    [SerializeField, Lockable]
    private Mesh _mesh;

    [SerializeField]
    private bool _shouldUpdate;

    [Space]
    [Header("Player Detection")]
    [SerializeField, Lockable]
    private Creature _creature;

    [SerializeField, Lockable]
    private BasicAI _ai;

    [SerializeField]
    private bool _canSeePlayer = false;

    private const float MAXANGLE = 360f;

    #region MeshCreation

    private void OnValidate()
    {
        if (_creature == null)
        {
            _creature = GetComponentInParent<Creature>();
        }

        if (_ai == null)
        {
            _ai = GetComponentInParent<BasicAI>();
        }

        if (_shouldUpdate)
        {
            UpdateFOVMesh();
        }
    }

    private void UpdateFOVMesh()
    {
        _mesh = new Mesh
        {
            vertices = GetNewVerticies(out var tris),
            triangles = tris,
            name = "FOV Mesh"
        };
        var hasMeshCollider = TryGetComponent(out _collider);
        if (!hasMeshCollider)
        {
            _collider = gameObject.AddComponent<MeshCollider>();
            _collider.convex = true;
            _collider.isTrigger = true;
        }

        _collider.sharedMesh = _mesh;
        _collider.sharedMesh.RecalculateNormals();

#if UNITY_EDITOR
        UnityEditor.AssetDatabase.CreateAsset(_mesh, $"Assets/Art/FOVMeshAssets/{_creature.Name} FOV.asset");
#endif
    }

    private Vector3[] GetNewVerticies(out int[] tris)
    {
        var verts = new List<Vector3>();
        var botTris = new List<int>();
        var topTris = new List<int>();

        if (_fov > MAXANGLE)
        {
            _fov = MAXANGLE;
        }
        else if (_fov < 0)
        {
            _fov = 0;
        }

        var radians = _fov * (Mathf.PI / 360);
        var minRads = -radians;
        var minDegree = new Vector3(Mathf.Sin(minRads), 0, -Mathf.Cos(minRads));
        var maxDegree = new Vector3(Mathf.Sin(radians), 0, -Mathf.Cos(radians));

        var bot = transform.localPosition;
        var top = bot + Vector3.up;
        if (!verts.Contains(bot))
        {
            verts.Add(bot);
        }

        if (!verts.Contains(top))
        {
            verts.Add(top);
        }

        var minBot = bot + (minDegree * _creature.MaxCanMoveDist);//_viewDistance);
        var minTop = minBot + Vector3.up;

        if (!verts.Contains(minBot))
        {
            verts.Add(minBot);
        }

        if (!verts.Contains(minTop))
        {
            verts.Add(minTop);
        }

        var midBot = bot + (transform.forward * _creature.MaxCanMoveDist);//_viewDistance);
        var midTop = midBot + Vector3.up;

        if (!verts.Contains(midBot))
        {
            verts.Add(midBot);
        }

        if (!verts.Contains(midTop))
        {
            verts.Add(midTop);
        }

        var points = Mathf.RoundToInt(_creature.MaxCanMoveDist/*_viewDistance*/ * .5f * Mathf.Max(_fov / 90, 1));
        var angleIncs = 0f;
        if (points > 0)
        {
            angleIncs = _fov / points;
        }

        var previousMinBot = midBot;
        var previousMinTop = midTop;
        var previousMaxBot = midBot;
        var previousMaxTop = midTop;
        for (var i = 1; i < points; i++)
        {
            var angle = angleIncs * i;
            var pointRad = angle * (Mathf.PI / 360);
            var pointDeg = new Vector3(Mathf.Sin(pointRad), 0, -Mathf.Cos(pointRad));
            var minPointDeg = new Vector3(Mathf.Sin(-pointRad), 0, -Mathf.Cos(-pointRad));
            var maxPointBot = bot + (pointDeg * _creature.MaxCanMoveDist);//_viewDistance);
            var maxPointTop = maxPointBot + Vector3.up;

            var minPointBot = bot + (minPointDeg * _creature.MaxCanMoveDist);//_viewDistance);
            var minPointTop = minPointBot + Vector3.up;

            if (!verts.Contains(maxPointBot))
            {
                verts.Add(maxPointBot);
            }

            botTris.Add(0);
            botTris.Add(verts.IndexOf(previousMaxBot));
            botTris.Add(verts.IndexOf(maxPointBot));
            previousMaxBot = maxPointBot;

            if (!verts.Contains(maxPointTop))
            {
                verts.Add(maxPointTop);
            }

            topTris.Add(1);
            topTris.Add(verts.IndexOf(previousMaxTop));
            topTris.Add(verts.IndexOf(maxPointTop));
            previousMaxTop = maxPointTop;

            if (!verts.Contains(minPointBot))
            {
                verts.Add(minPointBot);
            }

            botTris.Add(0);
            botTris.Add(verts.IndexOf(previousMinBot));
            botTris.Add(verts.IndexOf(minPointBot));
            previousMinBot = minPointBot;

            if (!verts.Contains(minPointTop))
            {
                verts.Add(minPointTop);
            }

            topTris.Add(1);
            topTris.Add(verts.IndexOf(previousMinTop));
            topTris.Add(verts.IndexOf(minPointTop));
            previousMinTop = minPointTop;
        }


        var maxBot = bot + (maxDegree * _creature.MaxCanMoveDist);//_viewDistance);
        var maxTop = maxBot + Vector3.up;

        if (!verts.Contains(maxBot))
        {
            verts.Add(maxBot);
        }

        botTris.Add(0);
        botTris.Add(verts.IndexOf(previousMaxBot));
        botTris.Add(verts.IndexOf(maxBot));

        botTris.Add(0);
        botTris.Add(verts.IndexOf(previousMinBot));
        botTris.Add(verts.IndexOf(minBot));

        if (!verts.Contains(maxTop))
        {
            verts.Add(maxTop);
        }

        topTris.Add(1);
        topTris.Add(verts.IndexOf(previousMaxTop));
        topTris.Add(verts.IndexOf(maxTop));

        topTris.Add(1);
        topTris.Add(verts.IndexOf(previousMinTop));
        topTris.Add(verts.IndexOf(minTop));

        var allTris = new List<int>();
        allTris.AddRange(botTris);
        allTris.AddRange(topTris);


        tris = allTris.ToArray();

        return verts.ToArray();
    }

    #endregion

    private void OnTriggerStay(Collider other)
    {
        if (_canSeePlayer)
        {
            return;
        }

        _canSeePlayer = CombatManager.ViewUnobscured(_creature, Player.Instance);
        if (_canSeePlayer)
        {
            _ai.DetectedEnemy(Player.Instance);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        _canSeePlayer = false;
    }
}