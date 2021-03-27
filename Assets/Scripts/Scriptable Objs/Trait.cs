using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 0, fileName = "New Trait", menuName = "ScriptObjs/Trait")]
public class Trait : ScriptableObject
{
    public string traitName;
    //Figure This out Later
    //Bonus
    //Penalty

    public enum Type
    {
        None = 0
    }
}
