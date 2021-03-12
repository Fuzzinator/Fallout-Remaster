using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
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

    [SerializeField]
    private TextMeshProUGUI _textField;

    private const string X = "x";
    
    public bool showHighlighter
    {
        set => gameObject.SetActive(value);
    }
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

    public void UpdateDisplay(HexMaker.Coordinates coord)
    {
        if (coord.walkable)
        {
            _textField.SetText(string.Empty);
        }
        else
        {
            _textField.SetText(X);
        }
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