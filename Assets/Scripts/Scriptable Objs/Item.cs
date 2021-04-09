using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Item : ScriptableObject
{
    [SerializeField]
    private int _weight;
    public int Weight => _weight;
    
    [SerializeField]
    private int _value;
    public int Value => _value;

    [SerializeField]
    private bool _stackable;
    public bool Stackable => _stackable;
}
