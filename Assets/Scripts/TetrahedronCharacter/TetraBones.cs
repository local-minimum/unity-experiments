﻿using UnityEngine;
using System.Collections;

namespace TetrahedronCharacter {


	[System.Serializable]
	public struct Bone {
		public string name;
		public Vector3 position;
		public Vector3 scale;
		public Quaternion rotation;
		public bool active;
	}

	public class TetraBones : MonoBehaviour {

		public Bone[] bones;

		private Vector3[] boneTips;

		static float cosVal = Mathf.Sqrt (3) / 2;
		Vector3[] verts = new Vector3[0];
		[SerializeField] Mesh mesh;

		Vector3[] TetraHedronVertices(int idBone) {
			
			Bone bone = bones[idBone];

			Vector3 origin = bone.position;
			Vector3 scale = bone.scale;

			Vector3 A = bone.rotation * new Vector3 (-cosVal * scale.x, 0, -0.5f * scale.z) + origin;
			Vector3 B = bone.rotation * Vector3.forward * scale.z + origin;
			Vector3 C = bone.rotation * Vector3.up * scale.y + origin;
			Vector3 D = bone.rotation * new Vector3 (cosVal * scale.x, 0, -0.5f * scale.z) + origin;

			return new Vector3[] {
				A, B, C,
				B, D, C,
				D, A, C,
				D, B, A
			};
		}

		void Reset() {
			bones = null;	
		}

		public void Generate() {
			Awake ();
			RenderMesh ();
		}

		public Vector3 BoneTipPosition(int bone) {
			if (verts.Length < bone * 12 + 2)
				Generate ();
			return transform.TransformPoint (verts [bone * 12 + 2]);
		}

		public Vector3 BoneBaseCenterPosition(int bone) {
			return transform.TransformPoint (bones [bone].position);
		}

		int ActiveBones {
			get {
				int boneCount = 0;
				for (int i = 0; i < bones.Length; i++) {
					if (bones [i].active)
						boneCount++;
				}
				return boneCount;
			}

		}

		void RenderMesh() {
			int boneCount = ActiveBones;
			if (verts.Length != boneCount * 12)
				verts = new Vector3[boneCount * 12];
			
			for (int i=0; i<bones.Length; i++) {
				if (bones[i].active)
					System.Array.Copy(TetraHedronVertices(i), 0, verts, i * 12, 12);
			}

			mesh.Clear ();

			mesh.vertices = verts;
			mesh.triangles = GetTris (verts.Length);
			mesh.uv = GetUV (boneCount);
			mesh.RecalculateNormals ();
			mesh.RecalculateBounds ();
		}

		int[] GetTris(int l) {

			int[] tris = new int[l];

			for (int i = 0; i < l; i++) {
				tris [i] = i;
			}
			return tris;
		}

		Vector2[] GetUV(int boneCont) {
			
			Vector2[] uv = new Vector2[boneCont * 12];
			Vector2 mid = new Vector2 (0.5f, 1f);
			for (int i = 0; i < boneCont; i++) {
				for (int j = 0; j < 4; j++) {
					uv [i * 12 + j * 3] = Vector2.right;
					uv [i * 12 + j * 3 + 1] = Vector2.zero;
					uv [i * 12 + j * 3 + 2] = mid;
				}
			}
			return uv;
		}

		void Awake() {
			
			mesh = new Mesh ();
			mesh.name = "TetraBones";

			var mf = GetComponent<MeshFilter> ();
			if (mf.sharedMesh != mesh)
				mf.sharedMesh = mesh;
			var col = GetComponent<MeshCollider> ();
			if (col.sharedMesh != mesh)
				col.sharedMesh = mesh;
			//col.convex = false;
			//col.convex = true;

		}

		void Update() {
			RenderMesh ();
		}
	}

}