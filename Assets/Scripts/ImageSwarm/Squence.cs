using UnityEngine;
using System.Collections.Generic;

namespace ImageSwarm {
	public class Squence : MonoBehaviour {
		[SerializeField] float[] delays;
		[SerializeField] Sprite[] images;

		ImageSwapper swapper;

		int index = 0;

		void Start() {
			swapper = GetComponent<ImageSwapper> ();
			StartCoroutine (sequence ());
		}

		IEnumerator<WaitForSeconds> sequence() {
			while (true) {	
				yield return new WaitForSeconds (delays [index]);
				swapper.SetTarget (images [index]);

				index++;
				index %= images.Length;
			}
		}
	}
}