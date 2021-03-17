using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Creature : MonoBehaviour, IOccupier
{
    [SerializeField]
    protected string _name;

    [SerializeField]
    protected SPECIAL _special;

    [SerializeField]
    protected Skills _skills;

    [SerializeField]
    protected int _currentLocation;

    [SerializeField, HideInInspector]
    protected HexMaker _hexMaker;
    public int CurrentLocation => _currentLocation;

    [SerializeField]
    protected HexDir _facingDir;

    [SerializeField]
    protected float _moveSpeed;
    private void OnEnable()
    {
        if (HexMaker.Instance?.Coords != null)
        {
            var coords = HexMaker.Instance.Coords;
            if (_currentLocation > -1 && _currentLocation < coords.Count)
            {
                var coord = coords[_currentLocation];
                coord.occupied = true;
                transform.position = coord.pos;
            }
        }
    }
    public void EnterCoordinate(Coordinates coord)
    {
        if (coord.occupied)
        {
            Debug.LogWarning("Player is entering occupied space. This shouldn't be happening.");
            return;
        }

        coord.occupied = true;
        coord.occupyingObject = this;
        _currentLocation = coord.index;
    }

    public void LeaveCoordinate()
    {
        if (_hexMaker == null)
        {
            _hexMaker = HexMaker.Instance;
        }
        
        if (_hexMaker != null && _currentLocation > 0 && _currentLocation < _hexMaker.Coords.Count)
        {
            var currentCoord = _hexMaker.Coords[_currentLocation];
            if (!currentCoord.occupied)
            {
                Debug.LogWarning("Player is leaving unoccupied space. This shouldn't be happening.");
            }

            if (currentCoord.occupyingObject as Player == this)
            {
                currentCoord.occupied = false;
                currentCoord.occupyingObject = null;
            }
            else
            {
                Debug.LogWarning("Player leaving space they did not occupy. This shouldn't be happening.");
            }
        }
    }
}
