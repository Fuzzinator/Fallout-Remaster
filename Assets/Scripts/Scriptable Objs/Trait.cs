using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using ThreePupperStudios.Lockable;
using UnityEngine;

[CreateAssetMenu(order = 0, fileName = "New Trait", menuName = "ScriptObjs/Trait")]
public class Trait : ScriptableObject
{
    [SerializeField, Lockable]
    private string _name;
    public string Name => _name;

    [SerializeField]
    private Type _trait;
    public Type TraitType => _trait;

    [SerializeField, TextArea]
    private string _description;
    public string Description => _description;

    [SerializeField]
    private ModType _benefit;
    public ModType Benefit => _benefit;

    [SerializeField]
    private ModType _penalty;
    public ModType Penalty => _penalty;

    [SerializeField]
    private int _benefitAmount;
    public int BenefitAmount => _benefitAmount;

    [SerializeField]
    private int _penaltyAmount;
    public int PenaltyAmount => _penaltyAmount;

    private void OnValidate()
    {
        if (string.IsNullOrEmpty(_name))
        {
            var nameVal = _trait.ToString();
            var culture = new CultureInfo("en-US", false).TextInfo;
            _name = culture.ToTitleCase(nameVal);
        }
    }

    public enum Type
    {
        None = 0,
        BloodyMess = 1,
        Bruiser = 2,
        ChemReliant = 3,
        ChemResistant = 4,
        FastMetabolism = 5,
        FastShot = 6,
        Finesse = 7,
        Gifted = 8,
        GoodNatured = 9,
        HeavyHanded = 10,
        Jinxed = 11,
        Kamikaze = 12,
        NightPerson = 13,
        OneHander = 14,
        Skilled = 15,
        SmallFrame = 16
    }
}
