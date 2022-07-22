using System.Collections;

using UnityEngine;

public class UnstableRotationObject : MonoBehaviour {
	const float MinSpeed = 10f;
	const float MaxSpeed = 60f;

	AxisAngle x = new AxisAngle();
	AxisAngle y = new AxisAngle();
	AxisAngle z = new AxisAngle();

	public Vector3 TargetAngles => new Vector3(x, y, z);

	public void Start() {
		x.current = transform.eulerAngles.x;
		y.current = transform.eulerAngles.y;
		z.current = transform.eulerAngles.z;

		StartCoroutine(StartRotating(x));
		StartCoroutine(StartRotating(y));
		StartCoroutine(StartRotating(z));
	}

	void Update() => transform.eulerAngles = TargetAngles;

	IEnumerator StartRotating(AxisAngle axisAngle) {
		while (true) {
			float t = 0;
			float Period = Random.Range(2f, 5f);
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
