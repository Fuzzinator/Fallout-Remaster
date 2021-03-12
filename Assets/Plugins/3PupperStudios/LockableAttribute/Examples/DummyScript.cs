using System.Collections;
using System.Collections.Generic;
using JetBrains.Annotations;
using UnityEngine;

namespace ThreePupperStudios.Lockable
{
	public class DummyScript : MonoBehaviour
	{
		[Lockable(false)] 
		public float lockableFloat;

		public float notLockableFloat;

		[Lockable]
		public string lockableString;
		
		
		[Lockable(false)]
		public long unlockedByDefaultLong;
		
		[Lockable(false, false)]
		public float unlockingFloat;
		
		[Lockable(true, false)]
		public float nonPersistentFloat;
		
		[Lockable(rememberSelection = false)]
		public string nonpersistentString;
		
		[SerializeField, Lockable] 
		private bool _isOn;

		[Lockable] 
		public Camera lockableCamera;

		[Lockable] 
		public SubClass mySubClass;
		
		[Lockable]
		public SerializedStruct myStruct;
		
		#region lock persistence

		#endregion

		[System.Serializable]
		public struct SerializedStruct
		{
			public float myFloat;
			public long myLong;
			[TextArea]
			public string myString;
			public Vector2 myVector2;
			public Vector3 myVector3;
		}
		
		[System.Serializable]
		public class SubClass
		{
			public Light myLight;
			public bool[] myBools;
			[Range(0,1)]
			public float myFloat;
		}
	}
}
