using UnityEngine;
using System.Collections.Generic;

namespace ImageSwarm {

	public class ImageSwapper : MonoBehaviour {

		Pixel[] pixels;
		int currentIndex = -1;
		[SerializeField, Range(1, 100)] int swapsPerUpdate = 10;
		[SerializeField] Sprite targetImage;
		[SerializeField, Range(0, 10)] float initialDelay = 2f;

		public Sprite target {
			set {
				//TODO: may cause many images at same time... OK?

				targetImage = value;
				currentIndex = 0;
			}
		}

		void Start() {
			pixels = GetComponentsInChildren<Pixel> ();
			//SetTarget (targetImage);
		}

		void Update() {
			if (currentIndex >= 0) {
				for (int i = 0, l = Mathf.Min(currentIndex + swapsPerUpdate, pixels.Length); i<l; i++) {
					pixels [i].SetImage(targetImage);
				}
				currentIndex += swapsPerUpdate;
				if (currentIndex >= pixels.Length) {
					currentIndex = -1;
				}
			}
		}

		public void SetTarget(Sprite image) {
			currentIndex = -1;
			targetImage = image;
			StartCoroutine(DelaySwap());
		}

		IEnumerator<WaitForSeconds> DelaySwap() {
			yield return new WaitForSeconds (initialDelay);
			currentIndex = 0;
		}
	}
}