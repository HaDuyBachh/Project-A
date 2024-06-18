using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierIdleState : SoldierBaseState
    {
        public SoldierIdleState(SoldierControl currentContext, SoldierFactory StateFactory)
       : base(currentContext, StateFactory)
        {
            
        }
        
        public override void CheckSwitchState()
        {
            switch (Ctx.NormMove)
            {
                case 1: SwitchState(Factory.Walk()); break;
                case 2: SwitchState(Factory.Run()); break;
            }    
        }

        public override void EnterState()
        {
            //Debug.Log("Đã vào Soldier Idle State");
            InitializaSubState();
        }

        public override void UpdateState()
        {
            Ctx.SetSpeedDelta(0);
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

