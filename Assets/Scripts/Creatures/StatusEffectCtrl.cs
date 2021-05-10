using System;
using System.Collections;
using System.Collections.Generic;
using ThreePupperStudios.Lockable;
using UnityEngine;
using Random = UnityEngine.Random;

public class StatusEffectCtrl : MonoBehaviour
{
    [SerializeField, Lockable]
    private Creature _creature;

    [SerializeField]
    private List<Effect> _queuedMinuteEffects = new List<Effect>();
    [SerializeField]
    private List<Effect> _queuedHourEffects = new List<Effect>();
    [SerializeField]
    private List<Effect> _queuedDayEffects = new List<Effect>();

    private bool _listeningForMinutes = false;
    private bool _listeningForHours = false;
    private bool _listeningForDays = false;

    private void OnValidate()
    {
        if (_creature == null)
        {
            TryGetComponent(out _creature);
        }
    }

    private void Start()
    {
        if (!_listeningForMinutes && _queuedMinuteEffects.Count > 0)
        {
            WorldClock.Instance.minuteTick += MinutePassedHandler;
            _listeningForMinutes = true;
        }
        if (!_listeningForHours && _queuedHourEffects.Count > 0)
        {
            WorldClock.Instance.hourTick += HourPassedHandler;
            _listeningForHours = true;
        }
        if (!_listeningForDays && _queuedDayEffects.Count > 0)
        {
            WorldClock.Instance.newDay += DayPassedHandler;
            _listeningForDays = true;
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

            var targetList = effect.DelayLengthType switch
            {
                Effect.DelayLength.Minute => _queuedMinuteEffects,
                Effect.DelayLength.Hour => _queuedHourEffects,
                _ => throw new System.ArgumentOutOfRangeException()
            };

            var index = 0;
            while (index < targetList.Count && targetList[index].EffectDelay < effect.EffectDelay)
            {
                index++;
            }
            targetList.Insert(index, effect);
        }

        if (!_listeningForMinutes && _queuedMinuteEffects.Count > 0)
        {
            WorldClock.Instance.minuteTick += MinutePassedHandler;
        }

        if (!_listeningForHours && _queuedHourEffects.Count > 0)
        {
            WorldClock.Instance.hourTick += HourPassedHandler;
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
                    human.poisonLvl = Mathf.Max(human.poisonLvl + effect.MaxEffectVal, 0);
                }
                break;
            }
            case Effect.Type.Radiated:
            {
                if (_creature is Human human)
                {
                    human.radiatedLvl = Mathf.Max(human.radiatedLvl + effect.MaxEffectVal, 0);
                }
                break;
            }
            case Effect.Type.Strength:
                _creature.ModSPECIAL(SPECIAL.Type.Strength, effect.MaxEffectVal);
                break;
            case Effect.Type.Perception:
                _creature.ModSPECIAL(SPECIAL.Type.Perception, effect.MaxEffectVal);
                break;
            case Effect.Type.Endurance:
                _creature.ModSPECIAL(SPECIAL.Type.Endurance, effect.MaxEffectVal);
                break;
            case Effect.Type.Charisma:
                _creature.ModSPECIAL(SPECIAL.Type.Charisma, effect.MaxEffectVal);
                break;
            case Effect.Type.Intelligence:
                _creature.ModSPECIAL(SPECIAL.Type.Intelligence, effect.MaxEffectVal);
                break;
            case Effect.Type.Agility:
                _creature.ModSPECIAL(SPECIAL.Type.Agility, effect.MaxEffectVal);
                break;
            case Effect.Type.Luck:
                _creature.ModSPECIAL(SPECIAL.Type.Luck, effect.MaxEffectVal);
                break;
            default:
                break;
        }
        //_currentEffects
    }

    private void MinutePassedHandler()
    {
        DecrementTime(_queuedMinuteEffects);
    }

    private void HourPassedHandler()
    {
        DecrementTime(_queuedHourEffects);
    }

    private void DayPassedHandler()
    {
        DecrementTime(_queuedDayEffects);
    }

    private void DecrementTime(List<Effect> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var effect = list[i];
            effect.EffectDelay -= 1;
            if (effect.EffectDelay == 0)
            {
                ApplyEffect(effect);
                list.RemoveAt(i);
                i--;
                continue;
            }

            list[i] = effect;
        }

        if (_queuedMinuteEffects.Count == 0)
        {
            WorldClock.Instance.minuteTick -= MinutePassedHandler;
            _listeningForMinutes = false;
        }
        if (_queuedHourEffects.Count == 0)
        {
            WorldClock.Instance.hourTick -= HourPassedHandler;
            _listeningForHours = false;
        }
        if (_queuedDayEffects.Count == 0)
        {
            WorldClock.Instance.newDay -= DayPassedHandler;
            _listeningForDays = false;
        }
    }

    private void OnDestroy()
    {
        WorldClock.Instance.minuteTick -= MinutePassedHandler;
        WorldClock.Instance.hourTick -= HourPassedHandler;
    }

    [System.Serializable]
    public struct Effect
    {
        [SerializeField]
        private int _effectDelay;
        public int EffectDelay
        {
            get => _effectDelay;
            set => _effectDelay = value;
        }

        [SerializeField]
        private DelayLength _delayLength;

        public DelayLength DelayLengthType => _delayLength;

        [SerializeField]
        private int _minEffectVal;

        public int MinEffectVal => _minEffectVal;

        [SerializeField]
        private int _maxEffectVal;

        public int MaxEffectVal => _maxEffectVal;

        [SerializeField]
        private Type _effect;

        public Type EffectType => _effect;

        public enum DelayLength
        {
            None = 0,
            Minute = 1,
            Hour = 60,
            Day = 1440,
        }

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