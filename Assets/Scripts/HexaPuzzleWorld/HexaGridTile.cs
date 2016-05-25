using UnityEngine;
using System.Collections.Generic;

namespace HexaPuzzleWorld {

	public class HexaGridTile : MonoBehaviour {

		static HexaGridTile draggingTile;
		static HexaGridTile hovered;

		public static bool DraggingSomething {
			get {
				return draggingTile != null;
			}
		}

		public static bool HoveringSomthing {
			get {
				return hovered != null;
			}
		}


		[SerializeField] bool locked = false;
		[SerializeField, HideInInspector] HexaTile tile;
		[SerializeField] float rotoThreshold = 0;
		[SerializeField] AnimationCurve rotoTransition;
		[SerializeField, Range(0, 0.1f)] float rotationSpeed = 0.03f;
		[SerializeField, Range(10, 20)] int rotationSteps = 10;
		[SerializeField, Range(0, 2)] float snapDistance = 1;
		public HexaGrid playingField;

		bool hovering {
			get {
				return hovered == this;
			}
		}

		bool dragged {
			get {
				return draggingTile == this;
			}
		}
			
		public bool HasBridge(Directions direction) {
			return tile.HasBridge(direction.Rotate(currentRotation));
		}

		bool rotating = false;
		int currentRotation = 0;

		void Start() {
			if (tile == null)
				tile = GetComponent<HexaTile> ();
			if (playingField == null)
				playingField = GetComponentInParent<HexaGrid> ();
			transform.rotation = playingField.GetRotation (currentRotation);
		}			

		public void SetDragging() {
			if (draggingTile == null) {
				draggingTile = this;				
				hovered = this;
			}
		}

		void Update() {
			
			if (Input.GetMouseButtonUp (0) && draggingTile == this) {
				draggingTile = null;
				hovered = null;
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
			if (dragged) {

				Vector3 groundPos = playingField.GetMouseProjection (0);
				var hex = playingField.GetClosestHex (groundPos);

				if (playingField.IsFree (hex) && playingField.GetDistanceToClosest (groundPos) < snapDistance) {					
					var target = playingField.GetWorldGridPos (hex);
					transform.position = Vector3.Lerp (transform.position, target, 0.5f);
				} else {
					transform.position = playingField.GetMouseProjection (tile.TileHeight);
				}
			} else if (!locked) {
				Vector3 groundPos = playingField.GetMouseProjection (0);
				var hex = playingField.GetClosestHex (groundPos);
				if (playingField.GetDistanceToClosest (groundPos) < snapDistance * 1.4f && playingField.Fits (this, hex)) {
					playingField.SetTile (hex, this);
				} else {
					Destroy (gameObject);
				}

			}
		}

		public void Lock() {
			locked = true;
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
			
		void OnDrawGizmosSelected() {
			Gizmos.DrawRay (transform.position, transform.up * 3);
		}
	}
}