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
    
    [UnityEngine.Serialization.FormerlySerializedAs("_magSize")]
    [SerializeField]
    private int _maxCharges;
    public int MaxCharges => _maxCharges;

    [SerializeField, TextArea]
    private string _description;
    public string Description => _description;
    
    [SerializeField]
    private bool _destroyWhenEmpty = false;
    public bool DestroyWhenEmpty => _destroyWhenEmpty;
}
