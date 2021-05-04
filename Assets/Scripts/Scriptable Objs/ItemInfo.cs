using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class ItemInfo : ScriptableObject
{
    [SerializeField]
    private string _name;
    private string Name => _name;
    
    [SerializeField]
    private int _weight;
    public int Weight => _weight;
    
    [SerializeField]
    private int _value;
    public int Value => _value;

    [SerializeField]
    private int _maxStackSize = 0;
    public int MaxStackSize => _maxStackSize;

    [SerializeField, TextArea]
    private string _description;
    public string Description => _description;
}
