using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(CombatManager), true), CanEditMultipleObjects]
public class CombatManagerCustomEditor : Editor
{
    private CombatManager _target;

    private void Awake()
    {
        _target = target as CombatManager;
    }
    
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.Space(10);

        if (!Application.isPlaying)
        {
            return;
        }
        
        if (GUILayout.Button("Toggle Combat Mode") && Player.Instance != null)
        {
            if (CombatManager.Instance.CombatMode)
            {
                CombatManager.TryEndCombat();   
            }
            else
            {
                Player.Instance.InitiateCombat();
            }
        }
    }
}
