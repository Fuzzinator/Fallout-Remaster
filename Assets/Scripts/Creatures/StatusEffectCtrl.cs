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
    private List<System.Tuple<ConsumableInfo.Type, Effect>> _queuedMinuteEffects =
        new List<System.Tuple<ConsumableInfo.Type, Effect>>();

    [SerializeField]
    private List<System.Tuple<ConsumableInfo.Type, Effect>>
        _queuedHourEffects = new List<System.Tuple<ConsumableInfo.Type, Effect>>();

    [SerializeField]
    private List<System.Tuple<ConsumableInfo.Type, Effect>> _queuedDayEffects =
        new List<System.Tuple<ConsumableInfo.Type, Effect>>();

    [SerializeField]
    private List<Addiction> _activeAddictions = new List<Addiction>();

    private bool _listeningForMinutes = false;
    private bool _listeningForHours = false;
    private bool _listeningForDays = false;

    [SerializeField, HideInInspector]
    private int _radiationDeathTimer = ONEDAY;

    private Dictionary<ConsumableInfo.Type, Effect[]> _activeEffects = new Dictionary<ConsumableInfo.Type, Effect[]>();

    private const int ONEDAY = 24;
    
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

    public void QueueEffects(ConsumableInfo consumableInfo)
    {
        if (!consumableInfo.Stackable && _activeEffects.ContainsKey(consumableInfo.ConsumableType))
        {
            for (var i = 0; i < _activeEffects[consumableInfo.ConsumableType].Length; i++)
            {
                var effect = _activeEffects[consumableInfo.ConsumableType][i];
                if (effect == null)
                {
                    continue;
                }

                var tup = _queuedMinuteEffects.Find(j => j.Item2 == effect);
                if (tup != null)
                {
                    _queuedMinuteEffects.Remove(tup);
                }

                tup = _queuedHourEffects.Find(j => j.Item2 == effect);
                if (tup != null)
                {
                    _queuedHourEffects.Remove(tup);
                }

                tup = _queuedDayEffects.Find(j => j.Item2 == effect);
                if (tup != null)
                {
                    _queuedDayEffects.Remove(tup);
                }

                ApplyEffect(effect.EffectDetailsArray);
            }

            _activeEffects.Remove(consumableInfo.ConsumableType);
        }

        _activeEffects[consumableInfo.ConsumableType] = Effect.GetNewEffects(consumableInfo.Effects);

        for (var i = 0; i < _activeEffects[consumableInfo.ConsumableType].Length; i++)
        {
            var effect = _activeEffects[consumableInfo.ConsumableType][i];

            if (effect.EffectDelay == 0)
            {
                ApplyEffect(effect.EffectDetailsArray);
                continue;
            }

            var targetList = effect.DelayLengthType switch
            {
                Effect.DelayLength.Minute => _queuedMinuteEffects,
                Effect.DelayLength.Hour => _queuedHourEffects,
                Effect.DelayLength.Day => _queuedDayEffects,
                _ => throw new System.ArgumentOutOfRangeException()
            };

            var index = 0;
            while (index < targetList.Count && targetList[index].Item2.EffectDelay < effect.EffectDelay)
            {
                index++;
            }

            targetList.Insert(index,
                new System.Tuple<ConsumableInfo.Type, Effect>(consumableInfo.ConsumableType, effect));
        }

        if (_creature is Player player && consumableInfo.AddictionType != Addiction.Type.None)
        {
            var indexOfExisting = _activeAddictions.FindIndex(i => i.AddictionType == consumableInfo.AddictionType);
            if (indexOfExisting >= 0)
            {
                ResetAddiction(_activeAddictions[indexOfExisting], consumableInfo.Addiction);
            }
            else
            {
                var addictionChance = Random.Range(0, 100);
                if(player.)
                //TODO if human has addiction related perk or traits apply logic here
                if (addictionChance <= consumableInfo.Addiction.AddictionChance)
                {
                    TriggerAddiction(consumableInfo.Addiction);
                }
            }
        }

        if (!_listeningForMinutes && (_queuedMinuteEffects.Count > 0 ||
                                      _activeAddictions.Exists(i =>
                                          AddictionMatchesType(i, Effect.DelayLength.Minute))))
        {
            WorldClock.Instance.minuteTick += MinutePassedHandler;
            _listeningForMinutes = true;
        }

        if (!_listeningForHours && (_queuedHourEffects.Count > 0 ||
                                    _activeAddictions.Exists(i => AddictionMatchesType(i, Effect.DelayLength.Hour))))
        {
            WorldClock.Instance.hourTick += HourPassedHandler;
            _listeningForHours = true;
        }

        if (!_listeningForDays && (_queuedDayEffects.Count > 0 ||
                                   _activeAddictions.Exists(i => AddictionMatchesType(i, Effect.DelayLength.Day))))
        {
            WorldClock.Instance.newDay += DayPassedHandler;
            _listeningForDays = true;
        }
    }

    private void ApplyEffect(IEnumerable<Effect.EffectDetails> effect)
    {
        foreach (var details in effect)
        {
            switch (details.EffectType)
            {
                case Effect.Type.None:
                case Effect.Type.NukaCola:
                    break;
                case Effect.Type.DamageResistance:
                    _creature.damageResistMod += details.MaxEffectVal;
                    break;
                case Effect.Type.RadiationResistance:
                    _creature.radResistMod += details.MaxEffectVal;
                    break;
                case Effect.Type.HitPoints:
                    _creature.GainCurrentHP(Random.Range(details.MinEffectVal, details.MaxEffectVal));
                    break;
                case Effect.Type.Poison:
                {
                    if (_creature is Human human)
                    {
                        human.poisonLvl = Mathf.Max(human.poisonLvl + details.MaxEffectVal, 0);
                    }

                    break;
                }
                case Effect.Type.Radiated:
                {
                    if (_creature is Human human)
                    {
                        human.UpdateRadiationLvl(details.MaxEffectVal);
                    }

                    break;
                }
                case Effect.Type.Strength:
                    _creature.ModSPECIAL(SPECIAL.Type.Strength, details.MaxEffectVal);
                    break;
                case Effect.Type.Perception:
                    _creature.ModSPECIAL(SPECIAL.Type.Perception, details.MaxEffectVal);
                    break;
                case Effect.Type.Endurance:
                    _creature.ModSPECIAL(SPECIAL.Type.Endurance, details.MaxEffectVal);
                    break;
                case Effect.Type.Charisma:
                    _creature.ModSPECIAL(SPECIAL.Type.Charisma, details.MaxEffectVal);
                    break;
                case Effect.Type.Intelligence:
                    _creature.ModSPECIAL(SPECIAL.Type.Intelligence, details.MaxEffectVal);
                    break;
                case Effect.Type.Agility:
                    _creature.ModSPECIAL(SPECIAL.Type.Agility, details.MaxEffectVal);
                    break;
                case Effect.Type.Luck:
                    _creature.ModSPECIAL(SPECIAL.Type.Luck, details.MaxEffectVal);
                    break;
                default:
                    break;
            }
        }

        //_currentEffects
    }

    private void ApplyInvertedEffects(Effect.EffectDetails[] effects)
    {
        var invertedEffects = effects;
        for (var j = 0; j < invertedEffects.Length; j++)
        {
            var effect = invertedEffects[j];
            effect.MinEffectVal *= -1;
            effect.MaxEffectVal *= -1;
            invertedEffects[j] = effect;
        }

        ApplyEffect(invertedEffects);
    }

    private void MinutePassedHandler()
    {
        DecrementEffectTime(_queuedMinuteEffects);
        if (_activeAddictions.Count > 0 &&
            _activeAddictions.Exists(i => AddictionMatchesType(i, Effect.DelayLength.Minute)))

        {
            UpdateAddictions(Effect.DelayLength.Minute);
        }
    }

    private void HourPassedHandler()
    {
        DecrementEffectTime(_queuedHourEffects);
        if (_activeAddictions.Count > 0 &&
            _activeAddictions.Exists(i => AddictionMatchesType(i, Effect.DelayLength.Hour)))
        {
            UpdateAddictions(Effect.DelayLength.Hour);
        }
    }

    private void DayPassedHandler()
    {
        DecrementEffectTime(_queuedDayEffects);
        if (_activeAddictions.Count > 0 &&
            _activeAddictions.Exists(i => AddictionMatchesType(i, Effect.DelayLength.Day)))
        {
            UpdateAddictions(Effect.DelayLength.Day);
        }
    }

    private void DecrementEffectTime(IList<System.Tuple<ConsumableInfo.Type, Effect>> list)
    {
        for (var i = 0; i < list.Count; i++)
        {
            var effect = list[i].Item2;
            effect.EffectDelay -= 1;
            if (effect.EffectDelay == 0)
            {
                ApplyEffect(effect.EffectDetailsArray);
                list.RemoveAt(i);
                RemoveFromDictionary(list[i]);
                i--;
                continue;
            }
        }

        if (_queuedMinuteEffects.Count == 0 &&
            !_activeAddictions.Exists(i => AddictionMatchesType(i, Effect.DelayLength.Minute)))
        {
            WorldClock.Instance.minuteTick -= MinutePassedHandler;
            _listeningForMinutes = false;
        }

        if (_queuedHourEffects.Count == 0 &&
            !_activeAddictions.Exists(i => AddictionMatchesType(i, Effect.DelayLength.Hour)))
        {
            WorldClock.Instance.hourTick -= HourPassedHandler;
            _listeningForHours = false;
        }

        if (_queuedDayEffects.Count == 0 &&
            !_activeAddictions.Exists(i => AddictionMatchesType(i, Effect.DelayLength.Day)))
        {
            WorldClock.Instance.newDay -= DayPassedHandler;
            _listeningForDays = false;
        }
    }

    private static bool AddictionMatchesType(Addiction addiction, Effect.DelayLength length)
    {
        var info = addiction.GetWithdrawInfo;
        if (!info.nowInWithdraw)
        {
            return addiction.WithdrawDelayLength == length;
        }
        else
        {
            return addiction.WithdrawLength == length;
        }
    }

    private void UpdateAddictions(Effect.DelayLength length)
    {
        for (var i = 0; i < _activeAddictions.Count; i++)
        {
            var addiction = _activeAddictions[i];
            var passedTime = addiction.TryPassTime(-1, length, out var withdrawInfo);
            if (!passedTime || !withdrawInfo.nowInWithdraw)
            {
                continue;
            }

            if (!withdrawInfo.wasInWithdraw)
            {
                ApplyEffect(addiction.Effects);
            }
            else if (withdrawInfo.withdrawEnding)
            {
                ApplyInvertedEffects(addiction.Effects);
                _activeAddictions.Remove(addiction);
                i--;
            }
        }
    }

    private void RemoveFromDictionary(System.Tuple<ConsumableInfo.Type, Effect> effect)
    {
        var item1 = effect.Item1;
        var item2 = effect.Item2;
        if (!_activeEffects.ContainsKey(item1))
        {
            return;
        }

        var allNull = true;
        var array = _activeEffects[item1];
        for (var i = 0; i < array.Length; i++)
        {
            if (array[i] == item2)
            {
                _activeEffects[item1][i] = null;
            }

            if (array[i] != null)
            {
                allNull = false;
            }
        }

        if (allNull)
        {
            _activeEffects.Remove(item1);
        }
    }

    private static void ResetAddiction(Addiction existingAddiction, Addiction newAddiction)
    {
        existingAddiction.SetValues(newAddiction);
    }

    private void TriggerAddiction(Addiction addiction)
    {
        _activeAddictions.Add(new Addiction(addiction));
        //TODO finish this logic
    }

    public void StartRadDeathTimer()
    {
        _radiationDeathTimer = ONEDAY;
        WorldClock.Instance.hourTick += CountDownRadDeathTimer;
    }

    public void StopRadDeathTimer()
    {
        WorldClock.Instance.hourTick -= CountDownRadDeathTimer;
        _radiationDeathTimer = ONEDAY;
    }

    private void CountDownRadDeathTimer()
    {
        _radiationDeathTimer--;
        if (_radiationDeathTimer <= 0)
        {
            WorldClock.Instance.hourTick -= CountDownRadDeathTimer;
            _creature.TakeDamage(999999);
        }
    }

    private void OnDestroy()
    {
        WorldClock.Instance.minuteTick -= MinutePassedHandler;
        WorldClock.Instance.hourTick -= HourPassedHandler;
        WorldClock.Instance.newDay -= DayPassedHandler;
        WorldClock.Instance.hourTick -= CountDownRadDeathTimer;
    }

    [System.Serializable]
    public class Effect
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
        private EffectDetails[] _effectDetails;

        public EffectDetails[] EffectDetailsArray => _effectDetails;

        public static Effect[] GetNewEffects(IReadOnlyList<Effect> effects)
        {
            var newEffects = new Effect[effects.Count];
            for (var i = 0; i < effects.Count; i++)
            {
                newEffects[i] = new Effect(effects[i]);
            }

            return newEffects;
        }

        public Effect(int delay, DelayLength length, int minVal, int maxVal, Type effect)
        {
            _effectDelay = delay;
            _delayLength = length;
            _effectDetails = new[]
            {
                new EffectDetails(minVal, maxVal, effect)
            };
        }

        public Effect(Effect effect)
        {
            _effectDelay = effect._effectDelay;
            _delayLength = effect._delayLength;
            _effectDetails = effect.EffectDetailsArray;
        }

        public Effect(EffectDetails[] details)
        {
            _effectDetails = details;
        }

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

            NukaCola = 20
        }

        [System.Serializable]
        public struct EffectDetails
        {
            [SerializeField]
            private int _minEffectVal;

            public int MinEffectVal
            {
                get => _minEffectVal;
                set => _minEffectVal = value;
            }

            [SerializeField]
            private int _maxEffectVal;

            public int MaxEffectVal
            {
                get => _maxEffectVal;
                set => _maxEffectVal = value;
            }

            [SerializeField]
            private Type _effect;

            public Type EffectType => _effect;

            public EffectDetails(int min, int max, Type effect)
            {
                _minEffectVal = min;
                _maxEffectVal = max;
                _effect = effect;
            }
        }
    }
}