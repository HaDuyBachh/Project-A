using UnityEngine;

namespace HaDuyBach
{
    public class PlayerGroundedState : PlayerBaseState , IRootState
    {
        public PlayerGroundedState(ThirdPersonController currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
        {
           IsRootState = true;
        }
        public override void CheckSwitchState()
        {
            if (Ctx.Input.jump && Ctx.Grounded && Ctx.JumpTimeOutDelta<=0.0f)
            {
                SwitchState(Factory.Jump());
            }
            else
            if (!Ctx.Grounded && Ctx.FallTimeOutDelta<=0)
            {
                SwitchState(Factory.Fall());
            }   
            else
            if (Ctx.Input.crouch)
            {
                SwitchState(Factory.Crouch());
            }    
            else
            if (Ctx.Driving)
            {
                SwitchState(Factory.Drive());
            }    
            
        } 

        public override void EnterState()
        {
            //Debug.Log("Đang vào GroudState");

            InitializaSubState();
            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Fall], false);
            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Grounded], true);

            // stop our velocity dropping infinitely when grounded
            if (Ctx.VerticalVelocity < 0.0f) Ctx.VerticalVelocity = -2f;

            Ctx.JumpTimeOutDelta = Ctx.JumpTimeout;
        }

        public override void UpdateState()
        {
            //Jump Timeout
            if (Ctx.JumpTimeOutDelta >= 0.0f)
            {
                Ctx.JumpTimeOutDelta -= Time.deltaTime;
            }

            //Fall Timeout
            if (!Ctx.Grounded)
            {
                if (Ctx.FallTimeOutDelta>0.0f) Ctx.FallTimeOutDelta -= Time.deltaTime;
            }
            else
            {
                Ctx.FallTimeOutDelta = Ctx.FallTimeout;
            } 
                

            CheckSwitchState();
        }

        public override void ExitState()
        {
            //Debug.Log("Đang thoát GroudState");
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

        }
    }

}
