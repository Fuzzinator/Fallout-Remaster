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

    [SerializeField]
    private Transform _tempPlayer;

    private int _currentHoveredIndex = -1;
    
    private Action<Coordinates> _showDistance = null;

    private Coroutine _playerMoving;

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
            if (coord == null)
            {
                _textField.SetText(X);
            }
            else
            {
                _textField.SetText(coord.distance <= 0 ? X : $"{coord.distance}");
            }

        };
    }

    private void OnDestroy()
    {
        GameManager.InputManager.Player.Look.performed -= LookHandler;
        GameManager.InputManager.Player.PrimaryClick.performed -= PrimaryClickHandler;
    }

    public void UpdateDisplay(Coordinates coord)
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
            
            if (coords != null && coords.walkable && coords.distance>=0)
            {
                if (_playerMoving != null)
                {
                    StopCoroutine(_playerMoving);
                }
                _playerMoving = StartCoroutine(MovePlayer());
            }
        }
    }

    private IEnumerator MovePlayer()
    {
        yield return null;
        var pathToTake = _hexMaker.PathToTarget.ToArray();
        var count = pathToTake.Length;
        var halfASecond = new WaitForSeconds(.25f);
        for (var i = 0; i < count; i++)
        {
            var pos = pathToTake[i];
            var coord = _hexMaker.Coords[pos];
            _tempPlayer.position = coord.pos;
            _hexMaker.IndexOfPlayerPos = pos;
            yield return halfASecond;
        }
    }
}