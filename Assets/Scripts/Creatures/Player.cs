using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class Player : Human
{
    #region Variables And Properties

    public static Player Instance { get; private set; }

    [SerializeField]
    private int _currentLvl = 1;

    [SerializeField]
    private int _unspentSkillPnts = 0;
    
    [SerializeField]
    private Trait _trait1;
    [SerializeField]
    private Trait _trait2;
    
    [SerializeField]
    private List<Perk> _activePerks = new List<Perk>();

    [SerializeField]
    private UpdateWindowShader _shaderUpdater;

    private int BaseHPIncrease => Mathf.FloorToInt(_special.Endurance * .5f) + 2;
    protected override int HealingRate => Mathf.CeilToInt(_special.Endurance * .3f)+HRModifiers();
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

    #region Movement
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
    protected override IEnumerator MoveCreature()
    {
        yield return null;

        if (_hexMaker == null)
        {
            _hexMaker = HexMaker.Instance;
        }

        var pathToTake = Player.Instance.TargetPath.ToArray();
        Player.Instance.TargetPath.Clear();
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
            var targetRotation = GetTargetRotation(currentCoord, coord, out var targetDir);

            var rotLerp = 0f;
            for (var f = 0f; f < 1; f += MoveSpeed * Time.deltaTime)
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
                _shaderUpdater.UpdateShaders();
                yield return null;
            }

            LeaveCoordinate();

            transform.position = coord.pos;

            EnterCoordinate(coord);
            _facingDir = targetDir;
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
    #endregion
    
    #region modifiers

    private int HRModifiers()
    {
        var healRateMod = 0;
        foreach (var perk in _activePerks)
        {
            if (perk.affectedProp == Perk.PropType.HPRecover)
            {
                healRateMod += perk.effectAmount;
            }
        }
        return healRateMod;
    }
    #endregion
}