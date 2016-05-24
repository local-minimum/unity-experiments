using UnityEngine;
using System.Collections.Generic;

namespace HexaPuzzleWorld {

	public class HexaGridTile : MonoBehaviour {

		static HexaGridTile draggingTile;

		[SerializeField] bool locked = false;
		[SerializeField, HideInInspector] HexaTile tile;
		[SerializeField] float rotoThreshold = 0;
		[SerializeField] AnimationCurve rotoTransition;
		[SerializeField, Range(0, 0.1f)] float rotationSpeed = 0.03f;
		[SerializeField, Range(10, 20)] int rotationSteps = 10;
		public HexaGrid playingField;

		bool hovering = false;

		bool rotating = false;
		static float rotationStep = 360 / 6;
		int currentRotation = 0;

		void Start() {
			if (tile == null)
				tile = GetComponent<HexaTile> ();

			transform.rotation = playingField.GetRotation (currentRotation);
		}

		void Update() {
			
			if (Input.GetMouseButtonUp (0) && draggingTile == this) {
				draggingTile = null;
			}

			if (hovering && !locked && !rotating) {

				if (Input.GetMouseButtonDown (0) && draggingTile == null) {
					draggingTile = this;
				}

				if (draggingTile == this || draggingTile == null) {
					var roto = Input.GetAxis ("Rotate");
					if (roto > rotoThreshold) {
						StartCoroutine (animRotate (currentRotation + 1));
					} else if (roto < -rotoThreshold) {
						StartCoroutine (animRotate (currentRotation - 1));
					}

					if (Input.GetButtonDown ("Submit") || Input.GetMouseButtonDown (2)) {
						tile.Generate ();
					}
				}
			} 
		}

		void LateUpdate() {
			if (draggingTile == this) {

				float h = tile.TileHeight;
				Vector3 pos = playingField.GetMouseProjection (h);
				float d = playingField.GetDistanceToClosest (transform.position);
				if (d / h < 2) {
					var target = playingField.GetWorldGridPos (playingField.GetClosestHex (pos));
					transform.position = Vector3.Lerp (transform.position, target, 0.5f);
				} else {
					transform.position = pos;
				}
			}
		}

		IEnumerator<WaitForSeconds> animRotate(int newRot) {
			rotating = true;
			Quaternion fromRot = transform.rotation;
			Quaternion toRot = playingField.GetRotation (newRot);
			currentRotation = newRot % 6;
			for (int i = 0; i < rotationSteps + 1; i++) {
				transform.rotation  = Quaternion.Lerp (fromRot, toRot, rotoTransition.Evaluate (i / (float)rotationSteps));
				yield return new WaitForSeconds (rotationSpeed);
			}

			rotating = false;
		}

		void OnMouseOver() {
			hovering = true;	
		}

		void OnMouseExit() {
			hovering = false;
		}

		void OnDrawGizmosSelected() {
			Gizmos.DrawRay (transform.position, transform.up * 3);
		}
	}
}