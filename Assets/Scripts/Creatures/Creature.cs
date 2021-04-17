using System;
using System.Collections;
using System.Collections.Generic;
using ThreePupperStudios.Lockable;
using UnityEngine;
using Random = UnityEngine.Random;

public class Creature : MonoBehaviour, IOccupier
{
    #region Variables and Properties

    [SerializeField] protected string _name;

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
    protected Weapon.AttackMode _activeWeaponMode;

    [SerializeField]
    protected bool _isAimedShot = false;

    [SerializeField]
    protected Armor _equipedArmor;

    [SerializeField]
    protected BasicAI _ai;

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

    [SerializeField]
    protected int _xPValue = 0;

    [SerializeField, Lockable(rememberSelection: false)]
    protected int _hpIncrease = 0;

    [SerializeField]
    protected int _baseHealth = 0;
#region Combat Targeting Things

    protected Creature _currentTarget;
    protected int _chanceHitTarget;
    protected string _messageToPrint;

  #endregion

    protected int _apToAC = 0;

    #region Lists

    #endregion

    #region Properties

    public bool Alive => _currentHealth > 0;

    public int CurrentLocation
    {
        get => _currentLocation;
        set => _currentLocation = value;
    }

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

    public virtual int MaxCanMoveDist => _currentAP;
    public virtual int CriticalChance => _special.Luck;

    public Creature TargetCreature => _currentTarget;
    public int ChanceToHitTarget => _chanceHitTarget;

    #endregion

    #region Constants

    private const string OUTOFRANGE = "Target out of range.";
    private const string TARGETBLOCKED = "Your aim is blocked.";
    private const string ATTACKMISSED = "'s attack missed";
    private const string NEEDMOREAP1 = "You need ";
    private const string NEEDMOREAP2 = " AP to attack";
    protected const int UNARMEDAPCOST = 3;

    #endregion

    #endregion

    private void Awake()
    {
        //I hate putting this in Awake (trying to reserve awake for singleton stuff) but this is the best I can think of right now.
        _currentAP = MaxActionPoints;
    }

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

        CombatManager.stateChanged += CombatStateChanged;
    }

    private void CombatStateChanged(bool isCombat)
    {
        if (isCombat)
        {
            StopAllCoroutines();
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

            if (currentCoord.occupyingObject as Creature == this)
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

    public virtual IEnumerator AIMoveCreature()
    {
        yield return MoveCreature();
    }

    protected virtual IEnumerator MoveCreature()
    {
        yield return null;

        _hexMaker = HexMaker.Instance;

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

            if (!coord.IsWalkable)
            {
                yield break;
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

    public virtual void SetHP(int value) //Temporary probably
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

    protected virtual bool TryGetTargetCreature(out Creature target) //TODO flesh this out
    {
        target = null;
        return false;
    }

    protected virtual void TryAttackCreature()
    {
        var apCost = UNARMEDAPCOST;
        if (_activeWeapon != null)
        {
            var weaponInfo = _activeWeapon.GetAttackTypeInfo(_activeWeaponMode);

            apCost = weaponInfo.ActionPointCost;
        }
        var canAttack = TryDecrementAP(apCost, ActionType.Attack);
        if (!canAttack)
        {
            _messageToPrint = $"{NEEDMOREAP1}{apCost}{NEEDMOREAP2}";
            return;
        }
        if (_chanceHitTarget > 0 && _currentTarget != null)
        {

            var randomVal = Random.Range(1, 100);
            var toHit = _chanceHitTarget - randomVal;
            if (toHit < 0) //Did the attack miss?
            {
                _messageToPrint = $"{_name}{ATTACKMISSED}";
                return;
            }
            ProcessAttack();
        }
    }

    protected virtual void ProcessAttack()
    {
        var baseDamage = MeleeDamage+Random.Range(1,2);
        if (_activeWeapon != null)
        {
            baseDamage = _activeWeapon.GetDamage();
            baseDamage *= _activeWeapon.CurrentAmmo.DamageMod;
        }
        
    }

    protected virtual int RandomHit()
    {
        return Random.Range(1, 100);
    }

    protected virtual int GetChanceToHit(int distance, Creature target)
    {
        var tPos = transform.position;
        var otPos = target.transform.position;
        var dist = Vector3.Distance(tPos, otPos);
        var ray = new Ray(tPos + Vector3.one, (tPos - otPos).normalized);
        RaycastHit[] hitInfos = new RaycastHit[0];
        var size = Physics.RaycastNonAlloc(ray, hitInfos, dist);
        if (size > 1)
        {
            List<Creature> creatures = null;
            var mask = CombatManager.Instance.TargetableObjs;
            if (hitInfos != null)
            {
                foreach (var hitInfo in hitInfos)
                {
                    if (mask == (mask | 1 << hitInfo.collider.gameObject.layer))
                    {
                        if (hitInfo.collider.gameObject == target.gameObject)
                        {
                            continue;
                        }

                        //TODO decrement chance to hit and add these new creatures to a list of possibility to be hit if the other attack misses
                    }
                    else
                    {
                        _messageToPrint = TARGETBLOCKED;
                        return -1;
                    }
                }
            }
        }

        var weaponSkill = Skills.Type.Unarmed;
        if (_activeWeapon != null)
        {
            weaponSkill = _activeWeapon.AssociatedSkill;

            var weaponInfo = _activeWeapon.GetAttackTypeInfo(_activeWeaponMode);

            if (distance > weaponInfo.Range)
            {
                _messageToPrint = OUTOFRANGE;
                return -1;
            }
        }
        else
        {
            if (distance > 1)
            {
                _messageToPrint = OUTOFRANGE;
                return -1;
            }
        }

        var chanceToHit = _skills.GetSkillLvl(weaponSkill) - 30;
        chanceToHit += ((_special.Perception - 2) * 16);
        chanceToHit -= (distance * 4);
        chanceToHit -= (target.ArmorClass);
        if (WorldClock.Instance != null && WorldClock.Instance.IsNight && distance > 5)
        {
            chanceToHit -= 10;
        }

        if (_isAimedShot) { }

        return chanceToHit;
    }

    protected virtual int GetCriticalChance(int chanceToHit)
    {
        var critChance = CriticalChance; //just matching original math used
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
        NonBinary //This is not in the original
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