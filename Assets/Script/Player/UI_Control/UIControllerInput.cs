using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HaDuyBach
{
    public class UIControllerInput : MonoBehaviour
    {
        [Header("Output")]
        public PlayerGetInput playerGetInput;

        public void Awake()
        {
            playerGetInput = FindObjectOfType<PlayerGetInput>();
        }
        public void VirtualMoveInput(Vector2 virtualMoveDirection)
        {
            playerGetInput.MoveInput(virtualMoveDirection);
        }

        public void VirtualLookInput(Vector2 virtualLookDirection)
        {
            playerGetInput.LookInput(virtualLookDirection);
        }

        public void VirtualJumpInput(bool virtualJumpState)
        {
            playerGetInput.JumpInput(virtualJumpState);
        }

        public void VirtualSprintInput(bool virtualSprintState)
        {
            playerGetInput.SprintInput(virtualSprintState);
        }

        public void VirtualCrouchInput()
        {
            playerGetInput.CrouchInput();
        }    

        public void AttackInput(bool HitState)
        {
            playerGetInput.AttackInput(HitState);
        }

        public void ActiveEquip(Item item)
        {
            playerGetInput.ActiveEquip(item);
        }

        private float TimeStart = 4.0f;
        private float TimeLimit = 1.0f;
        private float TimeNow = 0.0f;
        private int FPS = 0;
        private int MinFPS = 100;
        [SerializeField]
        private Text S_FPS;
        [SerializeField]
        private Text S_MinFPS;
        public void ShowFps()
        {
            TimeNow += Time.deltaTime;
            FPS++;
            if (TimeNow >= TimeLimit)
            {
                MinFPS = Mathf.Min(MinFPS, FPS);
                S_FPS.text = FPS + " FPS";
                S_MinFPS.text = "Min: " + MinFPS + " fps";
                FPS = 0;
                TimeNow -= TimeLimit;
            }    
        }    

        public void Update()
        {
            if (TimeStart < 0) ShowFps();
            else TimeStart -= Time.deltaTime;
        }
    }
}
