using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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

    //Patrolling
    [SerializeField]
    private int[] _waypoints;

    //Chasing / Fleeing
    [SerializeField]
    private Creature _targetCreature;


    public void GetPath()
    {
        switch (_currentState)
        {
            case State.Idle:
                break;
            case State.Chase:
                HexMaker.Instance.GetDistanceToCoord(HexMaker.GetCoord(_creature.CurrentLocation),
                    HexMaker.GetCoord(_targetCreature.CurrentLocation), _creature.TargetPath, null);
                break;
        }
    }


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
}