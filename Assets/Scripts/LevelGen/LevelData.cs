using System.Collections.Generic;

using UnityEngine;

public class LevelData : ScriptableObject {
	public bool WaitUntilTutorialIsOverToStart;
	public List<TutorialStep> tutorialInfoSteps;
	[Space]
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

public enum ScreenAnchor { TopRight, MiddleRight, BotRight, TopLeft, MiddleLeft, BotLeft }
[System.Serializable]
public class TutorialStep {
	public string Text;
	public ScreenAnchor PlaceToAppear;
	[Space]
	public float DelayToAppear = 0.5f;
	public float DelayToDisappear = 2f;
}
