using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CursorController : MonoBehaviour
{
    #region Static vars

    public static CursorController Instance { get; private set; }

    public static Action<CursorState> stateChanged;

    #endregion

    [SerializeField]
    private CursorState _currentState = CursorState.Movement;
    public CursorState CurrentState => _currentState;

    [Space]
    [SerializeField]
    private CursorState _overUIState = CursorState.None;
    [SerializeField]
    private CursorState _normalState = CursorState.None;

    [SerializeField]
    private bool _overUI;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    private void Start()
    {
        GameManager.InputManager.Player.SecondaryClick.performed += SecondaryClickHandler;
        CombatManager.stateChanged += CombatStateChanged;
    }

    private void SecondaryClickHandler(InputAction.CallbackContext context)
    {
        if (!_overUI)
        {
            var prevState = _currentState;
            switch (_currentState)
            {
                case CursorState.Movement:
                    _currentState++;
                    break;
                case CursorState.Interaction:
                    if (CombatManager.Instance.CombatMode)
                    {
                        _currentState++;
                    }
                    else
                    {
                        _currentState--;
                    }

                    break;
                case CursorState.Targeting:
                    _currentState = CursorState.Movement;
                    break;
            }

            if (_currentState < CursorState.MoveCameraUp && _currentState > CursorState.DownOverUI)
            {
                _normalState = _currentState;
            }
            if (prevState != _currentState)
            {
                stateChanged(_currentState);
            }
        }
    }

    private void CombatStateChanged(bool combat)
    {
        if (combat)
        {
            SetState(CursorState.Targeting);
        }
        else
        {
            SetState(CursorState.Movement);
        }
    }

    public static void SetState(CursorState state)
    {
        if (Instance == null)
        {
            Debug.LogWarning("No CursorController found in scene. Skipping settings state");
            return;
        }
        
        var prevState = Instance._currentState;
        Instance._currentState = state;
        if (Instance._overUI && state != CursorState.Targeting)
        {
            Instance._overUIState = state;
        }
        else if(state<CursorState.MoveCameraUp && state>CursorState.DownOverUI)
        {
            Instance._normalState = state;
        }

        if (prevState != state)
        {
            stateChanged(state);
        }
    }

    public static void ResetCursorState()
    {
        if (Instance == null)
        {
            Debug.LogWarning("No CursorController found in scene. Skipping settings state");
            return;
        }

        SetState(Instance._overUI ? Instance._overUIState : Instance._normalState);
    }

    public enum CursorState
    {
        None = 0,
        DefaultOverUI = 1,
        UpOverUI = 2,
        DownOverUI = 3,
        Movement = 5,
        Interaction = 6,
        Targeting = 7,
        Waiting = 8,
        MoveCameraUp = 10,
        MoveCameraDown = 11,
        MoveCameraLeft = 12,
        MoveCameraRight = 13,
        MoveCameraUpLeft = 14,
        MoveCameraUpRight = 15,
        MoveCameraDownLeft = 16,
        MoveCameraDownRight = 17,
        CantMoveCameraUp = 14,
        CantMoveCameraDown = 15,
        CantMoveCameraLeft = 16,
        CantMoveCameraRight = 17,
        CantMoveCameraUpLeft = 18,
        CantMoveCameraUpRight = 19,
        CantMoveCameraDownLeft = 20,
        CantMoveCameraDownRight = 21,
    }
}