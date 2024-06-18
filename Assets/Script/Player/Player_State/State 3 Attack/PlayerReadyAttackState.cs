using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class PlayerReadyAttackState : PlayerBaseState
    {
        public PlayerReadyAttackState(ThirdPersonController currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
        {

        }

        private bool SetWeightDone = false;

        public override void CheckSwitchState()
        {
            if (Ctx.Input.attack && Ctx.ReadyToAttackTimeoutDelta <= 0.0f)
            {
                SwitchState(Factory.Attack());
            }   
            else
            if (Ctx.ReadyToIdleTimeoutDelta <= 0.0f)
            {
                SwitchState(Factory.NoAttack());
            }
            else
            //Khi mà nhảy lên hoặc khi chuyển trạng thái rơi thì sẽ hủy trạng thái ready attack
            if (Ctx.Input.jump || (!Ctx.Grounded && Ctx.FallTimeOutDelta <= 0.0f))
            {
                Ctx.ReadyToIdleTimeoutDelta = -1.0f;
                SwitchState(Factory.NoAttack());
            }    
            
        }

        public override void EnterState()
        {
            //Debug.Log("Đang vào Ready Attack State");
            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.ReadyAttack], true);           
            
            SetWeightDone = false;

            //tránh việc bị về lại NoAttack khi mới vào và cũng tránh việc bị đặt lại khi chuyển SuperState khác
            if (Ctx.ReadyToIdleTimeoutDelta <= 0) Ctx.ReadyToIdleTimeoutDelta = Ctx.ReadyToIdleTimeout;

            Ctx.ChangeCameraFollow(0);
        }

        public override void ExitState()
        {
            //Debug.Log("Đang thoát Ready Attack State");
            // Tránh trường hợp khi bị đổi trạng thái sẽ bị reset lại
            if (Ctx.ReadyToIdleTimeoutDelta <= 0.0f)
            {
                Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.ReadyAttack], false);
                //Đáng lẽ đặt SetLayerWeight ở đây nhưng bị khưng animation nên sẽ đặt ở NoAttackState
            }    
        }

        public override void UpdateState()
        {
            Ctx.SetSameCameraDirect();
            Ctx.SetAnimDirect();

            Ctx.SetRigAndFireState(0, false);

            //sẽ reset đặt khi chuyển từ NoAttack -> Ready
            if (Ctx.ReadyToAttackTimeoutDelta >= 0.0f)
            {
                Ctx.ReadyToAttackTimeoutDelta -= Time.deltaTime;
            }    

            //sẽ được reset trong enter của Ready và AttackState
            if (Ctx.ReadyToIdleTimeoutDelta > 0.0f)
            {
                Ctx.ReadyToIdleTimeoutDelta -= Time.deltaTime;

                //nếu mà di chuyển nhanh về phía trước sẽ đẩy nhanh việc hủy trạng thái ready
                if (Ctx.Input.IsRunForward)
                {
                    Ctx.ReadyToIdleTimeoutDelta -= Time.deltaTime;
                }
            }

            //đặt LayerWeight để không bị khưng animation
            if (!SetWeightDone) Ctx.SetWeightLayerDelta(1, 0.5f, out SetWeightDone);

            CheckSwitchState();
        }

        public override void InitializaSubState()
        {

        }
    }
}
