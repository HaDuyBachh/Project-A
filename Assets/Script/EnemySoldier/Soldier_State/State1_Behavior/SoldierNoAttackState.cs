using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierNoAttackState : SoldierBaseState
    {
        public SoldierNoAttackState(SoldierControl currentContext, SoldierFactory StateFactory)
         : base(currentContext, StateFactory)
        {
            
        }

        public override void CheckSwitchState()
        {
            if (Ctx.Attack)
            {
                switch (Ctx.SoldierType)
                {
                    case SoldierControl.Type.None:
                    case SoldierControl.Type.Melee:
                    case SoldierControl.Type.MeleeShield:
                        SwitchState(Factory.MeleeAttack()); break;
                    case SoldierControl.Type.Handgun:
                    case SoldierControl.Type.Rifle:
                        SwitchState(Factory.GunAttack()); break;
                }    
            }     
        }

        public override void EnterState()
        {

        }

        public override void ExitState()
        {
            
        }

        public override void InitializaSubState()
        {
            
        }

        public override void UpdateState()
        {
            CheckSwitchState();
        }
    }
}
