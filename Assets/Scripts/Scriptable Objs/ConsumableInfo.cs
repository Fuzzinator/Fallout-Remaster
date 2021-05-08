using Serializable = System.SerializableAttribute;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 20, fileName = "New Consumable", menuName = "ScriptObjs/Consumables")]
public class ConsumableInfo : ItemInfo
{
    [SerializeField]
    private Effect[] _effects = new Effect[0];
    public Effect[] Effects => _effects;

    [SerializeField]
    private Addiction _addiction;
    public Addiction AddictionType => _addiction;

    [SerializeField]
    private int _addictionChance;
    public int AddictionChance => _addictionChance;

    public enum Addiction
    {
        None = 0,
        Alcohol = 2,
        Buffout = 4,
        Mentats = 8,
        NukaCola = 9,
        NukaColaQuantum = 11,
        RadAway = 13
    }

    public enum ModType
    {
        None = 0,
        Antidote = 1,
        Beer = 2,
    }

    [Serializable]
    public struct Effect
    {
        [SerializeField]
        private int _effectDelay;
        public int EffectDelay => _effectDelay;
        
        [SerializeField]
        private int _minEffectVal;
        public int MinEffectVal => _minEffectVal;
    
        [SerializeField]
        private int _maxEffectVal;
        public int MaxEffectVal => _maxEffectVal;

        [SerializeField]
        private Type _effect;
        public Type EffectType => _effect;

        public enum Type
        {
            None = 0,
            DamageResistance,
            RadiationResistance,
            HitPoints,
            Poison,
            Radiated,
            
            Strength = 10,
            Perception = 11,
            Endurance = 12,
            Charisma = 13,
            Intelligence = 14,
            Agility = 15,
            Luck = 16,
        }
    }
}
