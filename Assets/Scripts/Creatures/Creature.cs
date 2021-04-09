using System;
using System.Collections;
using System.Collections.Generic;
using ThreePupperStudios.Lockable;
using UnityEngine;
using Random = UnityEngine.Random;

public class Creature : MonoBehaviour, IOccupier
{
    #region Variables and Properties

    [SerializeField]
    protected string _name;
    public string Name => _name;
    
    [SerializeField]
    protected int _currentHealth; //cave rats have 6hp

    [SerializeField]
    protected int _currentAP;

    [SerializeField]
    protected SPECIAL _special;

    [SerializeField] 
    protected Skills _skills;

    [SerializeField]
    protected Weapon _activeWeapon;

    [SerializeField]
    protected Armor _equipedArmor;
    
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

    [SerializeField]
    protected int _xPValue = 0;
    
    [SerializeField, Lockable(rememberSelection:false)]
    protected int _hpIncrease = 0;

    [SerializeField]
    protected int _baseHealth = 0;
    
    protected int _apToAC = 0;
    
    #region Lists

    

    #endregion
    
    #region Properties

    public int CurrentLocation => _currentLocation;
    protected bool HasValidPath => TargetPath != null && TargetPath.Count > 0;
    
    public float MoveSpeed => _baseMoveSpeed * _speedModifier;
    protected int BaseHealth => _baseHealth + _special.Strength + (2 * _special.Endurance);
    public int MaxHealth => BaseHealth + _hpIncrease;
    public virtual int MaxActionPoints => Mathf.FloorToInt(_special.Agility * .5f) + 3;
    public virtual int ArmorClass => _special.Agility + ACMod();
    public virtual int MeleeDamage => _special.Strength > 5 ? _special.Strength - 5 : 1;
    public virtual int RadResistance => _special.Endurance * 2;
    public virtual int Sequence => _special.Perception * 2;
    public virtual int HealingRate => Mathf.CeilToInt(_special.Endurance * .3f);

    public virtual int CriticalChance => _special.Luck;

    #endregion

    #region Constants
    

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

    protected virtual IEnumerator MoveCreature()
    {
        yield return null;

        if (_hexMaker == null)
        {
            _hexMaker = HexMaker.Instance;
        }

        var pathToTake = TargetPath.ToArray();
        TargetPath.Clear();
        var count = pathToTake.Length;
        for (var i = 0; i < count; i++)
        {
            var coord = pathToTake[i];
            if (coord.index == _currentLocation)
            {
                continue;
            }

            if (CombatManager.Instance.CombatMode)
            {
                var willMove = TryDecrementAP(1, ActionType.Move);
                if (!willMove)
                {
                    break;
                }
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

    protected virtual bool TryDecrementAP(int cost, ActionType type)
    {
        if (cost > _currentAP)
        {
            return false;
        }

        _currentAP -= cost;
        
        return true;
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

    public virtual void SetHP(int value)//Temporary probably
    {
        _currentHealth = value;
    }

    #region Combat

    public virtual void InitiateCombat()
    {
        CombatManager.startTurn += TryStartTurn;
        CombatManager.StartCombat(this);
    }

    protected void TryStartTurn(Creature creature)
    {
        if (creature == this)
        {
            StartTurn();
        }
    }

    public virtual void StartTurn()
    {
        _currentAP = MaxActionPoints;
        _apToAC = 0;
    }

    public virtual void EndTurn()
    {
        _apToAC = _currentAP;
        CombatManager.ProgressCombat();
    }

    protected virtual int RandomHit()
    {
        return Random.Range(1, 100);
    }
    
    protected virtual int ChanceToHit(int randomVal)
    {
        var chanceToHit = 50;//temp

        chanceToHit -= randomVal;
        
        return chanceToHit;
    }
    protected virtual int GetCriticalChance(int chanceToHit)
    {
        var critChance = CriticalChance;//just matching original math used
        critChance += Mathf.FloorToInt(chanceToHit * .1f);
        return critChance;
    }
    protected virtual int ACMod()
    {
        var ac = 0;
        if (_equipedArmor != null)
        {
            ac += _equipedArmor.ArmorClass;
        }

        ac += _apToAC;

        return ac;
    }
    protected virtual int DamageResistance(DamageType damageType)
    {
        var resistance = 0;
        
        if (_equipedArmor != null)
        {
            resistance += _equipedArmor.GetResistance(damageType);
        }
        
        return resistance;
    }

    protected virtual int DamageThreshold(DamageType damageType)
    {
        var threshold = 0;
        
        if (_equipedArmor != null)
        {
            threshold += _equipedArmor.GetThreshold(damageType);
        }
        
        return threshold;
    }

    #endregion
    
    #region Enums

    public enum Gender
    {
        Male,
        Female,
        NonBinary//This is not in the original
    }

    public enum ActionType
    {
        None = 0,
        ObjectInteraction,
        Attack,
        Move,
        
    }
    #endregion
}