using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierCrouchState : SoldierBaseState, IRootState
    {
        public SoldierCrouchState(SoldierControl currentContext, SoldierFactory StateFactory)
       : base(currentContext, StateFactory)
        {
            IsRootState = true;
        }

        public override void CheckSwitchState()
        {

        }

        public override void EnterState()
        {
            InitializaSubState();
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
