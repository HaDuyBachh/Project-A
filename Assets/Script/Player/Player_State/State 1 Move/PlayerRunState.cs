using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class PlayerRunState : PlayerBaseState
    {
        public PlayerRunState(ThirdPersonController currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
        {

        }   
        public override void CheckSwitchState()
        {
            
            switch (Ctx.Input.standardMove)
            {
                case 0: SwitchState(Factory.Idle()); break;
                case 1: SwitchState(Factory.Walk()); break;
                case 3: SwitchState(Factory.Sprint()); break;
            }
        }

        public override void EnterState()
        {
            InitializaSubState();
        }

        public override void ExitState()
        {
        }

        public override void UpdateState()
        {
            Ctx.SetSpeedDelta(Ctx.RunSpeed);
            CheckSwitchState();
        }

        public override void InitializaSubState()
        {
            if (Ctx.Input.Equiped == null) SetSubState(Factory.Melee());
            else
                switch (Ctx.Input.Equiped.type)
                {
                    case Item.Type.none: SetSubState(Factory.Melee()); break;
                    case Item.Type.melee: SetSubState(Factory.Melee()); break;
                    case Item.Type.pistol: SetSubState(Factory.HandGun()); break;
                    case Item.Type.rifle: SetSubState(Factory.Rifles()); break;
                    case Item.Type.shotgun: SetSubState(Factory.Rifles()); break;
                    case Item.Type.smg: SetSubState(Factory.Rifles()); break;
                    case Item.Type.snip: SetSubState(Factory.Rifles()); break;
                    default: Debug.Log(Ctx.Input.Equiped.type); break;
                }
        }

    }
}
