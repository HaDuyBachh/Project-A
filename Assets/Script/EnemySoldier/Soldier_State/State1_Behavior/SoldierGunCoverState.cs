using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{

    public class SoldierGunCoverState : SoldierBaseState
    {
        public SoldierGunCoverState(SoldierControl currentContext, SoldierFactory StateFactory)
         : base(currentContext, StateFactory)
        {

        }

        private bool _arrived_value = false;
        private bool DealDamage = false;

        /// <summary>
        /// Đang trong pha vừa chạy vừa tấn công hay không?
        /// </summary>
        private bool _isAttackingPhase = false;
        private float _phaseTimeoutDelta = -1.0f;
        private float _phaseTimeout = 1.0f;
        private float _coverOrigin = 0.0f;
        public override void CheckSwitchState()
        {
            if (Ctx.Cover_Timeout_Delta <= 0)
            {
                SwitchState(Factory.Gun());
            }
        }

        public override void EnterState()
        {
            InitializaSubState();

            Ctx.NormMove = 2;
            Ctx.Cover_Timeout_Delta = Random.Range(0, Ctx.Cover_Timeout);
            _arrived_value = false;

            Ctx.RateFireTimeout = ListItem.getItem(Ctx.WeaponEquip).speed;
            Ctx.RateFireTimeoutDelta = Ctx.RateFireTimeout;

            _coverOrigin = Ctx.Cover_Timeout_Delta;

            _isAttackingPhase = false;
            _phaseTimeoutDelta = -1;

            DealDamage = false;

            Debug.Log("Đang phòng thủ " + Ctx.name);
        }

        public override void ExitState()
        {
            Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.Cover], false);
            Ctx.Nav.updateRotation = true;
            Ctx.IsRunningToCoverPoint = false;

            Ctx.Cover_Timeout_Delta = -1;
        }

        public override void InitializaSubState()
        {
            SetSubState(Factory.Run());
        }

        public override void UpdateState()
        {
            if (Ctx.IsArrived())
            {
                _arrived_value = true;
                Ctx.NormMove = 0;
            }

            if (Ctx.IsStuck())
            {
                Ctx.Cover_Timeout_Delta = -1;
                Debug.Log("Nhân vật bị kẹt " + Ctx.name);
            }

            if (_arrived_value)
            {
                if (_isAttackingPhase)
                {
                    if (BehaviorExt.CanBeSeenByEnemy(Ctx.Hip.position, Ctx.transform.up, 0, Ctx.EnemyPos(), Ctx.EnemyTag(), true) != -1)
                    {
                        Ctx.Cover_Timeout_Delta = -1;
                    }
                    else
                    {
                        Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.ReadyAttack], false);
                        Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.Attack], false);
                        Ctx.Nav.updateRotation = true;
                        _isAttackingPhase = false;
                    }

                    Debug.Log("Đang bắt đầu phòng thủ " + Ctx.name);
                }
                Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.Cover], true);

                Ctx.Cover_Timeout_Delta -= Time.deltaTime;
                if (Ctx.BeAttacked) Ctx.Cover_Timeout_Delta -= 2 * Time.deltaTime;

                if (Ctx.Cover_Timeout_Delta <= _coverOrigin - 0.3f &&
                    (BehaviorExt.CanBeSeenByEnemy(Ctx.Hip.position, Ctx.transform.up, 0, Ctx.EnemyPos(), Ctx.EnemyTag(), true) != -1))
                {
                    Ctx.Cover_Timeout_Delta = -1;
                    Debug.Log("Đã mất vị trí phòng thủ " + Ctx.name);
                }
            }
            else
            if (Ctx.CanAttackEnemy())
            {
                if (!_isAttackingPhase && _phaseTimeoutDelta > 0)
                {
                    _phaseTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    if (!_isAttackingPhase)
                    {
                        Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.ReadyAttack], true);
                        Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.Attack], true);
                        _isAttackingPhase = true;

                        _phaseTimeoutDelta = _phaseTimeout;
                    }

                    Ctx.Nav.updateRotation = false;
                    Ctx.Turn(Ctx.EnemyPos() - Ctx.transform.position, 12.0f);
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
                }
            }
            else
            {
                if (_isAttackingPhase && _phaseTimeoutDelta > 0)
                {
                    _phaseTimeoutDelta -= Time.deltaTime;
                }
                else
                {
                    if (_isAttackingPhase)
                    {
                        Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.ReadyAttack], false);
                        Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.Attack], false);
                        _isAttackingPhase = false;

                        _phaseTimeoutDelta = _phaseTimeout;

                        Ctx.Nav.updateRotation = true;
                        DealDamage = false;
                        Ctx.RateFireTimeoutDelta = Ctx.RateFireTimeout;
                    }
                }
            }


            CheckSwitchState();
        }
    }
}
