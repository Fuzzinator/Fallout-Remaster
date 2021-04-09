using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Creature), true)]
public class CreatureCustomEditor : Editor
{
    private Creature _target;
    private void OnEnable()
    {
        _target = target as Creature;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        EditorGUILayout.LabelField("Move Speed:", _target.MoveSpeed.ToString(CultureInfo.InvariantCulture));
        EditorGUILayout.Space(10);
        EditorGUILayout.LabelField("Max Health:", _target.MaxHealth.ToString(CultureInfo.InvariantCulture));
        EditorGUILayout.LabelField("Healing Rate:", _target.HealingRate.ToString(CultureInfo.InvariantCulture));
        EditorGUILayout.LabelField("AC:", _target.ArmorClass.ToString(CultureInfo.InvariantCulture));
        EditorGUILayout.LabelField("Initiative:", _target.Sequence.ToString(CultureInfo.InvariantCulture));
        EditorGUILayout.LabelField("Max AP:", _target.MaxActionPoints.ToString(CultureInfo.InvariantCulture));
        EditorGUILayout.LabelField("Melee Damage:", _target.MeleeDamage.ToString(CultureInfo.InvariantCulture));
        EditorGUILayout.LabelField("Crit Chance:", _target.CriticalChance.ToString(CultureInfo.InvariantCulture));
        EditorGUILayout.LabelField("Rad Resist:", _target.RadResistance.ToString(CultureInfo.InvariantCulture));

        EditorGUILayout.Space(20);
        if (!Application.isPlaying)
        {
            _target.SetHP(_target.MaxHealth);
            return;
        }
        if (CombatManager.Instance == null || !CombatManager.Instance.CombatMode)
        {
            return;
        }
        if (GUILayout.Button("End Turn"))
        {
            _target.EndTurn();
        }
    }
}
