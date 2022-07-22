using System.Collections.Generic;
using UnityEngine;

public static class Extensions {
	public static void Shuffle<T>(this List<T> list) {
		var shuffledList = new List<T>(list);
		list.Clear();

		for (int i = shuffledList.Count - 1; i >= 0; i--) {
			int removeIndex = Random.Range(0, shuffledList.Count);
			list.Add(shuffledList[removeIndex]);
			shuffledList.RemoveAt(removeIndex);
		}
	}
}