using UnityEngine;
using System.Collections.Generic;

namespace SelfOrganizingTrait {

	public enum Traits {Aesthetic, Irrational, Compasionate, Strength, Agility, Endurance, Touch, Hearing};

	public class SkillSystem : MonoBehaviour {

		public Node nodePrefab;

		public enum RandomDistribution {Uniform, Normal};

		public RandomDistribution nodeStartingSeed;
		[Range(0, 1)] public float initValueLimit;

		public float baseLearningRate = 1f;
		public float learningLvlDecay = 1f;
		public float neighbourhoodLvlDeay = 10f;
		public float neighbourhoodBaseDist = 1f;

		Node[] network;

		[Range(10, 1000)] public int currentNodes;

		[SerializeField] int nodesPerLevel = 10;
		[SerializeField] Transform networkParent;
		[SerializeField] int successUpdateIterations = 4;
		[SerializeField, Range(0, 10)] float systemUpdateDelay = 4;

		[SerializeField] float shakeNodeMagnitude = 0.1f;
		[SerializeField] float shakeFrequency = 5f;

		[SerializeField] int xp;
		[SerializeField] int lvl;

		public float randomValue {
			get {
				if (nodeStartingSeed == RandomDistribution.Uniform) {
					return Random.Range (-initValueLimit, initValueLimit);
				} else {
					// TODO: More efficient normal dist
					int n = 12;
					float val = 0;
					for (int i = 0; i < n; i++) {
						val += Random.Range (-initValueLimit, initValueLimit);
					}
					return val / n;
				}
			}
		}

		public int numberOfTraits {
			get {
				return System.Enum.GetValues (typeof(Traits)).Length;
			}
		}

		public Node[] GetNeighbourhood(float[] input, int[] weightIndices, int skillLvl, out Node bmu) {
			float neighbourhoodWidth = GetSqDist (skillLvl);
			List<Node> neighbours = new List<Node> ();
			float closest = -1;
			bmu = null;
			for (int i = 0; i < network.Length; i++) {
				var dist = network [i].GetSqDistance (input, weightIndices);
				if (dist < neighbourhoodWidth) {
					neighbours.Add (network [i]);
					if (bmu == null || closest < 0) {
						bmu = network [i];
						closest = dist;
					}
				}
			}
				
			return neighbours.ToArray ();
		}

		public Node[] GetNeighbourhood(Skill skill, out Node bmu) {
			return GetNeighbourhood (skill.weights, skill.weightIndices, skill.skillLvl, out bmu);
		}

		float GetSqDist(int lvl) {
			return Mathf.Pow (neighbourhoodBaseDist * Mathf.Exp (-lvl / neighbourhoodLvlDeay), 2f);
		}

		void UpdateNetwork(float[] input, int[] weightIndices, int skillLvl, Node bmu, Node[] neighbourhood) {
			float neighbourhoodWidth = GetSqDist (skillLvl);
			float learningRate = baseLearningRate * Mathf.Exp(-skillLvl/learningLvlDecay);
			for (int i = 0; i < neighbourhood.Length; i++) {
				neighbourhood [i].UpdateWeights (input, weightIndices, learningRate, bmu, neighbourhoodWidth);
			}
		}			

		public void UpdateNetwork(Skill skill, bool success) {
			StartCoroutine (updateNetwork(skill, success ? successUpdateIterations : 1));
		}

		IEnumerator<WaitForSeconds> updateNetwork(Skill skill, int iterations) {
			while (iterations > 0) {
				Node bmu;	
				Node[] neighbourhood = GetNeighbourhood (skill, out bmu);
				Debug.Log ("Learning " + skill.name + ", neighbourhood before " + neighbourhood.Length);
				UpdateNetwork (skill.weights, skill.weightIndices, skill.skillLvl, bmu, neighbourhood);
				iterations--;
				if (iterations > 0)
					yield return new WaitForSeconds (systemUpdateDelay);
			}

		}

		void SetupNetwork() {
			network = new Node[currentNodes];
			var pregenNet = GetComponentsInChildren<Node> ();
			System.Array.Copy (pregenNet, network, Mathf.Min (pregenNet.Length, network.Length));
			int i = 0;
			Node node;
			while (i < currentNodes) {
				if (network [i] == null) {
					network [i] = node = Instantiate (nodePrefab);
					node.name = "Node " + i;
					node.transform.SetParent (networkParent);
				}
				i++;
			}
		}

		void Start() {
			SetupNetwork ();
			StartCoroutine (shakeNodes ());
		}

		IEnumerator<WaitForSeconds> shakeNodes() {
			float[] shakes = new float[numberOfTraits];
			while (true) {
				for (int i = 0; i < network.Length; i++) {
					for (int j=0; j<shakes.Length;j++)
						shakes[j] = randomValue * shakeNodeMagnitude;
					network [i].Shake (shakes);
				}
				yield return new WaitForSeconds (shakeFrequency);
			}
		}

		public int AddXP(int xp) {
			this.xp += xp;
			return this.xp;
		}
	
		public bool AddLvl(int xpCost) {
			if (xpCost <= xp) {
				xp -= xpCost;
				lvl++;
				currentNodes += nodesPerLevel;
				SetupNetwork ();
				return true;
			}
			return false;
		}
	}
}