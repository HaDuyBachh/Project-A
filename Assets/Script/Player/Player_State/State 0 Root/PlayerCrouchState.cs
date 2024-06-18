using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class PlayerCrouchState : PlayerBaseState, IRootState
    {
        public PlayerCrouchState(ThirdPersonController currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
        {
            IsRootState = true;
        }
        public override void CheckSwitchState()
        {
            if (!Ctx.Input.crouch) SwitchState(Factory.Grounded());
            else
                //kiểm tra xem có nhảy trước không đã rồi mới vào trạng thái rơi
                if (Ctx.Input.jump) SwitchState(Factory.Jump());
            else
                if (!Ctx.Grounded) SwitchState(Factory.Fall());
            else
            if (Ctx.Driving)
            {
                SwitchState(Factory.Drive());
            }    
        }

        public override void EnterState()
        {
            Ctx.Input.standardMove = Mathf.Clamp(Ctx.Input.standardMove, 0, 1);

            InitializaSubState();

            //Debug.Log("Đang vào Crouch State ");

            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Crouch], true);
        }

        public override void ExitState()
        {
            //Debug.Log("Đang thoát Crouch State ");

            // để tránh bị lỗi khi chuyển trạng thái khác
            Ctx.Input.crouch = false;
            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Crouch], false);
        }

        public override void UpdateState()
        {
            CheckSwitchState();
        }

        public override void InitializaSubState()
        {
            // vì đã măc định Crouch state sẽ chỉ có trạng thái 0 hoặc 1 trong Input.stardMove
            switch (Ctx.Input.standardMove)
            {
                case 0: SetSubState(Factory.Idle()); break;
                case 1: SetSubState(Factory.Walk()); break;
                //case 2: SetSubState(Factory.Run()); break;
            }
        }

        public void HandleGravity()
        {
            
        }
    }

}
