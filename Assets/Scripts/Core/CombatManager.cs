using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CombatManager : MonoBehaviour
{
    [SerializeField]
    private bool _combatMode = false;

    public bool CombatMode
    {
        get => _combatMode;
        set
        {
            _combatMode = value;
        }
    }

    #region Static Vars

    private static CombatManager _instance;
    public static CombatManager Instance => _instance;

    public static Action<bool> stateChanged;
    

    #endregion

    private void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
        }
        else
        {
            Destroy(this);
        }
    }
}