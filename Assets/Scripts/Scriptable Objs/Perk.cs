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
    
    public PropType affectedProp;
    public Skills.Type affectedSkill;
    public int effectAmount;
    
    public enum PropType
    {
        None = 0,
        Strength = 1,
        Perception = 2,
        Endurance = 3,
        Charisma = 4,
        Intelligence = 5,
        Agility = 6,
        Luck = 7,
        HPBase = 10,
        HPLvlIncrease = 11,
        HPRecover = 12
    }
}
