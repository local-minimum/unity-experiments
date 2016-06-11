using UnityEngine;
using System.Collections.Generic;

namespace HexaPuzzleWorld {

	public class HexaGridMesh : MonoBehaviour {

		[SerializeField] HexaGrid playingField;

		[SerializeField, Range(-5,5)] float ridgeElevation;
		[SerializeField, Range(0, 2)] float ridgeWidth;
		[SerializeField, Range(0, 10)] float outerBuffer;
		[SerializeField, Range(0, 1)] float height;
		[SerializeField, Range(0, 15)] float spacing;

		static Quaternion rotA = Quaternion.Euler (0, 60, 0);
		static Quaternion rotB = Quaternion.Euler (0, 0, 0);
		static Quaternion rotMid = Quaternion.Euler (0, 30, 0);


		Mesh mesh;

		public void Generate() {
			SetupComponents ();	
			GenerateMesh ();
		}

		void SetupComponents() {
			if (mesh == null) {
				mesh = new Mesh ();
			}

			if (GetComponent<MeshRenderer> () == null) {
				gameObject.AddComponent<MeshRenderer> ();
			}

			var meshFilt = GetComponent<MeshFilter> ();
			if (meshFilt == null) {
				meshFilt = gameObject.AddComponent<MeshFilter> ();
			}

			meshFilt.sharedMesh = mesh;

			var meshCol = GetComponent<MeshCollider> ();
			if (meshCol == null) {
				meshCol = gameObject.AddComponent<MeshCollider> ();
			}
			meshCol.sharedMesh = mesh;				
		}

		void GenerateMesh() {
			List<Vector3> edgeNorms;
			mesh.Clear ();

			mesh.vertices = GetMeshVerts (out edgeNorms);
			mesh.triangles = GetMeshTris ();
			mesh.uv = GetMeshUV ();
			mesh.normals = GetNormals (edgeNorms);
			mesh.RecalculateBounds ();
		}

		Vector3[] GetMeshVerts(out List<Vector3> edgeNorms) {			
			List<Vector3> verts = new List<Vector3> ();	
			edgeNorms = new List<Vector3> ();
			foreach (Hex hex in playingField.EnumerateHexes()) {
				Vector3 localTileCenter = playingField.GetPosition (hex);
				foreach(Directions dir in System.Enum.GetValues(typeof(Directions))) {
					if (playingField.IsInsideGrid (hex.GetNeighbour(dir))) {
						Vector3 dirVector = dir.FlatTopToVector ();
						verts.AddRange (GetRidgePart (localTileCenter, rotB * dirVector, rotA * dirVector));
						edgeNorms.Add (rotMid * dirVector * -1);
					}
				}
			}

			return verts.ToArray ();
		}

		Vector3[] GetRidgePart(Vector3 origin, Vector3 directionA, Vector3 directionB) {
			Vector3 A = origin + spacing * directionA + Vector3.up * ridgeElevation;
			Vector3 B = origin + (spacing + ridgeWidth) * directionA + Vector3.up * ridgeElevation;
			Vector3 C = origin + spacing * directionB + Vector3.up * ridgeElevation;
			Vector3 D = origin + (spacing + ridgeWidth) * directionB + Vector3.up * ridgeElevation;
			Vector3 down = Vector3.down * height;
			return new Vector3[]{
				A, B, C,
				B, D, C, 
				A, C, C + down,
				A, C + down, A + down};
		}


		int[] GetMeshTris() {
			int l = mesh.vertices.Length;
			int[] tris = new int[l];
			for (int i = 0; i < l; i++) {
				tris[i] = i;
			}
			return tris;
		}

		Vector2[] GetMeshUV() {
			int l = mesh.vertices.Length;
			Vector2[] uv = new Vector2[l];
			return uv;
		}

		Vector3[] GetNormals(List<Vector3> tileSlotEdgeNorms) {
			
			int l = mesh.vertices.Length;
			int j = 0;

			Vector3[] norms = new Vector3[l];

			for (int i = 0; i < l; i++) {
				if (i % 12 < 6) {
					norms [i] = Vector3.up;
				} else {
					norms [i] = tileSlotEdgeNorms [j];
					if (i % 12 == 11) {
						j++;
					}
						
				}
			}
			return norms;
		}

		void OnEnable() {
			playingField.OnGridChange += Generate;
		}

		void OnDisable() {
			playingField.OnGridChange -= Generate;
		}

		void Awake() {
			if (playingField.Setup) {
				Generate ();
			}
		}
	}
}