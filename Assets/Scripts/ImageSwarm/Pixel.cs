using UnityEngine;
using System.Collections.Generic;

namespace ImageSwarm {
	public class Pixel : MonoBehaviour {

		[SerializeField, Range(0, 1)] float moveMagnitude = 0.1f;
		[SerializeField, Range(0, 1)] float zPositionScale = 0.3f;
		[SerializeField, Range(0, 360)] float noiseFrequency = 1f;
		[SerializeField, Range(0, 5)] float noisify = 1f;
		[SerializeField] Transform cameraTransform;
		[SerializeField, Range(0, 10)] float maxRadius = 1.5f;
		[SerializeField] Sprite image;

		float freq;
		Vector2 imageCenter;
		MeshRenderer mr;

		void Start() {
			freq = Random.Range (-noiseFrequency, noiseFrequency);
			SetImage (image);
			mr = GetComponent<MeshRenderer> ();
		}

		public void SetImage(Sprite image) {
			this.image = image;
			imageCenter = new Vector2 (image.texture.width / 2, image.texture.height / 2);
		}

		void Update () {

			BrownianMove ();
			transform.localPosition = Vector3.ClampMagnitude (transform.localPosition, maxRadius);
			SetColor ();
			transform.LookAt (cameraTransform);

		}

		void BrownianMove() {
			Vector3 pos = transform.position;
			float f = Time.timeSinceLevelLoad * freq;
			pos.x += moveMagnitude * (Mathf.PerlinNoise (pos.x, f) - Mathf.PerlinNoise(f, pos.x) + Random.Range(-noisify, noisify));
			pos.y += moveMagnitude * (-Mathf.PerlinNoise (pos.y, f) + Mathf.PerlinNoise(f, pos.y) + Random.Range(-noisify, noisify));
			pos.z = Mathf.PerlinNoise (pos.x, pos.y) * zPositionScale;
			transform.position = pos;

		}

		void SetColor() {
			Vector2 imagePos = ImageCoordinate;
			Color col = image.texture.GetPixel (Mathf.RoundToInt (imagePos.x), Mathf.RoundToInt (imagePos.y));
			mr.material.color = col;
		}

		Vector2 ImageCoordinate {
			get {
				return new Vector2 (
					imageCenter.x * (1 + transform.localPosition.x / maxRadius),
					imageCenter.y * (1 + transform.localPosition.y / maxRadius));					
			}
		}
	}
}