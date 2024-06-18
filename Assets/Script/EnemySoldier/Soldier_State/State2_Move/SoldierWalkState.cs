using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierWalkState : SoldierBaseState
    {
        public SoldierWalkState(SoldierControl currentContext, SoldierFactory StateFactory)
       : base(currentContext, StateFactory)
        {

        }

        public override void CheckSwitchState()
        {
            switch (Ctx.NormMove)
            {
                case 0: SwitchState(Factory.Idle()); break;
                case 2: SwitchState(Factory.Run()); break;
            }
        }

        public override void EnterState()
        {
            //Debug.Log("Đã vào Soldier Walk State");
            InitializaSubState();
        }

        public override void UpdateState()
        {
            Ctx.SetSpeedDelta(Ctx.WalkSpeed);
            Ctx.Nav.speed = Ctx.Speed;
            CheckSwitchState();
        }

        public override void ExitState()
        {

        }

        public override void InitializaSubState()
        {
            //SetSubState(Factory.NoAttack());
        }

       
    }
}
