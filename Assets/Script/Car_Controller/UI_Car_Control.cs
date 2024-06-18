using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class UI_Car_Control : MonoBehaviour
    {
        private GameObject vehicle;
        public GameObject EnterCar_Button;
        private ThirdPersonController Player;
        private GameObject _UI_Car, _UI_Player;
        void Start()
        {
            Player = FindObjectOfType<ThirdPersonController>();
          
            _UI_Car = transform.GetChild(2).gameObject;
            _UI_Player = transform.GetChild(1).gameObject;
        }

        public void invoke_veh(GameObject vehicle)
        {
            this.vehicle = vehicle;
            EnterCar_Button.SetActive(vehicle != null);
        }

        #region--------------------------------------------------- Enter & Exit Vehicle Control ------------------------------------------------------

        public void EnterCarAndOpenUI()
        {
            //Xác nhận người chơi đang lái xe
            Player.Driving = true;

            //lấy script lái của xe
            Player.CarControl = vehicle.GetComponent<CarControl>();

            //bỏ vật phẩm đang cầm ở tay, phải dùng ActiveEquip chứ k được thay trực tiếp
            Player.Input.ActiveEquip(new());

            // chỉnh trạng thái player in để xe chạy được
            Player.CarControl.IsPlayerOut = false;

            //Đặt lại tag cho phương tiện
            Tag_isPlayerEnter(true);

            // tắt bảng điều khiển nhân vật bật bảng điều khiển xe
            _UI_Car.SetActive(true);
            _UI_Player.SetActive(false);

            Player.ChangeCameraFollow(2);
            Player.ChangeCamera(1);

            // thay đổi camera target để xoay được Change(cameraRoot, AngleOverride)
            Player.ChangeCameraTarget(vehicle.GetComponent<Open_Car_UI>().getCameraRoot().gameObject, 10);

            EnterCar_Button.SetActive(false);

            Player.SetWeightLayerImm(1, 0);

            Player.OffRigImm();
        }

        //sửa tag để không bị đụng collider camera
        public void Tag_isPlayerEnter(bool state)
        {
            if (state) vehicle.tag = "Player's Vehicle";
            else vehicle.tag = "Vehicle";
        }

        public void ExitCarAndCloseUI()
        {
            Player.Driving = false;

            // chỉnh trạng thái player out để xe tự dừng lại
            Player.CarControl.IsPlayerOut = true;

            //Đặt lại tag cho phương tiện
            Tag_isPlayerEnter(false);

            // tắt bảng điều xe bật khiển nhân vật
            _UI_Car.SetActive(false);
            _UI_Player.SetActive(true);

            Player.ChangeCameraFollow(0);
            Player.ChangeCamera(0);

            // thay đổi camera target để xoay được Change(cameraRoot, AngleOverride)
            Player.ChangeCameraTarget(Player.transform.GetChild(0).gameObject, 10);

            //Xóa phương tiện hiện tại
            vehicle = null;
        }

        #endregion

        #region  --------------------------------------------------- Drive Control ------------------------------------------------------
        public void Click_Forward(bool state)
        {
            Player.CarControl.MoveInput = state ? 1 : 0;
        }
        public void Click_Backward(bool state)
        {
            Player.CarControl.MoveInput = state ? -1 : 0;
        }
        public void Click_Left(bool state)
        {
            Player.CarControl.SteerInput = state ? -1 : 0;
        }
        public void Click_Right(bool state)
        {
            Player.CarControl.SteerInput = state ? 1 : 0;
        }
        #endregion

    }

}