using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class WorldTextButton : WorldTextDisplayer {
	[SerializeField] Color outerColorHL = Color.green;
	[SerializeField] Color innerColorHL = Color.white;
	[field: Space]
	[field: SerializeField] public UnityEvent onClick { get; private set; }

	Renderer[] renderers;
	Dictionary<Renderer, Color> originalColorsPerRenderer = new Dictionary<Renderer, Color>();

	// Button logic
	void OnMouseOver() {
		// Initialize when first hovered.
		if (renderers == null) {
			renderers = GetComponentsInChildren<Renderer>();
			foreach (var rnd in renderers) { originalColorsPerRenderer.Add(rnd, rnd.sharedMaterial.color); }
		}

		// Turn all colors to 'highlight mode'
		foreach (var rnd in renderers) {
			var clr = rnd.sharedMaterial.color;
			if (clr.g > 0.3f && clr.r < 0.3f) { rnd.sharedMaterial.color = outerColorHL; }
			else { rnd.sharedMaterial.color = innerColorHL; }
		}
	}
	void OnMouseExit() { foreach (var rnd in renderers) { rnd.sharedMaterial.color = originalColorsPerRenderer[rnd]; } }
	void OnMouseUpAsButton() => onClick?.Invoke();

}
 