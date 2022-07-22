using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuManager : LevelSpawner {
	[Space]
	[SerializeField] WorldTextButton playButton;
	[SerializeField] WorldTextButton creditsButton;
	[SerializeField] WorldTextButton backButton;

	[SerializeField] GameObject credits;
	[Space]
	[SerializeField] LevelData logoLevel;


	protected override void Awake() {
		base.Awake();
		LoadLevel(logoLevel);
		playButton.onClick.AddListener(OnPlayButtonClicked);
		creditsButton.onClick.AddListener(OnCreditsButtonClicked);
		backButton.onClick.AddListener(OnBackToMenuButtonClicked);
	}
	protected override void OnLevelLoaded() {
		StartCoroutine(ApplyFinalTouches());

		IEnumerator ApplyFinalTouches() {
			Player.instance.gameObject.AddComponent<UnstableRotationObject>();
			Player.instance.gameObject.AddComponent<UnstableColorObject>();

			yield return new WaitForSeconds(1f);
			playButton.gameObject.SetActive(true);
			yield return new WaitForSeconds(0.5f);
			creditsButton.gameObject.SetActive(true);
		}
	}

	List<float> waitTimeBetweenWaves = new List<float>() { 0.5f, 3.4f, 0f, 0f };
	//List<float> waitTimeBetweenWaves = new List<float>() { 0, 0, 0, 0f };
	protected override float GetWaitTimePerPrefabSpawn(int waveIndex) => 5f / 60f;
	protected override float GetWaitTimeBetweenWaves(int waveIndex) => waitTimeBetweenWaves[waveIndex];

	void OnPlayButtonClicked() {
		// Disable the button components so events won't trigger multiple times.
		foreach (var btn in new[] { playButton, creditsButton }) {
			btn.enabled = false;
			if (btn.TryGetComponent<Collider>(out var col)) { col.enabled = false; }
		}
		
		// Find all spawners and unload them, then load the game scene.
		int despawnedLevelsAmount = 0;
		var allSpawners = FindObjectsOfType<LevelSpawner>();
		foreach (var spn in allSpawners) { spn.UnloadLevel(() => { if (++despawnedLevelsAmount == allSpawners.Length) { StartCoroutine(LoadAfterDelay()); } }); }

		IEnumerator LoadAfterDelay() { yield return new WaitForSeconds(0.5f); SceneManager.LoadScene(1); }
	}
	void OnCreditsButtonClicked() => StartCoroutine(AnimateCamera(new Vector3(-230, 80, -30), new Vector3(30, 90, 0), () => credits.SetActive(true)));
	void OnBackToMenuButtonClicked() => StartCoroutine(AnimateCamera(new Vector3(-55, 51, -57), new Vector3(30, 45, 0)));

	IEnumerator AnimateCamera(Vector3 targetPos, Vector3 targetRot, System.Action OnComplete = null) {
		const float totalTime = 0.8f;
		const float moveSpeed = 400f;
		const float rotSpeed = 100f;

		var cam = Camera.main;
		float t = 0;

		while (t < totalTime) {
			t += Time.deltaTime;

			cam.transform.position = Vector3.MoveTowards(cam.transform.position, targetPos, moveSpeed * Time.deltaTime);
			cam.transform.eulerAngles = Vector3.MoveTowards(cam.transform.eulerAngles, targetRot, rotSpeed * Time.deltaTime);
			yield return null;
		}

		OnComplete?.Invoke();
	}
}
