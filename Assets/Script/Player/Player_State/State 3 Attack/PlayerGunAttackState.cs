using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class PlayerGunAttackState : PlayerBaseState
    {
        public PlayerGunAttackState(ThirdPersonController currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
        {

        }

        private bool DealDamage = false;
        private bool SetWeightDone = false;

        public override void CheckSwitchState()
        {

            //Khi mà nhảy lên thì sẽ hủy trạng thái ready attack
            if (Ctx.Input.jump || !Ctx.Grounded)
            {
                Ctx.ReadyToIdleTimeoutDelta = -1.0f;
                SwitchState(Factory.ReadyGunAttack());
            }
            else
            if (!Ctx.Input.attack && Ctx.RateFireTimeoutDelta <= 0.0f) SwitchState(Factory.ReadyGunAttack());
        }

        public override void EnterState()
        {
            //Debug.Log("Đang vào Gun Attack State");

            InitializaSubState();
            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Attack], true);
            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.ReadyAttack], true);

            Ctx.ReadyToIdleTimeoutDelta = Ctx.ReadyToIdleTimeout;

            //Đặt lại thời gian để bắn
            Ctx.RateFireTimeoutDelta = Ctx.Input.Equiped.speed;

            Ctx.DealBulletDamage(Ctx.Input.Equiped.damage);
            Ctx.SetRigAndFireState(1, true);
            Ctx.RecoilSetUp(Ctx.Input.Equiped.recoil);

            DealDamage = false;
            SetWeightDone = false;
        }

        public override void ExitState()
        {
            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Attack], false);

            //Tắt hoạt ảnh bắn
            Ctx.TurnOffFire();
        }

        public override void UpdateState()
        {
            Ctx.SetSameCameraDirect();
            Ctx.SetAnimDirect();

            Ctx.SetRigAndFireState(1, false);
            Ctx.RecoilSetUp(RecoilValue.None);

            if (Ctx.RateFireTimeoutDelta > 0.0f) Ctx.RateFireTimeoutDelta -= Time.deltaTime;
            else
            {
                if (Ctx.Input.attack)
                {
                    //Đặt lại thời gian để bắn
                    Ctx.RateFireTimeoutDelta = Ctx.Input.Equiped.speed;

                    Ctx.DealBulletDamage(Ctx.Input.Equiped.damage);
                    Ctx.SetRigAndFireState(1, true);
                    Ctx.RecoilSetUp(Ctx.Input.Equiped.recoil);

                    DealDamage = false;
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