using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierGroundState : SoldierBaseState, IRootState
    {
        public SoldierGroundState(SoldierControl currentContext, SoldierFactory StateFactory)
       : base(currentContext, StateFactory)
        {
            IsRootState = true;
        }

        public override void CheckSwitchState()
        {
            if (Ctx.HP <= 0) SwitchState(Factory.Dead());
        }

        public override void EnterState()
        {
            //Debug.Log("Đã vào Soldier Ground State");
            
            InitializaSubState();

            Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.Grounded], true);
        }

        public override void UpdateState()
        {
            Ctx.RecordNoise();

            CheckSwitchState();
        }

        public override void ExitState()
        {
            Ctx.Animator.SetBool(Ctx.AnimList[SoldierControl.Anim.Grounded], false);
        }

        public void HandleGravity()
        {
            return;
        }

        public override void InitializaSubState()
        {
            SetSubState(Factory.Guard());
        }
    }
}