using System.Linq;

using NormalMode.Environment;
using UnityEngine;

namespace NormalMode {
	public class MovementResolver : MonoBehaviour {
		public static MovementResolver instance { get; private set; }
		void Awake() => instance = this;


		public (MoveValidity canMove, MoveResult onMove) TryMove(Transform transform, Vector3 direction) => TryMove(transform.position + 2 * direction);
		public (MoveValidity canMove, MoveResult onMove) TryMove(Vector3 position) {
			var obj = FindObjectsOfType<EnvironmentObject>().OrderBy(x => x.transform.position.y).FirstOrDefault(x => x.transform.position.x == position.x && x.transform.position.z == position.z);
			if (obj != null) { return obj.TryMove(); }
			else { return (MoveValidity.Success, MoveResult.Loss); } // Walked on empty cell
		}
	}
}