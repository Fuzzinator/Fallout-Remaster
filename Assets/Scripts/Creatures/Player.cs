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

        if (!_hexMaker.HasValidPath)
        {
            return;
        }
        
        var highlighter = HexHighlighter.Instance;

        if (highlighter == null || highlighter.HoveredCoord == null)
        {
            return;
        }
        
        var coords = highlighter.HoveredCoord;//_hexMaker.TryGetCoordinates();

        if (coords == null || !coords.walkable || coords.occupied || coords.distance < 0)
        {
            return;
        }
        
        if (_playerMoving != null)
        {
            StopCoroutine(_playerMoving);
        }

        _playerMoving = StartCoroutine(MovePlayer());
    }

    private IEnumerator MovePlayer()
    {
        yield return null;

        if (_hexMaker == null)
        {
            _hexMaker = HexMaker.Instance;
        }

        var pathToTake = _hexMaker.PathToTarget.ToArray();
        _hexMaker.PathToTarget.Clear();
        var count = pathToTake.Length;
        for (var i = 0; i < count; i++)
        {
            var coord = pathToTake[i];
            if (coord.index == _currentLocation)
            {
                continue;
            }

            var t = transform;
            
            var currentPos = t.position;
            var currentCoord = _hexMaker.Coords[_currentLocation];

            var currentRot = t.rotation;
            var targetRotation = GetTargetRotation(currentCoord, coord);

            var rotLerp = 0f;
            for (var f = 0f; f < 1; f += _moveSpeed * Time.deltaTime)
            {
                if (rotLerp < 1)
                {
                    transform.rotation = Quaternion.Lerp(currentRot, targetRotation, rotLerp);
                    rotLerp += _rotationSpeed * Time.deltaTime;
                }
                else
                {
                    transform.rotation = targetRotation;
                }
                transform.position = Vector3.Lerp(currentPos, coord.pos, f);
                yield return null;
            }

            LeaveCoordinate();

            transform.position = coord.pos;

            EnterCoordinate(coord);
        }
    }

    public override void EnterCoordinate(Coordinates coord)
    {
        base.EnterCoordinate(coord);
        var hexHighlighter = HexHighlighter.Instance;
        if (hexHighlighter != null && hexHighlighter.HoveredCoord != null)
        {
            hexHighlighter.UpdateDisplay(coord, hexHighlighter.HoveredCoord);
        }
    }

    private Quaternion GetTargetRotation(Coordinates currentCoord, Coordinates coord)
    {
        var targetRotation = Quaternion.LookRotation(transform.position - coord.pos);

        return targetRotation;
    }
}