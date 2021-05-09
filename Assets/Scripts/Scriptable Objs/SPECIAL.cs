using System;
using System.Collections;
using System.Collections.Generic;
using ThreePupperStudios.Lockable;
using UnityEngine;

[CreateAssetMenu(order = 0, fileName = "New SPECIAL", menuName = "ScriptObjs/SPECIAL")]
public class SPECIAL : ScriptableObject
{
    [SerializeField, Lockable]
    private int _baseStrength;

    [SerializeField, Lockable]
    private int _strMod;

    public int Strength => _baseStrength + _strMod;

    [Space]
    [SerializeField, Lockable]
    private int _basePerception;

    [SerializeField, Lockable]
    private int _perMod;

    public int Perception => _basePerception + _perMod;

    [Space]
    [SerializeField, Lockable]
    private int _baseEndurance;

    [SerializeField, Lockable]
    private int _endMod;

    public int Endurance => _baseEndurance + _endMod;

    [Space]
    [SerializeField, Lockable]
    private int _baseCharisma;

    [SerializeField, Lockable]
    private int _chaMod;

    public int Charisma => _baseCharisma + _chaMod;

    [Space]
    [SerializeField, Lockable]
    private int _baseIntelligence;

    [SerializeField, Lockable]
    private int _intMod;

    public int Intelligence => _baseIntelligence + _intMod;

    [Space]
    [SerializeField, Lockable]
    private int _baseAgility;

    [SerializeField, Lockable]
    private int _agiMod;

    public int Agility => _baseAgility + _agiMod;
    
    [Space]
    [SerializeField, Lockable]
    private int _baseLuck;

    [SerializeField, Lockable]
    private int _lucMod;

    public int Luck => _baseLuck + _lucMod;

    public int unspentPoints = 0;

    public int GetSPECIALLvl(Type type)
    {
        return type switch
        {
            Type.Strength => Strength,
            Type.Perception => Perception,
            Type.Endurance => Endurance,
            Type.Charisma => Charisma,
            Type.Intelligence => Intelligence,
            Type.Agility => Agility,
            Type.Luck => Luck,
            _ => 0
        };
    }
    
    public void ModSPECIAL(Type type, int value)
    {
        switch(type)
        {
            case Type.Strength:
                _strMod += value;
                break;
            case Type.Perception:
                _perMod += value;
                break;
            case Type.Endurance:
                _endMod += value;
                break;
            case Type.Charisma:
                _chaMod += value;
                break;
            case Type.Intelligence:
                _intMod += value;
                break;
            case Type.Agility:
                _agiMod += value;
                break;
            case Type.Luck:
                _lucMod += value;
                break;
            case Type.None:
            case Type.All:
            default:
                break;
        }
    }
    public enum Type
    {
        None = 0,
        Strength = 1,
        Perception = 2,
        Endurance = 3,
        Charisma = 4,
        Intelligence = 5,
        Agility = 6,
        Luck = 7,
        All = 10
    }
}