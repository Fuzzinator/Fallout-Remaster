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

    [SerializeField, HideInInspector]
    private HexMaker _hexMaker;

    [SerializeField]
    private TextMeshProUGUI _textField;

    private int _currentHoveredIndex = -1;

    private Action<Coordinates> _showDistance = null;

    private Vector2 _lastMousePos;
    private bool _mouseMovedLastFrame = false;

    public Coordinates HoveredCoord { get; private set; }

    /*private Coroutine _waitToUpdate;

    private Coordinates _targetCoord;*/
    private Coordinates _sourceCoord;

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

        GameManager.InputManager.Player.Look.performed += LookHandler;
        LookHandler(new InputAction.CallbackContext());
        Cursor.visible = false;

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

        _mouseMovedLastFrame = false;
    }

    private void Update()
    {
        /*if (_lastMousePos != Mouse.current.position.ReadValue())
        {
            _mouseMovedLastFrame = true;
            _lastMousePos = Mouse.current.position.ReadValue();
        }
        else
        {
            _mouseMovedLastFrame = false;
        }*/
    }

    private void OnDestroy()
    {
        GameManager.InputManager.Player.Look.performed -= LookHandler;
    }

    private void LookHandler(InputAction.CallbackContext obj)
    {
        var newCoord = _hexMaker.TryGetCoordinates();
        if (HoveredCoord == newCoord)
        {
            return;
        }

        HoveredCoord = newCoord;

        if (HoveredCoord == null)
        {
            return;
        }

        transform.position = HoveredCoord.pos;

        var player = Player.Instance;
        if (player != null)
        {
            var sourceCoord = _hexMaker.Coords[player.CurrentLocation];
            UpdateDisplay(sourceCoord, HoveredCoord);
        }
    }

    /*public void UpdateDisplayLater(Coordinates sourceCoord, Coordinates targetCoord)
    {
        if (_waitToUpdate != null)
        {
            _sourceCoord = sourceCoord;
            _targetCoord = targetCoord;
            return;
        }

        _waitToUpdate = StartCoroutine(WaitThenUpdateDisplay(sourceCoord, targetCoord));
    }

    private IEnumerator WaitThenUpdateDisplay(Coordinates sourceCoord, Coordinates targetCoord)
    {
        yield return null;
        _textField.SetText(string.Empty);
        _hexMaker.PathToTarget.Clear();
        while (_mouseMovedLastFrame)
        {
            yield return null;
        }
        UpdateDisplay(_sourceCoord, _targetCoord);
        _waitToUpdate = null;
    }*/

    public void UpdateDisplay(Coordinates sourceCoord, Coordinates targetCoord)
    {
        if (targetCoord == null ||
            (_currentHoveredIndex == targetCoord.index && _sourceCoord.index == sourceCoord.index))
        {
            return;
        }

        _sourceCoord = sourceCoord;
        _currentHoveredIndex = targetCoord.index;
        if (targetCoord.walkable)
        {
            _textField.SetText(string.Empty);
            /*var player = Player.Instance;
            if (player == null || _hexMaker == null)
            {
                return;
            }*/

            _hexMaker.GetDistanceToCoord(sourceCoord, targetCoord, _showDistance);
        }
        else
        {
            _textField.SetText(X);
        }
    }
}