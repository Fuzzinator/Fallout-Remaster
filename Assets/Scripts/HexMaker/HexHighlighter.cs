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

    private int _currentHoveredIndex = -1;
    
    private Action<HexMaker.Coordinates> _showDistance = null;

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
        GameManager.InputManager.Player.PrimaryClick.performed += PrimaryClickHandler;
        
        _showDistance = (coord) =>
        {
            _textField.SetText(coord.distance <= 0 ? X : $"{coord.distance}");
        };
    }

    private void OnDestroy()
    {
        GameManager.InputManager.Player.Look.performed -= LookHandler;
        GameManager.InputManager.Player.PrimaryClick.performed -= PrimaryClickHandler;
    }

    public void UpdateDisplay(HexMaker.Coordinates coord)
    {
        if (_textField == null || _currentHoveredIndex == coord.index)
        {
            return;
        }
        
        _currentHoveredIndex = coord.index;
        if (coord.walkable)
        {
            _textField.SetText(string.Empty);
            _hexMaker.GetDistanceFromPlayer(coord, _showDistance);
        }
        else
        {
            _textField.SetText(X);
        }
    }

    private void LookHandler(InputAction.CallbackContext obj)
    {
        if (_hoverHighlight)
        {
            if (_hexMaker != null)
            {
                _hexMaker.TryHighlightGrid(this);
            }
            else
            {
                Debug.LogError("HexHighlighter is missing HexMaker");
            }
        }
    }

    private void PrimaryClickHandler(InputAction.CallbackContext obj)
    {
        if (_hoverOnClick && _hexMaker != null)
        {
            var coords = _hexMaker.TryHighlightGrid(this);
            
            if (coords.walkable && coords.distance>=0)
            {
                _hexMaker.IndexOfPlayerPos = coords.index;
            }
        }
    }
}