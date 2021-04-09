using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 0, fileName = "New Skills", menuName = "ScriptObjs/Skills")]
public class Skills : ScriptableObject
{
    [SerializeField]
    private SPECIAL _specials;

    [SerializeField]
    private List<Skill> _skills = new List<Skill>();
    public List<Skill> CurrentSkills => _skills;

    public Type tag1;
    public Type tag2;
    public Type tag3 = Type.None;

    #region Consts

    private const int MAXSKILLLVL = 200;
    

    #endregion

    public int GetSkillLvl(Type type)
    {
        var skill = _skills.Find(i => i.skillName == type);
        if (skill == null)
        {
            return 0;
        }
        
        var skillAmount = _specials.GetSPECIALLvl(skill.special1) + _specials.GetSPECIALLvl(skill.special2);
        if (skill.special2 != SPECIAL.Type.None)
        {
            skillAmount = Mathf.FloorToInt(skillAmount * .5f);
        }

        skillAmount *= skill.PercentMod;
        skillAmount += skill.ModifiedLevel;

        if (tag1 == type || tag2 == type || tag3 == type)
        {
            skillAmount += 20;
        }
        
        return skillAmount;

    }

    public bool IncrementSkill(Type type, bool increase)
    {
        var increment = increase ? 1 : -1;
        if (tag1 == type || tag2 == type|| tag3 == type)
        {
            increment *= 2;
        }

        var skill = _skills.Find(i => i.skillName == type);
        if (skill == null)
        {
            return false;
        }

        if (skill.ModifiedLevel + increment > MAXSKILLLVL)
        {
            if (increment == 2)
            {
                increment = 1;
                if (skill.ModifiedLevel + increment > MAXSKILLLVL)
                {
                    return false;
                }
            }
        }
        skill.Increment(increment);
        return true;
    }

    public enum Type
    {
        None = 0,

        //Combat Skills
        SmallGuns = 1,
        BigGuns = 2,
        EnergyWeapons = 3,
        Unarmed = 4,
        MeleeWeapons = 5,
        Throwing = 6,

        //Active Skills
        FirstAid = 7,
        Doctor = 8,
        Sneak = 9,
        Lockpick = 10,
        Steal = 11,
        Traps = 12,
        Science = 13,
        Repair = 14,

        //Passive Skills
        Speech = 15,
        Barter = 16,
        Gambling = 17,
        Outdoorsman = 18
    }
}