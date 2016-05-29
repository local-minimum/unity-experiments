using UnityEngine;
using System.Collections.Generic;

namespace TetrahedronCharacter {
	public class LegPhysics: MonoBehaviour {

		public TetraBones bone;

		[SerializeField, Range(0, 1)] float forcePlace;
		[SerializeField] float muscleStrength;
		[SerializeField, Range(0, 1)] float groundingGrazePeriod;
		[SerializeField] Rigidbody rb;
		[SerializeField, Range(0, 1)] float ungroundedFactor;
		[SerializeField] LayerMask groundingLayers;
		[SerializeField] float maxSqDistContactToTip = 0.5f;

		float lastGround = 0;

		void Awake() {
			if (bone == null)
				bone = GetComponent<TetraBones> ();
		}

		public bool Grounded {
			get {
				return Time.timeSinceLevelLoad - lastGround < groundingGrazePeriod;
			}

			private set {
				lastGround = Time.timeSinceLevelLoad;
			}
		}

		public void ApplyForce(Vector3 direction) {
			Vector3 pos = Vector3.Lerp (bone.BoneTipPosition (0), bone.BoneBaseCenterPosition (0), forcePlace);
			rb.AddForceAtPosition (direction * (Grounded ? 1f : ungroundedFactor) * muscleStrength, pos, ForceMode.Impulse);
		}

		public void OnCollisionStay(Collision col) {
			if (((1<<col.gameObject.layer) & groundingLayers) != 0) {
				var tipPos = bone.BoneTipPosition (0);
				var contacts = col.contacts;
				for (int i = 0; i < contacts.Length; i++) {
					if ((contacts [i].point - tipPos).sqrMagnitude < maxSqDistContactToTip) {
						Grounded = true;
						return;
					}
				}
			}
		}
	}
}