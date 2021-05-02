using System;
using System.Collections;
using System.Collections.Generic;
using ThreePupperStudios.Lockable;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField]
    private bool _combatMode = false;

    [SerializeField]
    private LayerMask _targetableObjs;

    public LayerMask TargetableObjs => _targetableObjs;

    public bool CombatMode
    {
        get => _combatMode;
        set
        {
            _combatMode = value;
            stateChanged?.Invoke(value);
        }
    }

    private List<Creature> _turnOrder = new List<Creature>();
    private List<Creature> _friendlies = new List<Creature>();
    private List<Creature> _enemies = new List<Creature>();

    [SerializeField, Lockable]
    private Creature _currentCreature;

    private bool _surpriseRound = false;
    private Creature _victim;

    public bool CanEndCombat => _turnOrder.Count == 0 ||
                                (_turnOrder.Count == 1 && _turnOrder.Contains(Player.Instance)) ||
                                !_turnOrder.Contains(Player.Instance);

    #region Static Vars

    private static CombatManager _instance;
    public static CombatManager Instance => _instance;

    public static Action<bool> stateChanged;
    public static Action<Creature> startTurn;

    #endregion

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(this);
        }
        else
        {
            Destroy(this);
        }
    }

    public List<Creature> GetEnemies(BasicAI.Aggression aggression)
    {
        List<Creature> enemies = null;
        var enemiesNull = true;

        var targetAggression = aggression == BasicAI.Aggression.Friendly
            ? BasicAI.Aggression.Hostile
            : BasicAI.Aggression.Friendly;

        foreach (var creature in _turnOrder)
        {
            if (creature.Aggression == targetAggression)
            {
                if (enemiesNull)
                {
                    enemies = new List<Creature>();
                    enemiesNull = false;
                }

                enemies.Add(creature);
            }
        }

        return enemies;
    }

    public static void StartCombat(Creature instigator)
    {
        Instance._turnOrder.Clear();
        Instance._enemies.Clear();
        Instance._friendlies.Clear();

        Instance._currentCreature = null;
        Instance.CombatMode = true;
        StartSurpriseRound(instigator);
    }

    public static void TryEndCombat()
    {
        if (Instance.CanEndCombat)
        {
            EndCombat();
        }
    }

    public static void AddToCombat(Creature newCreature)
    {
        if (Instance._surpriseRound && Instance._victim == null)
        {
            Instance._victim = newCreature;
        }

        #region Track creature agro

        switch (newCreature.Aggression)
        {
            case BasicAI.Aggression.Friendly:
            {
                if (!Instance._friendlies.Contains(newCreature))
                {
                    Instance._friendlies.Add(newCreature);
                }

                break;
            }
            case BasicAI.Aggression.Hostile:
            {
                if (!Instance._enemies.Contains(newCreature))
                {
                    Instance._enemies.Add(newCreature);
                }

                break;
            }
            case BasicAI.Aggression.Neutral:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        #endregion

        if (Instance._turnOrder.Contains(newCreature))
        {
            //Debug.LogWarning($"{newCreature.Name} Already in turn order. Why are you trying to add it again?");
            return;
        }

        var index = 0;
        while (index < Instance._turnOrder.Count && Instance._turnOrder[index].Sequence > newCreature.Sequence)
        {
            index++;
        }

        Instance._turnOrder.Insert(index, newCreature);
    }

    public static void RemoveFromCombat(Creature targetCreature)
    {
        #region Track creature agro

        if (Instance._friendlies.Contains(targetCreature))
        {
            Instance._friendlies.Remove(targetCreature);
        }

        if (Instance._enemies.Contains(targetCreature))
        {
            Instance._enemies.Remove(targetCreature);
        }

        #endregion

        if (!Instance._turnOrder.Contains(targetCreature))
        {
            Debug.LogWarning($"{targetCreature.Name} Already not in turn order. Why are you trying to remove it?");
            return;
        }

        Instance._turnOrder.Remove(targetCreature);
        TryEndCombat();
    }

    public static void ProgressCombat()
    {
        if (Instance._turnOrder.Count == 0)
        {
            Debug.LogWarning("Turn order empty but trying to progress.");
            return;
        }

        TryEndCombat();
        if (Instance._surpriseRound && Instance._victim != null)
        {
            Instance._currentCreature = Instance._victim;
            Instance._victim = null;
            Instance._surpriseRound = false;
        }
        else if (Instance._victim != null)
        {
            Instance._victim = null;
            Instance._currentCreature = Instance._turnOrder[0];
        }
        else
        {
            var index = Instance._turnOrder.IndexOf(Instance._currentCreature);
            if (index < 0)
            {
                Debug.LogWarning(
                    $"{Instance._currentCreature} not in turn order and not in surprise round. What happened?");
                return;
            }

            if (index < Instance._turnOrder.Count - 1)
            {
                index++;
            }
            else
            {
                index = 0;
            }

            Instance._currentCreature = Instance._turnOrder[index];
        }

        Instance._currentCreature.StartTurn();
    }

    private static void StartSurpriseRound(Creature instigator)
    {
        Instance._currentCreature = instigator;
        AddToCombat(instigator);
        Instance._surpriseRound = true;
        Instance._currentCreature.StartTurn();
    }

    private static void EndCombat()
    {
        Instance.CombatMode = false;

        Instance._turnOrder.Clear();
        Instance._currentCreature = null;
    }

    public static bool IsMyTurn(Creature creature)
    {
        if (Instance == null)
        {
            return false;
        }

        return Instance._currentCreature == creature;
    }

    public static void ChangeAggression(Creature creature, BasicAI.Aggression aggression)
    {
        if (Instance == null)
        {
            return;
        }

        switch (creature.Aggression)
        {
            case BasicAI.Aggression.Friendly:
                Instance._friendlies.Remove(creature);
                break;
            case BasicAI.Aggression.Hostile:
                Instance._enemies.Remove(creature);
                break;
        }

        #region Track creature agro

        switch (aggression)
        {
            case BasicAI.Aggression.Friendly:
            {
                if (!Instance._friendlies.Contains(creature))
                {
                    Instance._friendlies.Add(creature);
                }

                break;
            }
            case BasicAI.Aggression.Hostile:
            {
                if (!Instance._enemies.Contains(creature))
                {
                    Instance._enemies.Add(creature);
                }

                break;
            }
            case BasicAI.Aggression.Neutral:
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        #endregion
    }

    public static bool ViewUnobscured(Creature source, Creature target)
    {
        var tPos = source.transform.position;
        var otPos = target.transform.position;
        var dist = Vector3.Distance(tPos, otPos);
        var ray = new Ray(tPos + Vector3.one, (tPos - otPos).normalized);
        RaycastHit[] hitInfos = new RaycastHit[0];
        var size = Physics.RaycastNonAlloc(ray, hitInfos, dist);
        if (size > 1)
        {
            List<Creature> creatures = null;
            var mask = CombatManager.Instance.TargetableObjs;
            if (hitInfos != null)
            {
                foreach (var hitInfo in hitInfos)
                {
                    if (mask == (mask | 1 << hitInfo.collider.gameObject.layer))
                    {
                        if (hitInfo.collider.gameObject == target.gameObject)
                        {
                            continue;
                        }

                        //TODO decrement chance to hit and add these new creatures to a list of possibility to be hit if the other attack misses
                    }
                    else
                    {
                        return false;
                    }
                }
            }
        }

        return true;
    }
}