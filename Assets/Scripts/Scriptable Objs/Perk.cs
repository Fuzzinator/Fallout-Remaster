using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 0, fileName = "New Perk", menuName = "ScriptObjs/Perk")]
public class Perk : ScriptableObject
{
    [SerializeField]
    private string _perkName;
    public string PerkName => _perkName;

    [SerializeField]
    private string _perkDescription;
    public string PerkDescription => _perkDescription;

    [SerializeField]
    private List<Requirement> _requirements = new List<Requirement>();
    public List<Requirement> Requirements => _requirements;

    [SerializeField]
    private int _ranks = 1;
    public int Ranks => _ranks;

    [SerializeField]
    private ModType _modType;
    public ModType ModType => _modType;

    [SerializeField]
    private Skills.Type _affectedSkill = Skills.Type.None;
    public Skills.Type AffectedSkill => _affectedSkill;

    [SerializeField]
    private WeaponInfo.Type _affectedWeapon = WeaponInfo.Type.None;
    public WeaponInfo.Type AffectedWeapon => _affectedWeapon;

    [SerializeField]
    private ArmorInfo.Type _affectedArmor = ArmorInfo.Type.None;
    public ArmorInfo.Type AffectedArmor => _affectedArmor;
    
    [SerializeField]
    private int effectAmout;
    public int EffectAmount => effectAmout*Ranks;
    
    
}
public enum ModType
{
    None = 0,
    SpecialInc = 1,
    SkillInc = 2,
    ActionPoints = 3,
    CarryWeight = 4,
    CritChance = 5,
    MeleeDamage = 6,
    Sequence = 7,//Sequence is Initiative
    ArmorClass = 8,
    
    HPBase = 10,
    HPLvlInc = 11,
    HPRecover = 12,
    PerkRate = 13,
    SkillLvlInc = 15,
    
    AddictResist = 16,
    RadResist = 17,
    DamageResist = 18,
    PoisonResist = 19,
    
    WeaponSpec = 20,
    ArmorSpec = 21,
    
    Movement = 30
}
