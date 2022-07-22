using System.Collections;

using NormalMode.Environment;

using UnityEngine;

namespace NormalMode {
	public class ManualDiceController : Player {
		[SerializeField] GameObject model;

		[Header("Jump Settings")]
		[SerializeField] float jumpHeight = 6;
		[SerializeField] AnimationCurve jumpCurve;
		[SerializeField] float jumpSpeed = 0.5f;

		bool isMoving;
		DiceFacing? queuedMoveDir;

		void Update() {
			// Go through bindings and check if anything that triggers a movement got pressed.
			foreach (var kc in PlayerOrientationHelper.moveBindings) { if (Input.GetKeyDown(kc.Key)) { queuedMoveDir = kc.Value; } }

			// If moving, check if we should queue the input for when current movement ends, or discard it.
			if (isMoving || queuedMoveDir == null) { return; }

			var moveDir = PlayerOrientationHelper.GetMoveDirBySide(transform, queuedMoveDir.Value);

			// If code reaches here, it means movement is ready to be performed.
			if (moveDir.y == 1) { StartCoroutine(PlayJumpAnim()); }
			else if (moveDir.y == 0) {
				var moveResults = MovementResolver.instance.TryMove(transform, moveDir);
				if (moveResults.canMove == MoveValidity.Success) { StartCoroutine(PlayRollAnim(moveDir, moveResults.onMove)); }
			}
			else if (moveDir.y == -1) { } // Maybe make floor shine red or crack?

			queuedMoveDir = null;
		}

		IEnumerator PlayRollAnim(Vector3 moveDir, MoveResult onMoveCompleted) {
			isMoving = true;

			// Cache original position and rotation to make up for floating point errors after the anim.
			var originalPos = transform.position;
			var originalRot = transform.rotation;
			var modelPos = model.transform.position;

			// Do some calculations
			var targetPos = transform.position + 2 * moveDir;
			var targetRot = Quaternion.Euler(90 * Vector3.Cross(Vector3.up, moveDir)) * transform.rotation;
			transform.position += Vector3.down + moveDir;           // Adjust the pivot
			model.transform.position = modelPos;                    // But make sure the model is placed properly.

			// Perform the roll over time
			const float totalTime = 0.5f;
			float t = 0;
			while (t < totalTime) {
				t += Time.deltaTime;
				var T = t / totalTime;

				transform.rotation = Quaternion.Slerp(originalRot, targetRot, T);
				yield return null;
			}

			// Make sure the position and rotation are correct
			transform.position = targetPos;
			transform.rotation = targetRot;
			model.transform.localPosition = Vector3.zero;

			// Perform any post-move actions that may have been triggered. This will also resume the 'isMoving' state eventually.
			PerformPostMoveAction(onMoveCompleted, originalPos, transform.position);
		}

		void PerformPostMoveAction(MoveResult postMoveAction, Vector3 previousPosition, Vector3 currentPosition) {
			switch (postMoveAction) {
				case MoveResult.Nothing:	{ isMoving = false; break; } // Nothing to do here.. Just allow the player to move again.
				case MoveResult.Win:		{ StartCoroutine(PlayWinAnim()); break; }
				case MoveResult.Loss:		{ StartCoroutine(PlayLossAnim()); break; }
				case MoveResult.MoveBack:	{ StartCoroutine(PlayRollAnim((previousPosition - currentPosition).normalized, MoveResult.Nothing)); break; }
			}
		}

		IEnumerator PlayJumpAnim() {
			isMoving = true;

			var startHeight = transform.position.y;
			var targetHeight = startHeight + jumpHeight;

			var t = 0f;
			while (t < jumpSpeed) {
				t += Time.deltaTime;
				var T = t / jumpSpeed;

				var targetY = Mathf.Lerp(startHeight, targetHeight, jumpCurve.Evaluate(T));
				transform.position = new Vector3(transform.position.x, targetY, transform.position.z);

				yield return null;
			}

			isMoving = false;
		}

		IEnumerator PlayLossAnim() {
			FindObjectOfType<GameCamera>().enabled = false;

			isMoving = true;

			var dropSpeed = 10f;
			var t = 0f;
			while (t < 1) {
				t += Time.deltaTime;

				dropSpeed += 50 * Time.deltaTime; // Apply super-strong gravity
				transform.position += Vector3.down * dropSpeed * Time.deltaTime;
				yield return null;
			}

			isMoving = false;

			OnLost?.Invoke();
		}

		IEnumerator PlayWinAnim() {
			FindObjectOfType<GameCamera>().enabled = false;

			isMoving = true;

			var startHeight = transform.position.y;
			var targetHeight = startHeight + 2;
			var startScale = transform.localScale;

			var t = 0f;
			const float totalTime = 2f;
			while (t < totalTime) {
				t += Time.deltaTime;
				var T = t / totalTime;

				transform.position = new Vector3(transform.position.x, Mathf.Lerp(startHeight, targetHeight, T), transform.position.z);
				transform.localScale = Vector3.Lerp(startScale, startScale * 0.95f, T);
				yield return null;
			}

			this.gameObject.AddComponent<UnstableRotationObject>();

			yield return new WaitForSeconds(1.5f);

			OnWon?.Invoke();
		}

	}
}
