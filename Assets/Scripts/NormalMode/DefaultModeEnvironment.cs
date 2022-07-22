using UnityEngine;


namespace NormalMode.Environment {

	/// <summary> Prefabs contain this component and specify their desired component, and have the related component's enum assigned. </summary>
	/// <remarks> This is done to not require a separate file for each component. This way we can store them all in the same one. </remarks>
	public class DefaultModeEnvironment : MonoBehaviour {
		enum EnvironmentType { DefaultFloor, Wall, Goal, Empty, Key, Door, SpecificDiceMoveOnly }
		[SerializeField] EnvironmentType type;

		void Awake() {
			switch (type) {
				case EnvironmentType.DefaultFloor:			{ this.gameObject.AddComponent<DefaultFloor>(); break; }
				case EnvironmentType.Wall:					{ this.gameObject.AddComponent<Wall>(); break; }
				case EnvironmentType.Goal:					{ this.gameObject.AddComponent<Goal>(); break; }
				case EnvironmentType.Empty:					{ this.gameObject.AddComponent<Empty>(); break; }
				case EnvironmentType.Key:					{ this.gameObject.AddComponent<Key>(); break; }
				case EnvironmentType.Door:					{ this.gameObject.AddComponent<Door>(); break; }
				case EnvironmentType.SpecificDiceMoveOnly:  { this.gameObject.AddComponent<DiceRequiredFloor>(); break; }
				default:							{ Debug.LogError("Bad type", this.gameObject); break; }
			}

			Destroy(this);
		}
	}

	public enum MoveValidity { Success, Fail }				// Whether a move is valid towards a specified position.
	public enum MoveResult { Nothing, Win, Loss, MoveBack }	// What happens once the player moves towards that position.


	/// <summary> Base class of which all environment objects should derive from. </summary>
	/// <remarks> Decides whether the player can move into the specific block, and what happens after he does. </remarks>
	public abstract class EnvironmentObject	: MonoBehaviour	{ public abstract (MoveValidity canMove, MoveResult onAfterMove) TryMove(); }

	public class DefaultFloor	: EnvironmentObject { public override (MoveValidity canMove, MoveResult onAfterMove) TryMove() => (MoveValidity.Success, MoveResult.Nothing); }
	public class Wall			: EnvironmentObject { public override (MoveValidity canMove, MoveResult onAfterMove) TryMove() => (MoveValidity.Fail, MoveResult.Nothing); }
	public class Empty			: EnvironmentObject { public override (MoveValidity canMove, MoveResult onAfterMove) TryMove() => (MoveValidity.Success, MoveResult.Loss); }
	public class Goal : EnvironmentObject {
		void Start() => GetComponentInChildren<ParticleSystem>().gameObject.SetActive(false);
		public override (MoveValidity canMove, MoveResult onAfterMove) TryMove() {
			GetComponentInChildren<ParticleSystem>(true).gameObject.SetActive(true);
			return (MoveValidity.Success, MoveResult.Win);
		}
	}
	public class Key : EnvironmentObject {
		public override (MoveValidity canMove, MoveResult onAfterMove) TryMove() {
			// TODO: Play SFX
			LevelManager.instance.KeysAmount++;

			// Destroy the key's renderer and leave the 'DefaultFloor' for the next time the player walks into the cell.
			Destroy(this.GetComponentInChildren<Renderer>());
			Destroy(this);
			this.gameObject.AddComponent<DefaultFloor>();

			return (MoveValidity.Success, MoveResult.Nothing);
		}
	}
	public class Door : EnvironmentObject {
		public override (MoveValidity canMove, MoveResult onAfterMove) TryMove() {
			// TODO: Play SFX
			LevelManager.instance.KeysAmount--;

			foreach (var c in this.GetComponentsInChildren<Renderer>()) { Destroy(c); }
			Destroy(this);
			this.gameObject.AddComponent<DefaultFloor>();

			return (MoveValidity.Success, MoveResult.Nothing);
		}
	}
	public class DiceRequiredFloor : EnvironmentObject {
		DiceFacing requiredFacing { get; set; }

		void Awake() {
			var numberSpecifier = GetComponent<DiceNumberSpecifier>();
			GetComponent<Renderer>().material.SetTexture("_MainTex", SpriteToTextureConverter.GetTextureFromSprite(numberSpecifier.texture));
			requiredFacing = numberSpecifier.facing;
		}
		public override (MoveValidity canMove, MoveResult onAfterMove) TryMove() {
			var diffFromPlayer = transform.position - Player.instance.transform.position;
			diffFromPlayer.y = 0;
			diffFromPlayer.Normalize();

			if (PlayerOrientationHelper.GetMoveDirBySide(Player.instance.transform, requiredFacing) != diffFromPlayer) { return (MoveValidity.Success, MoveResult.MoveBack); }
			return (MoveValidity.Success, MoveResult.Nothing);
		}
	}
}