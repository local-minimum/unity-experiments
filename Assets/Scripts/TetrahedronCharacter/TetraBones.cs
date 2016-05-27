using UnityEngine;
using System.Collections;

namespace TetrahedronCharacter {
	public class TetraBones : MonoBehaviour {

		[SerializeField] Vector3[] bones;
		[SerializeField] Vector3[] scales;

		float cosVal = Mathf.Sqrt (3) / 2;
		Vector3[] verts = new Vector3[0];
		Mesh mesh;

		Vector3[] TetraHedronVertices(int bone) {
			

			Vector3 origin = bones [bone];
			Vector3 scale = scales [bone];
			// Vector3 rotation = Vector3.zero;

			Vector3 A = new Vector3 (-cosVal * scale.x, 0, -0.5f * scale.z) + origin;
			Vector3 B = Vector3.forward * scale.z + origin;
			Vector3 C = Vector3.up * scale.y + origin;
			Vector3 D = new Vector3 (cosVal * scale.x, 0, -0.5f * scale.z) + origin;

			return new Vector3[] {
				A, B, C,
				B, D, C,
				D, A, C,
				D, A, B
			};
		}
		void RenderMesh() {
			if (verts.Length < bones.Length * 12)
				verts = new Vector3[bones.Length * 12];
			
			for (int i=0; i<bones.Length; i++) {				
				System.Array.Copy(TetraHedronVertices(i), 0, verts, i * 12, 12);
			}

			mesh.Clear ();
			mesh.vertices = verts;
			mesh.triangles = GetTris (verts.Length);
			mesh.uv = GetUV ();
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

		Vector2[] GetUV() {
			int l = bones.Length;
			Vector2 mid = new Vector2 (0.5f, 0.5f);
			Vector2[] uv = new Vector2[l * 12];
			for (int i = 0; i < l; i++) {
				for (int j = 0; j < 4; j++) {
					uv [i * 12 + j * 3] = Vector2.zero;
					uv [i * 12 + j * 3 + 1] = Vector2.right;
					uv [i * 12 + j * 3 + 2] = mid;
				}
			}
			return uv;
		}

		void Awake() {
			mesh = new Mesh ();
			GetComponent<MeshFilter> ().sharedMesh = mesh;
			GetComponent<MeshCollider> ().sharedMesh = mesh;
		}

		void Update() {
			RenderMesh ();
		}
	}

}