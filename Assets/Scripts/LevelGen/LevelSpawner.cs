using System.Collections;
using System.Collections.Generic;
using System.Linq;

using UnityEngine;

/// <summary> Base class for generating a level from <see cref="LevelData"/>. </summary>
/// <remarks> Useful for generating playable levels or text blocks usable as UI. </remarks>
public abstract class LevelSpawner : MonoBehaviour {

	[Tooltip("Customize the instructions with which tiles will spawn with. Customizable to fit needs (e.g.: for some UIs you may want a different prefab for 'floor' or 'Player'.")]
	[SerializeField] List<PrefabTileSwap> customTileInstructions;


	protected Transform levelParentGO { get; set; }

	protected virtual float waitTimeBetweenDespawningObjects => 0.07f;
	protected virtual bool useLocalScale => false;


	protected virtual void Awake() {
		levelParentGO = transform.Find("Level"); // Destroy existing children and cleanup any leftovers
		if (levelParentGO != null) { DestroyImmediate(levelParentGO.gameObject); }

		levelParentGO = new GameObject("Level").transform;
		levelParentGO.SetParent(transform);
		levelParentGO.localEulerAngles = Vector3.zero;
	}

	public virtual void LoadLevel(LevelData levelData) => StartCoroutine(PlaySpawnLevelAnim(levelData));
	public virtual void UnloadLevel(System.Action callback) => StartCoroutine(DisposeCurrentLevel(callback));

	protected virtual void OnLevelLoaded() { }
	protected virtual void OnLevelUnloaded(System.Action callback) => callback?.Invoke();

	protected abstract float GetWaitTimePerPrefabSpawn(int waveIndex);
	protected abstract float GetWaitTimeBetweenWaves(int waveIndex);

	protected virtual void OnObjectSpawned(GameObject obj) { }

	// Animation to spawn blocks for given level data.
	IEnumerator PlaySpawnLevelAnim(LevelData levelData) {
		var prioritizedSpawnInfo = GetSpawnInfoDict(levelData);

		// Go through each spawnInfoList by priority and spawn the necessary blocks.
		for (int i = -1, waveIndex = 0; i < 5; i++) {
			if (!prioritizedSpawnInfo.TryGetValue(i, out var spawnInfoList)) { continue; }
			spawnInfoList.RemoveAll(x => x.prefab == null);
			spawnInfoList.Shuffle();

			var waitT = GetWaitTimePerPrefabSpawn(waveIndex);
			var objIndex = 0;
			var t = 0f;

			// Spawn a certain amount of elements each frame. This will make spawning framerate-independent.
			while (objIndex < spawnInfoList.Count) {
				t += Time.deltaTime;

				var targetIndex = (int) Mathf.Min(t / waitT, spawnInfoList.Count);
				while (objIndex < targetIndex) {
					var obj = spawnInfoList[objIndex++];
					StartCoroutine(SpawnAndMoveTowardsPos(obj.prefab, obj.pos));
				}
				yield return null;
			}

			if (GetWaitTimeBetweenWaves(waveIndex) != -1) { yield return new WaitForSeconds(GetWaitTimeBetweenWaves(waveIndex++)); }
		}

		OnLevelLoaded();
	}
	Dictionary<int, List<(GameObject prefab, Vector3 pos)>> GetSpawnInfoDict(LevelData levelData) {
		// Make a map of custom spawn instructions and gather info that helps with spawning.
		var IDToCustomInstruction = customTileInstructions.ToDictionary(x => x.blockToReplace, x => x);
		var floorBlockID = levelData.Grid.Find(x => x.Cells.Any(y => y.name == "NormalFloor"))?.Cells.Find(x => x.name == "NormalFloor");
		var floorPrefab = floorBlockID == null ? null : IDToCustomInstruction.TryGetValue(floorBlockID, out var obj) ? obj.prefab : floorBlockID?.inGamePrefab;
		var floorPriority = customTileInstructions.Find(x => x.blockToReplace.name == "NormalFloor")?.priority ?? -1;
		var floorSpawnPosY = customTileInstructions.Find(x => x.blockToReplace.name == "NormalFloor")?.customYPos ?? 0;
		var emptyCell = levelData.Grid.Find(x => x.Cells.Any(y => y.name == "Empty"))?.Cells.Find(x => x.name == "Empty");

		// Gather grid data & populate spawn info dict from the level data.
		var spawnInfoDict = new Dictionary<int, List<(GameObject prefab, Vector3 pos)>>();
		for (int x = 0; x < levelData.Grid.Count; x++) {
			var column = levelData.Grid[x];

			for (int z = 0; z < column.Cells.Count; z++) {
				var cell = column.Cells[z];
				var zPos = (column.Cells.Count - 1 - z);					// Invert zPos because of isometric camera.
				if (cell == null || cell == emptyCell) { continue; }		// Skip if cell is empty.

				// Get the necessary data required for spawning the prefab on the correct place. Initialize with default values.
				var priority = cell == floorBlockID ? floorPriority : 0;	// Use the floor's priority if spawning a floor tile.
				var spawnFloorUnderneath = cell.SpawnFloorUnderneath;		// No need to spawn extra floor underneath the floor.
				(GameObject prefab, Vector3 pos) spawnInfo = (cell.inGamePrefab, new Vector3(2 * x, cell.SpawnOffsetY, 2 * zPos));

				// If we have custom instructions for spawning the specific block, use them.
				if (IDToCustomInstruction.TryGetValue(cell, out var customSpawnInfo)) {
					if (customSpawnInfo.customYPos != -2)	{ spawnInfo.pos.y = customSpawnInfo.customYPos; }
					if (customSpawnInfo.prefab != null)		{ spawnInfo.prefab = customSpawnInfo.prefab; }
					if (customSpawnInfo.priority > -2)		{ priority = customSpawnInfo.priority; }
					if (customSpawnInfo.skipSpawningFloor)	{ spawnFloorUnderneath = false; }
				}

				// Create a list for this priority group if one doesn't already exist, then add the block to it.
				if (!spawnInfoDict.ContainsKey(priority)) { spawnInfoDict.Add(priority, new List<(GameObject prefab, Vector3 pos)>()); }
				spawnInfoDict[priority].Add(spawnInfo);

				// And finally, add a floor cell to spawn below that, unless specified otherwise by custom instructions.
				if (spawnFloorUnderneath) {
					if (!spawnInfoDict.ContainsKey(floorPriority)) { spawnInfoDict.Add(floorPriority, new List<(GameObject prefab, Vector3 pos)>()); }
					spawnInfoDict[floorPriority].Add((floorPrefab, (new Vector3(2 * x, floorSpawnPosY, 2 * zPos))));
				}
			}
		}
		return spawnInfoDict;
	}

	// Animation to dispose the current level's blocks.
	IEnumerator DisposeCurrentLevel(System.Action callback = null) {
		var allObjects = new List<GameObject>();
		foreach (Transform child in levelParentGO) { allObjects.Add(child.gameObject); }
		allObjects.Shuffle();

		var waitT = waitTimeBetweenDespawningObjects;
		var objIndex = 0;
		var t = 0f;

		// Despawn a certain amount of elements each frame. This will make despawning framerate-independent.
		while (objIndex < allObjects.Count) {
			t += Time.deltaTime;
			var targetIndex = (int) Mathf.Min(t / waitT, allObjects.Count);
			while (objIndex < targetIndex) {
				var obj = allObjects[objIndex++];
				StartCoroutine(Despawn(obj));
			}
			yield return null;
		}

		OnLevelUnloaded(callback);
	}


	// Animation that spawns a GameObject and animates its alpha towards 1 and its position towards the target position.
	IEnumerator SpawnAndMoveTowardsPos(GameObject prefab, Vector3 targetPos) {
		targetPos = transform.TransformPoint(targetPos);

		var spawnPos = targetPos + transform.up * 5;
		var spawnedObj = Instantiate(prefab, levelParentGO, true);
		spawnedObj.transform.position = spawnPos;
		spawnedObj.transform.eulerAngles = transform.eulerAngles;
		if (useLocalScale) { spawnedObj.transform.localScale = transform.localScale; }

		spawnedObj.name = spawnedObj.name.Replace("(Clone)", "");

		OnObjectSpawned(spawnedObj);

		// Prepare to play the spawn animation. Move the object down while increasing its alpha
		var mat = spawnedObj.GetComponentInChildren<Renderer>().material;
		var currentColor = mat.color;
		var startAlpha = currentColor.a;

		const float totalTime = 0.15f;
		float t = 0;
		while (t < totalTime) {
			t += Time.deltaTime;

			var T = t / totalTime;

			currentColor.a = Mathf.Lerp(0, startAlpha, T);
			mat.SetColor("_Color", currentColor);
			spawnedObj.transform.position = Vector3.Lerp(spawnPos, targetPos, T);

			yield return null;
		}
	}

	// Animation to despawn the a GameObject by animating its alpha towards 0 and its position upwards.
	protected IEnumerator Despawn(GameObject obj) {
		var startPos = obj.transform.position;
		var endPos = startPos + Vector3.up * 5;

		var mat = obj.GetComponentInChildren<Renderer>()?.material;
		if (mat == null) { Destroy(obj); yield break; }
		var currentColor = mat.color;
		var startAlpha = currentColor.a;

		const float totalTime = 0.15f;
		float t = 0;
		while (t < totalTime) {
			t += Time.deltaTime;

			var T = t / totalTime;

			currentColor.a = Mathf.Lerp(0, startAlpha, 1 - T);
			mat.SetColor("_Color", currentColor);
			obj.transform.position = Vector3.Lerp(startPos, endPos, T);

			yield return null;
		}

		Destroy(obj);
	}

	/// <summary> </summary>
	[System.Serializable] class PrefabTileSwap {
		[Tooltip("The BlockID which we want to replace.")] public CellBlockID blockToReplace;
		[Tooltip("This Y position will be used instead.")] public float customYPos = -2;        // '-2' to not override.
		[Tooltip("This prefab will be spawned instead.")] public GameObject prefab;
		[Tooltip("Force not spawning floor for prefab.")] public bool skipSpawningFloor;		// True to make floor not spawn below the prefab.
		[Tooltip("Higher priority blocks will be spawned first.")] public int priority = -2;	// '-2' to not override.
	}
}