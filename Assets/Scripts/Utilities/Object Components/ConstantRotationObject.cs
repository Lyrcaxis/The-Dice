using UnityEngine;

public class ConstantRotationObject : MonoBehaviour {
	[SerializeField] Vector3 RotationPerSec = 90 * Vector3.up;

	void Update() => transform.Rotate(RotationPerSec * Time.deltaTime);

}
