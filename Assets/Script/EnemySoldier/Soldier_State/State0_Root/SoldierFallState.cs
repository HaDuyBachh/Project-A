using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierFallState : SoldierBaseState, IRootState
    {
        public SoldierFallState(SoldierControl currentContext, SoldierFactory StateFactory)
       : base(currentContext, StateFactory)
        {
            IsRootState = true;
        }

        public override void CheckSwitchState()
        {

        }

        public override void EnterState()
        {

        }

        public override void ExitState()
        {

        }

        public void HandleGravity()
        {
            
        }

        public override void InitializaSubState()
        {

        }

        public override void UpdateState()
        {

        }
    }

}
