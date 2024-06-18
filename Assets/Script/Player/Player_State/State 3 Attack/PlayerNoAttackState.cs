using UnityEngine;

namespace HaDuyBach
{
    public class PlayerNoAttackState : PlayerBaseState
    {
        public PlayerNoAttackState(ThirdPersonController currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
        {

        }

        private bool SetWeightDone = false;
        public void CheckSwitchAttackType()
        {
            switch (Ctx.Input.Equiped.type)
            {
                case Item.Type.none:    
                case Item.Type.melee:   
                       SwitchState(Factory.ReadyAttack()); 
                       break;
                case Item.Type.pistol:
                case Item.Type.rifle:
                case Item.Type.smg:     
                case Item.Type.shotgun: 
                case Item.Type.snip:    
                       SwitchState(Factory.ReadyGunAttack()); 
                       break;
            }
        }
        public override void CheckSwitchState()
        {
            // Nếu vẫn còn đang trong trạng thái ready mà bị ngắt quãng thì về lại ready 
            if (Ctx.ReadyToIdleTimeoutDelta > 0.0f && Ctx.Grounded && !Ctx.Input.jump && Ctx.FallTimeOutDelta > 0)
            {
                CheckSwitchAttackType();
            }   
            else
            // Nếu tấn công mà đang ở dưới đất không nhảy và đã hết thời gian đợi
            if (Ctx.Input.attack && Ctx.Grounded && !Ctx.Input.jump && Ctx.FallTimeOutDelta > 0)
            {
                CheckSwitchAttackType();
            }
        }

        public override void EnterState()
        {
            InitializaSubState();

            //Debug.Log("Đang vào No Attack State");

            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Attack], false);
            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.ReadyAttack], false);

            if (Ctx.ReadyToIdleTimeoutDelta <= 0.0f)
            {
                SetWeightDone = false;
            }

            Ctx.ReadyToAttackTimeoutDelta = Ctx.ReadyToAttackTimeout;

            Ctx.ChangeCameraFollow(0);
            Ctx.RecoilSetUp(null);
        }

        public override void ExitState()
        {
            //Debug.Log("Đang thoát No Attack State");
        }

        public override void UpdateState()
        {
            CheckSwitchState();

            Ctx.SetRigAndFireState(0.0f, false);

            //đặt LayerWeight sau để không bị khưng animation
            if (!SetWeightDone) Ctx.SetWeightLayerDelta(1, 0.0f,out SetWeightDone);

            //trừ dẫn RateFire nếu lớn hơn 0
            if (Ctx.RateFireTimeoutDelta >= 0.0f) Ctx.RateFireTimeoutDelta -= Time.deltaTime;
              
        }

        public override void InitializaSubState()
        {

        }
    }
}