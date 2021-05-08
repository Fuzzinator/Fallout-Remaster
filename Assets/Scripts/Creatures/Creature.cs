using System;
using System.Collections;
using System.Collections.Generic;
using ThreePupperStudios.Lockable;
using UnityEngine;
using UnityEngine.Serialization;
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
    protected ItemContainer _inventory;

    //[SerializeField]
    //protected Weapons _weapons;

    [SerializeField]
    protected Item _primaryItem;

    [SerializeField]
    protected Item _secondaryItem;

    [SerializeField]
    protected bool _primaryEquipped = true;

    [FormerlySerializedAs("_equipedArmor")]
    [SerializeField]
    protected ArmorInfo equipedArmorInfo;

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


    [SerializeField]
    private WeaponInfo.AttackMode _primaryAttackMode;

    public WeaponInfo.AttackMode PrimaryAttackMode => _primaryAttackMode;

    [SerializeField]
    private WeaponInfo.AttackMode _secondaryAttackMode;

    public WeaponInfo.AttackMode SecondaryAttackMode => _secondaryAttackMode;

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

    public Item ActiveItem => _primaryEquipped ? _primaryItem : _secondaryItem;
    public Item InactiveItem => _primaryEquipped ? _secondaryItem : _primaryItem;

    public WeaponInfo.AttackMode ActiveAttackMode => _primaryEquipped ? _primaryAttackMode : _secondaryAttackMode;
    public WeaponInfo.AttackMode InactiveAttackMode => _primaryEquipped ? _secondaryAttackMode : _primaryAttackMode;

    public Item PrimaryItem => _primaryItem;
    public Item SecondaryItem => _secondaryItem;

    #endregion

    #region Constants

    protected const int UNARMEDAPCOST = 3;
    protected const int INVENTORYAPCOST = 4;
    protected const int USEITEMAPCOST = 2;

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

    public virtual bool TryReloadWeapon(Item item)
    {
        var canReload = false;

        if (item != null)
        {
            var hasMatchingAmmoInInventory = _inventory.TryGetItem(item.Ammo, out var slot);

            if (item.Info is WeaponInfo info && info.UsesAmmo && hasMatchingAmmoInInventory)
            {
                canReload = !CombatManager.Instance.CombatMode || TryDecrementAP(INVENTORYAPCOST, ActionType.None);
                if (canReload)
                {
                    var newValue = item.ReloadCharges(slot.Count);
                    if (newValue > 0)
                    {
                        slot.SetCount(newValue);
                    }
                    else
                    {
                        _inventory.RemoveSlot(slot);
                    }

                    if (item.Charges < item.Info.MaxCharges)
                    {
                        hasMatchingAmmoInInventory = _inventory.TryGetItem(item.Ammo, out slot);
                        while (hasMatchingAmmoInInventory && item.Charges < item.Info.MaxCharges)
                        {
                            newValue = item.ReloadCharges(slot.Count);
                            if (newValue > 0)
                            {
                                slot.SetCount(newValue);
                            }
                            else
                            {
                                _inventory.RemoveSlot(slot);
                            }

                            if (item.Charges < item.Info.MaxCharges)
                            {
                                hasMatchingAmmoInInventory = _inventory.TryGetItem(item.Ammo, out slot);
                            }
                        }
                    }
                }
            }
        }

        return canReload;
    }

    public virtual bool TryUseItem(Item item)
    {
        if (item == null || item.Info == null)
        {
            return false;
        }
        
        var canUseItem = !CombatManager.Instance.CombatMode ||
                         TryDecrementAP(USEITEMAPCOST, ActionType.ObjectInteraction);
        
        if (canUseItem)
        {
            canUseItem = item.TryUseItem(1);
            if (canUseItem)
            {
                if (item.Info is ConsumableInfo info)
                {
                    foreach (var effect in info.Effects)
                    {
                        
                    }
                }
            }
        }

        return canUseItem;
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
            if (ActiveItem != null && ActiveItem.Info != null && ActiveItem.Info is WeaponInfo weapon)
            {
                var attackInfo = weapon.GetAttackTypeInfo(ActiveAttackMode);

                apCost = attackInfo.ActionPointCost;
                numOfAttacks = attackInfo.AmmoCost;
                if (!ActiveItem.CanUseItem())
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
                if (ActiveItem != null && ActiveItem.Info != null && !ActiveItem.TryUseItem(1))
                {
                    break;
                }

                var randomVal = RandomHit();
                var toHit = _chanceHitTarget - randomVal;
                if (toHit >= 0) //Did the attack hit?
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
        _primaryEquipped = !_primaryEquipped;
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
        if (ActiveItem != null && ActiveItem.Info != null && ActiveItem.Info is WeaponInfo weaponInfo)
        {
            baseDamage = weaponInfo.GetDamage();
            var hasAmmoInfo = ActiveItem.Ammo != null;
            if (hasAmmoInfo)
            {
                baseDamage *= ActiveItem.Ammo.DamageMod;
            }

            if (_currentTarget.equipedArmorInfo != null)
            {
                armorThreshold = _currentTarget.equipedArmorInfo.GetThreshold(weaponInfo.DmgType);
                armorResist = _currentTarget.equipedArmorInfo.GetResistance(weaponInfo.DmgType);
            }

            if (hasAmmoInfo)
            {
                ammoDRMod = ActiveItem.Ammo.DRMod;
            }
        }
        else if (_currentTarget.equipedArmorInfo != null)
        {
            armorThreshold = _currentTarget.equipedArmorInfo.GetThreshold(DamageType.Normal);
            armorResist = _currentTarget.equipedArmorInfo.GetResistance(DamageType.Normal);
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
        if (ActiveItem != null && ActiveItem.Info != null && ActiveItem.Info is WeaponInfo weaponInfo)
        {
            weaponSkill = weaponInfo.AssociatedSkill;

            var attackInfo = weaponInfo.GetAttackTypeInfo(ActiveAttackMode);
            if (!ActiveItem.CanUseItem())
            {
                _chanceHitTarget = -1;
                return _chanceHitTarget;
            }

            if (ActiveItem.Ammo != null)
            {
                ammoACMod = ActiveItem.Ammo.ACMod;
            }

            if (distance > attackInfo.Range)
            {
                //_messageToPrint = OUTOFRANGE;
                _targetOutOfRange = true;
                _chanceHitTarget = -1;
                return _chanceHitTarget;
            }
        }
        else
        {
            if (distance > 1)
            {
                //_messageToPrint = OUTOFRANGE;
                _targetOutOfRange = true;
                _chanceHitTarget = -1;
                return _chanceHitTarget;
            }
        }

        var viewUnobscured = CombatManager.ViewUnobscured(this, target);
        if (!viewUnobscured)
        {
            _targetBlocked = true;
            //_messageToPrint = TARGETBLOCKED;
            _chanceHitTarget = -1;
            return _chanceHitTarget;
        }

        var chanceToHit = _skills.GetSkillLvl(weaponSkill) - 30;
        chanceToHit += ((_special.Perception - 2) * 16);
        chanceToHit -= (distance * 4);
        chanceToHit -= (target.ArmorClass + ammoACMod); //Not positive ammoACMod should be used this way
        if (WorldClock.Instance != null && WorldClock.Instance.IsNight && distance > 5)
        {
            chanceToHit -= 10;
        }

        _chanceHitTarget = chanceToHit;
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
        if (equipedArmorInfo != null)
        {
            ac += equipedArmorInfo.ArmorClass;
        }

        ac += _apToAC;

        return ac;
    }

    protected virtual int DamageResistance(DamageType damageType)
    {
        var resistance = 0;

        if (equipedArmorInfo != null)
        {
            resistance += equipedArmorInfo.GetResistance(damageType);
        }

        return resistance;
    }

    protected virtual int DamageThreshold(DamageType damageType)
    {
        var threshold = 0;

        if (equipedArmorInfo != null)
        {
            threshold += equipedArmorInfo.GetThreshold(damageType);
        }

        return threshold;
    }

    public virtual WeaponInfo.AttackTypeInfo GetAttackTypeInfo(bool activeWeapon = true)
    {
        WeaponInfo.AttackTypeInfo attackInfo;

        var targetItem = activeWeapon ? ActiveItem : InactiveItem;
        var targetMode = activeWeapon ? ActiveAttackMode : InactiveAttackMode;

        if (targetItem == null || targetItem.Info == null)
        {
            var apCost = targetMode == WeaponInfo.AttackMode.AimedShot
                ? UNARMEDAPCOST + 1
                : UNARMEDAPCOST;
            attackInfo = new WeaponInfo.AttackTypeInfo(targetMode, 0, apCost, 0);
        }
        else if (targetItem.Info is WeaponInfo weaponInfo)
        {
            attackInfo = weaponInfo.GetAttackTypeInfo(targetMode);
        }
        else
        {
            attackInfo = new WeaponInfo.AttackTypeInfo(false);
        }

        return attackInfo;
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