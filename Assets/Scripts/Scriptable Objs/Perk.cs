using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 0, fileName = "New Perk", menuName = "ScriptObjs/Perk")]
public class Perk : ScriptableObject
{
    public string perkName;
    public string perkDescription;

    public List<Requirement> requirements = new List<Requirement>();

    public int ranks = 1;
    
    public ModType modType;
    public Skills.Type affectedSkill;
    [SerializeField]
    private int effectAmout;
    public int EffectAmount => effectAmout*ranks;
    
    
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
    HPBase = 10,
    HPLvlInc = 11,
    HPRecover = 12,
    SkillLvlInc = 15
}
