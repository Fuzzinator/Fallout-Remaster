using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LoggingManager : MonoBehaviour
{
    #region Constants

    private const string OUTOFRANGE = "Target out of range.";
    private const string YOUR = "Your";
    private const string YOU = "You";
    private const string TARGETBLOCKED = " aim is blocked.";
    private const string POSSESSIVE = "'s";
    private const string ATTACKMISSED = " attack missed.";

    private const string WAS = " was";
    private const string WERE = " were";
    private const string HIT = " hit for ";
    private const string HP = " hit points";

    private const string DIED = " and was killed";
    private const string PERIOD = ".";
    private const string NEEDMOREAP1 = "You need ";
    private const string NEEDMOREAP2 = " AP to attack";
    private const string NOAMMO = "Out of ammo.";
    private const string ISOUTOFAMMO = " is out of ammo.";

    private const string FORCRUSHING = "For crushing your enemies, you earn ";
    private const string EXPPOINTS = " exp. points.";
    #endregion

    public static LoggingManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }


    public static void LogMessage(MessageType type, Creature source, Creature target, string additional = "")
    {
        var message = string.Empty;
        switch (type)
        {
            case MessageType.OutOfAmmo:
                message = source is Player ? NOAMMO : $"{source.Name}{ISOUTOFAMMO}";
                break;
            case MessageType.OutOfRange:
                message = OUTOFRANGE;
                break;
            case MessageType.TargetBlocked:
                message = $"{YOUR}{TARGETBLOCKED}";
                break;
            case MessageType.AttackMissed:
                message = source is Player ? $"{YOUR}{ATTACKMISSED}" : $"{source.Name}{POSSESSIVE}{ATTACKMISSED}";
                break;
            case MessageType.AttackHit:
                var name = target is Player ? $"{YOU}{WERE}" : $"{target.name}{WAS}";
                message =
                    $"{name}{HIT}{additional}{HP}{(target.Alive ? string.Empty : DIED)}{PERIOD}";
                break;
            case MessageType.CreatureDied:
                break;
            case MessageType.NotEnoughAP:
                if (source is Player)
                {
                    message = $"{NEEDMOREAP1}{additional}{NEEDMOREAP2}";
                }
                else
                {
                    return;
                }
                break;
            case MessageType.GainedXP:
                if (source is Player)
                {
                    message = $"{FORCRUSHING}{additional}{EXPPOINTS}";
                }
                else
                {
                    return;
                }
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(type), type, null);
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }
        
        Instance.PrintMessage(message);
    }

    private void PrintMessage(string message)//TODO once the UI is set up, print messages there 
    {
        Debug.Log(message);
    }
}

public enum MessageType
{
    None = 0,//This shouldnt ever happen
    OutOfRange = 10,
    TargetBlocked = 11,
    AttackMissed = 12,
    AttackHit = 13,
    CreatureDied = 15,
    NotEnoughAP = 20,
    OutOfAmmo = 25,
    GainedXP = 30,
}