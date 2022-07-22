using System.Linq;

using NormalMode.Environment;
using UnityEngine;

namespace NormalMode {
	public class MovementResolver : MonoBehaviour {
		public static MovementResolver instance { get; private set; }
		void Awake() => instance = this;


		public (MoveValidity canMove, MoveResult onMove) TryMove(Transform transform, Vector3 direction) => TryMove(transform.position + 2 * direction);
		public (MoveValidity canMove, MoveResult onMove) TryMove(Vector3 position) {
			LevelManager.instance.GridObjectData.TryGetValue(new Vector2(position.x, position.z), out var obj);
			if (obj != null) { return obj.GetComponent<EnvironmentObject>().TryMove(); }	// Get results from the object player's trying to move on.
			else { return (MoveValidity.Success, MoveResult.Loss); }						// Walked on empty cell -- lose.
		}
	}
}