using System.Collections;
using System.Collections.Generic;
using ThreePupperStudios.Lockable;
using UnityEngine;

public class StatusEffectCtrl : MonoBehaviour
{
    [SerializeField, Lockable]
    private Creature _creature;

    [SerializeField]
    private List<Effect> _queuedEffects = new List<Effect>();

    [SerializeField]
    private List<Effect> _currentEffects = new List<Effect>();

    private void OnValidate()
    {
        if (_creature == null)
        {
            TryGetComponent(out _creature);
        }
    }

    public void QueueEffects(Effect[] effects)
    {
        foreach (var effect in effects)
        {
            if (effect.EffectDelay == 0)
            {
                ApplyEffect(effect);
                continue;
            }
            var index = 0;
            while (index < _queuedEffects.Count && _queuedEffects[index].EffectDelay < effect.EffectDelay)
            {
                index++;
            }
            _queuedEffects.Insert(index, effect);
        }
    }

    private void ApplyEffect(Effect effect)
    {
        switch (effect.EffectType)
        {
            case Effect.Type.None:
                break;
            case Effect.Type.DamageResistance:
                _creature.damageResistMod += effect.MaxEffectVal;
                break;
            case Effect.Type.RadiationResistance:
                _creature.radResistMod += effect.MaxEffectVal;
                break;
            case Effect.Type.HitPoints:
                _creature.GainCurrentHP(Random.Range(effect.MinEffectVal, effect.MaxEffectVal));
                break;
            case Effect.Type.Poison:
            {
                if (_creature is Human human)
                {
                    human.poisonLvl += effect.MaxEffectVal;
                }

                break;
            }
            case Effect.Type.Radiated:
            {
                if (_creature is Human human)
                {
                    human.poisonLvl += effect.MaxEffectVal;
                }
                break;
            }
            case Effect.Type.Strength:
                break;
            case Effect.Type.Perception:
                break;
            case Effect.Type.Endurance:
                break;
            case Effect.Type.Charisma:
                break;
            case Effect.Type.Intelligence:
                break;
            case Effect.Type.Agility:
                break;
            case Effect.Type.Luck:
                break;
            default:
                break;
        }
        //_currentEffects
    }


    [System.Serializable]
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