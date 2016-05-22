using UnityEngine;
using System.Collections;

namespace SelfOrganizingTrait {

	public class Node : MonoBehaviour {

		private float[] weights;
		[SerializeField] SkillSystem system;

		void Reset() {

			system = GetComponentInParent<SkillSystem> ();

			var added = GetComponentsInChildren<Node> ();
			for (int i = 0; i < added.Length; i++) {
				if (added[i] != null && added[i] != this) {
					Destroy (this);
					return;
				}
			}
		}
			
		void Start () {
			if (system == null)
				system = GetComponentInParent<SkillSystem> ();
			weights = new float[system.numberOfTraits];
			for (int i = 0; i < weights.Length; i++) {
				weights [i] = system.randomValue;
			}
		}

		public float GetSqDistance(float[] input, int[] weightIndices) {
			float dist = 0;
			for (int i = 0; i < weightIndices.Length; i++) {
				dist += Mathf.Pow (input [i] - weights [weightIndices [i]], 2);
			}
			return dist;
		}

		public float GetSqDistance(Node other, int[] weightIndices) {
			return GetSqDistance (other.weights, weightIndices);
		}

		public void UpdateWeights(float[] input, int[] weightIndices, float learningFactor, Node bmu, float sigma) {
			float theta = Mathf.Exp (-GetSqDistance (bmu, weightIndices) / Mathf.Pow (sigma, 2));
			for (int i = 0; i < weightIndices.Length; i++) {
				weights[weightIndices[i]] += theta * learningFactor * (input[i] - weights[weightIndices[i]]);

				// TODO: This is probalby not needed
				weights[weightIndices[i]] = Mathf.Clamp01(weights[weightIndices[i]]);
			}
		}

		public void Shake(float[] magnitudes) {
			if (weights == null)
				return;
			for (int i = 0; i < weights.Length; i++) {
				weights [i] = Mathf.Clamp01 (weights [i] + magnitudes [i]);
			}
		}
	}
}