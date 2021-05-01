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

    [SerializeField]
    private bool _enabled = true;

    public Coordinates HoveredCoord { get; private set; }

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
            if (!CombatManager.Instance.CombatMode)
            {
                return;
            }

            if (coord == null || !_enabled)
            {
                _textField.SetText(X);
            }
            else
            {
                var maxDistance = int.MaxValue;
                if (CombatManager.Instance.CombatMode)
                {
                    maxDistance = Player.Instance.MaxMovement;
                }

                _textField.SetText(coord.distance <= 0 || coord.distance >= maxDistance ? X : $"{coord.distance}");
            }
        };

        CombatManager.stateChanged += CombatStateChanged;
        CursorController.stateChanged += CursorStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.InputManager.Player.Look.performed -= LookHandler;
    }

    public static void TryEnable()
    {
        if (CursorController.Instance == null ||
            CursorController.Instance.CurrentState != CursorController.CursorState.Movement)
        {
            return;
        }

        Enable();
    }

    private static void Enable()
    {
        if (Instance != null)
        {
            Instance._enabled = true;
        }

        Instance.showHighlighter = true;
    }

    public static void Disable()
    {
        if (Instance != null)
        {
            Instance._enabled = false;
            Instance.ClearText();
        }

        Instance.showHighlighter = false;
    }

    private void ClearText()
    {
        _textField.SetText(string.Empty);
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
            UpdateDisplay(player.Coord, HoveredCoord);
        }
    }

    public void UpdateDisplay(Coordinates sourceCoord, Coordinates targetCoord)
    {
        if (targetCoord == null || !_enabled ||
            (_currentHoveredIndex == targetCoord.index && _sourceCoord.index == sourceCoord.index))
        {
            return;
        }

        _sourceCoord = sourceCoord;
        _currentHoveredIndex = targetCoord.index;
        if (targetCoord.IsWalkable)
        {
            _textField.SetText(string.Empty);
            var player = Player.Instance;
            if (player == null)
            {
                return;
            }

            _hexMaker.GetDistanceToCoord(sourceCoord, targetCoord, player.TargetPath,
                _showDistance);
        }
        else
        {
            _textField.SetText(X);
        }
    }

    private void CombatStateChanged(bool combatStarted)
    {
        if (combatStarted)
        {
            Disable();
        }
        else
        {
            CursorController.SetState(CursorController.CursorState.Movement);
            /*Enable();*/
            ClearText();
        }
    }

    private void CursorStateChanged(CursorController.CursorState state)
    {
        var shouldMove = state == CursorController.CursorState.Movement;
        if (shouldMove)
        {
            Enable();
        }
        else
        {
            Disable();
        }
    }
}