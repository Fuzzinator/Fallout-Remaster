using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using Action = System.Action;

public class Player : Human
{
    #region Variables And Properties

    public static Player Instance { get; private set; }

    [SerializeField]
    private int _currentXP = 0;

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

    public Action leveledUp;

    //private int _criticalChance;
    private int _bonusMovement = 0;
    private bool _canGetPerk = false;
    private int _xpGainedDuringCombat = 0;

    #region Properties

    public override int ArmorClass => ACMod();

    public override int MaxActionPoints => Mathf.FloorToInt(_special.Agility * .5f) + 5 + APMods();

    protected override int CarryWeight => 25 + CarryWeightMod();

    private int BaseHPIncrease => Mathf.FloorToInt(_special.Endurance * .5f) + 2 + HPIncMod();
    public override int HealingRate => Mathf.CeilToInt(_special.Endurance * .3f) + HRModifiers();

    public override int MeleeDamage
    {
        get
        {
            var damage = _special.Strength - 5;

            damage += MeleeDamageMod();

            if (damage < 1)
            {
                damage = 1;
            }

            return damage;
        }
    }

    private int PerkRate
    {
        get
        {
            var perkRate = 3;

            if (_trait1 != null && _trait1.TraitType == Trait.Type.Skilled && _trait1.Penalty == ModType.PerkRate)
            {
                perkRate = _trait1.PenaltyAmount;
            }
            else if (_trait2 != null && _trait2.TraitType == Trait.Type.Skilled && _trait2.Penalty == ModType.PerkRate)
            {
                perkRate = _trait2.PenaltyAmount;
            }

            return perkRate;
        }
    }

    protected override int PoisonResist => PoisonResistMod();
    public override int RadResistance => RadResistMod();

    private int SkillRate => (_special.Intelligence * 2) + 5 + SkillRateMod();
    public override int Sequence => (_special.Perception * 2) + SequenceMod();

    public override int CriticalChance => base.CriticalChance + CritMod();

    public int MaxMovement => _currentAP + _bonusMovement;

    public override BasicAI.Aggression Aggression => BasicAI.Aggression.Friendly;

    #endregion

    #region Const

    private const string FORCRUSHING = "For crushing your enemies, you earn ";
    private const string EXPPOINTS = " exp. points.";
    private const int LVLCAP = 21;
    private const int XPINCREMENT = 1000;

    #endregion

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
        CombatManager.stateChanged += CombatStateChanged;
        CursorController.stateChanged += CursorStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.InputManager.Player.PrimaryClick.performed -= PrimaryClickHandler;
        GameManager.InputManager.Player.Look.performed -= LookHandler;
        CombatManager.stateChanged -= CombatStateChanged;
        CursorController.stateChanged -= CursorStateChanged;
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

    #region General Stats

    public int GetSpecial(SPECIAL.Type type)
    {
        var special = _special.GetSPECIALLvl(type);

        special += GetTraitSpecialMods(_trait1, type);
        special += GetTraitSpecialMods(_trait2, type);

        return special;
    }

    public int GetSkill(Skills.Type type)
    {
        var skill = _skills.GetSkillLvl(type);

        skill += GetTraitSkillMods(_trait1, type);
        skill += GetTraitSkillMods(_trait2, type);

        return skill;
    }

    private static int GetTraitSpecialMods(Trait trait, SPECIAL.Type type)
    {
        var special = 0;
        switch (trait.TraitType)
        {
            case Trait.Type.Gifted:
                special += trait.BenefitAmount;
                break;
            case Trait.Type.Bruiser:
                if (type == SPECIAL.Type.Strength)
                {
                    special += trait.BenefitAmount;
                }

                break;
            case Trait.Type.NightPerson:
                if (type == SPECIAL.Type.Intelligence || type == SPECIAL.Type.Perception)
                {
                    if (WorldClock.Instance != null && WorldClock.Instance.IsNight)
                    {
                        special += trait.BenefitAmount;
                    }
                    else
                    {
                        special += trait.PenaltyAmount;
                    }
                }

                break;
            case Trait.Type.SmallFrame:
                if (type == SPECIAL.Type.Agility)
                {
                    special += trait.BenefitAmount;
                }

                break;
            default:
                break;
        }

        return special;
    }

    private static int GetTraitSkillMods(Trait trait, Skills.Type type)
    {
        var skill = 0;

        switch (trait.TraitType)
        {
            case Trait.Type.Gifted:
                skill += trait.PenaltyAmount;
                break;
            case Trait.Type.GoodNatured:
                if (type == Skills.Type.FirstAid || type == Skills.Type.Doctor ||
                    type == Skills.Type.Speech || type == Skills.Type.Barter)
                {
                    skill += trait.BenefitAmount;
                }
                else if (type == Skills.Type.SmallGuns || type == Skills.Type.BigGuns ||
                         type == Skills.Type.EnergyWeapons ||
                         type == Skills.Type.Throwing || type == Skills.Type.MeleeWeapons ||
                         type == Skills.Type.Unarmed)
                {
                    skill += trait.PenaltyAmount;
                }

                break;
            case Trait.Type.Skilled:
                skill += trait.BenefitAmount;
                break;
        }

        return skill;
    }

    #endregion

    #region Leveling

    private void IncreaseXP(int increase)
    {
        _currentXP += increase;
        if (!ShouldLvlUp())
        {
            return;
        }

        TriggerLvlUp();
    }

    private bool ShouldLvlUp()
    {
        var shouldLvl = false;

        var nxtLvl = _currentLvl + 1;
        if (_currentXP >= (nxtLvl * (nxtLvl - 1) * .5 * XPINCREMENT))
        {
            return _currentLvl < LVLCAP;
        }

        return shouldLvl;
    }

    private void TriggerLvlUp()
    {
        _currentLvl++;
        if (_currentLvl >= PerkRate && _currentLvl % PerkRate == 0)
        {
            _canGetPerk = true;
        }

        _unspentSkillPnts += SkillRate;
        _hpIncrease += BaseHPIncrease;

        leveledUp?.Invoke();
    }

    #endregion

    #region Movement

    private void TryMove()
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

    #region Combat

    public override void StartTurn()
    {
        _currentAP = MaxActionPoints;
        _apToAC = 0;
        _bonusMovement = GetBonusMovement();
        HexHighlighter.TryEnable();
    }

    public override void EndTurn()
    {
        HexHighlighter.Disable();
        base.EndTurn();
    }

    protected override void TryAttackCreature()
    {
        if (_currentTarget == null || !_currentTarget.Alive)
        {
            return;
        }

        base.TryAttackCreature();

        if (string.IsNullOrWhiteSpace(_messageToPrint))
        {
            return;
        }

        Debug.Log(_messageToPrint);
        _messageToPrint = string.Empty;
        if (_currentTarget == null || _currentTarget.Alive)
        {
            return;
        }

        _xpGainedDuringCombat += _currentTarget.XPValue;
        _currentTarget = null;
    }

    protected bool TryGetTargetCreature(out Creature target)
    {
        var cam = CameraController.Instance.TargetCamera;
        var ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        if (Physics.Raycast(ray, out var hitInfo, Mathf.Infinity, CombatManager.Instance.TargetableObjs))
        {
            return hitInfo.collider.TryGetComponent(out target);
        }

        target = null;
        return false;
    }

    protected override bool TryDecrementAP(int cost, ActionType type)
    {
        var additional = type switch
        {
            ActionType.Move => _bonusMovement,
            _ => 0
        };

        if (cost > _currentAP + additional)
        {
            return false;
        }

        switch (type)
        {
            case ActionType.Move:
                if (cost > _bonusMovement)
                {
                    cost -= _bonusMovement;
                    _bonusMovement = 0;
                    break;
                }
                else
                {
                    _bonusMovement -= cost;
                    return true;
                }
        }

        _currentAP -= cost;

        return true;
    }

    public override int GetChanceToHit(int distance, Creature target)
    {
        return base.GetChanceToHit(distance, target);
    }

    protected override int GetCriticalChance(int chanceToHit)
    {
        var critChance = base.GetCriticalChance(chanceToHit);

        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.CritChance)
            {
                critChance += perk.EffectAmount;
            }
            else if (perk.ModType == ModType.WeaponSpec && _activeWeapon.WeaponType == perk.AffectedWeapon)
            {
                critChance += perk.EffectAmount;
            }
        }

        if (_trait1 != null && _trait1.TraitType == Trait.Type.Finesse && _trait1.Benefit == ModType.CritChance)
        {
            critChance += _trait1.BenefitAmount;
        }
        else if (_trait2 != null && _trait2.TraitType == Trait.Type.Finesse && _trait2.Benefit == ModType.CritChance)
        {
            critChance += _trait2.BenefitAmount;
        }

        return critChance;
    }

    protected override int DamageResistance(DamageType damageType)
    {
        var resistance = base.DamageResistance(damageType);

        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.DamageResist)
            {
                resistance += perk.EffectAmount;
            }
        }

        return resistance;
    }

    private void RequestAimedShot()
    {
    }

    private int GetBonusMovement()
    {
        var move = 0;
        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.Movement)
            {
                move += perk.EffectAmount;
            }
        }

        return move;
    }

    #endregion

    #region modifiers

    private int HPIncMod()
    {
        var hpInc = 0;
        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.HPLvlInc)
            {
                hpInc += perk.EffectAmount;
            }
        }

        return hpInc;
    }

    private int HRModifiers()
    {
        var healRateMod = 0;
        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.HPRecover)
            {
                healRateMod += perk.EffectAmount;
            }
        }

        if (_trait1 != null && _trait1.TraitType == Trait.Type.FastMetabolism && _trait1.Benefit == ModType.HPRecover)
        {
            healRateMod += _trait1.BenefitAmount;
        }
        else if (_trait2 != null && _trait2.TraitType == Trait.Type.FastMetabolism &&
                 _trait2.Benefit == ModType.HPRecover)
        {
            healRateMod += _trait2.BenefitAmount;
        }

        return healRateMod;
    }

    private int SkillRateMod()
    {
        var skillInc = 0;
        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.SkillLvlInc)
            {
                skillInc += perk.EffectAmount;
            }
        }

        if (_trait1 != null && _trait1.TraitType == Trait.Type.Gifted && _trait1.Penalty == ModType.SkillLvlInc)
        {
            skillInc += _trait1.PenaltyAmount;
        }
        else if (_trait2 != null && _trait2.TraitType == Trait.Type.Gifted && _trait2.Penalty == ModType.SkillLvlInc)
        {
            skillInc += _trait2.PenaltyAmount;
        }

        return skillInc;
    }

    private int SequenceMod()
    {
        var sequence = 0;

        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.Sequence)
            {
                sequence += perk.EffectAmount;
            }
        }

        if (_trait1 != null && _trait1.TraitType == Trait.Type.Kamikaze && _trait1.Benefit == ModType.Sequence)
        {
            sequence += _trait1.BenefitAmount;
        }

        if (_trait2 != null && _trait2.TraitType == Trait.Type.Kamikaze && _trait2.Benefit == ModType.Sequence)
        {
            sequence += _trait2.BenefitAmount;
        }

        return sequence;
    }

    private int APMods()
    {
        var actionPoints = 0;
        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.ActionPoints)
            {
                actionPoints += perk.EffectAmount;
            }
        }

        return actionPoints;
    }

    private int MeleeDamageMod()
    {
        var meleeDamage = 0;

        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.MeleeDamage)
            {
                meleeDamage += perk.EffectAmount;
            }
        }

        if (_trait1 != null && _trait1.TraitType == Trait.Type.HeavyHanded && _trait1.Benefit == ModType.MeleeDamage)
        {
            meleeDamage += _trait1.BenefitAmount;
        }
        else if (_trait2 != null && _trait2.TraitType == Trait.Type.HeavyHanded &&
                 _trait2.Benefit == ModType.MeleeDamage)
        {
            meleeDamage += _trait2.BenefitAmount;
        }

        return meleeDamage;
    }

    private int PoisonResistMod()
    {
        var poisonResist = 0;

        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.PoisonResist)
            {
                poisonResist += perk.EffectAmount;
            }
        }

        if ((_trait1 == null || _trait1.TraitType != Trait.Type.FastMetabolism) &&
            (_trait2 == null || _trait2.TraitType != Trait.Type.FastMetabolism))
        {
            poisonResist += _special.Endurance * 5;
        }

        return poisonResist;
    }

    private int CarryWeightMod()
    {
        var carryWeight = _special.Strength;
        if (_trait1 != null && _trait1.TraitType == Trait.Type.SmallFrame && _trait1.Penalty == ModType.CarryWeight)
        {
            carryWeight *= _trait1.PenaltyAmount;
        }
        else if (_trait2 != null && _trait2.TraitType == Trait.Type.SmallFrame &&
                 _trait2.Penalty == ModType.CarryWeight)
        {
            carryWeight *= _trait2.PenaltyAmount;
        }
        else
        {
            carryWeight *= 25;
        }

        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.CarryWeight)
            {
                carryWeight += perk.EffectAmount;
            }
        }

        return carryWeight;
    }

    private int CritMod()
    {
        var critChance = 0;

        if (_trait1 != null && _trait1.Benefit == ModType.CritChance)
        {
            critChance += _trait1.BenefitAmount;
        }

        if (_trait2 != null && _trait2.Benefit == ModType.CritChance)
        {
            critChance += _trait2.BenefitAmount;
        }

        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.CritChance)
            {
                critChance += perk.EffectAmount;
            }
        }

        return critChance;
    }

    protected override int RadResistMod()
    {
        var radResist = base.RadResistMod();

        if ((_trait1 == null || _trait1.TraitType != Trait.Type.FastMetabolism) &&
            (_trait2 == null || _trait2.TraitType != Trait.Type.FastMetabolism))
        {
            radResist += _special.Endurance * 2;
        }

        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.RadResist)
            {
                radResist += perk.EffectAmount;
            }
        }

        return radResist;
    }

    protected override int ACMod()
    {
        var ac = base.ACMod();

        if ((_trait1 == null || _trait1.TraitType != Trait.Type.Kamikaze) &&
            (_trait2 == null || _trait2.TraitType != Trait.Type.Kamikaze))
        {
            ac += _special.Agility;
        }

        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.ArmorClass)
            {
                ac += perk.EffectAmount;
            }
        }

        return ac;
    }

    #endregion

    #region Listeners

    private void PrimaryClickHandler(InputAction.CallbackContext obj)
    {
        switch (CursorController.Instance.CurrentState)
        {
            case CursorController.CursorState.Movement:
                TryMove();
                break;
            case CursorController.CursorState.Targeting:
                if (_isAimedShot)
                {
                    RequestAimedShot();
                }
                else
                {
                    TryAttackCreature();
                }

                break;
        }
    }

    private void CombatStateChanged(bool isCombat)
    {
        if (!isCombat)
        {
            if (_xpGainedDuringCombat > 0)
            {
                IncreaseXP(_xpGainedDuringCombat);
                _messageToPrint = $"{FORCRUSHING}{_xpGainedDuringCombat}{EXPPOINTS}";
                Debug.Log(_messageToPrint);
                _xpGainedDuringCombat = 0;
            }
        }
    }

    private void LookHandler(InputAction.CallbackContext obj)
    {
        if (CursorController.Instance.CurrentState != CursorController.CursorState.Targeting)
        {
            return;
        }

        if (!TryGetTargetCreature(out _currentTarget))
        {
            _chanceHitTarget = -1;
            return;
        }

        var distance = HexMaker.Instance.GetDistanceToCoord(Coord, _currentTarget.Coord, TargetPath, wantToMove: false);
        _chanceHitTarget = GetChanceToHit(distance, _currentTarget);
        if (_currentTarget != null && _chanceHitTarget > 0)
        {
            Debug.Log($"Temp\n Chance to hit is {_chanceHitTarget} ");
        }
    }

    private void CursorStateChanged(CursorController.CursorState state)
    {
        if (state == CursorController.CursorState.Targeting)
        {
            GameManager.InputManager.Player.Look.performed += LookHandler;
            LookHandler(new InputAction.CallbackContext());
        }
        else
        {
            GameManager.InputManager.Player.Look.performed -= LookHandler;
        }
    }

    #endregion

    #region Enums

    public enum Reputation
    {
        VaultDweller = 1,
        VaultScion = 2,
        VaultVeteran = 3,
        VaultElite = 4,
        Wanderer = 5,
        DesertWanderer = 6,
        WandererOfTheWastes = 7,
        EliteWanderer = 8,
        Strider = 9,
        DesertStrider = 10,
        StriderOfTheWastes = 11,
        StriderElite = 12,
        VaultHero = 13,
        WanderingHero = 14,
        StridingHero = 15,
        HeroOfTheDesert = 16,
        HeroOfTheWastes = 17,
        HeroOfTheGlowingLands = 18,
        Paragon = 19,
        LivingLegend = 20,
        LastBestHopeForHumanity = 21
    }

    #endregion
}