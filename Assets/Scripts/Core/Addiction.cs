using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Effect = StatusEffectCtrl.Effect;

[System.Serializable]
public class Addiction
{
    [SerializeField]
    private Type _addictionType;
    public Type AddictionType => _addictionType;
    
    [SerializeField]
    private int _addictionChance;
    public int AddictionChance => _addictionChance;
    
    [SerializeField]
    private int _addictWithdrawDelay;
    public int AddictWithdrawDelay => _addictWithdrawDelay;

    [SerializeField]
    private Effect.DelayLength _withdrawDelayLength;
    public Effect.DelayLength WithdrawDelayLength => _withdrawDelayLength;
    [SerializeField]
    private int _addictWithdraw;
    public int AddictWithdraw => _addictWithdraw;

    [SerializeField]
    private Effect.DelayLength _withdrawLength;
    public Effect.DelayLength WithdrawLength => _withdrawLength;
    
    [SerializeField]
    private Effect.EffectDetails[] _effects;
    public Effect.EffectDetails[] Effects => _effects;
    
    public enum Type
    {
        None = 0,
        Alcohol = 2,
        Buffout = 4,
        Mentats = 8,
        NukaCola = 9,
        Psycho = 11,
        RadAway = 13
    }
}
