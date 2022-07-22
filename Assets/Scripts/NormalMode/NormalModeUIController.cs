using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

namespace NormalMode {
	public class NormalModeUIController : MonoBehaviour {
		[SerializeField] GameObject restartPanel;
		[SerializeField] GameObject victoryPanel;

		[SerializeField] Button restartButton;
		[SerializeField] Button nextLevelButton;

		[Header("Tutorial text objects")]
		[SerializeField] TMPro.TextMeshProUGUI topLeftText;
		[SerializeField] TMPro.TextMeshProUGUI middleLeftText;
		[SerializeField] TMPro.TextMeshProUGUI botLeftText;
		[Space]
		[SerializeField] TMPro.TextMeshProUGUI topRightText;
		[SerializeField] TMPro.TextMeshProUGUI middleRightText;
		[SerializeField] TMPro.TextMeshProUGUI botRightText;

		void Awake() {
			var levelManager = FindObjectOfType<LevelManager>();
			restartButton.onClick.AddListener(() => { levelManager.RestartLevel(); HideRestartPanel(); });
			nextLevelButton.onClick.AddListener(() => { levelManager.LoadNextLevel(); HideVictoryPanel(); });

			// Whenever a new level spawns, initialize player's callbacks and begin tutorial if any.
			levelManager.OnLevelInitialized += () => {
				Player.instance.OnWon += ShowVictoryPanel;
				Player.instance.OnLost += ShowRestartPanel;

				Player.instance.OnLost += () => {
					StopAllCoroutines();
					foreach (var txt in new[] { topLeftText, middleLeftText, botLeftText, topRightText, middleRightText, botRightText }) { txt.text = ""; }
				};

				foreach (var step in levelManager.currentLevel.tutorialInfoSteps) { StartCoroutine(StartTutorialSequence(step)); }
			};
		}

		void ShowRestartPanel() { restartPanel.SetActive(true); }
		void HideRestartPanel() { restartPanel.SetActive(false); }

		void ShowVictoryPanel() { victoryPanel.SetActive(true); }
		void HideVictoryPanel() { victoryPanel.SetActive(false); }


		TMPro.TextMeshProUGUI GetObjectForAnchor(ScreenAnchor anchor) {
			switch (anchor) {
				case ScreenAnchor.TopRight:		{ return topRightText; }
				case ScreenAnchor.MiddleRight:	{ return middleRightText; }
				case ScreenAnchor.BotRight:		{ return botRightText; }
				case ScreenAnchor.TopLeft:		{ return topLeftText; }
				case ScreenAnchor.MiddleLeft:	{ return middleLeftText; }
				case ScreenAnchor.BotLeft:		{ return botLeftText; }
				default: throw new UnityException("Invalid screen anchor");
			}
		}

		IEnumerator StartTutorialSequence(TutorialStep step) {
			yield return new WaitForSeconds(step.DelayToAppear);
			var textObj = GetObjectForAnchor(step.PlaceToAppear);
			textObj.text = step.Text;

			float t = 0;
			while (t < 0.5f) {
				t += Time.deltaTime;
				textObj.alpha = 2 * t;
				yield return null;
			}

			yield return new WaitForSeconds(step.DelayToDisappear);

			t = 0;
			while (t < 0.25f) {
				t += Time.deltaTime;
				textObj.alpha = 1 - (4 * t);
				yield return null;
			}
		}
	}
}