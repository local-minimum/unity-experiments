using UnityEngine;
using System.Collections.Generic;

namespace SelfOrganizingTrait {
	public class Skill : MonoBehaviour {

		public float[] weights;

		public int[] weightIndices;

		[SerializeField] Skill[] responseSkills;

		[SerializeField] SkillSystem system;
		[SerializeField] int xp;
		int _skillLvl = 1;

		List<int> rollHistory = new List<int> ();

		public int skillLvl {
			get {
				return _skillLvl;
			}
		}
			
		public int Xp {
			get {
				return xp;
			}
		}

		public SkillSystem skillSystem {
			get {
				return system;
			}
		}

		void Reset() {
			system = GetComponentInParent<SkillSystem> ();
		}

		public void Test() {
			Node bmu;
			int size = system.GetNeighbourhood (this, out bmu).Length;
			Debug.Log (name + " (" + _skillLvl + "): " + size + ", bmu=" + bmu);

			Battle arena = FindObjectOfType<Battle> ();
				 
			int roll = arena.GetRoll (this, Action.Offence);
			if (rollHistory.Count >= 10)
				rollHistory.RemoveAt (0);
			rollHistory.Add (roll);
			float avg = 0;
			for (int i=0, l=rollHistory.Count;i<l; i++) {
				avg += rollHistory[i];
			}
			avg /= rollHistory.Count;

			Debug.Log("Attack: " + roll + " Avg: " + avg);
		}

		public void Learn(bool success) {
			system.UpdateNetwork (this, success);
		}

		public int AddXP(int xp) {
			this.xp += xp;
			return this.xp;
		}

		public bool AddLevel(int xpCost) {
			if (xpCost <= xp) {
				xp -= xpCost;
				_skillLvl++;
				return true;
			}
			return false;
		}
	}
}