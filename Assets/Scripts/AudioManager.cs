using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour {
	static AudioManager instance { get; set; }
	void Awake() {
		if (instance != null) { DestroyImmediate(this.gameObject); return; }
		DontDestroyOnLoad((instance = this).gameObject);
	}
}
