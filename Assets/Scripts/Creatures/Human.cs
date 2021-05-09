using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Human : Creature
{
    [SerializeField]
    protected Gender _gender = Gender.Male;

    [SerializeField]
    protected int _age = 25;//This is the player default

    public int poisonLvl;

    public int radiatedLvl;
    
    [SerializeField]
    protected int _tempRadResist = 0;//Set by taking Rad-X
    
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

    //Need to make an actual drug system
    protected IEnumerator TurnDownRadResist()
    {
        yield return null;//Make this the length of how 24 hours. Which is rad-X half life
        _tempRadResist -= 50;//Value 1/2 of 1 Rad-X
    }
}
