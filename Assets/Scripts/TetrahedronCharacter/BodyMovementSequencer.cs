using UnityEngine;
using System.Collections.Generic;

namespace TetrahedronCharacter {
	public class BodyMovementSequencer : MonoBehaviour {

		[SerializeField] LegPhysics[] legs;

		[SerializeField] float jumpyness;

		[SerializeField] float cycleFrequency = 0.4f;

		[SerializeField, Range(0, 1)] float firstLegHeadStart = 0.1f;

		[SerializeField, Range(0, 2 * Mathf.PI)] float powerFrequency = 3f;

		[SerializeField] Transform targetTransform;

		float lastWalk = 0;

		bool AllGrounded {
			get {
				for (int i = 0; i < legs.Length; i++) {
					if (!legs [i].Grounded)
						return false;
				}
				return true;
			}
		}

		bool BackGrounded {
			get {
				for (int i = 1; i < legs.Length; i++) {
					if (!legs [i].Grounded)
						return false;
				}
				return true;
			}
		}

		bool TimeForNextCycle {
			get {
				return Time.timeSinceLevelLoad - lastWalk > cycleFrequency;
			}
				
		}

		void Update () {
			if (AllGrounded) {
				if (TimeForNextCycle)
					StartCoroutine (Cycle (true, 1f));
			} else if (BackGrounded) {
				if (TimeForNextCycle)
					StartCoroutine (Cycle (false, 0.9f));

			} else {
				if (TimeForNextCycle)
					StartCoroutine (Cycle (true, 0.5f));

			}
		}

		IEnumerator<WaitForSeconds> Cycle(bool fireFront, float forceModifier) {
			Vector3 direction;
			if (targetTransform)
				direction = (targetTransform.position - transform.position).normalized + Vector3.up * jumpyness;
			else
				direction = Vector3.up * jumpyness;

			direction *= forceModifier;

			lastWalk = Time.timeSinceLevelLoad;
			float baseStrength = 0.8f;
			float extraVigor = 0.7f;
			if (fireFront)
				legs [0].ApplyForce (direction * (baseStrength + extraVigor * Mathf.Abs(Mathf.Sin(Time.time * powerFrequency))));
			yield return new WaitForSeconds (cycleFrequency * firstLegHeadStart);
			for (int i = 1; i < legs.Length; i++) {
				legs [i].ApplyForce (direction * (baseStrength + extraVigor * Mathf.Abs(Mathf.Sin(Time.time * powerFrequency))));
			}
		}
	}
}