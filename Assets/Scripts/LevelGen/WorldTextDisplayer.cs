using System.Collections.Generic;
using UnityEngine;

public class WorldTextDisplayer : LevelSpawner {
	[SerializeField] LevelData textLevel;
	[SerializeField] List<float> waitTimesBetweenWaves;
	//[SerializeField] float waitTimeBeforeDespawningObjectsa;
	protected override bool useLocalScale => true;

	protected override void Awake() { base.Awake(); LoadLevel(textLevel); }

	protected override void OnLevelLoaded() {
		var col = this.gameObject.AddComponent<BoxCollider>();
		col.size = new Vector3(textLevel.Size.x, 1, textLevel.Size.y) * 2 - new Vector3(1, 0, 1);
		col.center = new Vector3(textLevel.Size.x, 0, textLevel.Size.y) - new Vector3(1, 0, 1);
	}

	//protected override float waitTimeBetweenDespawningObjects => 0.025f;

	protected override float GetWaitTimePerPrefabSpawn(int waveIndex) => 0.25f / 60f;
	protected override float GetWaitTimeBetweenWaves(int waveIndex) => waveIndex < waitTimesBetweenWaves.Count ? waitTimesBetweenWaves[waveIndex] : 0;
}
