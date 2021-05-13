using System;
using Serializable = System.SerializableAttribute;
using Effect = StatusEffectCtrl.Effect;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 20, fileName = "New Consumable", menuName = "ScriptObjs/Consumables")]
public class ConsumableInfo : ItemInfo
{
    [SerializeField]
    private Type _type;
    public Type ConsumableType => _type;
    
    [SerializeField]
    private Effect[] _effects = new Effect[0];
    public Effect[] Effects => _effects;

    [SerializeField]
    private Addiction _addiction;
    public Addiction Addiction => _addiction;
    public Addiction.Type AddictionType => _addiction.AddictionType;

    public enum Type
    {
        None = 0,
        Antidote = 1,
        Beer = 2,
        Booze = 3,
        Buffout = 4,
        Fruit = 5,
        IguanaOnAStick = 6,
        Mentats = 7,
        NukaCola = 8,
        Psycho = 9,
        RadAway = 10,
        RadX = 11,
        Stimpak = 12,
        SuperStimpak = 13
    }
}
