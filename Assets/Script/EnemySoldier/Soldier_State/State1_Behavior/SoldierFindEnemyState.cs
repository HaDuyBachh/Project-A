using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierFindEnemyState : SoldierBaseState
    {
        public SoldierFindEnemyState(SoldierControl currentContext, SoldierFactory StateFactory)
         : base(currentContext, StateFactory)
        {

        }

        /// <summary>
        /// Thể hiện rằng có muốn thay đổi quyết định hay không
        /// </summary>
        private bool isChangeDestination = false;

        public override void CheckSwitchState()
        {
            if (Ctx.Find_Patrol_Delta <= 0)
            {
                SwitchState(Factory.Patrol());
            }
            else
            if (Ctx.SeeEnemy())
            {
                switch (Ctx.SoldierType)
                {
                    case SoldierControl.Type.None:
                    case SoldierControl.Type.Melee: SwitchState(Factory.Melee()); break;

                    case SoldierControl.Type.MeleeShield: break;

                    case SoldierControl.Type.Handgun:
                    case SoldierControl.Type.Rifle: SwitchState(Factory.Gun()); break;
                }    
            }    
        }

        public override void EnterState()
        {
            InitializaSubState();

            // Nếu bị tấn công thì thông báo tới các Soldier khác trong group
            if (Ctx.BeAttacked)
            {
                foreach (var s in Ctx.Group.TempSoldier)
                {
                    var s_ctr = s.GetComponent<SoldierControl>();

                    //Không được trùng nhau
                    if (s_ctr == Ctx) continue;

                    //Thông báo đến các soldier khác
                    s_ctr.Suspec = true;

                    // Nếu soldier khác chưa có vị trí nghi ngờ riêng thì
                    //          thông báo vị trí nghi ngờ của bản thân hoặc vị trí đứng hiện tại (Random)
                    if (s_ctr.SusLocate == null)
                    {
                        s_ctr.SusLocate = Ctx.GetNearTarget((Vector3)Ctx.SusLocate, 3.0f);
                    }
                }
            }

            //Đặt điểm nghi ngờ và di chuyển đến 
            Ctx.NormMove = 1;
            if (Ctx.SusLocate == null) Ctx.SusLocate = Ctx.transform.position;
            Ctx.SetDestination((Vector3)Ctx.SusLocate, Ctx.StopDis);

            //Random việc thay đổi quyết định
            isChangeDestination = (Random.Range(0, 100) <= 50);

            //Đặt thời gian tìm kiếm
            Ctx.Find_Patrol_Delta = Ctx.Find_Patrol;
        }

        public override void UpdateState()
        {
            //Nếu chưa đến nơi  
            if (!Ctx.IsArrived())
            {
                //Nếu địa điểm nghi ngờ chưa bị xóa và địa điểm đích khác với địa điểm nghi ngờ 
                //Thì kiểm tra xem nhân vật có đang muốn thay đổi quyết định hay không
                if (Ctx.SusLocate != null && Ctx.Nav.destination != Ctx.SusLocate && isChangeDestination)
                {
                    isChangeDestination = (Random.Range(0, 100) <= 80);
                    Ctx.SetDestination((Vector3)Ctx.SusLocate, Ctx.StopDis);
                }
            }
            else
            //Nếu đã đến nơi  
            {

                //Nếu chưa dừng di chuyển => tức là chưa ghi nhận đã từng đến nơi thì ta thực hiện cài đặt là đã đến nơi
                if (Ctx.NormMove != 0)
                {
                    Ctx.NormMove = 0;
                    Ctx.SusLocate = null;

                    //Đặt random việc quyết định thay đổi đường đi
                    isChangeDestination = (Random.Range(0, 100) <= 50);

                    Debug.Log("Đã đến nơi");
                }
                else
                //Nếu đã dừng di chuyển mà vẫn có điểm nghi ngờ => tức là đã có điểm nghi ngờ mới
                if (Ctx.SusLocate != null)
                {
                    Ctx.NormMove = 1;
                    Ctx.SetDestination((Vector3)Ctx.SusLocate, Ctx.StopDis);

                    Debug.Log("Đã đã đặt điểm mới");
                }
            }

            //Nếu không còn điểm nghi ngờ nào
            if (Ctx.SusLocate == null)
            {
                if (Ctx.Find_Patrol_Delta >= 0) Ctx.Find_Patrol_Delta -= Time.deltaTime;
            }
            else
                Ctx.Find_Patrol_Delta = Ctx.Find_Patrol;


            CheckSwitchState();
        }


        public override void ExitState()
        {
            Ctx.Suspec = false;
            Ctx.Find_Patrol_Delta = -1;
            Ctx.SusLocate = null;
            Ctx.SetDestination(Ctx.transform.position, Ctx.StopDis);

            Ctx.CoverCount = 0;
        }

        public override void InitializaSubState()
        {
            SetSubState(Factory.Idle());
        }

    }
}
