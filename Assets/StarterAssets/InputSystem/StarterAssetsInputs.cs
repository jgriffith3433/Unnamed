using UnityEngine;
#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
using UnityEngine.InputSystem;
#endif

namespace StarterAssets
{
	public class StarterAssetsInputs : MonoBehaviour
	{
		[Header("Character Input Values")]
		public Vector2 move;
		public Vector2 look;
		public bool jump;
		public bool sprint;
		public bool mount;

		[Header("Movement Settings")]
		public bool analogMovement;

		[Header("Mouse Cursor Settings")]
		public bool cursorLocked = true;
		public bool cursorInputForLook = true;

#if ENABLE_INPUT_SYSTEM && STARTER_ASSETS_PACKAGES_CHECKED
		public void OnMove(InputValue value)
		{
			MoveInput(value.Get<Vector2>());
		}

		public void OnLook(InputValue value)
		{
			if(cursorInputForLook)
			{
				LookInput(value.Get<Vector2>());
			}
		}

		public void OnJump(InputValue value)
		{
			JumpInput(value.isPressed);
		}

		public void OnSprint(InputValue value)
		{
			SprintInput(value.isPressed);
		}

		public void OnMount(InputValue value)
		{
			MountInput(value.isPressed);
		}
#endif


		public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;
		} 

		public void LookInput(Vector2 newLookDirection)
		{
			look = newLookDirection;
		}

		public void JumpInput(bool newJumpState)
		{
			jump = newJumpState;
		}

		public void SprintInput(bool newSprintState)
		{
			sprint = newSprintState;
		}

		public void MountInput(bool newMountState)
		{
			mount = newMountState;
		}

		private void OnApplicationFocus(bool hasFocus)
		{
#if (UNITY_ANDROID || UNITY_IOS) && UNITY_EDITOR
			// Don't lock cursor in editor when testing mobile controls
			SetCursorState(false);
#else
			SetCursorState(cursorLocked);
#endif
		}

		private void SetCursorState(bool newState)
		{
#if (UNITY_ANDROID || UNITY_IOS) && UNITY_EDITOR
			// Keep cursor visible and unlocked in editor for mobile testing
			Cursor.lockState = CursorLockMode.None;
			Cursor.visible = true;
#else
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
#endif
		}
	}
	
}