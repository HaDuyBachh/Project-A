using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{

    public class PlayerDriveState : PlayerBaseState
    {
        public PlayerDriveState(ThirdPersonController currentContext, PlayerStateFactory playerStateFactory)
        : base(currentContext, playerStateFactory)
        {
            IsRootState = true;
        }
        public override void CheckSwitchState()
        {
            if (!Ctx.Driving)
            {
                SwitchState(Factory.Grounded());
            }    
        }

        private void NoNeedScript(bool state)
        {
            Ctx.GetComponent<CapsuleCollider>().enabled = state;
            Ctx.GetComponent<CharacterController>().enabled = state;
        }

        public override void EnterState()
        {
            InitializaSubState();
            
            Ctx.ChangeCameraFollow(2);

            NoNeedScript(false);

            Ctx.transform.SetParent(Ctx.CarControl.Sit);

            Ctx.transform.localPosition = Vector3.zero;
            Ctx.transform.localRotation = Quaternion.identity;
            Ctx.transform.localScale = Vector3.one;

            Ctx.Animator.Play("Driving");

        }

        public override void ExitState()
        {
            Ctx.transform.SetParent(null);

            Ctx.transform.position = Ctx.CarControl.Leave.GetChild(0).position;
            Ctx.transform.rotation = Ctx.CarControl.Leave.GetChild(0).rotation;
            Ctx.transform.localScale = Vector3.one;

            NoNeedScript(true);

            Ctx.Animator.Play("Melee Weapon Ready");

            Ctx.CarControl = null;
        }

        public override void InitializaSubState()
        {
            //SwitchState(Factory.Idle());
        }

        public override void UpdateState()
        {
            Ctx.transform.localPosition = Vector3.zero;
            Ctx.transform.localRotation = Quaternion.identity;

            CheckSwitchState();
        }
    }

}