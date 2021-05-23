using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Creature
{
    [SerializeField]
    protected Gender _gender = Gender.Male;

    [SerializeField]
    protected int _age = 25; //This is the player default

    public int poisonLvl;

    [SerializeField]
    private int _radiatedLvl;

    [SerializeField, HideInInspector]
    private RadiatedLevel _currentRadiatedLevel;

    [SerializeField]
    protected int _tempRadResist = 0; //Set by taking Rad-X

    protected virtual int CarryWeight => 25 + (_special.Strength * 25);

    public override int RadResistance => base.RadResistance + RadResistMod();

    protected virtual int PoisonResist => _special.Endurance * 5;

    protected virtual int RadResistMod()
    {
        var radResist = radResistMod;

        if (equipedArmorInfo != null)
        {
            radResist += equipedArmorInfo.RadResist;
        }

        radResist += _tempRadResist;

        return radResist;
    }

    public virtual void UpdateRadiationLvl(int modifier)
    {
        var currentLvl = _radiatedLvl;
        _radiatedLvl += modifier;
        if (_radiatedLvl < 0)
        {
            _radiatedLvl = 0;
        }

        var radLvlIncreased = false;
        var radLvlDecreased = false;
        while (_currentRadiatedLevel != RadiatedLevel.IntenseAgony && _radiatedLvl >= (int) (_currentRadiatedLevel + 1))
        {
            _currentRadiatedLevel += 1;
            radLvlIncreased = true;
        }

        while (_currentRadiatedLevel != RadiatedLevel.None && _radiatedLvl < (int) (_currentRadiatedLevel - 1))
        {
            _currentRadiatedLevel -= 1;
            radLvlDecreased = true;
        }

        switch (_currentRadiatedLevel)
        {
            case RadiatedLevel.None:
                if (radLvlDecreased)
                {
                    ModSPECIAL(SPECIAL.Type.Strength, 1);
                }

                break;
            case RadiatedLevel.SlightlyFatigued:
                if (radLvlDecreased)
                {
                    healRateMod += 3;
                    ModSPECIAL(SPECIAL.Type.Agility, 1);
                }
                else if (radLvlIncreased)
                {
                    ModSPECIAL(SPECIAL.Type.Strength, -1);
                }

                break;
            case RadiatedLevel.VomitingDoesNotStop:
                if (radLvlDecreased)
                {
                    healRateMod += 2;
                    ModSPECIAL(SPECIAL.Type.Strength, 1);
                    ModSPECIAL(SPECIAL.Type.Endurance, 1);
                    ModSPECIAL(SPECIAL.Type.Agility, 1);
                }
                else if (radLvlIncreased)
                {
                    healRateMod -= 3;
                    ModSPECIAL(SPECIAL.Type.Agility, -1);
                }

                break;
            case RadiatedLevel.HairFallingOut:
                if (radLvlDecreased)
                {
                    healRateMod += 5;
                    ModSPECIAL(SPECIAL.Type.Strength, 2);
                    ModSPECIAL(SPECIAL.Type.Perception, 3);
                    ModSPECIAL(SPECIAL.Type.Endurance, 2);
                    ModSPECIAL(SPECIAL.Type.Charisma, 3);
                    ModSPECIAL(SPECIAL.Type.Intelligence, 1);
                    ModSPECIAL(SPECIAL.Type.Agility, 3);
                }
                else if (radLvlIncreased)
                {
                    healRateMod -= 2;
                    TakeDamage(5);
                    ModSPECIAL(SPECIAL.Type.Strength, -1);
                    ModSPECIAL(SPECIAL.Type.Endurance, -1);
                    ModSPECIAL(SPECIAL.Type.Agility, -1);
                }

                break;
            case RadiatedLevel.SkinFallingOff:
                if (radLvlDecreased)
                {
                    ModSPECIAL(SPECIAL.Type.Strength, 2);
                    ModSPECIAL(SPECIAL.Type.Perception, 2);
                    ModSPECIAL(SPECIAL.Type.Endurance, 2);
                    ModSPECIAL(SPECIAL.Type.Charisma, 2);
                    ModSPECIAL(SPECIAL.Type.Intelligence, 2);
                    ModSPECIAL(SPECIAL.Type.Agility, 1);
                }
                else if (radLvlIncreased)
                {
                    healRateMod -= 5;
                    TakeDamage(15);
                    ModSPECIAL(SPECIAL.Type.Strength, -2);
                    ModSPECIAL(SPECIAL.Type.Perception, -3);
                    ModSPECIAL(SPECIAL.Type.Endurance, -2);
                    ModSPECIAL(SPECIAL.Type.Charisma, -3);
                    ModSPECIAL(SPECIAL.Type.Intelligence, -1);
                    ModSPECIAL(SPECIAL.Type.Agility, -3);
                }

                break;
            case RadiatedLevel.IntenseAgony:
                if (radLvlIncreased)
                {
                    //Start death timer
                    _statusCtrl.StartRadDeathTimer();

                    TakeDamage(20);
                    ModSPECIAL(SPECIAL.Type.Strength, -2);
                    ModSPECIAL(SPECIAL.Type.Perception, -2);
                    ModSPECIAL(SPECIAL.Type.Endurance, -2);
                    ModSPECIAL(SPECIAL.Type.Charisma, -2);
                    ModSPECIAL(SPECIAL.Type.Intelligence, -2);
                    ModSPECIAL(SPECIAL.Type.Agility, -1);
                }

                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (_radiatedLvl < (int) RadiatedLevel.IntenseAgony)
        {
            _statusCtrl.StopRadDeathTimer();
        }
        
        if (_special.Strength <= 0 || _special.Perception <= 0 || _special.Endurance <= 0 ||
            _special.Charisma <= 0 || _special.Intelligence <= 0 || _special.Agility <= 0 || _special.Luck <= 0)
        {
            TakeDamage(999999);
        }

        switch (true)
        {
            case var b when _radiatedLvl >= 0 && _radiatedLvl < (int) RadiatedLevel.SlightlyFatigued:
                break;
            case var b when _radiatedLvl >= (int) RadiatedLevel.SlightlyFatigued &&
                            _radiatedLvl < (int) RadiatedLevel.VomitingDoesNotStop:
                break;
            case var b when _radiatedLvl >= (int) RadiatedLevel.VomitingDoesNotStop &&
                            _radiatedLvl < (int) RadiatedLevel.HairFallingOut:
                break;
            case var b when _radiatedLvl >= (int) RadiatedLevel.HairFallingOut &&
                            _radiatedLvl < (int) RadiatedLevel.SkinFallingOff:
                break;
            case var b when _radiatedLvl >= (int) RadiatedLevel.SkinFallingOff &&
                            _radiatedLvl < (int) RadiatedLevel.IntenseAgony:
                break;
            case var b when _radiatedLvl >= (int) RadiatedLevel.IntenseAgony:
                break;
        }
    }

    private enum RadiatedLevel
    {
        None = 0,
        SlightlyFatigued = 150,
        VomitingDoesNotStop = 300,
        HairFallingOut = 450,
        SkinFallingOff = 600,
        IntenseAgony = 1000
    }
}