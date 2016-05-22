using UnityEngine;
using System.Collections;

namespace SelfOrganizingTrait {

	public enum BattleOutcome {CriticalFail, Fail, Undecided, Success, CriticalSuccess};
	public enum Difficulty {VeryEasy, Easy, Normal, Hard, VeryHard, Impossible};
	public enum Action {Offence, Defence};

	public class Battle : MonoBehaviour {
		[SerializeField] int offenseDie = 6;
		[SerializeField] int offensePointDie = 2;
		[SerializeField] int defenseDie = 6;
		[SerializeField] int defensePointDie = 2;
		[SerializeField] float critThreshold = 0.1f;
		[SerializeField] int[] difficultyDice;

		public BattleOutcome Fight(Skill offender, Skill defender) {
			var offenseRoll = GetRoll (offender, offenseDie, offensePointDie);
			var defenceRoll = GetRoll (defender, defenseDie, defensePointDie);
			return CalculateOutcome (offenseRoll, defenceRoll);
		}

		public BattleOutcome Fight(Skill offender, Difficulty difficulty,  int difficultyLevel) {
			var offenseRoll = GetRoll (offender, offenseDie, offensePointDie);
			var defenceRoll = GetRoll (difficulty, difficultyLevel);
			return CalculateOutcome (offenseRoll, defenceRoll);
		}

		BattleOutcome CalculateOutcome(int offenseRoll, int defenseRoll) {
			if (offenseRoll == defenseRoll)
				return BattleOutcome.Undecided;
			if (offenseRoll > defenseRoll) {
				return (offenseRoll - defenseRoll) / defenseRoll > critThreshold ? BattleOutcome.CriticalSuccess : BattleOutcome.Success;
			} else {
				return (defenseRoll - offenseRoll) / defenseRoll > critThreshold ? BattleOutcome.CriticalFail : BattleOutcome.Fail;
			}
		}
			
		public int GetRoll(Difficulty difficulty, int difficultyLevel) {
			int roll = 0;
			for (int i = 0; i < difficultyLevel; i++)
				roll += GetRoll (difficulty);
			return roll;
		}

		public int GetRoll(Difficulty difficulty) {
			return Random.Range (1, difficultyDice [(int) difficulty] + 1);
		}

		public int GetRoll(Skill skill, Action action) {
			if (action == Action.Offence) {
				return GetRoll (skill, offenseDie, offensePointDie);
			} else {
				return GetRoll (skill, defenseDie, defensePointDie);
			}
		}

		public int GetRoll(Skill skill, int lvlDie, int pointDie) {
			int roll = 0;
			for (int i = 0; i < skill.skillLvl; i++) {
				roll += Random.Range (1, lvlDie + 1);
			}
			Node bmu;
			for (int i = 0, l = skill.skillSystem.GetNeighbourhood (skill, out bmu).Length; i < l; i++) {
				roll += Random.Range (0, pointDie);
			}
			return roll;
		}
	}
}