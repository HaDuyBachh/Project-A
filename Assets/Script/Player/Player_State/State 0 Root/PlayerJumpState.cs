using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class PlayerJumpState : PlayerBaseState, IRootState
    {
        public PlayerJumpState(ThirdPersonController currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
        {
            IsRootState = true;
        }
        public override void CheckSwitchState()
        {
            if (Ctx.Grounded) SwitchState(Factory.Grounded());
            else
                if (Ctx.FallTimeOutDelta <= 0.01) SwitchState(Factory.Fall());
            else
            if (Ctx.Driving)
            {
                SwitchState(Factory.Drive());
            }
        }

        public override void EnterState()
        {
            InitializaSubState();

            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Jump], true);
            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Grounded], false);

            //Debug.Log("đang vào Jump State");
           
            // the square root of H * -2 * G = how much velocity needed to reach desired height
            Ctx.VerticalVelocity = Mathf.Sqrt(Ctx.JumpHeight * -2f * Ctx.Gravity);

            if (Ctx.Input.standardMove == 3) Ctx.VerticalVelocity *= 1.2f;

            // reset the jump timeout timer
            Ctx.JumpTimeOutDelta = Ctx.JumpTimeout;

            HandleGravity();
        }

        public override void UpdateState()
        {
            //fall timeout
            if (Ctx.FallTimeout >= 0.0f)
            {
                Ctx.FallTimeOutDelta -= Time.deltaTime;
            }

            HandleGravity();

            CheckSwitchState();
        }

        public override void ExitState()
        {
            //Debug.Log("Đang thoát jump state");

            // reset the fall timeout timer
            Ctx.FallTimeOutDelta = Ctx.FallTimeout;

            Ctx.Input.jump = false;

            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Jump], false);
        }

        public override void InitializaSubState()
        {
            switch (Ctx.Input.standardMove)
            {
                case 0: SetSubState(Factory.Idle()); break;
                case 1: SetSubState(Factory.Walk()); break;
                case 2: SetSubState(Factory.Run()); break;
                case 3: SetSubState(Factory.Sprint()); break;
            }   
        }

        public void HandleGravity()
        {
            if (Ctx.VerticalVelocity < Ctx.TerminalVelocity)
            {
                Ctx.VerticalVelocity += Ctx.Gravity * Time.deltaTime;
            }
        }

    }
}
