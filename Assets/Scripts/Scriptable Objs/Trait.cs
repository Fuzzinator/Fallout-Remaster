using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 0, fileName = "New Trait", menuName = "ScriptObjs/Trait")]
public class Trait : ScriptableObject
{
    public string traitName;

    public bool affectsSkills;

    public ModType modType;
    public Skills.Type affectedSkill = Skills.Type.None;
    public SPECIAL.Type affectedSpecial = SPECIAL.Type.None;
    public int effectAmount;

    //Figure This out Later
    //Bonus
    //Penalty

    public enum Type
    {
        None = 0
    }
}
