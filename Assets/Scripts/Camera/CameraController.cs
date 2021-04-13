using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
    public static CameraController Instance { get; private set; }
    [SerializeField]
    private Camera _camera;

    public Camera TargetCamera => _camera;

    [SerializeField]
    private float _moveSpeed = 5;

    [SerializeField]
    private UpdateWindowShader _shaderUpdater;

    private int _borderSize = 10;

    [SerializeField]
    private Vector4 _positionLimits = Vector4.zero;

    private Vector4 _actualPosLimits = Vector4.zero;

    //I will eventually add scrolling in and out and rotating around the map
    //I'll use this as a reference https://catlikecoding.com/unity/tutorials/hex-map/part-5/

    private Coroutine _movingCamera;
    private bool _moveCamera;

    private void OnValidate()
    {
        if (_camera == null)
        {
            TryGetComponent(out _camera);
        }

        UpdatePosLimits();
    }

    private void OnDisable()
    {
        _movingCamera = null;
        _moveCamera = false;
    }

    private void OnDestroy()
    {
        GameManager.InputManager.Player.Look.performed -= LookHandler;
        GameManager.InputManager.Player.ScrollWheel.performed -= ScrollWheelHandler;
    }

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
        UpdatePosLimits();
        GameManager.InputManager.Player.Enable();
        GameManager.InputManager.Player.Look.performed += LookHandler;
        GameManager.InputManager.Player.ScrollWheel.performed += ScrollWheelHandler;
    }


    private void UpdatePosLimits()
    {
        var height = _camera.orthographicSize;
        var width = (_camera.aspect - 1) * height; //height * _camera.aspect;
        _actualPosLimits = new Vector4(_positionLimits.x - width, _positionLimits.y, _positionLimits.z + width,
            _positionLimits.w);
    }

    private void LookHandler(InputAction.CallbackContext obj)
    {
        CheckCursorPos(Mouse.current.position.ReadValue());
    }

    private void ScrollWheelHandler(InputAction.CallbackContext obj)
    {
    }

    private void CheckCursorPos(Vector2 cursorPos)
    {
        var cursorRight = cursorPos.x >= Screen.width - _borderSize;
        var cursorLeft = cursorPos.x <= _borderSize;
        var cursorUp = cursorPos.y >= Screen.height - _borderSize;
        var cursorDown = cursorPos.y <= _borderSize;

        if (cursorRight || cursorLeft || cursorUp || cursorDown)
        {
            if (!_moveCamera)
            {
                _moveCamera = true;
                _movingCamera ??= StartCoroutine(TryMoveCamera());
            }
        }
        else
        {
            if (_moveCamera)
            {
                CursorController.ResetCursorState();
            }
            _moveCamera = false;
            if (_movingCamera != null)
            {
                StopCoroutine(_movingCamera);
                _movingCamera = null;
            }
        }
    }

    private IEnumerator TryMoveCamera()
    {
        yield return null;

        while (_moveCamera)
        {
            var screenCenter = new Vector2(Screen.width, Screen.height) * .5f;
            var mousePos = Mouse.current.position.ReadValue();
            var dir = (mousePos - screenCenter).normalized;
            var origPos = transform.localPosition;
            var newPos = origPos + (Vector3) (dir * (_moveSpeed * Time.deltaTime));
            var cantLeft = newPos.x > _actualPosLimits.x;
            var cantRight = newPos.x < _actualPosLimits.z;
            if (cantLeft || cantRight)
            {
                newPos.x = origPos.x;
            }

            var cantUp = newPos.y > _actualPosLimits.y;
            var cantDown = newPos.y < _actualPosLimits.w;
            if (cantUp || cantDown)
            {
                newPos.y = origPos.y;
            }

            transform.localPosition = newPos;
            yield return null;
            var cursorRight = mousePos.x >= Screen.width - _borderSize;
            var cursorLeft = mousePos.x <= _borderSize;
            var cursorUp = mousePos.y >= Screen.height - _borderSize;
            var cursorDown = mousePos.y <= _borderSize;

            _moveCamera = cursorRight || cursorLeft || cursorUp || cursorDown;

            _shaderUpdater.UpdateShaders();
            TryUpdateCursor(cursorRight, cursorLeft, cursorUp, cursorDown, cantLeft, cantRight, cantUp, cantDown);
        }

        _moveCamera = false;
        _movingCamera = null;
    }

    private void TryUpdateCursor(bool moveL, bool moveR, bool moveU, bool moveD, bool cantL, bool cantR, bool cantU, bool cantD)
    {
        if (moveL)
        {
            if (moveD)
            {
                //leftDown
                if (cantL && cantD)
                {
                    //Cant Move Left and Down
                    CursorController.SetState(CursorController.CursorState.CantMoveCameraDownLeft);
                }
                else
                {
                    //Can Move Left and Down
                    CursorController.SetState(CursorController.CursorState.MoveCameraDownLeft);
                }
            }
            else if (moveU)
            {
                //leftUp
                if (cantL && cantU)
                {
                    //Cant Move Left and Up
                    CursorController.SetState(CursorController.CursorState.CantMoveCameraUpLeft);
                }
                else
                {
                    //Can Move Left and Up
                    CursorController.SetState(CursorController.CursorState.MoveCameraUpLeft);
                }
            }
            else
            {
                //JustLeft
                if (cantL)
                {
                    //Cant Move Left
                    CursorController.SetState(CursorController.CursorState.CantMoveCameraLeft);
                }
                else
                {
                    //Can Move Left
                    CursorController.SetState(CursorController.CursorState.MoveCameraLeft);
                }
            }
        }
        else if (moveR)
        {
            if (moveD)
            {
                //RightDown
                if (cantR && cantD)
                {
                    //Cant Move Right and Down
                    CursorController.SetState(CursorController.CursorState.CantMoveCameraDownRight);
                }
                else
                {
                    //Can Move Right and Down
                    CursorController.SetState(CursorController.CursorState.MoveCameraDownRight);
                }
            }
            else if (moveU)
            {
                //RightUp
                if (cantR && cantU)
                {
                    //Cant Move Right and Up
                    CursorController.SetState(CursorController.CursorState.CantMoveCameraUpRight);
                }
                else
                {
                    //Can Move Right and Up
                    CursorController.SetState(CursorController.CursorState.MoveCameraUpRight);
                }
            }
            else
            {
                //JustRight
                if (cantR)
                {
                    //Cant Move Right
                    CursorController.SetState(CursorController.CursorState.CantMoveCameraRight);
                }
                else
                {
                    //Can Move Right
                    CursorController.SetState(CursorController.CursorState.MoveCameraRight);
                }
            }
        }
        else
        {
            if (moveD)
            {
                //JustDown
                if (cantD)
                {
                    //Cant Move Down
                    CursorController.SetState(CursorController.CursorState.CantMoveCameraDown);
                }
                else
                {
                    //Can Move Down
                    CursorController.SetState(CursorController.CursorState.MoveCameraDown);
                }
            }
            else if (moveU)
            {
                //JustUp
                if (cantU)
                {
                    //Cant Move Up
                    CursorController.SetState(CursorController.CursorState.CantMoveCameraUp);
                }
                else
                {
                    //Can Move Up
                    CursorController.SetState(CursorController.CursorState.MoveCameraUp);
                }
            }
            else
            {
                CursorController.ResetCursorState();
            }
        }
    }
}