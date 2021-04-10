using System;
using System.Collections;
using System.Collections.Generic;
using Core;
using UnityEngine;
using Random = UnityEngine.Random;

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
    [SerializeField]
    private int[] _waypoints;

    private int _currentWaypoint = 0;

    //Chasing / Fleeing
    [SerializeField]
    private Creature _targetCreature;

    private WaitForSeconds _wait;

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

                HexMaker.Instance.GetDistanceToCoord(HexMaker.GetCoord(_creature.CurrentLocation),
                    HexMaker.GetCoord(_waypoints[_currentWaypoint]), _creature.TargetPath);
                break;
            case State.Chase:
                if (_targetCreature == null)
                {
                    Debug.LogWarning("Trying to run from, nothing?", this);
                    break;
                }

                HexMaker.Instance.GetDistanceToCoord(HexMaker.GetCoord(_creature.CurrentLocation),
                    HexMaker.GetCoord(_targetCreature.CurrentLocation), _creature.TargetPath);
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
            
            HexMaker.Instance.GetDistanceToCoord(HexMaker.GetCoord(_creature.CurrentLocation),
                randOption, _creature.TargetPath);
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

    private void TryGetFleePos()
    {
        var coordOptions = GetWalkableMaxDistCoords();
        var currentPos = HexMaker.GetCoord(_creature.CurrentLocation);

        Coordinates furthestCoord = null;
        var furthestDist = -1;
        var creatureCoord = HexMaker.GetCoord(_creature.CurrentLocation);
        
        foreach (var coord in coordOptions)
        {
            var dist = HexMaker.Instance.GetDistanceToCoord(creatureCoord, coord, null);
            if (dist > furthestDist)
            {
                var creatureDist =
                    HexMaker.Instance.GetDistanceToCoord(currentPos, furthestCoord, _creature.TargetPath);
                
                if (creatureDist > 0)
                {
                    furthestDist = dist;
                    furthestCoord = coord;
                }
            }
        }
    }

    private List<Coordinates> GetWalkableMaxDistCoords()
    {
        var coordOptions = new List<Coordinates>();
        var currentPos = HexMaker.GetCoord(_creature.CurrentLocation);
        //Getting actualStartPos
        var nextPos = GetSequentialNeighbors(currentPos, HexDir.N, coordOptions, _creature.MaxCanMoveDist, false);
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