using UnityEngine;
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

		static float cosVal = Mathf.Sqrt (3) / 2;
		Vector3[] verts = new Vector3[0];
		Mesh mesh;

		Vector3[] TetraHedronVertices(int idBone) {
			
			Bone bone = bones[idBone];

			Vector3 origin = bone.position;
			Vector3 scale = bone.scale;

			Vector3 A = new Vector3 (-cosVal * scale.x, 0, -0.5f * scale.z) + origin;
			Vector3 B = Vector3.forward * scale.z + origin;
			Vector3 C = Vector3.up * scale.y + origin;
			Vector3 D = new Vector3 (cosVal * scale.x, 0, -0.5f * scale.z) + origin;

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
			for (int i = 0; i < boneCont; i++) {
				for (int j = 0; j < 4; j++) {
					uv [i * 12 + j * 3] = Vector2.right + Vector2.down;
					uv [i * 12 + j * 3 + 1] = Vector2.left + Vector2.down;
					uv [i * 12 + j * 3 + 2] = Vector2.up;
				}
			}
			return uv;
		}

		void Awake() {
			if (mesh == null) {
				mesh = new Mesh ();
				mesh.name = "TetraBones";
			}
			GetComponent<MeshFilter> ().sharedMesh = mesh;
			GetComponent<MeshCollider> ().sharedMesh = mesh;
		}

		void Update() {
			RenderMesh ();
		}
	}

}