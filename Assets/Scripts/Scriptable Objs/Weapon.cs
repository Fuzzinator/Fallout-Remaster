using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 10, fileName = "New Weapon", menuName = "ScriptObjs/Weapon")]
public class Weapon : Item
{
    [SerializeField]
    private string _weaponName;
    public string WeaponName => _weaponName;

    [SerializeField]
    private AttackMode _attackMode;
    public AttackMode AttackType => _attackMode;

    [SerializeField]
    private int _minDamage;
    [SerializeField]
    private int _maxDamage;
    public int WeaponDamage => GetDamage();

    [SerializeField]
    private int _actionPointCost;
    public int ActionPointCost => _actionPointCost;

    [SerializeField]
    private int _range = 1;
    public int Range => _range;

    [SerializeField]
    private AmmoType _ammoType = AmmoType.None;
    public AmmoType Ammo => _ammoType;
    
    [SerializeField]
    private int _magSize;
    public int MagSize => _magSize;
    
    [SerializeField]
    private int _ammoCost;
    public int AmmoCost => _ammoCost;

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


    private int GetDamage()
    {
        return Random.Range(_minDamage, _maxDamage);
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

    //Mostly just a naming convention but here's what we've got
    //Starts with B means bullet.
    //Starts with S means shell.
    
    public enum AmmoType
    {
        None = 0,
        B223 = 1, //.223 FMJ
        B44Magnum = 2, //.44 Magnum FMJ and .44 Magnum JHP
        B10mm = 3, //10mm AP and 10mm JHP
        S12Gauge = 4, //12 gauge shotgun shell
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