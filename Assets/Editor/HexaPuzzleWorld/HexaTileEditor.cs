using UnityEngine;
using UnityEditor;

namespace HexaPuzzleWorld {

	[CustomEditor(typeof(HexaTile))]
	public class HexaTileEditor : Editor {

		public override void OnInspectorGUI ()
		{
			base.OnInspectorGUI ();

			if (GUILayout.Button ("Generate")) {
				(target as HexaTile).Generate ();
			}
		}

	}
}