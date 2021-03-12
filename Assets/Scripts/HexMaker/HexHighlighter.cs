using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

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

    private Vector2 _lastMousePos = Vector2.zero;
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

    private void Start()
    {
        GameManager.InputManager.Player.Enable();
        GameManager.InputManager.Player.Look.performed += LookHandler;
        
    }

    private void LookHandler(InputAction.CallbackContext obj)
    {
        if (_hexMaker != null)
        {
            _hexMaker.TryHighlightGrid();
        }
        else
        {
            Debug.LogError("HexHighlighter is missing HexMaker");
        }
        //}
        /*else if (_hoverOnClick && _hexMaker != null && _hexMaker.UsesCollider)
        {
            if (Input.GetMouseButtonDown(0))
            {
                _hexMaker.TryHighlightGrid();
            }
        }*/
    }
}