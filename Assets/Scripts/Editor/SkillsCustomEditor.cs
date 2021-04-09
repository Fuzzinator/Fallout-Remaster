using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(Skills))]
public class SkillsCustomEditor : Editor
{
    private Skills _target;

    private void Awake()
    {
        _target = target as Skills;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        foreach (var skill in _target.CurrentSkills)
        {
            var origAmount = _target.GetSkillLvl(skill.skillName);
            var newAmount = origAmount;
            newAmount = EditorGUILayout.IntField(skill.skillName.ToString(), newAmount);
            if (newAmount != origAmount)
            {
                newAmount -= origAmount;
                var increase = newAmount > 0;
                newAmount = Mathf.Abs(newAmount);
                while (newAmount > 0)
                {
                    _target.IncrementSkill(skill.skillName, increase);
                    newAmount--;
                }
            }
        }
    }
}