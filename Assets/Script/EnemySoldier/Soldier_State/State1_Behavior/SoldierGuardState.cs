using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{

    public class SoldierGuardState : SoldierBaseState
    {
        public SoldierGuardState(SoldierControl currentContext, SoldierFactory StateFactory)
         : base(currentContext, StateFactory)
        {

        }
        public override void CheckSwitchState()
        {
            if (Ctx.Gruard_Patrol_Delta <= 0)
            {
                SwitchState(Factory.Patrol());
            }
            else
            if (Ctx.Suspec || Ctx.BeAttacked)
            {
                SwitchState(Factory.Find());
            }
        }


        public override void EnterState()
        {
            //Debug.Log("Đã vào Soldier Guard State");

            Ctx.NormMove = 0;

            InitializaSubState();

            //Random thời gian để đi tuần lượt tiếp theo
            Ctx.Gruard_Patrol_Delta = Random.Range(Ctx.Gruard_Patrol / 3f, Ctx.Gruard_Patrol + 1f);
        }
        public override void UpdateState()
        {
            if (Ctx.Gruard_Patrol_Delta >= 0)
            {
                Ctx.Gruard_Patrol_Delta -= Time.deltaTime;
            }

            CheckSwitchState();
        }

        public override void ExitState()
        {

        }

        public override void InitializaSubState()
        {
            SetSubState(Factory.Idle());
        }
    }
}
