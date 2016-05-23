using UnityEngine;
using System.Collections.Generic;

namespace HexaPuzzleWorld {

	public class HexaGridTile : MonoBehaviour {

		[SerializeField] bool locked = false;
		[SerializeField, HideInInspector] HexaTile tile;
		[SerializeField] float rotoThreshold = 0;
		[SerializeField] AnimationCurve rotoTransition;
		[SerializeField, Range(0, 0.1f)] float rotationSpeed = 0.03f;
		[SerializeField, Range(10, 20)] int rotationSteps = 10;

		bool hovering = false;

		bool rotating = false;
		static float rotationStep = 360 / 6;
		int currentRotation = 0;

		void Start() {
			if (tile == null)
				tile = GetComponent<HexaTile> ();
		}

		void Update() {
			if (hovering && !locked && !rotating) {
				var roto = Input.GetAxis ("Rotate");
				if (roto > rotoThreshold) {
					StartCoroutine(animRotate(currentRotation + 1));
				} else if (roto < -rotoThreshold) {
					StartCoroutine(animRotate(currentRotation - 1));
				}

				if (Input.GetButtonDown("Submit")) {
					tile.Generate();
				}
			} 
		}

		IEnumerator<WaitForSeconds> animRotate(int newRot) {
			rotating = true;
			for (int i = 0; i < rotationSteps + 1; i++) {
				transform.localRotation = Quaternion.AngleAxis (Mathf.Lerp(currentRotation, newRot, rotoTransition.Evaluate(i/(float)rotationSteps)) * rotationStep, Vector3.up);
				yield return new WaitForSeconds (rotationSpeed);
			}
			currentRotation = newRot % 6;
			Debug.Log ("Rotated");
			rotating = false;
		}

		void OnMouseOver() {
			hovering = true;	
		}

		void OnMouseExit() {
			hovering = false;
		}

		void OnDrawGizmosSelected() {
			Gizmos.DrawRay (transform.position, transform.up * 5);
		}
	}
}