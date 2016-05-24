using UnityEngine;
using System.Collections.Generic;

namespace HexaPuzzleWorld {

	public enum TriType {None, Abyss, Ground, Wall};
	public enum Directions {N, NE, SE, S, SW, NW};

	public class HexaTile : MonoBehaviour {

		[SerializeField, Range(1, 6)] int meshSubdivisionLvl = 1;
		[SerializeField, Range(0, 10)] float size = 1f;
		[SerializeField, Range(0, 10)] int smoothings = 0;
		[SerializeField, Range(0, 3)] float heightFactor = 0.6f;
		[SerializeField] float[] typeProbs;
		[SerializeField] bool fillVerticals = true;
		[SerializeField, HideInInspector] Mesh mesh;
		[SerializeField, HideInInspector] TriType[,]  triGrid;
		[SerializeField, HideInInspector] List<Directions> openExits;

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
		public float TileHeight {
			get {
				return (GetHeight (TriType.Wall) - GetHeight (TriType.None)) * heightFactor;
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

			if (meshSubdivisionLvl < 3) {
				Debug.LogWarning ("Tile has too small division to ensure edge logic");
				return;
			}

			int rows = Rows;
			int cols = MaxCols;

			int outerRow = GetTrisInRow (0);
			int outerColStart = (cols - outerRow) / 2;
			int midPt = (outerRow - 1) / 2;
			int halfRows = rows / 2;
			int gateSize = (outerRow - 1) / 2;
			int gateOffset = (gateSize - 1) / 2;
			int[] sideGates = new int[4];
			int rowGateOffsets = halfRows / 4;
			openExits.Clear ();

			for (int row = 0; row < rows; row ++) {

				if (row == 0 || row == rows - 1) {
					//Ensuring top and bottom

					int gate = 0;

					for (int col = 0; col < outerRow; col++) {

						if (Mathf.Abs (col - midPt) <= gateOffset) {
							if (triGrid [row, col + outerColStart] == TriType.Ground)
								gate++;
							//} else if (triGrid [row, col] == TriType.Ground) {
						} else {
							triGrid [row, outerColStart + col] = GetClosestNonGround (row, outerColStart + col);					
						}
					}

					if (gate > gateOffset / 2 && gate < gateSize) {
						
						if (row == 0)
							openExits.Add (Directions.N);
						else
							openExits.Add (Directions.S);
						
						for (int offset = -gateOffset; offset <= gateOffset; offset++) {
							triGrid [row, outerColStart + midPt - offset] = TriType.Ground;
						}
					} else {
						//Debug.Log ("Clearing gate " + row);
						for (int offset = -gateOffset; offset <= gateOffset; offset++) {
							triGrid [row, outerColStart + midPt - offset] = GetClosestNonGround (row, outerColStart + midPt - offset);
						}
					}
				} else {
					
					int colsInRow = GetTrisInRow (row);
					int colStart = (cols - colsInRow) / 2;

					if (Mathf.Abs ((row % halfRows) - (halfRows / 2 - 0.5f)) < rowGateOffsets) {
						if (triGrid [row, colStart] == TriType.Ground) {
							sideGates [(row < halfRows ? 0 : 2)]++;
						}
						if (triGrid [row, colStart + colsInRow - 1] == TriType.Ground) {
							sideGates [1 + (row < halfRows ? 0 : 2)]++;
						}
						if (triGrid [row, colStart + 1] == TriType.Ground) {
							sideGates [(row < halfRows ? 0 : 2)]++;
						}
						if (triGrid [row, colStart + colsInRow - 2] == TriType.Ground) {
							sideGates [1 + (row < halfRows ? 0 : 2)]++;
						}

					} else {
						triGrid [row, colStart] = GetClosestNonGround (row, colStart);
						triGrid [row, colStart + 1] = GetClosestNonGround (row, colStart);
						triGrid [row, colStart + colsInRow - 1] = GetClosestNonGround (row, colStart);
						triGrid [row, colStart + colsInRow - 2] = GetClosestNonGround (row, colStart);
					}

				}
			}

			if (sideGates [0] > rowGateOffsets)
				openExits.Add (Directions.NW);
			if (sideGates [1] > rowGateOffsets)
				openExits.Add (Directions.NE);
			if (sideGates [2] > rowGateOffsets)
				openExits.Add (Directions.SW);
			if (sideGates [3] > rowGateOffsets)
				openExits.Add (Directions.SE);


			for (int row = 0; row < rows; row++) {

				int colsInRow = GetTrisInRow (row);
				int colStart = (cols - colsInRow) / 2;

				if (Mathf.Abs ((row % halfRows) - (halfRows / 2 - 0.5f)) < rowGateOffsets) {
					bool leftGate = sideGates [(row < halfRows ? 0 : 2)] > rowGateOffsets;
					bool rightGate = sideGates [1 + (row < halfRows ? 0 : 2)] > rowGateOffsets;
					triGrid [row, colStart] = leftGate ? TriType.Ground : GetClosestNonGround (row, colStart);
					triGrid [row, colStart + 1] = leftGate ? TriType.Ground : GetClosestNonGround (row, colStart);
					triGrid [row, colStart + colsInRow - 1] = rightGate ? TriType.Ground : GetClosestNonGround (row, colStart + colsInRow - 1);
					triGrid [row, colStart + colsInRow - 2] = rightGate ? TriType.Ground : GetClosestNonGround (row, colStart + colsInRow - 2);

				}

			}
		}

		TriType GetClosestNonGround(int x, int y) {
			return TriType.Abyss;
		}

		int Rows {
			get {
				if (triGrid == null)
					return (int)Mathf.Pow (2, meshSubdivisionLvl);
				else
					return triGrid.GetLength (0);
			}
		}

		int MaxCols {
			get {
				if (triGrid == null)
					return 2 * Rows - 1;
				else
					return triGrid.GetLength (1);
			}
		}

		int GetTrisInRow(int row) {
			int rows = Rows;
			return rows + 1 + 2 * (Mathf.Min(row, rows/2) - Mathf.Max (0, row - rows/2 + 1));
		}

		void GenerateMesh() {
			int triCount = NumberOfTriTiles;
			mesh.Clear ();
			Vector3[] xtraNormals;
			List<TriType> xtraLvls;
			mesh.vertices = GetMeshVerts (triCount, out xtraNormals, out xtraLvls);
			mesh.triangles = GetMeshTris ();
			mesh.uv = GetMeshUV (triCount, xtraLvls);
			mesh.normals = GetNormals (triCount, xtraNormals);
			mesh.RecalculateBounds ();
		}

		Vector3[] GetNormals(int ups, Vector3[] wallNormals) {
			Vector3[] normals = new Vector3[ups * 3 + wallNormals.Length];
			for (int i = 0; i < ups*3; i++) {
				normals [i] = Vector3.up;
			}
			System.Array.Copy (wallNormals, 0, normals, ups * 3, wallNormals.Length);
			return normals;
		}

		Vector3[] GetMeshVerts(int triCount, out Vector3[] extraNorms, out List<TriType> xtraLvls) {
			Vector3[] verts = new Vector3[triCount * 3];
			// Debug.Log (verts.Length);
			int rows = Rows;
			xtraLvls = new List<TriType> ();
			List<Vector3> wallNorms = new List<Vector3> ();
			List<Vector3> xtraVerts = new List<Vector3> ();
			int maxCols = MaxCols;
			int idVert = 0;
			bool subDiv1 = meshSubdivisionLvl != 1;
			float step = size / rows;
			int prevRow = 0;
			float zFactor = Mathf.Sqrt (3) / 2;
			float rimHeight = GetHeight (TriType.None) * heightFactor;
			TriType prevLvl = TriType.None;
			TriType lvl = TriType.None;

			Dictionary<float, TriType> heightLookup = new Dictionary<float, TriType> ();
			heightLookup [GetHeight (TriType.Abyss) * heightFactor] = TriType.Abyss;
			heightLookup [GetHeight (TriType.Ground) * heightFactor] = TriType.Ground;
			heightLookup [GetHeight (TriType.Wall) * heightFactor] = TriType.Wall;

			for (int row = 0; row < rows; row++) {
				int trisInRow = GetTrisInRow (row);
				int startTri = (maxCols - trisInRow) / 2;
				for (int col = startTri, endTri = trisInRow + startTri; col < endTri; col++) {

					lvl = triGrid [row, col];
					float y = GetHeight (lvl) * heightFactor;
					float x = step * (col - maxCols / 2f + 0.5f);
					float z = 2 * step * (row - rows / 2f + 0.5f) * zFactor;

					if (col % 2 == row % 2 != subDiv1) {
						verts [idVert] = new Vector3 (x, y, z - step * zFactor);
						verts [idVert + 2] = new Vector3 (x + step, y, z + step * zFactor);
						verts [idVert + 1] = new Vector3 (x - step, y, z + step * zFactor);
						if (fillVerticals && col != startTri && triGrid [row, col] != triGrid [row, col - 1]) {
							
							xtraVerts.Add (verts[idVert - 2]);
							xtraLvls.Add (prevLvl);
							xtraVerts.Add (verts[idVert]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (verts[idVert - 1]);
							xtraLvls.Add (prevLvl);

							xtraVerts.Add (verts[idVert - 2]);
							xtraLvls.Add (prevLvl);
							xtraVerts.Add (verts[idVert + 1]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (verts[idVert]);
							xtraLvls.Add (lvl);

							for (int i=0;i<6;i++)
								wallNorms.Add (Vector3.left);
						}

						if (IsEdgeS(row)) {

							xtraVerts.Add (new Vector3 (x + step, rimHeight, z + step * zFactor));
							xtraLvls.Add (TriType.None);
							xtraVerts.Add (verts[idVert + 1]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (new Vector3 (x - step, rimHeight, z + step * zFactor));
							xtraLvls.Add (TriType.None);

							xtraVerts.Add (verts[idVert + 2]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (verts[idVert + 1]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (new Vector3 (x + step, rimHeight, z + step * zFactor));
							xtraLvls.Add (TriType.None);

							for (int i=0;i<6;i++)
								wallNorms.Add (Vector3.back);
						}

						if (IsEdgeNW (row, col)) {

							xtraVerts.Add (new Vector3 (x - step, rimHeight, z + step * zFactor));
							xtraLvls.Add (TriType.None);
							xtraVerts.Add (verts[idVert + 1]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (new Vector3 (x, rimHeight, z - step * zFactor));
							xtraLvls.Add (TriType.None);

							xtraVerts.Add (verts[idVert + 1]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (verts[idVert]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (new Vector3 (x, rimHeight, z - step * zFactor));
							xtraLvls.Add (TriType.None);


							for (int i=0;i<6;i++)
								wallNorms.Add (Vector3.left);

						}
							
						if (IsEdgeNE (row, col)) {

							xtraVerts.Add (new Vector3 (x + step, rimHeight, z + step * zFactor));
							xtraLvls.Add (TriType.None);
							xtraVerts.Add (new Vector3 (x, rimHeight, z - step * zFactor));
							xtraLvls.Add (TriType.None);
							xtraVerts.Add (verts[idVert + 2]);
							xtraLvls.Add (lvl);

							xtraVerts.Add (verts[idVert]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (verts[idVert + 2]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (new Vector3 (x, rimHeight, z - step * zFactor));
							xtraLvls.Add (TriType.None);

							for (int i=0;i<6;i++)
								wallNorms.Add (Vector3.forward);
						}
														
					} else {
						
						verts [idVert] = new Vector3 (x - step, y, z - step * zFactor);
						verts [idVert + 2] = new Vector3 (x + step, y, z - step * zFactor);
						verts [idVert + 1] = new Vector3 (x, y, z + step * zFactor);

						if (fillVerticals && col != startTri && triGrid [row, col] != triGrid [row, col - 1]) {
							
							xtraVerts.Add (verts[idVert]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (verts[idVert - 1]);
							xtraLvls.Add (prevLvl);
							xtraVerts.Add (verts[idVert + 1]);
							xtraLvls.Add (lvl);

							xtraVerts.Add (verts[idVert]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (verts[idVert - 3]);
							xtraLvls.Add (prevLvl);
							xtraVerts.Add (verts[idVert - 1]);
							xtraLvls.Add (prevLvl);

							for (int i=0;i<6;i++)
								wallNorms.Add (Vector3.right);
							
						} 

						if (fillVerticals && row > 0 && (col > startTri || row >= rows / 2) && triGrid [row, col] != triGrid [row - 1, col]) {
							int prevOffset = Mathf.Min (trisInRow, prevRow) + Mathf.Abs ((prevRow - trisInRow) / 2);

							TriType otherLvl = triGrid [row - 1, col];

							xtraVerts.Add (verts[idVert]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (verts[idVert - 3 * prevOffset + 2]);
							xtraLvls.Add (otherLvl);
							xtraVerts.Add (verts[idVert - 3 * prevOffset + 1]);
							xtraLvls.Add (otherLvl);

							xtraVerts.Add (verts[idVert]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (verts[idVert + 2]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (verts[idVert - 3 * prevOffset + 2]);
							xtraLvls.Add (otherLvl);

							for (int i=0;i<6;i++)
								wallNorms.Add (Vector3.forward);
							
						}

						if (IsEdgeN(row)) {

							xtraVerts.Add (new Vector3 (x + step, rimHeight, z - step * zFactor));
							xtraLvls.Add (TriType.None);
							xtraVerts.Add (new Vector3 (x - step, rimHeight, z - step * zFactor));
							xtraLvls.Add (TriType.None);
							xtraVerts.Add (verts[idVert]);
							xtraLvls.Add (lvl);

							xtraVerts.Add (verts[idVert]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (verts[idVert + 2]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (new Vector3 (x + step, rimHeight, z - step * zFactor));
							xtraLvls.Add (TriType.None);

							for (int i=0;i<6;i++)
								wallNorms.Add (Vector3.forward);
						}

						if (IsEdgeSE (row, col)) {
							
							xtraVerts.Add (new Vector3 (x + step, rimHeight, z - step * zFactor));
							xtraLvls.Add (TriType.None);
							xtraVerts.Add (verts[idVert + 1]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (new Vector3 (x, rimHeight, z + step * zFactor));
							xtraLvls.Add (TriType.None);

							xtraVerts.Add (verts[idVert + 2]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (verts[idVert + 1]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (new Vector3 (x + step, rimHeight, z - step * zFactor));
							xtraLvls.Add (TriType.None);

							for (int i=0;i<6;i++)
								wallNorms.Add (Vector3.forward);
						}

						if (IsEdgeSW (row, col)) {

							xtraVerts.Add (new Vector3 (x - step, rimHeight, z - step * zFactor));
							xtraLvls.Add (TriType.None);
							xtraVerts.Add (new Vector3 (x, rimHeight, z + step * zFactor));
							xtraLvls.Add (TriType.None);
							xtraVerts.Add (verts[idVert + 1]);
							xtraLvls.Add (lvl);

							xtraVerts.Add (verts[idVert + 1]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (verts[idVert]);
							xtraLvls.Add (lvl);
							xtraVerts.Add (new Vector3 (x - step, rimHeight, z - step * zFactor));
							xtraLvls.Add (TriType.None);

							for (int i=0;i<6;i++)
								wallNorms.Add (Vector3.forward);
						}
					}
					prevLvl = lvl;
					idVert+=3;
				}
				prevRow = trisInRow;

			}
			Vector3[] allVerts = new Vector3[verts.Length + xtraVerts.Count];
			System.Array.Copy (verts, allVerts, verts.Length);
			System.Array.Copy (xtraVerts.ToArray (), 0, allVerts, verts.Length, xtraVerts.Count);
			extraNorms = wallNorms.ToArray ();
			return allVerts;
		}

		bool IsEdgeN(int row) {
			return row == 0;
		}

		bool IsEdgeNW(int row, int col) {
			return (Rows / 2 - 1) - row == col;
		}

		bool IsEdgeSW(int row, int col) {
			return row - Rows / 2 == col;
		}

		bool IsEdgeS(int row) {
			return row == Rows - 1;
		}

		bool IsEdgeSE(int row, int col) {
			return row - (Rows / 2 - 1) == MaxCols - col;
		}

		bool IsEdgeNE(int row, int col) {
			return Rows / 2 - row == MaxCols - col;
		}
			
		int[] GetMeshTris() {
			int[] tris = new int[mesh.vertices.Length];
			for (int i = 0; i < tris.Length; i++) {
				tris [i] = i;
			}
			return tris;
		}

		Vector2[] GetMeshUV(int triCount, List<TriType> xtraLvls) {
			Vector2[] uv = new Vector2[triCount * 3 + xtraLvls.Count];
			int rows = Rows;
			int maxCols = MaxCols;
			int idUV = 0;
			for (int row = 0; row < rows; row++) {
				int trisInRow = GetTrisInRow (row);
				int startTri = (maxCols - trisInRow) / 2;
				for (int col = startTri, endTri = trisInRow + startTri; col < endTri; col++) {
					var pos = GetUVCoordinate (triGrid [row, col]);						
					uv [idUV] = pos;
					uv [idUV + 1] = pos;
					uv [idUV + 2] = pos;

					idUV += 3;
				}
			}
			for (int i = 0, o = triCount * 3; o < uv.Length; i++, o++) {				
				uv [o] = GetUVCoordinate (xtraLvls [i]);
			}

			return uv;
		}

		Vector2 GetUVCoordinate(TriType triType) {
			if (triType == TriType.Ground) {
				return new Vector2(0.5f, 0.5f);

			} else if (triType == TriType.Abyss) {
				return new Vector2(0.5f, 0.2f);
			} else if (triType == TriType.Wall) {
				return new Vector2(0.5f, 0.8f);
			} else {
				return new Vector2(0.5f, 0.1f);
			}

		}

		float GetHeight(TriType triType) {
			if (triType == TriType.Abyss)
				return -1f;
			else if (triType == TriType.Wall)
				return 1f;
			else if (triType == TriType.Ground)
				return 0f;
			else
				return -1.5f;
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

		void Awake() {
			mesh = null;
			Reset ();
			Generate ();
		}
	}
}