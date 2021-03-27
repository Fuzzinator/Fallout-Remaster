using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 0, fileName = "New Requirement", menuName = "ScriptObjs/Requirement")]
public class Requirement : ScriptableObject
{
    public int lvl = 1;
    public SPECIAL.Type special;
    public int specialLvl;
    public Skills.Type skill;
    public int skillLvl;
}
