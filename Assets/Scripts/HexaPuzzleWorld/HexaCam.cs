using UnityEngine;
using System.Collections.Generic;

namespace HexaPuzzleWorld {

	public class HexaCam : MonoBehaviour {

		[SerializeField] HexaGrid playingField;
		[SerializeField, Range(0, 40)] float distance = 10;
		[SerializeField] LayerMask rayHitLayerMask;

		Camera cam;
		bool initPos = true;

		void Start() {
			cam = GetComponent<Camera> ();
		}

		void Update() {
			if (HexaGridTile.DraggingSomething || initPos) {
				Vector3 target = playingField.transform.position + playingField.transform.up * distance;
				transform.position = Vector3.Lerp (transform.position, target, 0.9f);
				transform.LookAt (playingField.transform.position);
				initPos = false;
			} else {
				var ray = cam.ScreenPointToRay (Input.mousePosition);
				RaycastHit hit;
				if (Physics.Raycast (ray, out hit, distance * 2, rayHitLayerMask)) {
					var hexGridTile = hit.collider.gameObject.GetComponent<HexaGridTile> ();
					if (hexGridTile) {
						hexGridTile.hovering = true;
					} else {
						HexaGridTile.HoveringSomthing = false;
					}
				} else {
					HexaGridTile.HoveringSomthing = false;
				}
			}

		}
	}
}