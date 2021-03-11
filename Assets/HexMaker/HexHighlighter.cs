using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexHighlighter : MonoBehaviour
{
    [SerializeField]
    private MeshFilter _meshFilter;

    [SerializeField]
    private bool _hoverHighlight;

    [SerializeField]
    private bool _hoverOnClick;

    [SerializeField]
    private HexMaker _hexMaker;

    private Vector3 _lastMousePos = Vector3.zero;

    public Mesh sharedMesh
    {
        get => _meshFilter != null ? _meshFilter.sharedMesh : null;
        set
        {
            if (_meshFilter != null)
            {
                _meshFilter.sharedMesh = value;
            }
        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (_hoverHighlight && _lastMousePos != Input.mousePosition)
        {
            _lastMousePos = Input.mousePosition;
            if (_hexMaker != null && _hexMaker.UsesCollider)
            {
                _hexMaker.TryHighlightGrid();
            }
        }
        else if (_hoverOnClick && _hexMaker != null && _hexMaker.UsesCollider)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _hexMaker.TryHighlightGrid();
            }
        }
    }
}