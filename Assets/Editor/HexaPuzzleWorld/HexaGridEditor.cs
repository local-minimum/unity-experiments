﻿using UnityEngine;
using UnityEditor;

namespace HexaPuzzleWorld {
	[CustomEditor(typeof(HexaGrid))]
	public class HexaGridEditor : Editor {

		public override void OnInspectorGUI ()
		{
			EditorGUI.BeginChangeCheck ();

			EditorGUILayout.PropertyField (serializedObject.FindProperty ("rings"));
			EditorGUILayout.PropertyField (serializedObject.FindProperty ("spacing"));

			if (EditorGUI.EndChangeCheck ()) {				
				(target as HexaGrid).SetupGrid ();
				serializedObject.ApplyModifiedProperties ();
			}
		}
	}
}