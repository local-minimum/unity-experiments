using UnityEngine;
using System.Collections.Generic;

namespace TetrahedronCharacter {

	public class SpawnPoint : MonoBehaviour {
		[SerializeField] BodyMovementSequencer prefab;

		[Range(0, 5)] public float frequency;

		[SerializeField, Range(0, 0.5f)] float frequencyNoise;

		void OnEnable() {
			StartCoroutine (Spawner());
		}

		IEnumerator<WaitForSeconds> Spawner() {
			while (enabled) {

				BodyMovementSequencer instance = Instantiate (prefab);

				instance.transform.position = transform.position;
				instance.transform.SetParent (transform);
				instance.transform.localRotation = Quaternion.identity;
				instance.transform.localScale = Vector3.one;

				yield return new WaitForSeconds (frequency + Random.Range (-frequency * frequencyNoise, frequency * frequencyNoise));
			}
		}
	}
}