using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierMeleeAttackState : SoldierBaseState
    {
        public SoldierMeleeAttackState(SoldierControl currentContext, SoldierFactory StateFactory)
       : base(currentContext, StateFactory)
        {

        }

        private float normTime;
        private bool Attacked = false;
        private bool DealDamage = false;

        public override void CheckSwitchState()
        {
            if (!Ctx.Attack && normTime > 0.98f)
            {
                SwitchState(Factory.Melee());
            }
        }

        public override void EnterState()
        {
            InitializaSubState();

            Ctx.Animator.SetInteger(Ctx.AnimList[SoldierControl.Anim.Rand], Random.Range(0, 4 * 16) % 4);
            Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.Attack], true);
            Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.ReadyAttack], true);

            DealDamage = false;
            Ctx.Attack = true;
            Ctx.NormMove = 0;
        }

        public override void ExitState()
        {
            Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.Attack], false);
            Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.ReadyAttack], false);

            Attacked = false;
            Ctx.Attack = false;
        }

        public override void InitializaSubState()
        {
            SetSubState(Factory.Idle());
        }

        public override void UpdateState()
        {
            normTime = Ctx.Animator.GetCurrentAnimatorStateInfo(0).normalizedTime;

            Ctx.Attack = Ctx.CanAttackEnemy();

            if (Ctx.Attack)
            {
                Ctx.Turn(Ctx.Player.transform.position - Ctx.transform.position, 18.0f);
            }

            //Nếu chưa gây Damage 
            if (!DealDamage && normTime > 0.45f)
            {
                DealDamage = true;
                Ctx.DealMeleeDamage();
            }

            if (normTime <= 0.45f)
            {
                DealDamage = false;
                Attacked = false;
            }
            else
            if (normTime <= 0.9f)
            {
                if (Ctx.Attack)
                {
                    //Thực hiện 1 lần việc random() trong 1 lần lặp Animation
                    if (!Attacked)
                    {
                        Ctx.Animator.SetInteger(Ctx.AnimList[SoldierControl.Anim.Rand], Random.Range(0, 4 * 16) % 4);
                    }
                    Attacked = true;
                }
            }

            CheckSwitchState();
        }
    }
}
