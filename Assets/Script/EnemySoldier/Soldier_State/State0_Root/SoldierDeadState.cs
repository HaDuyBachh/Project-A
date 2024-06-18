using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierDeadState : SoldierBaseState
    {
        public SoldierDeadState(SoldierControl currentContext, SoldierFactory StateFactory)
         : base(currentContext, StateFactory)
        {

        }

        float _time_Disapper = 4.0f;

        public override void CheckSwitchState()
        {
            
        }

        
        public override void EnterState()
        {
            if (Rand.RandPer(50,100)) Ctx.Animator.Play("Base Layer.Dead Fall Back", 0, 0);
            else Ctx.Animator.Play("Base Layer.Dead Front", 0, 0);

            Ctx.Nav.isStopped = true;

            Debug.Log(Ctx.name + " đã nghẻo");

            _time_Disapper = 8.0f;

            Ctx.SetSoldierIsDead();
        }

        public override void ExitState()
        {
            
        }

        public override void InitializaSubState()
        {
            
        }

        public override void UpdateState()
        {
            _time_Disapper -= Time.deltaTime;
            if (_time_Disapper <= 0)
            {
                Ctx.gameObject.SetActive(false);
                Ctx.ResetAllValue();
            }
        }
    }
}
