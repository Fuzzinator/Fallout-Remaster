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
    private int _range = 1;
    public int Range => _range;

    [SerializeField]
    private Skills.Type _associatedSkill;
    public Skills.Type AssociatedSkill => _associatedSkill;

    [SerializeField]
    private Type _weaponType;
    public Type WeaponType => _weaponType;

    [SerializeField]
    private DamageType _dmgType;
    public DamageType DmgType => _dmgType;
    public enum Type
    {
        None = 0,
        Melee = 1,
        CloseRange = 2,
        MidRange = 3,
        LongRange = 4
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
