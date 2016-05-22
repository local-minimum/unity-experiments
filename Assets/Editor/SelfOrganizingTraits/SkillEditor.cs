using UnityEngine;
using UnityEditor;
using System.Linq;
using System.Collections.Generic;

namespace SelfOrganizingTrait {
	
	[CustomEditor(typeof(Skill))]
	public class SkillEditor : Editor {
		public override void OnInspectorGUI ()
		{
			
			Skill myTaget = target as Skill;

			EditorGUILayout.HelpBox (string.Format ("{2}\n Lvl: {0}\tXp: {1}",
				myTaget.skillLvl, myTaget.Xp, myTaget.name
			), MessageType.Info);

			EditorGUILayout.BeginHorizontal ();
			if (GUILayout.Button ("Test")) {
				(target as Skill).Test ();
			}
			if (GUILayout.Button ("Succeed")) {
				(target as Skill).Learn (true);
			}
			if (GUILayout.Button ("Fail")) {
				(target as Skill).Learn (false);
			}
			EditorGUILayout.EndHorizontal ();

			DrawSkillSetter ();

			// Could make general thingy for response things
			EditorGUI.BeginChangeCheck ();
			var responses = serializedObject.FindProperty ("responseSkills");
			EditorGUILayout.BeginHorizontal ();
			EditorGUILayout.PropertyField (responses);
			responses.arraySize = EditorGUILayout.IntField ("Size", responses.arraySize);
			EditorGUILayout.EndHorizontal ();
			if (responses.isExpanded) {
				for (int i = 0; i < responses.arraySize; i++) {
					EditorGUILayout.PropertyField (responses.GetArrayElementAtIndex (i));
				}
			} else {
				EditorGUI.indentLevel++;
				if (responses.arraySize == 0)
					EditorGUILayout.HelpBox ("Skill is checked against predefined difficulty of task", MessageType.Info);
				else
					EditorGUILayout.HelpBox(string.Join(", ", YieldNames(responses).ToArray()), MessageType.Info);
				EditorGUI.indentLevel--;
			}
			if (EditorGUI.EndChangeCheck ()) {
				serializedObject.ApplyModifiedProperties ();
			}
		}

		IEnumerable<string> YieldNames(SerializedProperty prop) {
			for (int i = 0; i < prop.arraySize; i++) {
				var item = prop.GetArrayElementAtIndex (i);
				if (item.objectReferenceValue == null)
					yield return "[null]";
				else
					yield return (item.objectReferenceValue as Skill).name;
			}
		}

		void DrawSkillSetter() {

			EditorGUILayout.LabelField ("Traits");
			EditorGUI.indentLevel++;
			string[] traitNames = System.Enum.GetNames (typeof(Traits));
			bool[] activated = new bool[traitNames.Length];
			var weights = serializedObject.FindProperty ("weights");
			var weightIndicies = serializedObject.FindProperty ("weightIndices");

			for (int i = 0; i < weightIndicies.arraySize; i++) {
				var prop = weightIndicies.GetArrayElementAtIndex (i);
				if (prop.intValue < 0 || prop.intValue >= activated.Length || activated [prop.intValue]) {
					ShiftRemoveWeights (weights, weightIndicies, i);
					i--;
				} else
					activated [prop.intValue] = true;
			}

			EditorGUI.BeginChangeCheck ();

			int j = 0;
			for (int i = 0; i < traitNames.Length; i++) {
				EditorGUILayout.BeginHorizontal ();
				if (activated [i] != EditorGUILayout.ToggleLeft (traitNames [i], activated [i])) {
					// Debug.Log("Before: " + string.Join(", ", GetIntValues(weightIndicies).ToArray()));
					if (activated [i]) {
						//Removes
						ShiftRemoveWeights (weights, weightIndicies, j);
						j--;
					} else {
						//Adds
						ShiftInsertWeights (weights, weightIndicies, i, j);
					}
					// Debug.Log("After: " + string.Join(", ", GetIntValues(weightIndicies).ToArray()));

				}

				if (activated [i]) {					
					if (j >= 0) {
						var prop = weights.GetArrayElementAtIndex (j);
						prop.floatValue = EditorGUILayout.Slider (prop.floatValue, -1, 1);
					}
					j++;
				}

				EditorGUILayout.EndHorizontal ();
			}

			EditorGUI.indentLevel--;
			if (EditorGUI.EndChangeCheck ()) {
				serializedObject.ApplyModifiedProperties ();
			}
		}

		void ShiftInsertWeights(SerializedProperty weights, SerializedProperty weightIndices, int newIndex, int weightIndexIndex) {			
			weights.arraySize ++;
			weightIndices.arraySize ++;
			for (int i = weightIndices.arraySize - 1; i > weightIndexIndex; i--) {				
				weights.GetArrayElementAtIndex (i).floatValue = weights.GetArrayElementAtIndex (i - 1).floatValue;
				weightIndices.GetArrayElementAtIndex (i).intValue = weightIndices.GetArrayElementAtIndex (i - 1).intValue;
			}
			weights.GetArrayElementAtIndex (weightIndexIndex).floatValue = 0;
			weightIndices.GetArrayElementAtIndex (weightIndexIndex).intValue = newIndex;
		}

		void ShiftRemoveWeights(SerializedProperty weights, SerializedProperty weightIndices, int weightIndexIndex) {
			for (int i = weightIndexIndex; i < weightIndices.arraySize - 1; i++) {
				weights.GetArrayElementAtIndex (i).floatValue = weights.GetArrayElementAtIndex (i + 1).floatValue;
				weightIndices.GetArrayElementAtIndex (i).intValue = weightIndices.GetArrayElementAtIndex (i + 1).intValue;
			}
			weightIndices.arraySize--;
			weights.arraySize--;
		}

		IEnumerable<string> GetIntValues(SerializedProperty prop) {
			for (int i = 0; i < prop.arraySize; i++) {
				yield return prop.GetArrayElementAtIndex (i).intValue.ToString ();
			}
		}

	}
}