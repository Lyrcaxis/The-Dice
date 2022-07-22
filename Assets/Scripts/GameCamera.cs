using UnityEngine;


[RequireComponent(typeof(Camera))]
public class GameCamera : MonoBehaviour {
	[SerializeField] Transform target = default;
	[SerializeField] float upDistance = default;
	[SerializeField] Vector3 angle = default;

	[RuntimeInitializeOnLoadMethod] static void InitializeFramerate() => Application.targetFrameRate = 60;

	void Awake() => GetComponent<Camera>().fieldOfView = Screen.width > 2000 ? 15 : Screen.width > 1000 ? 12 : 10;
	public void Start() { if (Player.instance) { target = Player.instance.transform.GetChild(0); } }

	void LateUpdate() {
		if (target == null) { return; }

		transform.eulerAngles = angle;
		transform.position = Vector3.MoveTowards(transform.position, target.position - transform.forward * upDistance, 10 * Time.deltaTime);
	}

	public void UpdateInstantly() {
		if (Player.instance) { target = Player.instance.transform; }
		else { target = FindObjectOfType<Player>().transform; }

		if (target == null) { Debug.LogError("Could not find target."); return; }

		transform.eulerAngles = angle;
		transform.position = target.position - transform.forward * upDistance;
	}
}
