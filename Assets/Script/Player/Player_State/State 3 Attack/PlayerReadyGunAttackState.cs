using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class PlayerReadyGunAttackState : PlayerBaseState
    {
        public PlayerReadyGunAttackState(ThirdPersonController currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
        {

        }

        private bool SetWeightDone = false;
        public override void CheckSwitchState()
        {
            //Khi mà nhảy lên hoặc khi chuyển trạng thái rơi thì sẽ hủy trạng thái ready attack
            if (Ctx.Input.jump || (!Ctx.Grounded && Ctx.FallTimeOutDelta <= 0.0f))
            {
                Ctx.ReadyToIdleTimeoutDelta = -1.0f;
                SwitchState(Factory.NoAttack());
            }
            else
            //Khi đang ở trên mặt đất thực hiện tấn công và thời gian súng bắn < 0
            if (Ctx.Input.attack && Ctx.ReadyToAttackTimeoutDelta <= 0.0f && Ctx.RateFireTimeoutDelta <= 0.0f && Ctx.Grounded)
            {
                SwitchState(Factory.GunAttack());
            }
            else
            //Khi kết thúc thời gian chờ thì về No Attack
            if (Ctx.ReadyToIdleTimeoutDelta <= 0.0f)
            {
                SwitchState(Factory.NoAttack());
            }
        }

        public override void EnterState()
        {
            // Debug.Log("Đang vào Ready Gun Attack State");
            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.ReadyAttack], true);

            SetWeightDone = false;

            //tránh việc bị về lại NoAttack khi mới vào và cũng tránh việc bị đặt lại khi chuyển SuperState khác
            if (Ctx.ReadyToIdleTimeoutDelta <= 0)
            { 
                Ctx.ReadyToIdleTimeoutDelta = Ctx.ReadyToIdleTimeout;
            }

            Ctx.ChangeCameraFollow(1);
        }

        public override void ExitState()
        {
            // Debug.Log("Đang thoát Ready Gun Attack State");
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

            Ctx.SetRigAndFireState(1,false);
            Ctx.RecoilSetUp(RecoilValue.None);

            //Trừ đi để lần tiếp theo có thể vào gun attack sau khi trạng thái bị chuyển đổi
            if (Ctx.RateFireTimeoutDelta >= 0.0f)
            {
                Ctx.RateFireTimeoutDelta -= Time.deltaTime;
            }    

            //sẽ được đặt khi chuyển từ NoAttack -> Ready
            if (Ctx.ReadyToAttackTimeoutDelta >= 0.0f)
            {
                Ctx.ReadyToAttackTimeoutDelta -= Time.deltaTime;
            }

            //sẽ được reset trong enter của Ready và AttackState
            if (Ctx.ReadyToIdleTimeoutDelta > 0.0f)
            {
                Ctx.ReadyToIdleTimeoutDelta -= Time.deltaTime;

                //nếu mà di chuyển nhanh về phía trước sẽ đẩy nhanh việc hủy trạng thái ready
                //và trong 1s đầu tiên thì không đẩy nhanh việc hủy để tránh lỗi
                if (Ctx.Input.IsRunForward && Ctx.ReadyToIdleTimeoutDelta <= Ctx.ReadyToIdleTimeout-1.0f)
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
