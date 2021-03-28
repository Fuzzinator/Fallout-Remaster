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

    protected override  int MaxActionPoints => Mathf.FloorToInt(_special.Agility * .5f) + APMods();

    protected override int CarryWeight => 25 + CarryWeightMod();

    private int BaseHPIncrease => Mathf.FloorToInt(_special.Endurance * .5f) + 2 + HPIncMod();
    protected override int HealingRate => Mathf.CeilToInt(_special.Endurance * .3f) + HRModifiers();

    protected override int MeleeDamage
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

            if (_trait1.modType == ModType.Skilled || _trait2.modType == ModType.Skilled)
            {
                perkRate = 4;
            }

            return perkRate;
        }
    }

    protected override int PoisonResist => PoisonResistMod();

    private int SkillRate => (_special.Intelligence * 2) + 5 + SkillRateMod();
    protected override int Sequence => (_special.Perception*2)+SequenceMod();

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
    
    #region Combat

    protected override int CriticalChance(int chanceToHit)
    {
        var critChance = base.CriticalChance(chanceToHit);

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

        if (_trait1.modType == ModType.CritChance)
        {
            critChance += _trait1.effectAmount;
        }

        if (_trait2.modType == ModType.CritChance)
        {
            critChance += _trait2.effectAmount;
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
        if (_trait1.modType == ModType.HPRecover)
        {
            healRateMod += _trait1.effectAmount;
        }
        if (_trait2.modType == ModType.HPRecover)
        {
            healRateMod += _trait2.effectAmount;
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

        if (_trait1.modType == ModType.SkillLvlInc)
        {
            skillInc += _trait1.effectAmount;
        }
        if (_trait2.modType == ModType.SkillLvlInc)
        {
            skillInc += _trait2.effectAmount;
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
        
        if (_trait1.modType == ModType.Sequence)
        {
            sequence += _trait1.effectAmount;
        }
        if (_trait2.modType == ModType.Sequence)
        {
            sequence += _trait2.effectAmount;
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
        
        if (_trait1.modType == ModType.MeleeDamage)
        {
            meleeDamage += _trait1.effectAmount;
        }
        if (_trait2.modType == ModType.MeleeDamage)
        {
            meleeDamage += _trait2.effectAmount;
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
        
        //This is looking for the Fast Metabolism trait
        if (_trait1.modType != ModType.PoisonResist && _trait2.modType != ModType.PoisonResist)
        {
            poisonResist += _special.Endurance * 5;
        }
        
        return poisonResist;
    }
    private int CarryWeightMod()
    {
        var carryWeight = _special.Strength;
        if (_trait1.modType == ModType.CarryWeight || _trait2.modType == ModType.CarryWeight)
        {
            carryWeight *= 15;
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

    protected override int RadResistMod()
    {
        var radResist = base.RadResistMod();

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

        foreach (var perk in _activePerks)
        {
            if (perk.ModType == ModType.ArmorClass)
            {
                ac += perk.EffectAmount;
            }
        }

        if (_trait1.modType == ModType.ArmorClass || _trait2.modType == ModType.ArmorClass)
        {
            ac -= _special.Agility;//Checking for Kamikaze trait which sets natural AC to 0
        }
        
        return ac;
    }

    #endregion
}