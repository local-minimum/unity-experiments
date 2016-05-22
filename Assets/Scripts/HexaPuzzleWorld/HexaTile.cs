using UnityEngine;
using System.Collections.Generic;

namespace HexaPuzzleWorld {

	public enum TriType {None, Abyss, Ground, Wall};

	public class HexaTile : MonoBehaviour {

		[SerializeField, Range(1, 10)] int meshSubdivisionLvl = 1;
		[SerializeField, Range(0, 10)] float size = 1f;
		[SerializeField, Range(0, 10)] int smoothings = 0;
		[SerializeField, Range(0, 3)] float heightFactor = 0.6f;
		[SerializeField] float[] typeProbs;
		[SerializeField] bool fillVerticals = true;
		[SerializeField, HideInInspector] Mesh mesh;
		[SerializeField, HideInInspector] TriType[,]  triGrid;


		void Reset() {
			var rend = GetComponent<MeshRenderer> ();
			if (rend == null) {
				rend = gameObject.AddComponent<MeshRenderer> ();			
			}
			var filt = GetComponent<MeshFilter> ();
			if (filt == null) {
				filt = gameObject.AddComponent<MeshFilter> ();
			}
			if (mesh == null) {
				mesh = new Mesh ();
			}
			mesh.name = "HexaTile";
			filt.sharedMesh = mesh;
		}

		public void Generate() {
			GenerateTris ();
			GenerateMesh ();
		}

		void GenerateTris() {			
			InitTris ();
			EnsureEdges ();
			for (int i = 0; i < smoothings; i++) {
				SmoothTris ();
			}
			EnsureEdges ();
		}

		void InitTris() {
			int rows = Rows;
			int maxCols = MaxCols;
			// Debug.Log ("Rows x Cols " + rows + " x " + maxCols);
			triGrid = new TriType[rows, maxCols];
			int triTypes = System.Enum.GetValues (typeof(TriType)).Length;

			for (int row = 0; row < rows; row++) {
				int tris = GetTrisInRow (row);
				// Debug.Log (tris);
				int startTri = (maxCols - tris) / 2;
				for (int col = startTri, endTri = tris + startTri; col < endTri; col++) {
					// Debug.Log (row + ": " + col);
					triGrid [row, col] = GetTriType();
				}
			}
		}

		TriType GetTriType() {
			float val = Random.value;
			for (int i = 0; i < typeProbs.Length; i++) {
				if (val <= typeProbs [i])
					return (TriType)i+1;
				val -= typeProbs [i];
			}
			return TriType.None;
		}

		void SmoothTris() {
			int rows = triGrid.GetLength (0);
			int cols = triGrid.GetLength (1);
			TriType[,] newTris = new TriType[rows, cols];
			for (int row = 0; row < rows; row++) {
				int tris = GetTrisInRow (row);
				int startTri = (cols - tris) / 2;
				for (int col = startTri, endTri = tris + startTri; col < endTri; col++) {
					newTris [row, col] = GetSmoothTriType (row, col, rows, tris, true);			
				}
			}
			triGrid = newTris;
		}

		TriType GetSmoothTriType(int row, int col, int maxRows, int maxCols, bool pointsUp) {
			int[] votes = new int[4];
			votes [(int) triGrid [row, col]] += 3;

			if (col > 0) {
				votes [(int)triGrid [row, col - 1]] += 2;
			}
			if (col < maxCols - 1) {
				votes[(int)triGrid[row, col + 1]] += 2;
			}
			if (col > 1) {
				votes [(int)triGrid [row, col - 2]] += 1;
			}
			if (col < maxCols - 2) {
				votes [(int)triGrid [row, col + 2]] += 1;
			}

			int dir = pointsUp ? -1 : 1;
			int row2 = row + dir;
			if (row2 >= 0 && row2 < maxRows) {
				
				votes [(int)triGrid [row2, col]] += 2;
				
				if (col > 0) {
					votes [(int)triGrid [row2, col - 1]] += 1;
				}
				if (col < maxCols - 1) {
					votes[(int)triGrid[row2, col + 1]] += 1;
				}
				if (col > 1) {
					votes [(int)triGrid [row2, col - 2]] += 1;
				}
				if (col < maxCols - 2) {
					votes [(int)triGrid [row2, col + 2]] += 1;
				}

			}

			int row3 = row - dir;
			if (row3 >= 0 && row3 < maxRows) {

				votes [(int)triGrid [row3, col]] += 2;

				if (col > 0) {
					votes [(int)triGrid [row3, col - 1]] += 1;
				}
				if (col < maxCols - 1) {
					votes[(int)triGrid[row3, col + 1]] += 1;
				}

			}

			TriType vote = TriType.None;
			int nVotes = 0;
			for (int i = 1; i < votes.Length; i++) {
				if (votes [i] > nVotes) {
					nVotes = votes [i];
					vote = (TriType)i;
				}
			}
			return vote;
		}

		void EnsureEdges() {
			if (meshSubdivisionLvl < 3)
				Debug.LogWarning ("Tile has too small division to ensure edge logic");
				return;
		}

		int Rows {
			get {
				return (int) Mathf.Pow(2, meshSubdivisionLvl);
			}
		}

		int MaxCols {
			get {
				int rows = Rows;
				return 2 * Rows - 1;
			}
		}

		int GetTrisInRow(int row) {
			int rows = Rows;
			return rows + 1 + 2 * (Mathf.Min(row, rows/2) - Mathf.Max (0, row - rows/2 + 1));
		}

		void GenerateMesh() {
			int triCount = NumberOfTriTiles;
			mesh.Clear ();
			int[] walls;
			mesh.vertices = GetMeshVerts (triCount, out walls);
			mesh.triangles = GetMeshTris (triCount, walls);
			mesh.uv = GetMeshUV (triCount);
			mesh.RecalculateNormals ();
			mesh.RecalculateBounds ();
		}

		Vector3[] GetMeshVerts(int triCount, out int[] walls) {
			Vector3[] verts = new Vector3[triCount * 3];
			// Debug.Log (verts.Length);
			int rows = Rows;
			List<int> wallVerts = new List<int> ();
			int maxCols = MaxCols;
			int idVert = 0;
			bool subDiv1 = meshSubdivisionLvl != 1;
			float step = size / rows;
			int prevRow = 0;
			for (int row = 0; row < rows; row++) {
				int trisInRow = GetTrisInRow (row);
				int startTri = (maxCols - trisInRow) / 2;
				for (int col = startTri, endTri = trisInRow + startTri; col < endTri; col++) {
					float y = GetHeight (triGrid [row, col]) * heightFactor;
					float x = step * (col - maxCols / 2f);
					float z = 2 * step * (row - rows / 2f);

					if (col % 2 == row % 2 != subDiv1) {
						verts [idVert] = new Vector3 (x, y, z - step);
						verts [idVert + 2] = new Vector3 (x + step, y, z + step);
						verts [idVert + 1] = new Vector3 (x - step, y, z + step);
						if (fillVerticals && col != startTri && triGrid [row, col] != triGrid [row, col - 1]) {
							wallVerts.Add (idVert - 2);
							wallVerts.Add (idVert);
							wallVerts.Add (idVert - 1);
							wallVerts.Add (idVert - 2);
							wallVerts.Add (idVert + 1);
							wallVerts.Add (idVert);
						}
					} else {
						
						verts [idVert] = new Vector3 (x - step, y, z - step);
						verts [idVert + 2] = new Vector3 (x + step, y, z - step);
						verts [idVert + 1] = new Vector3 (x, y, z + step);

						if (fillVerticals && col != startTri && triGrid [row, col] != triGrid [row, col - 1]) {
							wallVerts.Add (idVert);
							wallVerts.Add (idVert - 1);
							wallVerts.Add (idVert + 1);
							wallVerts.Add (idVert);
							wallVerts.Add (idVert - 3);
							wallVerts.Add (idVert - 1);
						} 

						if (fillVerticals && row > 0 && (col > startTri || row >= rows / 2) && triGrid [row, col] != triGrid [row - 1, col]) {
							int prevOffset = Mathf.Min (trisInRow, prevRow) + Mathf.Abs ((prevRow - trisInRow) / 2);

							wallVerts.Add (idVert);
							wallVerts.Add (idVert - 3 * prevOffset + 2);
							wallVerts.Add (idVert - 3 * prevOffset + 1);
							wallVerts.Add (idVert);
							wallVerts.Add (idVert + 2);
							wallVerts.Add (idVert - 3 * prevOffset + 2);

						}
					}
						
					idVert+=3;
				}
				prevRow = trisInRow;
			}
			walls = wallVerts.ToArray ();
			return verts;
		}

		int[] GetMeshTris(int triCount, int[] walls) {
			int[] tris = new int[triCount * 3 + walls.Length];
			for (int i = 0; i < tris.Length; i++) {
				tris [i] = i;
			}
			System.Array.Copy (walls, 0, tris, triCount * 3, walls.Length);
			return tris;
		}

		Vector2[] GetMeshUV(int triCount) {
			Vector2[] uv = new Vector2[triCount * 3];
			int rows = Rows;
			int maxCols = MaxCols;
			int idUV = 0;
			float x = 0;
			float y = 0;
			float f = 0.4f / 2f;
			for (int row = 0; row < rows; row++) {
				int trisInRow = GetTrisInRow (row);
				int startTri = (maxCols - trisInRow) / 2;
				for (int col = startTri, endTri = trisInRow + startTri; col < endTri; col++) {
					if (triGrid [row, col] == TriType.Ground) {
						x = 0.25f;
						y = 0.75f;
					} else if (triGrid [row, col] == TriType.Abyss) {
						x = 0.25f;
						y = 0.25f;
					} else if (triGrid [row, col] == TriType.Wall) {
						x = 0.75f;
						y = 0.75f;
					} else {
						x = 0.75f;
						y = 0.25f;
					}
						
					uv [idUV] = new Vector2 (x, y - f);
					uv [idUV + 1] = new Vector2 (x - f, y + f);
					uv [idUV + 2] = new Vector2 (x + f, y + f);

					idUV += 3;
				}
			}

			return uv;
		}

		float GetHeight(TriType triType) {
			if (triType == TriType.Abyss)
				return -1;
			else if (triType == TriType.Wall)
				return 1;
			else
				return 0;
		}

		int NumberOfTriTiles {
			get {
				int tris = 0;
				for (int row = 0, rows = Rows; row < rows; row++) {
					tris += GetTrisInRow (row);
				}
					
				return tris;
			}
		}
	}
}