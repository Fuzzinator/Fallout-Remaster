using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Human
{
    #region Variables And Properties
    public static Player Instance { get; private set; }
    
    private Coroutine _playerMoving;

    #endregion

    #region MonoBehaviours
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
        GameManager.InputManager.Player.PrimaryClick.performed += PrimaryClickHandler;
    }

    private void OnDestroy()
    {
        GameManager.InputManager.Player.PrimaryClick.performed -= PrimaryClickHandler;
    }
    #if UNITY_EDITOR
    private void OnDrawGizmos()
    {
        if (_hexMaker == null)
        {
            _hexMaker = FindObjectOfType<HexMaker>(false);
        }

        if (_hexMaker == null)
        {
            return;
        }
        
        if (_currentLocation > -1 && _currentLocation < _hexMaker.Coords.Count)
        {
            Gizmos.color = Color.blue;
            Gizmos.DrawWireSphere(_hexMaker.Coords[_currentLocation].pos, .46f);
            Gizmos.color = Color.white;
        }
    }
    #endif
    #endregion
    private void PrimaryClickHandler(InputAction.CallbackContext obj)
    {
        if (_hexMaker == null)
        {
            _hexMaker = HexMaker.Instance;
        }

        var highlighter = HexHighlighter.Instance;

        if (highlighter != null && highlighter.HoveredCoord != null)
        {
            var coords = _hexMaker.TryGetCoordinates();

            if (coords != null && coords.walkable && !coords.occupied && coords.distance >= 0)
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
        
        if (_hexMaker == null)
        {
            _hexMaker = HexMaker.Instance;
        }

        var pathToTake = _hexMaker.PathToTarget.ToArray();
        var count = pathToTake.Length;
        for (var i = 0; i < count; i++)
        {
            var pos = pathToTake[i];
            var coord = _hexMaker.Coords[pos];
            var currentPos = transform.position;
            for (var f = 0f; f < 1; f += _moveSpeed * Time.deltaTime)
            {
                transform.position = Vector3.Lerp(currentPos, coord.pos, f);
                yield return null;
            }

            LeaveCoordinate();

            transform.position = coord.pos;

            EnterCoordinate(coord);
        }
    }
}