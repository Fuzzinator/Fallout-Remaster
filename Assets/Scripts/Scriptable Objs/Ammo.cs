using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ammo : Item
{
    
    public enum Type
    {
        None = 0,
        _223 = 1, //.223 FMJ
        _44Magnum = 2, //.44 Magnum FMJ and .44 Magnum JHP
        _10mm = 3, //10mm AP and 10mm JHP
        _12Gauge = 4, //12 gauge shotgun shell
    }
}
