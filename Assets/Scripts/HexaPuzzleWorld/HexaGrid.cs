using UnityEngine;
using System.Collections;

namespace HexaPuzzleWorld {

	public struct Hex {
		public int q;
		public int r;

		public Hex(int q, int r) {
			this.q = q;
			this.r = r;
		}

		public static Hex ClampMagnitude(Hex hex, int magnitude) {
			return new Hex (Mathf.Clamp (hex.q, -magnitude, magnitude), Mathf.Clamp (hex.r, -magnitude, magnitude));
		}

		public Hex N {
			get {
				return new Hex (q - 1, r);
			}
		}

		public Hex S {

			get {
				return new Hex (q + 1, r);
			}
		}

		public Hex NW {
			get {
				return new Hex (q - 1, r + 1);
			}				
		}

		public Hex SW {
			get {
				return new Hex (q, r + 1);
			}
		}

		public Hex SE {
			get {
				return new Hex (q + 1, r - 1);
			}
		}

		public Hex NE {
			get {
				return new Hex (q, r - 1);
			}
		}

		public Hex GetNeighbour(Directions direction) {
			switch (direction) {
			case Directions.N:
				return N;
			case Directions.NE:
				return NE;
			case Directions.NW:
				return NW;
			case Directions.S:
				return S;
			case Directions.SE:
				return SE;
			case Directions.SW:
				return SW;
			default:
				return this;
			}
		}
	}

	public class HexaGrid : MonoBehaviour {

		[SerializeField, Range(1, 50)] int rings;
		[SerializeField, Range(0, 2)] float spacing;

		[SerializeField, HideInInspector] HexaGridTile[,] grid;
		[SerializeField, HideInInspector] Vector3[,] gridPos;

		[SerializeField] HexaGridTile tilePrefab;

		public static Vector3 HexToCube(float q, float r) {
			return new Vector3 (q, -q - r, r);
		}

		public static Vector3 HexToCube(Hex hex) {
			return new Vector3 (hex.q, -hex.q - hex.r, hex.r);
		}

		public static Vector3 HexToCube(Vector2 hex) {
			return new Vector3 (hex.x, -hex.x - hex.y, hex.y);
		}

		public static Hex CubeToHex(Vector3 cube) {
			return new Hex ((int) cube.x, (int) cube.z);
		}

		public static Vector3 RoundCube(Vector3 cube) {
			int rx = Mathf.RoundToInt (cube.x);
			int ry = Mathf.RoundToInt (cube.y);
			int rz = Mathf.RoundToInt (cube.z);

			float dx = Mathf.Abs (rx - cube.x);
			float dy = Mathf.Abs (ry - cube.y);
			float dz = Mathf.Abs (rz - cube.z);

			if ((dx > dy) && (dx > dz)) {
				rx = -ry - rz;
			} else if (dy > dz) {
				ry = -rx - rz;
			} else {
				rz = -rz - ry;
			}
			return new Vector3 (rx, ry, rz);
		}

		public static Hex RoundHex(float q, float r) {
			return CubeToHex (RoundCube (HexToCube (q, r)));
		}

		public static Hex RoundHex(Vector2 hex) {
			return CubeToHex (RoundCube (HexToCube (hex)));
		}

		public static int CubeDistance(Vector3 a, Vector3 b) {
			return Mathf.RoundToInt(Mathf.Max (Mathf.Abs (a.x - b.x), Mathf.Abs (a.y - b.y), Mathf.Abs (a.z - b.z)));
		}

		public static int HexDistance(Hex a, Hex b) {
			return CubeDistance (HexToCube (a), HexToCube (b));
		}

		public static int HexDistanceToCenter(Hex a) {
			return CubeDistance (HexToCube (a), Vector3.zero);
		}

		public static int HexDistanceToCenter(int q, int r) {
			return CubeDistance (HexToCube (q, r), Vector3.zero);
		}

		public static Vector3 ClampCubeMagnitude(Vector3 a, float magnitude) {
			float f = Mathf.Max (Mathf.Abs (a.x), Mathf.Abs (a.y), Mathf.Abs (a.z));
			if (f > magnitude)
				return a / f * magnitude;
			else
				return a;
		}

		public void SetupGrid() {
			if (grid != null && grid.GetLength (0) == CalculateGridSize ()) {
				return;
			}

			grid = null;
			gridPos = null;
			grid = new HexaGridTile[GridSize, GridSize];
			gridPos = new Vector3[GridSize, GridSize];
			int n = N;
			//Debug.Log (GridSize + ", " + n);
			for (int q = -n; q < n + 1; q++) {
				for (int r = -n; r < n + 1; r++) {
					if (HexDistanceToCenter (q, r) > rings)
						continue;
					// Debug.Log (q + ": " + r + ", " + n + " -> " + (q + n) + ": " + (r + n + Mathf.Min(q, 0)));
					gridPos [q + n, r + n + Mathf.Min(q, 0)] = GetLocalGridPos (q, r);
				}
			}

		}
			
		public HexaGridTile GetTile(int q, int r) {
			int n = N;
			return grid [q + n, r + n + Mathf.Min (0, q)];
		}

		public bool IsFree(Hex hex) {
			if (HexDistanceToCenter (hex) > rings)
				return false;
			
			int n = N;
			return grid [hex.q + n, hex.r + n + Mathf.Min (0, hex.q)] == null;
		}

		public bool Fits(HexaGridTile tile, Hex hex) {
			if (!IsFree (hex))
				return false;

			foreach (Directions d in System.Enum.GetValues(typeof(Directions))) {
				var other = GetTile (hex.GetNeighbour(d));
				Debug.Log (other);
				if (other && other.HasBridge (d.Opposing()) != tile.HasBridge (d))
					return false;
			}
			
			return true;
		}

		public HexaGridTile GetTile(Hex hex) {
			if (HexDistanceToCenter (hex) > rings)
				return null;
			int n = N;
			return grid [hex.q + n, hex.r + n + Mathf.Min (0, hex.q)];
		}
		public void SetTile(Hex hex, HexaGridTile tile) {
			int n = N;
			grid [hex.q + n, hex.r + n + Mathf.Min (0, hex.q)] = tile;
			tile.transform.localPosition = GetPosition (hex);
			tile.Lock ();
		}

		public Vector3 GetPosition(int q, int r) {
			int n = N;
			try {
				return gridPos [q + n, r + n + Mathf.Min (0, q)];
			} catch (System.IndexOutOfRangeException) {
				Debug.LogError ("Pos requested: " + q + ": " + r);
				throw new System.IndexOutOfRangeException();
			}
		}

		public Vector3 GetPosition(Hex hex) {
			return GetPosition (hex.q, hex.r);
		}

		public Quaternion GetRotation(int step) {
			return Quaternion.AngleAxis (30 + step * 60, transform.up);
		}
			
		int N {
			get {
				return (GridSize - 1) / 2;
			}
		}

		Vector3 GetLocalGridPos(int q, int r) {
			float x = spacing * Mathf.Sqrt (3) * (q + r / 2f);
			float z = spacing * 3f / 2f * r;
			return new Vector3 (x, 0, z);
		}

		Vector2 GetHexPosition(Vector3 localPos) {
			return new Vector2(
				(localPos.x * Mathf.Sqrt (3) / 3f - localPos.z / 3f) / spacing,
				localPos.z * 2f / 3f / spacing);
		}

		public Vector3 GetLocalGridPos(Hex hex) {
			return GetLocalGridPos (hex.q, hex.r);
		}
		public Vector3 GetWorldGridPos(Hex hex) {
			return transform.TransformPoint (GetLocalGridPos (hex.q, hex.r));
		}

		/*
		public Hex GetClosestHex(Vector3 worldPosition) {			
			var localPos = transform.InverseTransformPoint (worldPosition);
			var cube = ClampCubeMagnitude(HexToCube(GetHexPosition(localPos)), rings);

			return CubeToHex (RoundCube (cube));			
		}*/

		public Hex GetClosestHex(Vector3 worldPosition) {
			var localPos = transform.InverseTransformPoint (worldPosition);
			float closestSqDist = -1f;
			int closestQ = 0;
			int closestR = 0;

			int n = N;
			for (int q = -n; q < n + 1; q++) {
				for (int r = -n; r < n + 1; r++) {
					if (HexDistanceToCenter (q, r) > rings)
						continue;
				
					float sqDist = Vector3.SqrMagnitude(localPos -  GetPosition(q, r));
					if (sqDist < closestSqDist || closestSqDist < 0) {
						closestQ = q;
						closestR = r;
						closestSqDist = sqDist;
					}
				}
			}

			return new Hex (closestQ, closestR);
		}
	
		public float GetDistanceToClosest(Vector3 worldPosition) {
			Hex closest = GetClosestHex (worldPosition);
			return Vector3.Distance(worldPosition, transform.TransformPoint(GetPosition (closest)));
		}

		public int GridSize {
			get {
				if (grid == null)
					return CalculateGridSize ();
				else
					return grid.GetLength (0);
			}
		}

		public float Occupation {
			get {
				float filled = 0;
				float positions = 0;
				int n = N;
				for (int q = -n; q < n + 1; q++) {
					for (int r = -n; r < n + 1; r++) {
						if (HexDistanceToCenter (q, r) > rings)
							continue;
						positions++;
						if (GetTile (q, r) != null)
							filled++;
					}
				}

				return filled / positions;
			}
		}

		public Vector3 GetMouseProjection(float elevation) {			
			Ray ray = Camera.main.ScreenPointToRay (Input.mousePosition);
			Plane ground = new Plane (transform.up, transform.TransformPoint (Vector3.up * elevation));
			float hitDist;
			if (ground.Raycast (ray, out hitDist)) {
				return ray.GetPoint (hitDist);
			} else {
				Debug.LogError ("Camera is not facing ground");
				return Vector3.zero;
			}
		}

		int CalculateGridSize() {
			return 2 * rings + 1;
		}

		void OnDrawGizmosSelected() {			
			Gizmos.color = Color.cyan;
			Vector3 mouseEnter = GetMouseProjection (0);
			//Gizmos.DrawCube (transform.TransformPoint(GetLocalGridPos(RoundHex(GetHexPosition(transform.InverseTransformPoint(mouseEnter))))),  Vector3.one * spacing * 0.25f);
			Gizmos.DrawCube(GetWorldGridPos(GetClosestHex(mouseEnter)), Vector3.one * spacing * 0.25f);
			Gizmos.color = Color.magenta;
			Gizmos.DrawWireCube(mouseEnter, Vector3.one * spacing * 0.4f);
			if (grid == null || gridPos == null)
				return;
			int n = N;
			try {
				for (int q = -n; q < n + 1; q++) {
					for (int r = -n; r < n + 1; r++) {
						if (HexDistanceToCenter (q, r) > rings)
							continue;
						var tile = GetTile (q, r);
						var pos = GetLocalGridPos (q, r);
						Gizmos.color = tile == null ? Color.green : Color.red;
						Gizmos.DrawWireSphere (transform.TransformPoint(pos), spacing * 0.5f);
					}
				}
			} catch (System.IndexOutOfRangeException) {
				// Fail silently while updating
			}
		}

		void Awake() {
			SetupGrid ();
		}

		void Update()  {
			if (Input.GetMouseButtonDown (0) && !HexaGridTile.HoveringSomthing) {
				var hgt = Instantiate (tilePrefab);
				hgt.playingField = this;
				hgt.transform.SetParent (transform);
				hgt.transform.localPosition = transform.TransformPoint(GetMouseProjection (0));
				hgt.SetDragging ();
			}
		}
	}
}