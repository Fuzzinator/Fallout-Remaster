using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static InputManager _inputManager;

    public static InputManager InputManager
    {
        get { return _inputManager ??= new InputManager(); }
    }
}
