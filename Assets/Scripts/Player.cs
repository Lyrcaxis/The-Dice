
using UnityEngine;

/// <summary> Base class for an object that's controlled by the player. </summary>
public abstract class Player : MonoBehaviour {
	public static Player instance { get; private set; }
	protected virtual void Awake() => instance = this;

	public System.Action OnInitialized;
	public System.Action OnLost;
	public System.Action OnWon;
}
