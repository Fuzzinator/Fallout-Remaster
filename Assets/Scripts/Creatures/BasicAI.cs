using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;
using AttackSuccess = Creature.AttackSuccess;

public class BasicAI : MonoBehaviour
{
    [SerializeField]
    private Creature _creature;

    [SerializeField]
    private State _currentState;

    public State CurrentState => _currentState;

    [SerializeField]
    private Aggression _aggression;

    public Aggression CurrentAggression => _aggression;

    [SerializeField]
    private float _waitTime = 2f;

    //Patrolling
    [Header("Patrolling")]
    [SerializeField]
    private int[] _waypoints;

    private int _currentWaypoint = 0;

    //Chasing / Fleeing
    [Header("Chasing/Fleeing")]
    [SerializeField]
    private Creature _targetCreature;

    [Header("Combat")]
    [SerializeField]
    private bool _tryHealAtLow = false;

    [SerializeField]
    private bool _tryHealAtVLow = true;

    private bool _instigator = false;

    private WaitForSeconds _wait;

    #region const

    private const int FULLHEALTH = 100; //This is a percent
    private const int MEDIUMHEALTH = 75; //This is a percent
    private const int LOWHEALTH = 50; //This is a percent
    private const int VERYLOWHEALTH = 25; //This is a percent
    private const int MINCHANCETOHIT = 20;

    #endregion

    #region Unity Event Functions

    private void OnValidate()
    {
        if (_creature == null)
        {
            gameObject.TryGetComponent(out _creature);
        }
    }

    private void Start()
    {
        if (_creature.Alive)
        {
            CombatManager.stateChanged += CombatStateChanged;
            _wait = new WaitForSeconds(_waitTime);
            StartCoroutine(RunNormalAI());
        }
    }

    #endregion


    private void CombatStateChanged(bool isCombat)
    {
        if (isCombat)
        {
            StopAllCoroutines();
        }
        else
        {
            StartCoroutine(RunNormalAI());
        }
    }

    private IEnumerator RunNormalAI()
    {
        while (_creature.Alive)
        {
            yield return null;
            GetPath();
            yield return _creature.AIMoveCreature();
            switch (_currentState)
            {
                case State.Wander:
                case State.Patrol:
                    yield return _wait;
                    break;
            }
        }
    }

    public void StartTurn()
    {
        StartCoroutine(RunCombatLogicTree());
    }

    #region Determine what to do for turn

    protected virtual IEnumerator RunCombatLogicTree()
    {
        switch (_currentState)
        {
            case State.Fleeing:
                GetPath();
                yield return _creature.AIMoveCreature();
                _creature.EndTurn();
                yield break;
            case State.Dead:
                yield break;
            /*case State.Idle:
            case State.Wander:
            case State.Patrol:
            case State.Guard:
            case State.Chase:
            default:
                break;*/
        }

        if (WantsToHeal())
        {
            var healed = TryToHeal();
            if (!healed && HealthVeryLow())
            {
                _currentState = State.Fleeing;
                GetPath();
                yield return _creature.AIMoveCreature();
                _creature.EndTurn();
            }
            else if (healed)
            {
                yield return _creature.AnimController.UseAnimWait;
            }
        }

        if (_aggression == Aggression.Hostile)
        {
            var chanceToHit = _creature.GetChanceToHit(1, Player.Instance);
            if (chanceToHit < MINCHANCETOHIT)
            {
                _currentState = State.Fleeing;
                GetPath();
                yield return _creature.AIMoveCreature();
                _creature.EndTurn();
                yield break;
            }
        }

        var foundTarget = false;
        var distToTarget = 0;
        if (_instigator)
        {
            _targetCreature = _creature.TargetCreature;
            distToTarget = HexMaker.Instance.GetDistanceToCoord(_creature.Coord, _targetCreature.Coord,
                _creature.TargetPath, wantToMove: false);
            CombatManager.AddToCombat(_targetCreature);
            foundTarget = true;
        }
        else
        {
            foundTarget = TryGetTarget(out distToTarget);
        }

        if (!foundTarget)
        {
            _creature.EndTurn();
            yield break;
        }

        //If not possible for AI to get to target
        if (distToTarget < 0)
        {
            _currentState = State.Fleeing;
            GetPath();
            yield return _creature.AIMoveCreature();
            _creature.EndTurn();
            yield break;
        }

        var weaponInfo = _creature.GetAttackTypeInfo();
        var path = _creature.TargetPath.ToArray();
        _creature.TargetPath.Clear();
        var dist = distToTarget - weaponInfo.Range;
        for (var i = 0; i < path.Length; i++)
        {
            _creature.TargetPath.Add(path[i]);
            if (dist > 1)
            {
                yield return _creature.AIMoveCreature();
            }
            else
            {
                if (CombatManager.ViewUnobscured(_creature, _targetCreature))
                {
                    break;
                }

                yield return _creature.AIMoveCreature();
            }

            dist--;
            distToTarget--;
            _creature.TargetPath.Remove(path[i]);

            if (_creature.MaxCanMoveDist <= 0)
            {
                _creature.TargetPath.Clear();
                _creature.EndTurn();
                yield break;
            }
        }

        _creature.TargetPath.Clear();

        if (_creature.MaxCanMoveDist <= 0)
        {
            _creature.EndTurn();
            yield break;
        }

        if (_creature.TargetCreature != null)
        {
            var chanceToHit = _creature.GetChanceToHit(distToTarget, _targetCreature);
            var attackSuccess = _creature.TryAttackCreature();
            var firstAttack = true;
            var tryAgain = attackSuccess != AttackSuccess.NotEnoutAP;
            while (firstAttack || tryAgain)
            {
                switch (attackSuccess)
                {
                    case AttackSuccess.None:
                    case AttackSuccess.NoTarget:
                    case AttackSuccess.NoChanceToHit:
                        tryAgain = false;
                        break;
                    case AttackSuccess.NotEnoutAP:
                        if (firstAttack)
                        {
                            var inactiveNull = _creature.InactiveItem == null || _creature.InactiveItem.Info == null;
                            if (inactiveNull || _creature.InactiveItem.Info is WeaponInfo)
                            {
                                var attackInfo = _creature.GetAttackTypeInfo(false);
                                if (!attackInfo.IsValidWeapon || _creature.MaxCanMoveDist < attackInfo.ActionPointCost ||
                                    (!inactiveNull && !_creature.InactiveItem.CanUseItem()))
                                {
                                    yield return null;
                                    tryAgain = false;
                                    break;
                                }

                                yield return _creature.SwapQuickbarItems();
                                tryAgain = true;
                                break;
                            }
                        }

                        tryAgain = false;
                        break;
                    case AttackSuccess.NoAmmo:
                        var canReload = _creature.TryReloadWeapon(_creature.ActiveItem);
                        if (canReload)
                        {
                            //wait for anim to finish
                            yield return null;
                            tryAgain = true;
                        }
                        else
                        {
                            tryAgain = false;
                        }

                        break;
                    case AttackSuccess.AttackMissed:
                        //wait for anim to finish
                        yield return null;
                        break;
                    case AttackSuccess.AttackHit:
                    case AttackSuccess.AttackCritical:
                        //wait for anim to finish
                        yield return null;
                        break;

                    default:
                        throw new ArgumentOutOfRangeException();
                }

                firstAttack = false;
                if (tryAgain)
                {
                    attackSuccess = _creature.TryAttackCreature();
                }
            }

            yield return null;
        }

        yield return null;
        _creature.EndTurn();
    }

    protected virtual bool TryGetTarget(out int distance)
    {
        var foundTarget = false;
        distance = -1;

        _targetCreature = null;
        _creature.SetTargetCreature(null);

        if (CombatManager.Instance != null)
        {
            var enemies = CombatManager.Instance.GetEnemies(_aggression);
            var closestDist = int.MaxValue;
            if (enemies != null)
            {
                foreach (var enemy in enemies)
                {
                    var dist = HexMaker.Instance.GetDistanceToCoord(_creature.Coord, enemy.Coord,
                        _creature.TargetPath, null, closestDist, false);

                    if (dist >= closestDist || dist < 0)
                    {
                        continue;
                    }

                    closestDist = dist;
                    distance = dist;
                    _targetCreature = enemy;
                    _creature.SetTargetCreature(_targetCreature);
                    foundTarget = true;
                }
            }
        }

        return foundTarget;
    }

    private bool WantsToHeal()
    {
        if (_tryHealAtLow || _tryHealAtVLow)
        {
            var percent = Mathf.Max((FULLHEALTH / (float) _creature.MaxHealth) * _creature.CurrentHealth, 1);
            if (_tryHealAtLow && percent <= LOWHEALTH)
            {
                return true;
            }

            if (_tryHealAtVLow && percent <= VERYLOWHEALTH)
            {
                return true;
            }
        }

        return false;
    }

    private bool HealthVeryLow()
    {
        var percent = Mathf.Max((FULLHEALTH / (float) _creature.MaxHealth) * _creature.CurrentHealth, 1);
        return percent <= VERYLOWHEALTH;
    }

    private bool TryToHeal() //TODO once inventory is set up, flesh this out
    {
        var creatureHasStimPackInInventory = false;
        if (creatureHasStimPackInInventory)
        {
            var canOpenInventory = _creature.TryOpenInventory();
            if (canOpenInventory)
            {
                //ai uses stim pack
                return true;
            }
        }

        return false;
    }

    #endregion

    public void ChangeAggression(Aggression aggression)
    {
        CombatManager.ChangeAggression(_creature, aggression);
        _aggression = aggression;
    }

    public void GetPath()
    {
        switch (_currentState)
        {
            case State.Idle:
            case State.Guard:
            case State.Dead:
                break;
            case State.Wander:
                TryGetRandomPos(_creature.MaxCanMoveDist);
                break;
            case State.Patrol:
                if (_waypoints.Length == 0)
                {
                    Debug.LogWarning($"{_creature.Name}'s AI is set to Patrol, but it doesn't have any waypoints.",
                        this);
                    return;
                }

                if (_currentWaypoint < _waypoints.Length - 1)
                {
                    _currentWaypoint++;
                }
                else
                {
                    _currentWaypoint = 0;
                }

                HexMaker.Instance.GetDistanceToCoord(_creature.Coord, HexMaker.GetCoord(_waypoints[_currentWaypoint]),
                    _creature.TargetPath);
                break;
            case State.Chase:
                if (_targetCreature == null)
                {
                    Debug.LogWarning("Trying to run from, nothing?", this);
                    break;
                }

                HexMaker.Instance.GetDistanceToCoord(_creature.Coord, _targetCreature.Coord, _creature.TargetPath);
                break;
            case State.Fleeing:
                if (_targetCreature == null)
                {
                    Debug.LogWarning($"{_creature.Name} is trying to flee but has to Target Creature", this);
                    return;
                }

                TryGetFleePos();
                break;
        }
    }

    private void TryGetRandomPos(int distance, int maxTries = 10)
    {
        var coordOptions = GetWalkableMaxDistCoords();
        if (coordOptions.Count > 0)
        {
            var randOption = coordOptions[Random.Range(0, coordOptions.Count)];

            HexMaker.Instance.GetDistanceToCoord(_creature.Coord, randOption, _creature.TargetPath);
        }

        /*while (true)
        {
            var curentPos = transform.position;
            var randomPos = (Random.insideUnitCircle * Random.Range(0, distance)) +
                            new Vector2(curentPos.x, curentPos.z);
            var coord = HexMaker.Instance.GetCoordinates(new Vector3(randomPos.x, 0, randomPos.y));
            if (coord.IsWalkable)
            {
                var currentCoord = HexMaker.GetCoord(_creature.CurrentLocation);
                var dist = HexMaker.Instance.GetDistanceToCoord(currentCoord, coord, _creature.TargetPath);
                if (dist < 0 && maxTries > 0)
                {
                    maxTries -= 1;
                    continue;
                }
            }
            else
            {
                coord = null;
                if (maxTries > 0)
                {
                    maxTries -= 1;
                    continue;
                }
            }

            return;
        }*/
    }

    private bool TryGetFleePos()
    {
        var foundPath = false;
        var coordOptions = GetWalkableMaxDistCoords();

        Coordinates furthestCoord = null;
        var furthestDist = -1;

        foreach (var coord in coordOptions)
        {
            var dist = HexMaker.Instance.GetDistanceToCoord(_creature.Coord, coord, null);
            if (dist > furthestDist)
            {
                var creatureDist =
                    HexMaker.Instance.GetDistanceToCoord(_creature.Coord, furthestCoord, _creature.TargetPath);

                if (creatureDist > 0)
                {
                    furthestDist = dist;
                    furthestCoord = coord;
                    foundPath = true;
                }
            }
        }

        return foundPath;
    }

    private List<Coordinates> GetWalkableMaxDistCoords()
    {
        var coordOptions = new List<Coordinates>();
        //Getting actualStartPos
        var nextPos = GetSequentialNeighbors(_creature.Coord, HexDir.N, coordOptions,
            _creature.MaxCanMoveDist, false);
        if (nextPos == null)
        {
            Debug.LogWarning("The starting point for max dist is null. Is the creatures AP 0?", this);
            return coordOptions;
        }

        coordOptions.Add(nextPos);

        //SE
        nextPos = GetSequentialNeighbors(nextPos, HexDir.SE, coordOptions, _creature.MaxCanMoveDist);
        //S
        nextPos = GetSequentialNeighbors(nextPos, HexDir.S, coordOptions, _creature.MaxCanMoveDist);
        //SW
        nextPos = GetSequentialNeighbors(nextPos, HexDir.SW, coordOptions, _creature.MaxCanMoveDist);
        //NW
        nextPos = GetSequentialNeighbors(nextPos, HexDir.NW, coordOptions, _creature.MaxCanMoveDist);
        //N
        nextPos = GetSequentialNeighbors(nextPos, HexDir.N, coordOptions, _creature.MaxCanMoveDist);
        //NE
        nextPos = GetSequentialNeighbors(nextPos, HexDir.NE, coordOptions, _creature.MaxCanMoveDist);

        return coordOptions;
    }

    private static Coordinates GetSequentialNeighbors(Coordinates startingPoint, HexDir direction,
        List<Coordinates> coords,
        int dist, bool shouldAdd = true)
    {
        var neighbor = startingPoint.GetNeighbor((int) direction);
        Coordinates neighborCoord = null;
        for (var i = 0; i < dist; i++)
        {
            neighborCoord = HexMaker.GetCoord(neighbor.index);
            if (neighborCoord != null)
            {
                if (neighborCoord.IsWalkable)
                {
                    coords.Add(neighborCoord);
                }

                neighbor = neighborCoord.GetNeighbor((int) direction);
            }
        }

        return neighborCoord;
    }

    public void DetectedEnemy(Creature target)
    {
        if (!CombatManager.Instance.CombatMode)
        {
            _creature.SetTargetCreature(target);
            _instigator = true;
            _creature.InitiateCombat();
        }
        else
        {
            CombatManager.AddToCombat(_creature);
        }
    }

    #region Enums

    public enum State
    {
        Idle = 0,
        Wander = 1,
        Patrol = 2,
        Guard = 3,
        Chase = 4,
        Fleeing = 5,
        Dead = 10
    }

    public enum Aggression
    {
        Neutral,
        Friendly,
        Hostile
    }

    #endregion
}