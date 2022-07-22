using System.Collections.Generic;

using UnityEngine;

/// <summary> Useful utils to turn a sprite to a single standalone texture, for use within a shader. </summary>
public static class SpriteToTextureConverter {
	static Dictionary<Sprite, Texture> textureMap = new Dictionary<Sprite, Texture>();

	public static Texture GetTextureFromSprite(Sprite sprite) {
		if (textureMap.TryGetValue(sprite, out var texture)) { return texture; }
		return textureMap[sprite] = ConvertSpriteToTexture(sprite);
	}

	static Texture ConvertSpriteToTexture(Sprite sprite) {
		var croppedTexture = new Texture2D((int) sprite.rect.width, (int) sprite.rect.height);
		var rect = sprite.textureRect;
		var pixels = sprite.texture.GetPixels((int) rect.x, (int) rect.y, (int) rect.width, (int) rect.height);
		croppedTexture.SetPixels(pixels);
		croppedTexture.Apply();

		return croppedTexture;
	}

}
