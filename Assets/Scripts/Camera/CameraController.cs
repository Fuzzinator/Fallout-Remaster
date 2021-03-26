using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class CameraController : MonoBehaviour
{
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
        if (cursorPos.x >= Screen.width - _borderSize || cursorPos.x <= _borderSize ||
            cursorPos.y >= Screen.height - _borderSize || cursorPos.y <= _borderSize)
        {
            if (!_moveCamera)
            {
                _moveCamera = true;
                _movingCamera ??= StartCoroutine(TryMoveCamera());
            }
        }
        else
        {
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

        var mousePos = Mouse.current.position.ReadValue();
        while (_moveCamera)
        {
            var screenCenter = new Vector2(Screen.width, Screen.height) * .5f;
            mousePos = Mouse.current.position.ReadValue();
            var dir = (mousePos - screenCenter).normalized;
            var origPos = transform.localPosition;
            var newPos = origPos + (Vector3) (dir * (_moveSpeed * Time.deltaTime));
            if (newPos.x > _actualPosLimits.x || newPos.x < _actualPosLimits.z)
            {
                newPos.x = origPos.x;
            }

            if (newPos.y > _actualPosLimits.y || newPos.y < _actualPosLimits.w)
            {
                newPos.y = origPos.y;
            }

            transform.localPosition = newPos;
            yield return null;
            _moveCamera = mousePos.x >= Screen.width - _borderSize || mousePos.x <= _borderSize ||
                          mousePos.y >= Screen.height - _borderSize || mousePos.y <= _borderSize;
            _shaderUpdater.UpdateShaders();
        }

        _moveCamera = false;
        _movingCamera = null;
    }
}