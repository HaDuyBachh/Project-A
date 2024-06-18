using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class PlayerFallState : PlayerBaseState, IRootState
    {
        public PlayerFallState(ThirdPersonController currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
        {
            IsRootState = true;
        }
        public override void CheckSwitchState()
        {
            if (Ctx.Grounded) SwitchState(Factory.Grounded());
            else
            if (Ctx.Driving)
            {
                SwitchState(Factory.Drive());
            }
        }

        public override void EnterState()
        {
            //Debug.Log("Đang vào Fall State");

            InitializaSubState();

            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Grounded], false);
            //Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Fall], true);
            
        }

        public override void ExitState()
        {
            //Debug.Log("Đang thoát Fall State");

            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Fall], false);
            Ctx.FallTimeOutDelta = Ctx.FallTimeout;
        }

        public override void UpdateState()
        {
            if (Ctx.FallTimeout >= 0.0f)
            {
                Ctx.FallTimeOutDelta -= Time.deltaTime;
                if (Ctx.FallTimeOutDelta <= 0.01f) Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Fall], true);
            }

            HandleGravity();
            CheckSwitchState();
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
