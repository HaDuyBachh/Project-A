using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierRunState : SoldierBaseState
    {
        public SoldierRunState(SoldierControl currentContext, SoldierFactory StateFactory)
       : base(currentContext, StateFactory)
        {

        }

        public override void CheckSwitchState()
        {
            switch (Ctx.NormMove)
            {
                case 0: SwitchState(Factory.Idle()); break;
                case 1: SwitchState(Factory.Walk()); break;
            }
        }

        public override void EnterState()
        {
            InitializaSubState();
        }

        public override void UpdateState()
        {
            Ctx.SetSpeedDelta(Ctx.RunSpeed);
            Ctx.Nav.speed = Ctx.Speed;
            CheckSwitchState();
        }

        public override void ExitState()
        {

        }

        public override void InitializaSubState()
        {
           // SetSubState(Factory.NoAttack());
        }
    }
}
