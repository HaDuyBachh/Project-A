using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierGunState : SoldierBaseState
    {
        public SoldierGunState(SoldierControl currentContext, SoldierFactory StateFactory)
         : base(currentContext, StateFactory)
        {

        }

        private bool _canAttackEnemy_Value = false;
        private float _cantAttack = 0.0f;
        public override void CheckSwitchState()
        {
            if (Ctx.TimeFollowRemainDelta <= 0)
            {
                SwitchState(Factory.Patrol());
            }
            else
            if (Ctx.IsRunningToCoverPoint)
            {
                SwitchState(Factory.GunCover());
            }
            else
            if (Ctx.AttackTimeoutDelta > 0 && _canAttackEnemy_Value)
            {
                SwitchState(Factory.GunAttack());
            }
        }

        public override void EnterState()
        {
            InitializaSubState();

            Ctx.Reckless = false;
            Ctx.IsRunningToCoverPoint = false;

            Ctx.TimeFollowRemainDelta = Ctx.TimeFollow;
            Ctx.NormMove = 0;

            if (Ctx.CoverCount == 0) Ctx.Change_CoverPoint_Delta = Ctx.Change_CoverPoint;

            _canAttackEnemy_Value = false;
        }

        public override void ExitState()
        {
            Ctx.TimeFollowRemainDelta = 0.0f;
        }

        public override void InitializaSubState()
        {
            SetSubState(Factory.Idle());
        }

        public override void UpdateState()
        {
            _canAttackEnemy_Value = Ctx.CanAttackEnemy();

            //Đổi điểm phòng thủ sau một thời gian cố định
            if (Time.time - Ctx.Change_CoverPoint_Delta >= Ctx.Change_CoverPoint)
            {
                Ctx.Change_CoverPoint_Delta = Time.time + Random.Range(Ctx.Change_CoverPoint/2,Ctx.Change_CoverPoint);
                if (!Ctx.FindNewCoverPoint()) Ctx.CoverCount = 0;
            }

            if (_canAttackEnemy_Value)
            {
                _cantAttack += Time.deltaTime;
                if (_cantAttack > 1.5f)
                {
                    Ctx.Reckless = true;
                }
            }
            else
                _cantAttack = 0;

            if (Ctx.IsStuck())
            {
                Ctx.Nav.ResetPath();
                Ctx.CoverCount = 0;
                Ctx.Reckless = true;

                Debug.Log("Đã bị kẹt " + Ctx.name);
            }

            if (Ctx.StillFollow())
                Ctx.TimeFollowRemainDelta = Ctx.TimeFollowRemainDelta;
            else
                Ctx.TimeFollowRemainDelta -= Time.deltaTime;

            if (Ctx.Reckless)
            {
                Debug.Log("đang thực hiện tấn công liều lĩnh " + Ctx.name);
                if (_canAttackEnemy_Value)
                {
                    Ctx.NormMove = 0;
                    Ctx.AttackTimeoutDelta = Random.Range(Ctx.AttackTimeout / 2, Ctx.AttackTimeout);
                }
                else
                {
                    Ctx.SetDestination(Ctx.EnemyPos(), Ctx.StopDis);
                    Ctx.NormMove = 2;
                }
            }
            else
            if (Ctx.CoverCount > 0)
            {
                if (Ctx.IsNearAttackPlace && Ctx.AttackTimeoutDelta <= 0)
                {
                    Ctx.SetDestination(Ctx.CoverPoint.First, 0.1f);
                    Ctx.IsNearAttackPlace = false;

                    Ctx.NormMove = 2;
                    Ctx.IsRunningToCoverPoint = true;

                    Debug.Log("Tấn công xong, chạy đến điểm phòng thủ " + Ctx.name);
                }
                else
                if (!Ctx.IsNearAttackPlace)
                {
                    if (_canAttackEnemy_Value)
                    {
                        if (!Ctx.FindNewCoverPoint()) Ctx.CoverCount = 0;
                        else
                        {
                            Ctx.SetDestination(Ctx.CoverPoint.Second, 0.1f);
                            Ctx.IsNearAttackPlace = true;

                            Ctx.NormMove = 0;
                            Ctx.AttackTimeoutDelta = Random.Range(Ctx.AttackTimeout / 2, Ctx.AttackTimeout);
                            Debug.Log("Đang ở điểm phòng thủ nhưng vẫn bị tấn công " + Ctx.name);
                        }
                    }
                    else
                    if (Ctx.CanAttackEnemyInAttackPlace())
                    {
                        Ctx.SetDestination(Ctx.CoverPoint.Second, 0.1f);
                        Ctx.IsNearAttackPlace = true;

                        Ctx.NormMove = 2;
                        Ctx.AttackTimeoutDelta = Random.Range(Ctx.AttackTimeout / 2, Ctx.AttackTimeout);
                        Debug.Log("Chạy đến điểm tấn công " + Ctx.name);
                    }
                    else
                    {
                        if (!Ctx.FindNewCoverPoint())
                        {
                            Ctx.CoverCount = 0;
                            Debug.Log("Xóa hết điểm tấn công vì tất cả điểm tấn công không thỏa mãn " + Ctx.name);
                        }
                        else
                        {
                            Ctx.SetDestination(Ctx.CoverPoint.First, 0.1f);
                            Ctx.IsNearAttackPlace = false;

                            Ctx.NormMove = 2;
                            Ctx.IsRunningToCoverPoint = true;
                            Debug.Log("Đổi điểm Cover mới vì điểm tấn công hiện tại không thể tấn công " + Ctx.name);
                        }
                    }
                }
            }
            else
            if (Ctx.FindListCoverPoint(20) && Ctx.FindNewCoverPoint())
            {
                Ctx.SetDestination(Ctx.CoverPoint.First, 0.1f);
                Ctx.IsNearAttackPlace = false;

                Ctx.IsRunningToCoverPoint = true;

                Debug.Log("Đã tìm được điểm Cover và đang chạy đến " + Ctx.name);
            }
            else
            if (_canAttackEnemy_Value)
            {
                Ctx.Reckless = true;
                Ctx.NormMove = 0;
                Ctx.AttackTimeoutDelta = Random.Range(Ctx.AttackTimeout / 2, Ctx.AttackTimeout);

                Debug.Log("Không tìm được điểm Cover nhưng có thể tấn công " + Ctx.name);
            }
            else
            {
                Ctx.Reckless = true;
                Ctx.SetDestination(Ctx.EnemyPos(), Ctx.StopDis);
                Ctx.NormMove = 2;

                Debug.Log("Không tìm được điểm và không thể tấn công " + Ctx.name);
            }


            CheckSwitchState();
        }
    }
}
