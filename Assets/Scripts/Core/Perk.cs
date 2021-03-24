using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 0, fileName = "New Perk", menuName = "ScriptObjs/Perk")]
public class Perk : ScriptableObject
{
    public string perkName;
    public string perkDescription;
    public Type affectedSpecial;
    
    public enum Type
    {
        Strength,
        Perception,
        Endurance,
        Charisma,
        Intelligence,
        Luck,
        HP,
        AP,
        Skill
    }
}
