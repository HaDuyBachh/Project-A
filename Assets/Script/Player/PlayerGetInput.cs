using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class PlayerGetInput : MonoBehaviour
    {
		private ThirdPersonController Player;

		[Header("Control")]
		public bool IsPC = false;

		[Header("Character Input Values")]
		public Vector2 move = Vector2.zero;
		public Vector2 look = Vector2.zero;
		public bool OnLooking = false;
		public bool jump = false;
		public bool sprint = false;
		/// <summary>
		/// chuyển đổi thành move/Player.ClampMoveValue để dễ sử dụng: <br></br>
		/// - khi không chạy: giá trị min: -1; giá trị max: 1 <br></br>
		/// - khi chạy giá trị min -1; giá trị max > 1
		/// </summary>
		public Vector3 moveNormalize = Vector3.zero;
		/// <summary>
		/// Kết quả cho ra là giá trị Move.magnitude/Player. <br></br>
		/// ClampMoveValue được chuẩn hóa: <br></br>
		///		0 => 0		<br></br> 
		///		0->0.5 => 1 <br></br>
		///		0.5->1 =>2  <br></br> 
		///		Lưu ý: riêng trạng thái Crouch giá trị max giới hạn là 1
		/// </summary>
		public int standardMove = 0;
		/// <summary>
		/// Kiểm tra xem trong trạng thái Ready có đang chạy về phía trước hay không
		/// </summary>
		public bool IsRunForward = false;
		public bool attack = false;
		public int SlotNow = 0;
		public bool crouch = false;
		/// <summary>
		/// Vũ khí trang bị hiện tại
		/// </summary>
		public Item Equiped = new();
		public Item PreEquip = new();


		private EquipMenuControl Eq;

		private void Awake()
		{
			Player = gameObject.GetComponent<ThirdPersonController>();

			//Platform target
			if (Application.platform == RuntimePlatform.Android || Application.platform == RuntimePlatform.IPhonePlayer) IsPC = false;
			if (IsPC)
			{
				Cursor.lockState = CursorLockMode.Locked;
				Cursor.visible = false;
				Eq = FindObjectOfType<EquipMenuControl>();
			}

		}
		public void Update()
		{
			//PC control
			if (IsPC)
			{
				Vector2 v = Vector2.zero;
				if (Input.GetKey(KeyCode.W)) v += Vector2.up;
				if (Input.GetKey(KeyCode.S)) v += Vector2.down;
				if (Input.GetKey(KeyCode.A)) v += Vector2.left;
				if (Input.GetKey(KeyCode.D)) v += Vector2.right;
				v = v.normalized * Player.ClampMoveValue;

				if (Input.GetKey(KeyCode.LeftShift)) v += v.normalized * Player.ClampMoveValue;

				MoveInput(v);

				LookInput(new Vector2(Input.GetAxis("Mouse X") * 10, -Input.GetAxis("Mouse Y") * 10));

				AttackInput(Input.GetMouseButton(0));

				JumpInput(Input.GetKey(KeyCode.Space));

				if (Input.GetKeyDown(KeyCode.LeftControl)) CrouchInput();

				if (Input.GetKeyDown(KeyCode.Keypad1) || Input.GetKeyDown(KeyCode.Alpha1)) Eq.Click_Slot_0();
				if (Input.GetKeyDown(KeyCode.Keypad2) || Input.GetKeyDown(KeyCode.Alpha2)) Eq.Click_Slot_1();
				if (Input.GetKeyDown(KeyCode.Keypad3) || Input.GetKeyDown(KeyCode.Alpha3)) Eq.Click_Slot_2();
				if (Input.GetKeyDown(KeyCode.Keypad4) || Input.GetKeyDown(KeyCode.Alpha4)) Eq.Click_Slot_3();
				if (Input.GetKeyDown(KeyCode.Keypad5) || Input.GetKeyDown(KeyCode.Alpha5)) Eq.Click_Slot_4();
				if (Input.GetKeyDown(KeyCode.Keypad6) || Input.GetKeyDown(KeyCode.Alpha6)) Eq.Click_Slot_5();
			}
        }

        public void MoveInput(Vector2 newMoveDirection)
		{
			move = newMoveDirection;

			//normalize vector move
			//sử dụng để chuyển các giá trị trong animator
			moveNormalize.x = move.x / Player.ClampMoveValue;
			moveNormalize.z = move.y / Player.ClampMoveValue;

			//sử dụng để di chuyển
			if (move != Vector2.zero)
			{
				if (move.magnitude <= Player.ClampMoveValue * 0.5f)
				{
					standardMove = 1;
				}
				else
				if (move.magnitude <= Player.ClampMoveValue)
				{
					standardMove = 2;
				}
				else
					standardMove = 3;

				//nếu trong trạng thái ngồi sẽ giảm tốc độ max
				if (crouch) standardMove = Mathf.Clamp(standardMove, 0, 1);
			}
			else standardMove = 0;

			//Kiểm tra xem có đang tiến lên phía trước theo hướng 45 độ không
			IsRunForward = (Mathf.Abs(moveNormalize.x) <= 0.5f && moveNormalize.z >= 0.5f);

			//Nếu đang tấn công
			//Hoặc nếu thời gian chuyển từ Ready -> Idle >= 0.5f
			//Hoặc nếu trong trạng thái ready mà không tiến lên nhanh theo góc 45 độ về phía trước 
			//=> tốc độ sẽ bị giảm
			if (Player.RateFireTimeoutDelta > 0.0f || Player.ReadyToIdleTimeoutDelta >= Player.ReadyToIdleTimeout - 0.5f
				|| (Player.ReadyToIdleTimeoutDelta > 0 && !IsRunForward))
			{
				moveNormalize.z = Mathf.Clamp(moveNormalize.z, -0.5f, 0.5f);
				standardMove = Mathf.Clamp(standardMove, 0, 1);
			}
		}
		public void LookInput(Vector2 newLookDirection)
		{
			OnLooking = (newLookDirection != Vector2.zero);
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
		public void AttackInput(bool newHitState)
		{
			attack = newHitState;
		}
		public void CrouchInput()
        {
			crouch = !crouch;
        }			
		public void ActiveEquip(Item item)
		{
			PreEquip = Equiped;
			Equiped = item;
			Player.ActiveItemInHand();		
		}
		private void SetCursorState(bool newState)
		{
			Cursor.lockState = newState ? CursorLockMode.Locked : CursorLockMode.None;
		}
	}
}
