using UnityEngine;
using System.Collections.Generic;

namespace HexaPuzzleWorld {

	public class HexaCam : MonoBehaviour {

		[SerializeField] HexaGrid playingField;
		[SerializeField, Range(0, 40)] float distance = 10;

		void Update() {
			if (HexaGridTile.DraggingSomething) {
				Vector3 target = playingField.transform.position + playingField.transform.up * distance;
				transform.position = Vector3.Lerp (transform.position, target, 0.9f);
				transform.LookAt (playingField.transform.position);
			}
		}
	}
}