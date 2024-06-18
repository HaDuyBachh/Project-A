using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierMeleeCoverState : SoldierBaseState
    {
        public SoldierMeleeCoverState(SoldierControl currentContext, SoldierFactory StateFactory)
       : base(currentContext, StateFactory)
        {

        }

        public override void CheckSwitchState()
        {
            if (Ctx.Cover_Timeout_Delta <=0 )
            {
                switch (Ctx.SoldierType)
                {
                    case SoldierControl.Type.None:
                    case SoldierControl.Type.Melee: SwitchState(Factory.Melee()); break;

                    case SoldierControl.Type.MeleeShield: break;

                    case SoldierControl.Type.Handgun:
                    case SoldierControl.Type.Rifle: break;
                }
            }    
            
        }

        /// <summary>
        /// Kiểm tra xem đã đến nơi chưa
        /// </summary>
        private bool Arrived = false;
        /// <summary>
        /// Thời gian thực hiện kiểm tra Player có đang đi tới chỗ chủ thể hay không
        /// </summary>
        private float CheckPlayerDelta = 0.6f;

        public override void EnterState()
        {
            InitializaSubState();

            Ctx.NormMove = 2;
            Ctx.Cover_Timeout_Delta = Ctx.Cover_Timeout;
            Arrived = false;
        }

        public override void ExitState()
        {
            Ctx.IsRunningToCoverPoint = false;
            Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.Cover], false);
        }

        public override void InitializaSubState()
        {
            SetSubState(Factory.Idle());
        }

        public override void UpdateState()
        {
            //Vốn dĩ phải đặt thế này mà không dùng trực tiếp hàm luôn là vì chủ thể có thể bị xê dịch khi bị các vật khác tác động 
            // dẫn tới IsArrived ghi nhận là chưa đến nơi và gây ra tình trạng không chạy được
            if (Ctx.IsArrived()) Arrived = true;

            if (Arrived)
            {
                if (Ctx.Cover_Timeout_Delta == Ctx.Cover_Timeout)
                {
                    Ctx.NormMove = 0;
                    Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.Cover], true);
                }
                Ctx.Cover_Timeout_Delta -= Time.deltaTime;
                CheckPlayerDelta -= Time.deltaTime;

                if (CheckPlayerDelta < 0)
                {
                    if (!Ctx.IsThisLocateHideFromPlayer(Ctx.Hip.position))
                    {
                        Ctx.Cover_Timeout_Delta = -1;
                    }
                    CheckPlayerDelta = 0.6f;
                } 
            }

            CheckSwitchState();
        }
    }

}