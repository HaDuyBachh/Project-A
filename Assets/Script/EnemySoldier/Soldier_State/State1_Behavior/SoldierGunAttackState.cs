using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierGunAttackState : SoldierBaseState
    {
        public SoldierGunAttackState(SoldierControl currentContext, SoldierFactory StateFactory)
       : base(currentContext, StateFactory)
        {

        }

        private bool DealDamage = false;
        private float _timeToFindPathDelta = 0.0f;
        private float _cantAttackTime = 0.0f;
        private bool _arrived_value = false;
        public override void CheckSwitchState()
        {
            if (Ctx.AttackTimeoutDelta <= 0 || _cantAttackTime > 2.5f)
            {
                SwitchState(Factory.Gun());
            }
        }

        public override void EnterState()
        {
            InitializaSubState();

            DealDamage = false;

            Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.ReadyAttack], true);
            Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.Attack], true);
            Ctx.Animator.SetFloat(Ctx.AnimList[SoldierControl.Anim.AttackSpeed], Mathf.Min(3.5f,1/ListItem.getItem(Ctx.WeaponEquip).speed));

            _timeToFindPathDelta = -1;
            Ctx.NormMove = 0;
            _cantAttackTime = 0.0f;

            Ctx.SetDestination(Ctx.transform.position, Ctx.StopDis);

            _arrived_value = true;

            Debug.Log("Đang bắt đầu tấn công " + Ctx.name);
        }

        public override void ExitState()
        {
            Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.ReadyAttack], false);
            Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.Attack], false);
            Ctx.Animator.SetFloat(Ctx.AnimList[SoldierControl.Anim.AttackSpeed], 1f);

            Ctx.AttackTimeoutDelta = -1;

        }

        public override void InitializaSubState()
        {
            SetSubState(Factory.Idle());
        }

        public override void UpdateState()
        {
            //Bổ trợ thêm để chân thực hơn
            if (Ctx.NormMove > 1 && Ctx.Nav.remainingDistance <= 2.5f) Ctx.NormMove = 1;
           
            if (Ctx.IsStuck())
            {
                if (Ctx.FindAndMoveAroundEnemy(Ctx.AttackDistance))
                {
                    Ctx.NormMove = (short)(Rand.RandPer(80, 100) ? 1 : 2);
                    _timeToFindPathDelta = Random.Range(1.0f, 3.0f);
                    _arrived_value = false;

                    Debug.Log("Đang tìm được đi xung quanh kẻ thù khi bị kẹt " + Ctx.name);
                }
            }    

            if (Ctx.Reckless)
            {
                if (Ctx.IsArrived())
                {
                    Ctx.NormMove = 0;
                    _arrived_value = true;
                }

                if (_arrived_value)
                {
                    if (_timeToFindPathDelta > 0)
                    {
                        _timeToFindPathDelta -= Time.deltaTime;
                        if (Ctx.BeAttacked) _timeToFindPathDelta -= Time.deltaTime;
                    }
                    else
                    if (Ctx.FindAndMoveAroundEnemy(Ctx.AttackDistance))
                    {
                        Ctx.NormMove = (short)(Rand.RandPer(80, 100) ? 1 : 2);
                        _timeToFindPathDelta = Random.Range(1.0f, 3.0f);
                        _arrived_value = false;
                        Debug.Log("Đang tìm được đi xung quanh kẻ thù " + Ctx.name);
                    }
                }
            }

            Ctx.Nav.updateRotation = false;
            Ctx.Turn(Ctx.EnemyPos() - Ctx.transform.position, 18.0f);
            Ctx.UpdateAnimationMoveDir();

            if (!DealDamage && Ctx.RateFireTimeoutDelta <= Ctx.RateFireTimeout * 0.5f)
            {
                DealDamage = true;
                Ctx.DealBulletDamage();
            }

            if (Ctx.RateFireTimeoutDelta > 0.0f) Ctx.RateFireTimeoutDelta -= Time.deltaTime;
            else
            {
                Ctx.RateFireTimeoutDelta = Ctx.RateFireTimeout;
                DealDamage = false;
            }

            Ctx.AttackTimeoutDelta -= Time.deltaTime;
            if (Ctx.BeAttacked) Ctx.AttackTimeoutDelta -= Time.deltaTime;

            if (!Ctx.CanAttackEnemy()) _cantAttackTime += Time.deltaTime;
            else
                _cantAttackTime = 0.0f;

            Debug.Log("Thời gian tấn công còn lại: " + Ctx.AttackTimeoutDelta + "     " 
                + Ctx.CanAttackEnemy() + "   " + Ctx.name);

            CheckSwitchState();
        }
    }
}