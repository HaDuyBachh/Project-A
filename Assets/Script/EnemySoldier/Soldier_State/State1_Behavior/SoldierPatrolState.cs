using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierPatrolState : SoldierBaseState
    {
        public SoldierPatrolState(SoldierControl currentContext, SoldierFactory StateFactory)
         : base(currentContext, StateFactory)
        {

        }

        public override void CheckSwitchState()
        {
            if (Ctx.IsArrived())
            {
                SwitchState(Factory.Guard());
            }    
            else
            if (Ctx.Suspec || Ctx.BeAttacked)
            {
                SwitchState(Factory.Find());
            }
        }

        private int GuardLocateId = -1;

        public override void EnterState()
        {
            InitializaSubState();

            //Đặt tốc độ di chuyển là đi bộ, đặt điểm đích là vị trí gốc
            Ctx.NormMove = 1;

            //Tìm vị trí canh gác mới
            Ctx.SetNewGuardLocal(GuardLocateId = Ctx.FindNewGuardLocal());
        }

        public override void UpdateState()
        {
            //Vị trí đang đi đến đã có người đặt chỗ trước và người đó id lớn hơn
            if (GuardLocateId != -1 && Ctx.Group.GuardLocateID[GuardLocateId] > Ctx.ID)
            {
                Ctx.SetNewGuardLocal(GuardLocateId = Ctx.FindNewGuardLocal());
            }

            CheckSwitchState();
        }


        public override void ExitState()
        {
            GuardLocateId = -1;
        }

        public override void InitializaSubState()
        {
            SetSubState(Factory.Walk());
        }

    }
}
