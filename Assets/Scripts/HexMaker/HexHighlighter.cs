using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class HexHighlighter : MonoBehaviour
{
    #region Variables And Properties

    public static HexHighlighter Instance { get; private set; }

    [SerializeField]
    private MeshFilter _meshFilter;

    [SerializeField]
    private bool _hoverHighlight;

    [SerializeField, HideInInspector]
    private HexMaker _hexMaker;

    [SerializeField]
    private TextMeshProUGUI _textField;

    [SerializeField]
    private Transform _tempPlayer;

    private int _currentHoveredIndex = -1;

    private Action<Coordinates> _showDistance = null;


    public Coordinates HoveredCoord { get; private set; }

    private const string X = "x";

    public bool showHighlighter
    {
        set => gameObject.SetActive(value);
    }

    public Mesh SharedMesh
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

    #endregion

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        GameManager.InputManager.Player.Enable();

        if (_hoverHighlight)
        {
            GameManager.InputManager.Player.Look.performed += LookHandler;
            Cursor.visible = false;
        }

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
    }

    private void LookHandler(InputAction.CallbackContext obj)
    {
        if (_hoverHighlight)
        {
            if (_hexMaker != null)
            {
                HoveredCoord = _hexMaker.TryGetCoordinates(this);
            }
            else
            {
                Debug.LogError("HexHighlighter is missing HexMaker");
            }
        }
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
            var player = Player.Instance;
            if (player == null || _hexMaker == null)
            {
                return;
            }
            
            _hexMaker.GetDistanceToCoord(_hexMaker.Coords[player.CurrentLocation],coord, _showDistance);
        }
        else
        {
            _textField.SetText(X);
        }
    }
}