using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using Random = UnityEngine.Random;

[CreateAssetMenu(order = 10, fileName = "New Weapon", menuName = "ScriptObjs/Weapon")]
public class Weapon : Item
{
    [SerializeField]
    private int _minDamage;
    [SerializeField]
    private int _maxDamage;
    public int WeaponDamage => GetDamage();

    [SerializeField]
    private AttackTypeInfo[] _typeAndInfo;

    [SerializeField]
    private bool _usesAmmo;
    public bool UsesAmmo => _usesAmmo;

    [SerializeField]
    private Ammo.Type _ammoType = Ammo.Type.None;
    public Ammo.Type UsedAmmoType => _ammoType;

    [SerializeField]
    private Ammo _currentAmmo;
    public Ammo CurrentAmmo => _currentAmmo;

    [SerializeField]
    private int _ammoInClip = 0;
    public int AmmoInClip => _ammoInClip;

    [SerializeField]
    private int _magSize;
    public int MagSize => _magSize;

    [SerializeField]
    private int _strRequired;
    public int StrRequired => _strRequired;

    [SerializeField]
    private Skills.Type _associatedSkill;
    public Skills.Type AssociatedSkill => _associatedSkill;

    [SerializeField]
    private Type _weaponType;
    public Type WeaponType => _weaponType;

    [SerializeField]
    private DamageType _dmgType;
    public DamageType DmgType => _dmgType;

    [SerializeField]
    private bool _oneHanded = true;
    public bool OneHanded => _oneHanded;

    public bool CanUseWeapon => (_usesAmmo && _ammoInClip>0) || !_usesAmmo;
    
    public bool TryUseWeapon(int cost)
    {
        if (_usesAmmo)
        {
            if (_ammoInClip >= cost)
            {
                _ammoInClip -= cost;
                return true;
            }
            else
            {
                return false;
            }
        }
        else
        {
            return true;
        }
    }

    public int GetDamage()
    {
        return Random.Range(_minDamage, _maxDamage);
    }

    public AttackTypeInfo GetAttackTypeInfo(AttackMode mode)
    {
        foreach (var info in _typeAndInfo)
        {
            if (info.CurrentAttackMode == mode)
            {
                return info;
            }
        }
        return new AttackTypeInfo();
    }

    public enum Type
    {
        None = 0,
        Melee = 1,
        CloseRange = 2,
        MidRange = 3,
        LongRange = 4
    }

    public enum AttackMode
    {
        SingleShot,
        AimedShot,
        BurstFire,
        Thrown,
        Swing,
        Thrust,
        Punch,
        Placed
    }

    [System.Serializable]
    public struct AttackTypeInfo
    {
        [SerializeField]
        private AttackMode _attackMode;
        public AttackMode CurrentAttackMode => _attackMode;

        [SerializeField]
        private int _range;
        public int Range => _range;
        [SerializeField]
        private int _actionPointCost;
        public int ActionPointCost => _actionPointCost;
        [SerializeField]
        private int _ammoCost;
        public int AmmoCost => _ammoCost;
        
        public bool IsValidWeapon => _range>=0;

        public AttackTypeInfo(AttackMode attackMode, int range, int actionPointCost, int ammoCost)
        {
            _attackMode = attackMode;
            _range = range;
            _actionPointCost = actionPointCost;
            _ammoCost = ammoCost;
        }
        public AttackTypeInfo(bool valid)
        {
            _attackMode = AttackMode.SingleShot;
            _range = -1;
            _actionPointCost = 0;
            _ammoCost = 0;
        }
    }
}
public enum DamageType
{
    None = 0,
    Normal = 1,
    Laser = 2,
    Fire = 3,
    Plasma = 4,
    Explosive = 5,
    Electrical = 6
}