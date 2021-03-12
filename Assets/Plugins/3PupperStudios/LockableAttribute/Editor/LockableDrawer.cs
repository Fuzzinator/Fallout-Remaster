using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

namespace ThreePupperStudios.Lockable
{
    [CustomPropertyDrawer(typeof(LockableAttribute))]
    internal class LockableDrawer : PropertyDrawer
    {
        #region Variables

        #region Constant Strings

        private const string ID_SAVE_NAME = "3PupperStudiosLockableAttributeIDList";
        private const string VALUES_SAVE_NAME = "3PupperStudiosLockableAttributeValsList";
        private const string LOCKED_ICON = "LockableAttributeLockedIcon";
        private const string UNLOCKED_ICON = "LockableAttributeUnlockedIcon";
        private const string LOCK = "Lock";
        private const string MANDARINLOCK = "锁定";
        private const string UNLOCK = "Unlock";
        private const string MANDARINUNLOCK = "解锁";
#if !UNITY_2019_2_OR_NEWER
        private const string
            LOCAL_IDENTIFIER = "m_LocalIdentfierInFile"; //note the misspelling. Unity Spells this wrong

        private const string INSPECTOR_MODE = "inspectorMode";
#endif

        #endregion

        private LockableAttribute _attribute;
        private SerializedProperty _currentProperty;
        private bool _locked;

        private GenericMenu _menu;

        private readonly Dictionary<SerializedProperty, LockableAttribute> _propertyAttributePair =
            new Dictionary<SerializedProperty, LockableAttribute>();

        private readonly Dictionary<Scene, List<SerializedProperty>> _sceneProperties =
            new Dictionary<Scene, List<SerializedProperty>>();

        private readonly Dictionary<string, bool> _storedProperties = new Dictionary<string, bool>();

        #endregion

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorApplication.contextualPropertyMenu += OnPropertyContextMenu;
            EditorGUI.BeginProperty(position, label, property);
            _attribute = attribute as LockableAttribute;
            _currentProperty = property;

            if (property.isExpanded)
            {
                label.text = property.displayName;
            }

            if (_attribute == null)
            {
                if (EditorApplication.contextualPropertyMenu != null)
                {
                    EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
                }

                return;
            }

            if (_attribute.rememberSelection)
            {
                var id = GetId(property.serializedObject.targetObject);

                var propName = $"{id} {property.name}";
                if (!_storedProperties.ContainsKey(propName))
                {
                    ReadSavedProperties();
                }

                if (_storedProperties.ContainsKey(propName))
                {
                    _attribute.locked = _storedProperties[propName];
                }
            }

            _locked = _attribute.locked;

            GUI.enabled = !_locked;
            var img = label.image;
            if (_attribute.showIcon)
            {
                if (_locked)
                {
                    var icon = Resources.Load(LOCKED_ICON) as Texture2D;
                    label.image = icon;
                }
                else
                {
                    var icon = Resources.Load(UNLOCKED_ICON) as Texture2D;
                    label.image = icon;
                }
            }

            EditorGUI.PropertyField(position, property, label, true);
            label.image = img;

            GUI.enabled = true;

            EditorGUI.EndProperty();
            if (EditorApplication.contextualPropertyMenu != null)
            {
                EditorApplication.contextualPropertyMenu -= OnPropertyContextMenu;
            }
        }

        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            var totalHeight = EditorGUI.GetPropertyHeight(property, label, true) +
                              EditorGUIUtility.standardVerticalSpacing;

            return totalHeight;
        }

        private void UpdateAfterSave(Scene scene)
        {
            if (!_sceneProperties.ContainsKey(scene))
            {
                return;
            }

            foreach (var property in _sceneProperties[scene])
            {
                if (property?.serializedObject?.targetObject == null)
                {
                    continue;
                }

                var iD = GetId(property.serializedObject.targetObject);
                var propRef = $"{iD} {property.name}";
                _storedProperties[propRef] = _propertyAttributePair[property].IsDefaultAttribute();
                SaveProperties();
            }

            _sceneProperties.Remove(scene);
        }

        private void SaveProperties()
        {
            var iDList = _storedProperties.Keys.ToArray();
            var valueList = _storedProperties.Values.ToArray();
            var iDString = JsonHelper.ToJson(iDList);
            var valueString = JsonHelper.ToJson(valueList);
            EditorPrefs.SetString(ID_SAVE_NAME, iDString);
            EditorPrefs.SetString(VALUES_SAVE_NAME, valueString);
        }

        private void ReadSavedProperties()
        {
            var iDList = JsonHelper.FromJson<string>(EditorPrefs.GetString(ID_SAVE_NAME));
            var valueList = JsonHelper.FromJson<bool>(EditorPrefs.GetString(VALUES_SAVE_NAME));
            if (iDList == null || valueList == null)
            {
                return;
            }

            for (var i = 0; i < iDList.Length; i++)
            {
                var id = iDList[i];
                _storedProperties[id] = valueList[i];
            }
        }

        private static ulong GetId(Object obj)
        {
            if (obj != null)
            {
#if UNITY_2019_2_OR_NEWER
                var guid = GlobalObjectId.GetGlobalObjectIdSlow(obj);
                var id = guid.targetObjectId;
                
                return id;

#else
                var inspectorModeInfo =
                    typeof(SerializedObject).GetProperty(INSPECTOR_MODE,
                        BindingFlags.NonPublic | BindingFlags.Instance);

                var serializedObject = new SerializedObject(obj);
                inspectorModeInfo?.SetValue(serializedObject, InspectorMode.Debug, null);

                var
                    localIdProp = serializedObject.FindProperty(LOCAL_IDENTIFIER);

                var localId = localIdProp.longValue;
                if (localId <= 0)
                {
#if UNITY_2018_3_OR_NEWER
                    var prefabType = PrefabUtility.GetPrefabAssetType(obj);
                    if (prefabType != PrefabAssetType.NotAPrefab)
                    {
                        var prefab = PrefabUtility.GetPrefabInstanceHandle(obj);
#else
                    var prefabType = PrefabUtility.GetPrefabType(obj);
                    if (prefabType != PrefabType.None)
                    {
                        var prefab = PrefabUtility.GetPrefabParent(obj);
#endif
                        return GetId(prefab);
                    }

                    var sceneDirty = false;
                    if (obj as GameObject != null)
                    {
                        sceneDirty = ((GameObject) obj).scene.isDirty;
                    }
                    else if ((obj as MonoBehaviour)?.gameObject != null)
                    {
                        sceneDirty = ((MonoBehaviour) obj).gameObject.scene.isDirty;
                    }

                    if (sceneDirty && localId == 0)
                    {
                        localId = obj.GetInstanceID();
                    }
                }

                return (ulong) localId;
#endif
            }

            return 0;
        }

        private void Lock()
        {
            _attribute.locked = true;

            if (!_attribute.rememberSelection)
            {
                return;
            }

            UpdateTracked(_currentProperty);

            SaveProperties();
        }

        private void Unlock()
        {
            _attribute.locked = false;

            if (!_attribute.rememberSelection)
            {
                return;
            }

            UpdateTracked(_currentProperty);

            SaveProperties();
        }

        private void OnPropertyContextMenu(GenericMenu menu, SerializedProperty property)
        {
            if (Application.systemLanguage != SystemLanguage.Chinese)
            {
                menu.AddItem(new GUIContent(LOCK), _locked, Lock);
                menu.AddItem(new GUIContent(UNLOCK), !_locked, Unlock);
            }
            else
            {
                menu.AddItem(new GUIContent(MANDARINLOCK), _locked, Lock);
                menu.AddItem(new GUIContent(MANDARINUNLOCK), !_locked, Unlock);
            }
        }

        private void UpdateTracked(SerializedProperty property)
        {
            _propertyAttributePair[_currentProperty] = _attribute;
            var iD = GetId(_currentProperty.serializedObject.targetObject);
            var propRef = $"{iD} {_currentProperty.name}";
            _storedProperties[propRef] = _attribute.locked;

            var go = (property.serializedObject.targetObject as MonoBehaviour)?.gameObject;
            if (go == null || !go.scene.isDirty)
            {
                return;
            }

            if (_sceneProperties.Keys.Count == 0)
            {
                EditorSceneManager.sceneSaved += UpdateAfterSave;
            }

            if (_sceneProperties.ContainsKey(go.scene))
            {
                if (!_sceneProperties[go.scene].Contains(property))
                {
                    _sceneProperties[go.scene].Add(property);
                }
            }
            else
            {
                _sceneProperties[go.scene] = new List<SerializedProperty>() {property};
            }
        }

        public static class JsonHelper
        {
            public static T[] FromJson<T>(string json)
            {
                var wrapper = JsonUtility.FromJson<Wrapper<T>>(json);
                return wrapper?.items;
            }

            public static string ToJson<T>(T[] array, bool prettyPrint = false)
            {
                var wrapper = new Wrapper<T>
                {
                    items = array
                };
                return JsonUtility.ToJson(wrapper, prettyPrint);
            }

            [Serializable]
            private class Wrapper<T>
            {
                public T[] items;
            }
        }
    }
}