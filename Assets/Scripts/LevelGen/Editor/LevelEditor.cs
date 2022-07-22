using UnityEngine;

using UnityEditor;
using System.Collections.Generic;
using System.Linq;

class LevelEditor : EditorWindow {
	[SerializeField] string LevelName = "New Level";
	[SerializeField] Vector2Int GridSize = Vector2Int.one * 10;
	[SerializeField] Vector2Int StartIndex = Vector2Int.zero;

	[SerializeField] LevelData levelData = new LevelData();
	[SerializeField] CellBlockID selectedBlock;
	[SerializeField] List<CellBlockID> blocksList;
	[SerializeField] Vector2 scrollPos;

	[SerializeField] Color originalBackgroundColor;



	Event e;
	bool isMouseHeld;


	static bool isInitialized;


	[MenuItem("Window/Level Editor %G")]
	public static void ShowWindow() {
		//DestroyImmediate(GetWindow<LevelEditor>()); // uncomment if current editor window is stuck for w/e reason.
		GetWindow<LevelEditor>();
	}

	void Awake() {
		blocksList = AssetDatabase.FindAssets($"t:{typeof(CellBlockID).Name}").Select(GUID => AssetDatabase.GUIDToAssetPath(GUID)).Select(path => AssetDatabase.LoadAssetAtPath<CellBlockID>(path)).ToList();
		originalBackgroundColor = GUI.backgroundColor;

		if (levelData.Grid.Count == 0 || levelData.Grid.Any(x => x.Cells.Any(y => y == null))) {
			GridSize = Vector2Int.one * 10;
			StartIndex = Vector2Int.zero;
			levelData.Grid = GetEmptyGrid(100, 100);
		}

		isInitialized = true;
	}
	void OnGUI() {
		if (!isInitialized) { Awake(); }

		e = Event.current;

		if (!isMouseHeld) { isMouseHeld = e.type == EventType.MouseDown; }
		else if (e.type == EventType.MouseUp) { isMouseHeld = false; }

		DrawOutlines();

		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
		EditorGUILayout.Space(10);
		EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(true));
		{
			EditorGUILayout.Space(10);
			DrawTopPanel();

			EditorGUILayout.Space(10);
			DrawGrid();
		}
		EditorGUILayout.Space(10);
		EditorGUILayout.EndVertical();
		EditorGUILayout.Space(10);
		EditorGUILayout.EndHorizontal();

		this.Repaint();
	}

	void DrawOutlines() {
		const float offset = 2;
		EditorGUI.DrawRect(new Rect(0, 0, this.position.width, this.position.height), Color.black);
		EditorGUI.DrawRect(new Rect(offset, offset, this.position.width - 2 * offset, this.position.height - 2 * offset), Color.grey);
	}
	void DrawTopPanel() {
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false));
		{
			// Draw the Palette with a header
			EditorGUILayout.BeginVertical(GUILayout.MaxWidth(100));
			{
				EditorGUILayout.LabelField("Palette:", GUILayout.Width(60));
				DrawPalette();
			}
			EditorGUILayout.EndVertical();

			GUILayout.Space(20);

			// Then, start a vertical layout that contains the Grid Size, Level Name, and Save/Load buttons.
			EditorGUILayout.BeginVertical(GUILayout.MaxWidth(0));
			{
				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Space(10);
					EditorGUILayout.LabelField("Grid Size:", GUILayout.Width(80));
					GridSize = Vector2Int.CeilToInt(EditorGUILayout.Vector2IntField(new GUIContent(""), GridSize, GUILayout.Width(100)));
				}
				EditorGUILayout.EndHorizontal();


				EditorGUILayout.BeginHorizontal();
				{
					GUILayout.Space(10);
					EditorGUILayout.LabelField("Level Name:", GUILayout.Width(80));
					LevelName = EditorGUILayout.TextField("", LevelName, GUILayout.Width(100));
				}
				EditorGUILayout.EndHorizontal();

				EditorGUILayout.BeginHorizontal();
				{
					if (GUILayout.Button("Save", GUILayout.MaxWidth(100))) { ExportLevel(); }
					if (GUILayout.Button("Load", GUILayout.MaxWidth(100))) { LoadLevel(); }
				}
				EditorGUILayout.EndHorizontal();
			}
			EditorGUILayout.EndVertical();

			EditorGUILayout.Space(10);

			// And finish with drawing the arrow buttons for moving StartIndex.
			EditorGUILayout.BeginVertical(GUILayout.MaxWidth(80));
			{
				var buttonNames = new[] {
					new[] { "", "^", ""},
					new[] { "<", "", ">"},
					new[] { "", "v", "",},
				};
				var buttonFuncs = new[] {
					new System.Action[] { null, () => MoveIndex(0, -1), null},
					new System.Action[] { () => MoveIndex(-1, 0), null, () => MoveIndex(1, 0)},
					new System.Action[] { null, () => MoveIndex(0, 1), null },
				};

				for (int i = 0; i < buttonNames.Length; i++) {
					EditorGUILayout.BeginHorizontal();
					for (int j = 0; j < buttonNames[i].Length; j++) {
						var rect = EditorGUILayout.GetControlRect(GUILayout.Width(18), GUILayout.Height(18));
						if (!string.IsNullOrWhiteSpace(buttonNames[i][j])) {
							if (GUI.Button(rect, buttonNames[i][j])) { buttonFuncs[i][j].Invoke(); };
						}
					}
					EditorGUILayout.EndHorizontal();
				}
			}
			EditorGUILayout.EndVertical();
		}
		EditorGUILayout.EndHorizontal();
	}

	void MoveIndex(int x, int y) {
		var targetIndex = StartIndex + new Vector2Int(x, y);
		var isValidTargetindex = targetIndex.x >= 0 && targetIndex.y >= 0 && targetIndex.x <= 100 - GridSize.x && targetIndex.y <= 100 - GridSize.y;
		// Apply the desired target index if it's within the grid's range, or expand the grid if needed..
		if (isValidTargetindex) { StartIndex = targetIndex; }
		else {
			var size = new Vector2Int(levelData.Grid.Count, levelData.Grid[0].Cells.Count);

			var newGrid = GetEmptyGrid(2 * size.x, 2 * size.y); // Create a new grid with double the size of the smaller grid.
			StartIndex = GridSize / 2;							// Adjust the StartIndex to match the size of the new grid.
			for (int i = 0; i < size.x; i++) {					// Copy the smaller grid's values to the new one, with the required offset.
				for (int j = 0; j < size.y; j++) { newGrid[i + StartIndex.x][j + StartIndex.y] = levelData.Grid[i].Cells[j]; }
			}
			levelData.Grid = newGrid;
			StartIndex += targetIndex;
		}
	}

	void DrawPalette() {
		const int width = 25;
		const int height = 25;

		const int maxHorizontalPaletteCount = 14;
		int rowsCount = 1 + blocksList.Count / maxHorizontalPaletteCount;
		int curIndex = 0;

		EditorGUILayout.BeginVertical();
		for (int i = 0; i < rowsCount; i++) {
			EditorGUILayout.BeginHorizontal();
			for (int j = 0; j < maxHorizontalPaletteCount; j++) {
				var obj = blocksList[curIndex++];
				var rect = EditorGUILayout.GetControlRect(GUILayout.Width(width), GUILayout.Height(height));

				// Draw outline on the selected cell.
				if (obj == selectedBlock) {
					GUI.backgroundColor = Color.yellow;
					var newRect = new Rect(rect);
					newRect.position -= Vector2.one * 5;
					newRect.size += Vector2.one * 5 * 2;
					GUI.Box(newRect, "");
				}

				// Draw the label -- the first letter of its name.
				GUI.backgroundColor = obj.Color;

				GUI.Box(rect, "");
				if (GUI.Button(rect, "")) { selectedBlock = obj; }
				GUI.Label(rect, obj.name[0].ToString());

				if (curIndex >= blocksList.Count) { break; }
			}
			EditorGUILayout.EndHorizontal();
		}
		EditorGUILayout.EndVertical();

		GUI.backgroundColor = originalBackgroundColor;
	}
	void DrawGrid() {
		const int cellWidth = 25;
		const int cellHeight = 25;

		scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
		EditorGUILayout.BeginHorizontal(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false), GUILayout.MaxWidth(cellWidth * GridSize.x), GUILayout.MaxHeight(cellHeight * GridSize.y));
		for (int i = StartIndex.x; i < GridSize.x + StartIndex.x; i++) {
			EditorGUILayout.BeginVertical(GUILayout.ExpandWidth(false), GUILayout.ExpandHeight(false));
			for (int j = StartIndex.y; j < GridSize.y + StartIndex.y; j++) {
				var rect = EditorGUILayout.GetControlRect(GUILayout.Width(cellWidth), GUILayout.Height(cellHeight));
				if (rect.Contains(e.mousePosition) && isMouseHeld && e.button == 0) { levelData.Grid[i][j] = selectedBlock; }

				GUI.backgroundColor = levelData.Grid[i][j]?.Color ?? Color.white;
				var charToDraw = levelData.Grid[i][j]?.name[0].ToString() ?? "";
				charToDraw = charToDraw.Replace("N", "").Replace("E", ""); // Do not draw labels for NormalFloor and EmptyCell
				GUI.Box(rect, "");
				GUI.Box(rect, charToDraw);
			}
			EditorGUILayout.EndVertical();
		}
		GUI.backgroundColor = originalBackgroundColor;
		EditorGUILayout.EndHorizontal();
		EditorGUILayout.EndScrollView();
	}


	List<GridColumn> GetEmptyGrid(int sizeX, int sizeY) {
		var emptyCell = blocksList.Find(x => x.name.StartsWith("Empty"));

		var emptyGrid = new List<GridColumn>();
		for (int x = 0; x < sizeX; x++) {
			emptyGrid.Add(new GridColumn());
			for (int y = 0; y < sizeY; y++) { emptyGrid[x].Add(emptyCell); }
		}
		return emptyGrid;
	}
	void ExportLevel() {
		var assetPath = $"Assets/Scriptable Objects/Levels/{LevelName}.asset";
		bool fileExists = System.IO.File.Exists(assetPath);
		if (fileExists && !EditorUtility.DisplayDialog("Update existing file?", $"File already exists at filepath:\n{assetPath}\n\nAre you sure you want to update it?", "Yes", "No")) { return; }
		if (string.IsNullOrWhiteSpace(LevelName)) { Debug.LogError("Level name cannot be empty"); return; }

		var newGrid = new List<GridColumn>();
		for (int x = 0; x < GridSize.x; x++) {
			newGrid.Add(new GridColumn());

			for (int y = 0; y < GridSize.y; y++) { newGrid[x].Add(levelData.Grid[x + StartIndex.x][y + StartIndex.y]); }
		}

		var level = AssetDatabase.LoadAssetAtPath<global::LevelData>(assetPath) ?? CreateInstance<global::LevelData>();
		level.name = LevelName;
		level.Grid = newGrid;
		if (!fileExists) { AssetDatabase.CreateAsset(level, assetPath); }

		AssetDatabase.SaveAssets();
	}
	void LoadLevel() {
		var level = AssetDatabase.FindAssets($"t:{typeof(global::LevelData).Name}").Select(GUID => AssetDatabase.GUIDToAssetPath(GUID)).Select(path => AssetDatabase.LoadAssetAtPath<global::LevelData>(path)).ToList().Find(x => x.name.ToLower().Contains(LevelName.ToLower()));
		if (!level) { Debug.LogError($"No level found named {LevelName}"); return; }

		GridSize = new Vector2Int(level.Size.x, level.Size.y);
		levelData.Grid = GetEmptyGrid(100, 100);
		StartIndex = Vector2Int.zero;

		for (int i = 0; i < level.Grid.Count; i++) {
			for (int j = 0; j < level.Grid[i].Cells.Count; j++) { levelData.Grid[i][j] = level.Grid[i].Cells[j]; }
		}
	}

	[System.Serializable] class LevelData { public List<GridColumn> Grid = new List<GridColumn>(100); }
}
