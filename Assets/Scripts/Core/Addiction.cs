using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;
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
    [FormerlySerializedAs("_addictWithdrawDelay")]
    private int _withdrawDelay;

    public int WithdrawDelay => _withdrawDelay;

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

    [SerializeField]
    private bool _withdrawStarted = false;

    public WithdrawInfo GetWithdrawInfo => new WithdrawInfo()
    {
        wasInWithdraw = _withdrawDelay <= 0,
        nowInWithdraw = _withdrawStarted,
        withdrawEnding = _addictWithdraw <= 0
    };

    public void SetValues(Addiction newAddiction)
    {
        _withdrawDelay = newAddiction._withdrawDelay;
        _addictWithdraw = newAddiction._addictWithdraw;
        _withdrawStarted = newAddiction._withdrawStarted;
    }

    public bool TryPassTime(int amount, Effect.DelayLength length, out WithdrawInfo withdrawInfo)
    {
        withdrawInfo = new WithdrawInfo
        {
            nowInWithdraw = _withdrawStarted,
            wasInWithdraw = _withdrawStarted
        };
        
        if (!_withdrawStarted)
        {
            if (_withdrawDelayLength != length)
            {
                return false;
            }

            _withdrawDelay -= amount;
            if (_withdrawDelay <= 0)
            {
                _withdrawStarted = true;
                withdrawInfo.nowInWithdraw = true;
            }
        }
        else
        {
            if (_withdrawLength != length)
            {
                return false;
            }

            _addictWithdraw -= amount;
            if (_addictWithdraw <= 0)
            {
                withdrawInfo.withdrawEnding = true;
            }
        }

        return true;
    }

    public struct WithdrawInfo
    {
        public bool wasInWithdraw;
        public bool nowInWithdraw;
        public bool withdrawEnding;
    }

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

    public Addiction(Addiction addiction)
    {
        _addictionType = addiction._addictionType;
        _addictionChance = addiction._addictionChance;
        _withdrawDelay = addiction._withdrawDelay;
        _withdrawDelayLength = addiction._withdrawDelayLength;
        _addictWithdraw = addiction._addictWithdraw;
        _withdrawLength = addiction._withdrawLength;
        _effects = addiction._effects;
    }
}