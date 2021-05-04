using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 15, fileName = "New Ammo", menuName = "ScriptObjs/Ammo")]
public class AmmoInfo : ItemInfo
{
    [SerializeField]
    private int _acMod;
    public int ACMod => _acMod;

    [SerializeField]
    private int _damageMult;
    [SerializeField]
    private int _damageDiv;
    public float DamageMod => _damageMult / (float)_damageDiv;

    [SerializeField]
    private int _magSize;
    public int MagSize => _magSize;
    
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
