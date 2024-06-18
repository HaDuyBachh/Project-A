using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{

    public class PlayerRiflesState : PlayerBaseState, IWeaponState
    {
        public PlayerRiflesState(ThirdPersonController currentContext, PlayerStateFactory playerStateFactory)
         : base(currentContext, playerStateFactory)
        {

        }

        public void CheckSwitchWeapon()
        {
            //cấu trúc của CheckSwitch giữa các Weapon State, bỏ các trạng thái của chính nó đi, ví dụ handgun sẽ không tự vào handgun
            if (Ctx.ChangeWeapon)
            {
                //đặt Ctx.ChangeWeapon để tránh lỗi OverFlow nếu có
                Ctx.ChangeWeapon = false;

                switch (Ctx.Input.Equiped.type)
                {
                    case Item.Type.none: 
                    case Item.Type.melee: 
                        SwitchState(Factory.Melee()); break;
                    case Item.Type.pistol: 
                        SwitchState(Factory.HandGun()); break;
                    case Item.Type.rifle: 
                    case Item.Type.smg: 
                    case Item.Type.shotgun: 
                    case Item.Type.snip: 
                        SwitchState(Factory.Rifles()); break;
                }

                //Khi chuẩn vũ khi sẽ đặt lại thời gian chuyển đổi của ready attack state
                //Không đặt ở hàm ExitState vì khi di chuyển sẽ khởi tạo cái mới gây ra tình trạng ready bị hủy khi di chuyển
                Ctx.ReadyToIdleTimeoutDelta = -1.0f;
            }
        }

        public override void CheckSwitchState()
        {
            CheckSwitchWeapon();
        }

        public override void EnterState()
        {
            InitializaSubState();

            //Debug.Log("Đang vào Rifles State");
            if (Ctx.Input.Equiped != null) Ctx.Animator.SetInteger(Ctx.AnimList[ThirdPersonController.Anim.Weapon], (int)Ctx.Input.Equiped.type);
        }

        public override void ExitState()
        {
            //Debug.Log("Đang thoát Rifles State");
        }

        public override void UpdateState()
        {
            CheckSwitchState();
        }

        public override void InitializaSubState()
        {
            if (Ctx.ReadyToIdleTimeoutDelta > 0.0f || (Ctx.Input.attack && Ctx.Grounded && !Ctx.Input.jump && Ctx.FallTimeOutDelta > 0))
            {
                SetSubState(Factory.ReadyGunAttack());
            }
            else
                SetSubState(Factory.NoAttack());
        }


    }
}