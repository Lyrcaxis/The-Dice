using System.Collections;
using System.Collections.Generic;
using System.Linq;

using NormalMode.Environment;

using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : LevelSpawner {

	[SerializeField] List<LevelData> allLevels = new List<LevelData>();

	public Dictionary<Vector2, GameObject> GridObjectData { get; } = new Dictionary<Vector2, GameObject>();

	public LevelData currentLevel { get; set; }
	public int KeysAmount { get; set; }

	protected override float waitTimeBetweenDespawningObjects {
		get {
			if (currentLevel.Size.x > 80) { return 0; }
			if (Player.instance.transform.position.y < 0) { return 1 / 60f; }
			return Mathf.CeilToInt(45f / (currentLevel.Size.x * currentLevel.Size.y)) / 60f;
		}
	}

	public System.Action OnLevelInitialized { get; set; }
	public static LevelManager instance { get; private set; }

	protected override void Awake() {
		instance = this;
		base.Awake();
		LoadNextLevel();
	}

	protected override float GetWaitTimeBetweenWaves(int waveIndex) => 0.5f;
	protected override float GetWaitTimePerPrefabSpawn(int waveIndex) => waveIndex == 0 ? 0.05f : 0.1f;

	protected override void OnObjectSpawned(GameObject obj) {
		if (obj.GetComponent<EnvironmentObject>() == null) { return; } // Do not register any non-environment objects
		GridObjectData[new Vector2(obj.transform.position.x, obj.transform.position.z)] = obj;
	}

	public override void LoadLevel(LevelData levelData) {
		currentLevel = levelData;
		base.LoadLevel(levelData);
	}
	public override void UnloadLevel(System.Action callback) {
		FindObjectOfType<GameCamera>().enabled = false;
		GridObjectData.Clear();
		base.UnloadLevel(callback);
	}

	protected override void OnLevelLoaded() {
		Player.instance.enabled = true;
		Player.instance.transform.SetParent(this.transform);
		transform.Find("Level/Goal").SetParent(this.transform);
		FindObjectOfType<GameCamera>().enabled = true;
		FindObjectOfType<GameCamera>().Start();

		OnLevelInitialized?.Invoke();
		Player.instance.OnInitialized?.Invoke();
	}
	protected override void OnLevelUnloaded(System.Action callback) {
		StartCoroutine(DespawnGoalAndPlayer());

		IEnumerator DespawnGoalAndPlayer() {
			bool didPlayerLose = Player.instance.transform.position.y < 0;
			if (!didPlayerLose) { yield return new WaitForSeconds(0.5f); }
			StartCoroutine(Despawn(transform.Find("Goal").gameObject));
			StartCoroutine(Despawn(Player.instance.gameObject));
			yield return new WaitForSeconds(1.5f);
			callback?.Invoke();
		}
	}


	[ContextMenu("Restart Level")]
	public void RestartLevel() => UnloadLevel(() => LoadLevel(currentLevel));

	[ContextMenu("Load next level")]
	public void LoadNextLevel() {
		if (currentLevel != null) {
			if (currentLevel != allLevels.Last()) {
				var nextLevel = allLevels[allLevels.IndexOf(currentLevel) + 1];
				UnloadLevel(() => LoadLevel(nextLevel));
			}
			else { UnloadLevel(() => SceneManager.LoadScene(0)); }
		}
		else { LoadLevel(allLevels[0]); }
	}
}
