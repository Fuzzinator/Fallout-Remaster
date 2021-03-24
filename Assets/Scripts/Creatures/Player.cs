using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Human
{
    #region Variables And Properties

    public static Player Instance { get; private set; }
    

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
        GameManager.InputManager.Player.Enable();
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

        if (!HasValidPath)
        {
            return;
        }
        
        var highlighter = HexHighlighter.Instance;

        if (highlighter == null || highlighter.HoveredCoord == null)
        {
            return;
        }
        
        var coords = highlighter.HoveredCoord;

        if (coords == null || !coords.walkable || coords.occupied || coords.distance < 0)
        {
            return;
        }
        
        if (_isMoving != null)
        {
            StopCoroutine(_isMoving);
        }

        _isMoving = StartCoroutine(MoveCreature());
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


}