using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 10, fileName = "New Weapon", menuName = "ScriptObjs/Weapon")]
public class Weapon : ScriptableObject
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
    private int _ammoCost;
    public int AmmoCost => _ammoCost;

    [SerializeField]
    private int _value;
    public int Value => _value;

    [SerializeField]
    private int _weight;
    public int Weight => _weight;

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
