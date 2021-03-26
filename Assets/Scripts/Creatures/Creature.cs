using System;
using System.Collections;
using System.Collections.Generic;
using ThreePupperStudios.Lockable;
using UnityEngine;

public class Creature : MonoBehaviour, IOccupier
{
    #region Variables and Properties

    [SerializeField]
    protected string _name;
    
    [SerializeField]
    protected int _currentHealth;

    [SerializeField]
    protected SPECIAL _special;

    [SerializeField] 
    protected Skills _skills;
    
    [SerializeField]
    protected int _currentLocation;

    protected HexMaker _hexMaker;

    [SerializeField] 
    protected HexDir _facingDir;

    protected float _speedModifier = 1f;
    [SerializeField] 
    protected float _baseMoveSpeed = 5;

    [SerializeField, Lockable] 
    protected float _rotationSpeed = 10;

    protected Coroutine _isMoving;

    public readonly List<Coordinates> TargetPath = new List<Coordinates>();
    public readonly Coroutine GettingPath = null;
    
    [SerializeField, Lockable(rememberSelection:false)]
    protected int _hpIncrease = 0;
    
    #region Lists

    

    #endregion
    
    #region Properties

    protected float MoveSpeed => _baseMoveSpeed * _speedModifier;
    public int CurrentLocation => _currentLocation;
    protected bool HasValidPath => TargetPath != null && TargetPath.Count > 0;
    protected int BaseHealth => BASEHEALTH + _special.Strength + (2 * _special.Endurance);
    protected int MaxHealth => BaseHealth + _hpIncrease;

    #endregion

    #region Constants
    private const int BASEHEALTH = 15;
    

    #endregion

    #endregion

    private void Start()
    {
        if (HexMaker.Instance?.Coords != null)
        {
            var coords = HexMaker.Instance.Coords;
            if (_currentLocation > -1 && _currentLocation < coords.Count)
            {
                var coord = coords[_currentLocation];
                EnterCoordinate(coord);
                //coord.occupied = true;
                transform.position = coord.pos;
                var neighbor = coord.GetNeighbor((int) _facingDir);
                var targetRot = GetTargetRotation(coord, coords[neighbor.index], out _facingDir);
                transform.rotation = targetRot;
            }
        }
    }

    public virtual void EnterCoordinate(Coordinates coord)
    {
        if (coord.occupied)
        {
            Debug.LogWarning("Creature is entering occupied space. This shouldn't be happening.");
            return;
        }

        coord.occupied = true;
        coord.occupyingObject = this;
        _currentLocation = coord.index;
    }

    public virtual void LeaveCoordinate()
    {
        if (_hexMaker == null)
        {
            _hexMaker = HexMaker.Instance;
        }

        if (_hexMaker != null && _currentLocation > 0 && _currentLocation < _hexMaker.Coords.Count)
        {
            var currentCoord = _hexMaker.Coords[_currentLocation];
            if (!currentCoord.occupied)
            {
                Debug.LogWarning("Creature is leaving unoccupied space. This shouldn't be happening.");
            }

            if (currentCoord.occupyingObject as Player == this)
            {
                currentCoord.occupied = false;
                currentCoord.occupyingObject = null;
            }
            else
            {
                Debug.LogWarning("Creature leaving space they did not occupy. This shouldn't be happening.");
            }
        }
    }

    protected IEnumerator MoveCreature()
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
                yield return null;
            }

            LeaveCoordinate();

            transform.position = coord.pos;

            EnterCoordinate(coord);
            _facingDir = targetDir;
        }
    }

    protected Quaternion GetTargetRotation(Coordinates currentCoord, Coordinates targetCoord, out HexDir targetDir)
    {
        var targetRotation = Quaternion.LookRotation(transform.position - targetCoord.pos);

        var isNeighbor = currentCoord.CheckIfNeighbor(targetCoord.index, out targetDir);
        if (!isNeighbor)
        {
            Debug.LogWarning("The next hex is not a neighbor of the current one? Something fucked up.");
        }

        return targetRotation;
    }
}