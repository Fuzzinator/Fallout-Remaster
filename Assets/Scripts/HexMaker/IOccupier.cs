using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOccupier
{
    public void EnterCoordinate(Coordinates coord);
    public void LeaveCoordinate();
}
