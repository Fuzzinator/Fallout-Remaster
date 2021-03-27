using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 0, fileName = "New Skill", menuName = "ScriptObjs/Single Skill")]
public class Skill : ScriptableObject
{
    public Skills.Type skillName;

    [SerializeField]
    private int _addedValue;

    [TextArea]
    public string description;

    public SPECIAL.Type special1;
    public SPECIAL.Type special2;

    [SerializeField]
    private int _baseLevel;

    public int ModifiedLevel
    {
        get
        {
            var level = _baseLevel+_addedValue;

            if ((int) skillName > 6)
            {
                if (Settings.gameDifficulty == Settings.Difficulty.Easy)
                {
                    level += 20;
                }
                else if (Settings.gameDifficulty == Settings.Difficulty.Hard)
                {
                    level -= 10;
                }
            }
            
            return level;
        }
    }

    [SerializeField]
    private int _percentMod;

    public int PercentMod => _percentMod;

    public void Increment(int value)
    {
        _addedValue += value;
    }
}
