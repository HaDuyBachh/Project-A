using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{

    public class SoldierMeleeState : SoldierBaseState
    {
        public SoldierMeleeState(SoldierControl currentContext, SoldierFactory StateFactory)
         : base(currentContext, StateFactory)
        {

        }

        private float _timeToFindPathDelta = 0.0f;
        private bool _arrived_value = false;

        public override void CheckSwitchState()
        {
            if (Ctx.TimeFollowRemainDelta <= 0)
            {
                SwitchState(Factory.Patrol());
            }
            else
            if (Ctx.CanAttackEnemy())
            {
                SwitchState(Factory.MeleeAttack());
            }
            else
            if (Ctx.IsRunningToCoverPoint)
            {
                SwitchState(Factory.MeleeCover());
            }
        }

        public override void EnterState()
        {
            InitializaSubState();

            //Random hành vi tấn công
            Ctx.Reckless = false;

            //Đặt chạy đến điểm ẩn nấp thành false
            Ctx.IsRunningToCoverPoint = false;

            //Đặt lại tốc độ chạy và thời gian theo dấu mục tiêu
            Ctx.NormMove = 0;
            Ctx.TimeFollowRemainDelta = Ctx.TimeFollow;

            _arrived_value = true;

        }
        public override void UpdateState()
        {
            //Vẫn còn đi theo người chơi hay không
            if (!Ctx.StillFollow())
                Ctx.TimeFollowRemainDelta -= Time.deltaTime;
            else
                Ctx.TimeFollowRemainDelta = Ctx.TimeFollow;

            if (Ctx.IsStuck())
            {
                Ctx.Reckless = true;
                Ctx.CoverCount = 0;
                _arrived_value = true;
                Debug.Log("Đã bị kẹt");
            }

            if (!Ctx.Reckless && Ctx.BeingTargetByEnemy() && (Ctx.CoverCount > 0 && Ctx.FindNewCoverPoint()
                              || (Ctx.FindListCoverPoint(20) && Ctx.FindNewCoverPoint())))
            {
                Debug.Log("Đi đến điểm ẩn nấp");
                Ctx.SetDestination(Ctx.CoverPoint.First, 0);
                Ctx.IsRunningToCoverPoint = true;
                Ctx.NormMove = 2;
            }
            else
            {
                if (Ctx.CanApproachEnemy())
                {
                    Ctx.NormMove = 2;
                    Ctx.Nav.stoppingDistance = Ctx.AttackDistance;
                }
                else
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
                            Ctx.Turn(Ctx.EnemyPos() - Ctx.transform.position, 6.0f);
                            _timeToFindPathDelta -= Time.deltaTime;
                            if (Ctx.BeAttacked) _timeToFindPathDelta -= Time.deltaTime;
                        }
                        else
                        if (Ctx.FindAndMoveAroundEnemy(Ctx.AttackDistance*5f))
                        {
                            Debug.Log("Đang chạy quanh người chơi");
                            Ctx.NormMove = 2;
                            _timeToFindPathDelta = 0.5f;
                            _arrived_value = false;
                        }
                    }
                }
            }

            CheckSwitchState();
        }

        public override void ExitState()
        {
            Ctx.TimeFollowRemainDelta = 0.0f;
        }

        public override void InitializaSubState()
        {
            SetSubState(Factory.Idle());
        }

    }
}
