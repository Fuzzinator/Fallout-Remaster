using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 11, fileName = "New Armor", menuName = "ScriptObjs/Armor")]
public class Armor : Item
{
    [Header("Main Values")]
    [SerializeField]
    private string _description;
    public string Description => _description;

    [SerializeField]
    private int _armorClass;
    public int ArmorClass => _armorClass;


    [SerializeField]
    private Type _armorType = Type.None;
    public Type ArmorType => _armorType;

    [SerializeField]
    private int _radResist;
    public int RadResist => _radResist;
    
    [SerializeField]
    private int _strMod;
    public int StrMod => _strMod;

    [Header("Damage Resistance")]
    [SerializeField]
    private int _resistNormal;
    public int ResistNormal => _resistNormal;

    [SerializeField]
    private int _resistLaser;
    public int ResistLaser => _resistLaser;

    [SerializeField]
    private int _resistFire;
    public int ResistFire => _resistFire;

    [SerializeField]
    private int _resistPlasma;
    public int ResistPlasma => _resistPlasma;

    [SerializeField]
    private int _resistExplosive;
    public int ResistExplosive => _resistExplosive;

    [SerializeField]
    private int _resistElectric;
    public int ResistElectric => _resistElectric;
    

    [Header("Damage Threshold")]
    [SerializeField]
    private int _normalThreshold;
    public int NormalThreshold => _normalThreshold;

    [SerializeField]
    private int _laserThreshold;
    public int LaserThreshold => _laserThreshold;

    [SerializeField]
    private int _fireThreshold;
    public int FireThreshold => _fireThreshold;

    [SerializeField]
    private int _plasmaThreshold;
    public int PlasmaThreshold => _plasmaThreshold;

    [SerializeField]
    private int _explosiveThreshold;
    public int ExplosiveThreshold => _explosiveThreshold;

    [SerializeField]
    private int _electricThreshold;
    public int ElectricThreshold => _electricThreshold;

    public int GetResistance(DamageType damageType)
    {
        return damageType switch
        {
            DamageType.Normal => _resistNormal,
            DamageType.Laser => _resistLaser,
            DamageType.Fire => _resistFire,
            DamageType.Plasma => _resistPlasma,
            DamageType.Explosive => _resistExplosive,
            DamageType.Electrical => _resistElectric,
            _ => 0
        };
    }
    public int GetThreshold(DamageType damageType)
    {
        return damageType switch
        {
            DamageType.Normal => _normalThreshold,
            DamageType.Laser => _laserThreshold,
            DamageType.Fire => _fireThreshold,
            DamageType.Plasma => _plasmaThreshold,
            DamageType.Explosive => _explosiveThreshold,
            DamageType.Electrical => _electricThreshold,
            _ => 0
        };
    }

    public enum Type
    {
        None = 0,
        Clothing = 1,
        Leather = 2,
        Metal = 3,
        Combat = 4,
        Power = 5
    }
}
