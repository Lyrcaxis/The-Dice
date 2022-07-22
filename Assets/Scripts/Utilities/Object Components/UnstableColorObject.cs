using System.Collections;

using UnityEngine;

public class UnstableColorObject : MonoBehaviour {
	const float MinSpeed = .25f;
	const float MaxSpeed = 5f;

	AxisAngle x = new AxisAngle();
	AxisAngle y = new AxisAngle();
	AxisAngle z = new AxisAngle();

	public Vector3 TargetAngles => new Vector3(x, y, z);

	Material mat { get; set; }

	public void Start() {
		mat = GetComponentInChildren<Renderer>().material;
		x.current = mat.color.r;
		y.current = mat.color.g;
		z.current = mat.color.b;

		StartCoroutine(StartRotating(x));
		StartCoroutine(StartRotating(y));
		StartCoroutine(StartRotating(z));

	}

	void Update() {
		var targetColor = Color.white;
		targetColor.r = Mathf.PingPong(Mathf.Abs(x), 1);
		targetColor.g = Mathf.PingPong(Mathf.Abs(y), 1);
		targetColor.b = Mathf.PingPong(Mathf.Abs(z), 1);
		mat.color = targetColor;
	}

	IEnumerator StartRotating(AxisAngle axisAngle) {
		while (true) {
			float t = 0;
			float Period = Random.Range(0.25f, 2f);
			float target = Random.Range(-1, 1);

			while (t <= Period) {
				t += Time.deltaTime;

				float T = t / Period;

				var speed = MinSpeed + ((MaxSpeed - MinSpeed) * Mathf.Pow(Mathf.Sin(Mathf.PI * T), 2));
				axisAngle.current += target * speed * Time.deltaTime;
				yield return null;
			}

			yield return null;
		}
	}

	class AxisAngle {
		public float current = 0;

		public static implicit operator float(AxisAngle axisAngle) => axisAngle.current;
	}
}