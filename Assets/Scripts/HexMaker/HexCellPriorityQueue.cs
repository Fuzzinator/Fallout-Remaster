using System.Collections;
using System.Collections.Generic;

public class HexCellPriorityQueue
{
    private readonly List<Coordinates> _list = new List<Coordinates>();

    //private List<Coordinates> _sourceList;
    private int _count = 0;

    public int Count => _count;

    private int _minimum = int.MaxValue;

    /*public HexCellPriorityQueue(ref List<Coordinates> list)
    {
        _sourceList = list;
    }*/

    public void Enqueue(Coordinates cell)
    {
        _count++;
        var priority = cell.SearchPriority;
        _minimum = 0;

        if (priority < _minimum)
        {
            _minimum = priority;
        }

        while (priority >= _list.Count)
        {
            _list.Add(null);
        }

        cell.nextWithSamePriority = _list[priority]; //Testing
        _list[priority] = cell;

        /*if (priority >= _list.Count || priority < 0)
        {
            cell.nextWithSamePriority = null;
            _list.Add(cell);
            _list.Sort(Compare);
        }
        else
        {
            _list.Insert(priority, cell);
            _list[priority] = cell;
        }*/
    }

    public Coordinates Dequeue()
    {
        _count--;
        for (; _minimum < _list.Count; _minimum++)
        {
            var coord = _list[_minimum];
            if (coord != null)
            {
                var nextCoord = coord.nextWithSamePriority;
                /*if (nextCoord == -1)
                {
                    _list[_minimum] = null;
                }
                else
                {
                    _list[_minimum] = _sourceList[nextCoord];
                }*/
                _list[_minimum] = nextCoord;

                return coord;
            }
        }

        return null;
    }

    public void Change(Coordinates cell, int oldPriority)
    {
        var current = _list[oldPriority];
        if (current == null)
        {
            return;
        }

        var next = current.nextWithSamePriority;
        if (current == cell)
        {
            //if (next <= -1)
            //{
            _list[oldPriority] = next; //_sourceList[next];
            //}
        }
        else
        {
            while (next != null && next != cell)
            {
                current = next;
                next = current.nextWithSamePriority;
            }
        }

        current.nextWithSamePriority = cell.nextWithSamePriority;
        Enqueue(cell);
        _count--;
    }

    public void Clear()
    {
        foreach (var coord in _list)
        {
            if (coord != null)
            {
                coord.nextWithSamePriority = null;
            }
        }
        _list.Clear();
        _count = 0;
        _minimum = int.MaxValue;
    }

    /*int Compare(Coordinates a, Coordinates b)
    {
        var aNull = a == null;
        var bNull = b == null;
        if (!aNull && bNull)
        {
            return 1;
        }

        if (aNull && !bNull)
        {
            return -1;
        }

        if (aNull && bNull)
        {
            return 0;
        }

        if (a.SearchPriority > b.SearchPriority)
            return 1;
        if (a.SearchPriority < b.SearchPriority)
            return -1;
        else
            return 0;
    }*/
}