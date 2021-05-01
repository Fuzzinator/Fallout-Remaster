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
    protected int _currentHealth;

    [SerializeField]
    protected int _currentAP;

    [SerializeField]
    protected SPECIAL _special;

    [SerializeField]
    protected Skills _skills;

    [SerializeField]
    protected Weapon.AttackMode _activeWeaponMode;

    [SerializeField]
    protected Item _primaryItem;

    [SerializeField]
    protected Item _secondaryItem;

    [SerializeField]
    protected Weapon.AttackMode _secondaryWeaponMode;

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

    [SerializeField]
    private AnimatorController animatorController;

    #region Combat Targeting Things

    protected Creature _currentTarget;
    protected int _chanceHitTarget;
    protected bool _targetOutOfRange = false;
    protected bool _targetBlocked = false;

    #endregion

    protected int _apToAC = 0;

    #region Lists

    #endregion

    #region Properties

    public int CurrentHealth => _currentHealth;

    public bool Alive => _currentHealth > 0;

    public int CurrentLocation
    {
        get => _currentLocation;
        set => _currentLocation = value;
    }

    public Coordinates Coord => HexMaker.GetCoord(_currentLocation);

    public int XPValue => _xPValue;

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

    public AnimatorController AnimController => animatorController;

    public virtual BasicAI.Aggression Aggression => _ai != null ? _ai.CurrentAggression : BasicAI.Aggression.Neutral;

    protected Weapon ActiveWeapon => _primaryItem as Weapon;

    public Item PrimaryItem => _primaryItem;
    public Item SecondaryItem => _secondaryItem;

    #endregion

    #region Constants

    protected const int UNARMEDAPCOST = 3;
    protected const int INVENTORYAPCOST = 4;

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

    public virtual bool TryOpenInventory()
    {
        if (CombatManager.Instance != null)
        {
            var canOpenInventory =
                !CombatManager.Instance.CombatMode || TryDecrementAP(INVENTORYAPCOST, ActionType.None);
            if (canOpenInventory)
            {
                OpenInventory();
            }

            return canOpenInventory;
        }

        return false;
    }

    protected virtual void OpenInventory()
    {
    }

    public virtual bool TryReloadWeapon()
    {
        var canReload = false;

        if (ActiveWeapon != null)
        {
            var hasMatchingAmmoInInventory = false; //TODO Once inventory has been updated, make this work
            if (ActiveWeapon.UsesAmmo && hasMatchingAmmoInInventory)
            {
                canReload = !CombatManager.Instance.CombatMode || TryDecrementAP(INVENTORYAPCOST, ActionType.None);
                ReloadWeapon(ActiveWeapon);
                //TriggerAnimation
            }
        }

        return canReload;
    }

    protected virtual void ReloadWeapon(Weapon weapon)
    {
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

    protected virtual bool TryDecrementAP(int cost, ActionType type)
    {
        if (cost > _currentAP)
        {
            return false;
        }

        _currentAP -= cost;

        return true;
    }

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
        _ai?.StartTurn();
    }

    public virtual void EndTurn()
    {
        _apToAC = _currentAP;
        if (CombatManager.IsMyTurn(this))
        {
            CombatManager.ProgressCombat();
        }
    }

    public virtual void SetTargetCreature(Creature target) //TODO flesh this out
    {
        _currentTarget = target;
    }

    public virtual AttackSuccess TryAttackCreature()
    {
        var attackSuccess = AttackSuccess.AttackMissed;
        if (_currentTarget == null)
        {
            attackSuccess = AttackSuccess.NoTarget;
            return attackSuccess;
        }

        if (_chanceHitTarget > 0)
        {
            var apCost = UNARMEDAPCOST;
            var numOfAttacks = 1;
            if (ActiveWeapon != null)
            {
                var weaponInfo = ActiveWeapon.GetAttackTypeInfo(_activeWeaponMode);

                apCost = weaponInfo.ActionPointCost;
                numOfAttacks = weaponInfo.AmmoCost;
                if (!ActiveWeapon.CanUseWeapon)
                {
                    LoggingManager.LogMessage(MessageType.OutOfAmmo, this, _currentTarget);

                    attackSuccess = AttackSuccess.NoAmmo;
                    return attackSuccess;
                }
            }

            var canAttack = TryDecrementAP(apCost, ActionType.Attack);
            if (!canAttack)
            {
                LoggingManager.LogMessage(MessageType.NotEnoughAP, this, _currentTarget, apCost.ToString());

                attackSuccess = AttackSuccess.NotEnoutAP;
                return attackSuccess;
            }

            var allMissed = true;
            var totalDamage = 0;
            for (int i = 0; i < numOfAttacks; i++)
            {
                if (ActiveWeapon != null && !ActiveWeapon.TryUseWeapon(1))
                {
                    break;
                }

                var randomVal = RandomHit();
                var toHit = _chanceHitTarget - randomVal;
                if (toHit >= 0) //Did the attack miss?
                {
                    allMissed = false;
                    totalDamage += ProcessAttack(toHit);
                    attackSuccess = AttackSuccess.AttackHit;
                }
            }

            if (allMissed)
            {
                LoggingManager.LogMessage(MessageType.AttackMissed, this, _currentTarget);
                attackSuccess = AttackSuccess.AttackMissed;
            }
            else
            {
                LoggingManager.LogMessage(MessageType.AttackHit, this, _currentTarget, totalDamage.ToString());
                attackSuccess = AttackSuccess.AttackHit;
            }
        }
        else
        {
            attackSuccess = AttackSuccess.NoChanceToHit;
        }

        if (_currentTarget.Alive)
        {
            CombatManager.AddToCombat(_currentTarget);
        }

        return attackSuccess;
    }

    public virtual IEnumerator SwapQuickbarItems()
    {
        var primary = _primaryItem;
        var primaryMode = _activeWeaponMode;
        var secondary = _secondaryItem;
        var secondaryMode = _secondaryWeaponMode;

        _primaryItem = secondary;
        _activeWeaponMode = secondaryMode;
        _secondaryItem = primary;
        _secondaryWeaponMode = primaryMode;
        yield return null;
        //Play animation
    }

    protected virtual int ProcessAttack(int toHit)
    {
        var damage = GetDamage(toHit);
        _currentTarget.TakeDamage(damage);
        return damage;
    }

    protected virtual int GetDamage(int toHit)
    {
        var baseDamage = (float) MeleeDamage + Random.Range(1, 2);
        var armorThreshold = 0;
        var armorResist = 0;
        var ammoDRMod = 0;
        if (ActiveWeapon != null)
        {
            baseDamage = ActiveWeapon.GetDamage();
            baseDamage *= ActiveWeapon.CurrentAmmo.DamageMod;

            if (_currentTarget._equipedArmor != null)
            {
                armorThreshold = _currentTarget._equipedArmor.GetThreshold(ActiveWeapon.DmgType);
                armorResist = _currentTarget._equipedArmor.GetResistance(ActiveWeapon.DmgType);
            }

            if (ActiveWeapon.CurrentAmmo != null)
            {
                ammoDRMod = ActiveWeapon.CurrentAmmo.DRMod;
            }
        }
        else if (_currentTarget._equipedArmor != null)
        {
            armorThreshold = _currentTarget._equipedArmor.GetThreshold(DamageType.Normal);
            armorResist = _currentTarget._equipedArmor.GetResistance(DamageType.Normal);
        }

        var critChance = GetCriticalChance(toHit);
        var wasCrit = false; //TODO flesh out the critical stuff
        var armorIgnore = 1f;
        if (wasCrit)
        {
            armorIgnore = 5f;
        }

        baseDamage *= Settings.CombatMultiplier;

        baseDamage -= armorThreshold / armorIgnore;

        var resistance = 100f;

        resistance -= Mathf.Max((armorResist / armorIgnore) + ammoDRMod, 0);

        var finalDamage = Mathf.RoundToInt(baseDamage * resistance / 100f);
        return finalDamage;
    }

    public virtual void TakeDamage(int damage)
    {
        _currentHealth -= damage;
        if (_currentHealth > 0)
        {
            return;
        }
        else
        {
            TriggerDeath();
        }
    }

    protected virtual void TriggerDeath()
    {
        CombatManager.RemoveFromCombat(this);
    }

    protected virtual int RandomHit()
    {
        return Random.Range(1, 100);
    }

    public virtual int GetChanceToHit(int distance, Creature target)
    {
        var tPos = transform.position;
        var otPos = target.transform.position;

        var weaponSkill = Skills.Type.Unarmed;
        var ammoACMod = 0;
        _targetOutOfRange = false;
        _targetBlocked = false;
        if (ActiveWeapon != null)
        {
            weaponSkill = ActiveWeapon.AssociatedSkill;

            var weaponInfo = ActiveWeapon.GetAttackTypeInfo(_activeWeaponMode);
            if (!ActiveWeapon.CanUseWeapon)
            {
                return -1;
            }

            if (ActiveWeapon.CurrentAmmo != null)
            {
                ammoACMod = ActiveWeapon.CurrentAmmo.ACMod;
            }

            if (distance > weaponInfo.Range)
            {
                //_messageToPrint = OUTOFRANGE;
                _targetOutOfRange = true;
                return -1;
            }
        }
        else
        {
            if (distance > 1)
            {
                //_messageToPrint = OUTOFRANGE;
                _targetOutOfRange = true;
                return -1;
            }
        }

        var viewUnobscured = CombatManager.ViewUnobscured(this, target);
        if (!viewUnobscured)
        {
            _targetBlocked = true;
            //_messageToPrint = TARGETBLOCKED;
            return -1;
        }

        var chanceToHit = _skills.GetSkillLvl(weaponSkill) - 30;
        chanceToHit += ((_special.Perception - 2) * 16);
        chanceToHit -= (distance * 4);
        chanceToHit -= (target.ArmorClass + ammoACMod); //Not positive ammoACMod should be used this way
        if (WorldClock.Instance != null && WorldClock.Instance.IsNight && distance > 5)
        {
            chanceToHit -= 10;
        }

        if (_isAimedShot)
        {
        }

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

    public virtual Weapon.AttackTypeInfo GetAttackTypeInfo(bool primary = true)
    {
        Weapon.AttackTypeInfo weaponInfo;
        if (primary)
        {
            var weapon = _primaryItem as Weapon;
            if (weapon != null)
            {
                weaponInfo = weapon.GetAttackTypeInfo(_activeWeaponMode);
            }
            else if (_primaryItem == null)
            {
                var apCost = _activeWeaponMode == Weapon.AttackMode.AimedShot ? UNARMEDAPCOST + 1 : UNARMEDAPCOST;
                weaponInfo = new Weapon.AttackTypeInfo(_activeWeaponMode, 1, apCost, 0);
            }
            else
            {
                weaponInfo = new Weapon.AttackTypeInfo(false);
            }
        }
        else
        {
            var weapon = _secondaryItem as Weapon;
            if (weapon != null)
            {
                weaponInfo = weapon.GetAttackTypeInfo(_activeWeaponMode);
            }
            else
            {
                weaponInfo = new Weapon.AttackTypeInfo(false);
            }
        }

        return weaponInfo;
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

    public enum AttackSuccess
    {
        None = 0,
        NoTarget = 1,
        NoAmmo = 2,
        NotEnoutAP = 3,
        NoChanceToHit = 4,
        AttackMissed = 5,
        AttackHit = 6,
        AttackCritical = 7
    }

    #endregion
}