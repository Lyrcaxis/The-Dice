using UnityEngine;

[CreateAssetMenu]
public class CellBlockID : ScriptableObject {
	public Color Color = Color.white;
	public GameObject inGamePrefab;
	public float SpawnOffsetY;
	public bool SpawnFloorUnderneath = true;
}