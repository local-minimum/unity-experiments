using UnityEngine;
using UnityEditor;

namespace TetrahedronCharacter {

	[CustomEditor(typeof(TetraBones))]
	public class TetraBonesEditor : Editor {

		private TetraBones bone;
		private int selectedBone = -1;
		Tool lastTool = Tool.None;
		Tool curTool = Tool.None;

		public override void OnInspectorGUI ()
		{
			
			base.OnInspectorGUI ();

			if (GUILayout.Button("Add Bone")) {
				Undo.RecordObject (bone, "Add Bone ");
				EditorUtility.SetDirty (bone);

				if (bone.bones == null)
					bone.bones = new Bone[1];
				else {
					var moreBones = new Bone[bone.bones.Length + 1];
					System.Array.Copy (bone.bones, moreBones, bone.bones.Length);
					bone.bones = moreBones;	

				}
				var newBone = new Bone ();
				newBone.name = "unknown";
				newBone.active = true;
				newBone.position = Vector3.up;
				newBone.scale = Vector3.one;
				newBone.rotation = Quaternion.identity;
				bone.bones[bone.bones.Length - 1] = newBone;
				bone.Generate ();
			}
				

			if (selectedBone > 0) {
				//TODO: More info...
				EditorGUILayout.HelpBox ("Bone " + bone.bones [selectedBone].name, MessageType.Info);
			} else {
				EditorGUILayout.HelpBox ((bone.bones == null ? 0 : bone.bones.Length) + " bones", MessageType.Info);

			}
		}

		void OnEnable() {
			bone = target as TetraBones;
			lastTool = Tools.current;

		}

		void OnDisable() {
			Tools.current = lastTool;
		}

		private void OnSceneGUI() {
			

			if (selectedBone >= 0 && Tools.current != Tool.None) {
				curTool = Tools.current;			
				Tools.current = Tool.None;
			} else if (curTool == Tool.Rect) {
				selectedBone = -1;
				curTool = Tool.None;
				Tools.current = lastTool;
			}

			if (bone.bones == null)
				return;
			
			Transform boneTransform = bone.transform;
			float handleScale = HandleUtility.GetHandleSize (boneTransform.position);
			Quaternion handleRotation = Tools.pivotRotation == PivotRotation.Local ? boneTransform.rotation : Quaternion.identity;
			for (int i = 0; i < bone.bones.Length; i++) {
				var curBone = bone.bones [i];
				if (!curBone.active)
					continue;				
				Vector3 pos = boneTransform.TransformPoint (curBone.position);
				if (Handles.Button (pos, handleRotation, 0.1f * handleScale, 0.12f * handleScale, Handles.DotCap)) {
					selectedBone = i;
					Tools.current = Tool.Move;
					curTool = Tool.Move;
				}
				if (selectedBone == i) {
					EditorGUI.BeginChangeCheck ();
					if (curTool == Tool.Rotate) {
						Quaternion qt = Handles.DoRotationHandle (curBone.rotation, pos);
						if (EditorGUI.EndChangeCheck ()) {
							Undo.RecordObject (bone, "Rotate Bone " + i);
							EditorUtility.SetDirty (bone);
							bone.bones [i].rotation = qt;
							bone.Generate ();
						}
					} else if (curTool == Tool.Scale) {
						Vector3 scale = Handles.ScaleHandle (curBone.scale, pos, curBone.rotation, handleScale);
						if (EditorGUI.EndChangeCheck ()) {
							Undo.RecordObject (bone, "Scale Bone " + i);
							EditorUtility.SetDirty (bone);
							bone.bones [i].scale = scale;
							bone.Generate ();
						}

					} else if (curTool == Tool.Move) {
						pos = Handles.DoPositionHandle (pos, handleRotation);
						if (EditorGUI.EndChangeCheck ()) {
							Undo.RecordObject (bone, "Move Bone " + i);
							EditorUtility.SetDirty (bone);
							bone.bones [i].position = boneTransform.InverseTransformPoint (pos);
							bone.Generate ();
						}

					}
				}
			}
		}
	}
}