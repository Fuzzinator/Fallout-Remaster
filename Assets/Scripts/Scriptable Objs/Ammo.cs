using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : Item
{
    [SerializeField]
    private int _acMod;

    public int ACMod => _acMod;

    [SerializeField]
    private int _damageMult;

    //public int DamageMultiplier => _damageMult;

    [SerializeField]
    private int _damageDiv;

    //public int DamageDivisor => _damageDiv;

    public int DamageMod => _damageMult / _damageDiv;
    
    [SerializeField]
    private int _drMod;

    public int DRMod => _drMod;
    public enum Type
    {
        None = 0,
        _223 = 1, //.223 FMJ
        _44Magnum = 2, //.44 Magnum FMJ and .44 Magnum JHP
        _10mm = 3, //10mm AP and 10mm JHP
        _12Gauge = 4, //12 gauge shotgun shell
    }
}
