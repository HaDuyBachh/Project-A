using UnityEngine;

namespace HaDuyBach
{
    public class PlayerAttackState : PlayerBaseState
    {
        private bool Attacked = false;
        private float normTime;
        private bool SetWeightDone = false;
        private bool DealDamage = false;
        public PlayerAttackState(ThirdPersonController currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
        {

        }

        public override void CheckSwitchState()
        {
            if (normTime > 0.9f && !Attacked)
            {
                SwitchState(Factory.ReadyAttack());
            }
        }

        public override void EnterState()
        {
            //Debug.Log("Đang vào Attack State");

            InitializaSubState();
            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Attack], true);
            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.ReadyAttack], true);

            Ctx.ReadyToIdleTimeoutDelta = Ctx.ReadyToIdleTimeout;

            DealDamage = false;
            SetWeightDone = false;

        }

        public override void ExitState()
        {
            //Debug.Log("Đang thoát Attack State");

            Attacked = false;
            Ctx.Animator.SetBool(Ctx.AnimList[ThirdPersonController.Anim.Attack], false);

        }

        public override void UpdateState()
        {
            Ctx.SetSameCameraDirect();
            Ctx.SetAnimDirect();

            Ctx.SetRigAndFireState(0, false);

            normTime = Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            //Gây ra Melee Damage
            if (!DealDamage && normTime > 0.45f)
            {
                DealDamage = true;
                Ctx.DealMeleeDamage(Ctx.Input.Equiped.damage);
            }    

            if (normTime <= 0.45f)
            {
                DealDamage = false;
                Attacked = false;
            }
            else
            if (normTime <= 0.9f)
            {
                if (Ctx.Input.attack)
                {
                    Attacked = true;
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
