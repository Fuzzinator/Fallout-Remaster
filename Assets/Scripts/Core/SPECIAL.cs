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
    private int _baseLuck;

    [SerializeField, Lockable]
    private int _lucMod;

    public int Luck => _baseLuck + _lucMod;
}