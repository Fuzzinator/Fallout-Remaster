using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(order = 12, fileName = "New Location", menuName = "ScriptObjs/Location")]
public class Location : ScriptableObject
{
    [SerializeField]
    private string _locationName = string.Empty;
    public string LocationName => _locationName;

    [SerializeField]
    private Status _status = Status.Unknown;
    public Status CurrentStatus => _status;

    [SerializeField]
    private int _index = -1;
    public int Index => _index;

    [SerializeField]
    private Map[] _maps = new Map[0];

    private Map _activeMap;
    public Map ActiveMap => _activeMap;

    public void SetActiveMap(int index)
    {
        _activeMap = _maps[index];
    }



    public enum Status
    {
        Unknown = 0,
        Known = 1,
        Visited = 2,
        Invaded = 3
    }
}
