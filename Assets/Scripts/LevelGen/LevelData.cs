using System.Collections.Generic;

using UnityEngine;

public class LevelData : ScriptableObject {
	public bool WaitUntilTutorialIsOverToStart;
	public List<GridColumn> Grid = new List<GridColumn>();

	public Vector2Int Size => new Vector2Int(Grid.Count, Grid[0].Cells.Count);
}

[System.Serializable]
public class GridColumn {
	public List<CellBlockID> Cells = new List<CellBlockID>();

	public CellBlockID this[int index] {
		get => Cells[index];
		set => Cells[index] = value;
	}
	public void Add(CellBlockID cell) => Cells.Add(cell);
}

