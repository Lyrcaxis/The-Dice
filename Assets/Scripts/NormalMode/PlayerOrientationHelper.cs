using System.Collections.Generic;

using UnityEngine;

namespace NormalMode {
	public enum DiceFacing { One, Two, Three, Four, Five, Six }

	/// <summary> Contains useful utility functions that help determine orientation. </summary>
	public static class PlayerOrientationHelper {

		public static Dictionary<KeyCode, DiceFacing> moveBindings = new Dictionary<KeyCode, DiceFacing>() {
			{ KeyCode.Alpha1, DiceFacing.One    },
			{ KeyCode.Alpha2, DiceFacing.Two    },
			{ KeyCode.Alpha3, DiceFacing.Three  },
			{ KeyCode.Alpha4, DiceFacing.Four   },
			{ KeyCode.Alpha5, DiceFacing.Five   },
			{ KeyCode.Alpha6, DiceFacing.Six    },

			{ KeyCode.Keypad1, DiceFacing.One   },
			{ KeyCode.Keypad2, DiceFacing.Two   },
			{ KeyCode.Keypad3, DiceFacing.Three },
			{ KeyCode.Keypad4, DiceFacing.Four  },
			{ KeyCode.Keypad5, DiceFacing.Five  },
			{ KeyCode.Keypad6, DiceFacing.Six   },
		};


		static List<Vector3> unitVectors = new List<Vector3>() { Vector3.up, Vector3.down, Vector3.forward, Vector3.back, Vector3.right, Vector3.left };
		public static Vector3 GetMoveDirBySide(Transform transform, DiceFacing facing) => unitVectors.Find(x => GetDiceFacing(transform.InverseTransformDirection(x)) == facing);

		static DiceFacing GetDiceFacing(Vector3 side) {
			if (side == Vector3.up)			{ return DiceFacing.Two; }
			if (side == Vector3.down)		{ return DiceFacing.Five; }
			if (side == Vector3.forward)	{ return DiceFacing.Three; }
			if (side == Vector3.back)		{ return DiceFacing.Four; }
			if (side == Vector3.right)		{ return DiceFacing.One; }
			if (side == Vector3.left)		{ return DiceFacing.Six; }

			throw new UnityException($"Unrecognized side {side}. This method acceptsonly normalized vectors.");
		}

	}

}
