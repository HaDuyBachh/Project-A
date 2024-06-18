using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace HaDuyBach
{
    public class SlotControl : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
    {
        private Inventory_Control Inv;
        public int index;
        private float click = -1;
        private bool isDraging = false;
        private GameObject temporary_Image;
        private GameObject Status_Item_Menu;
        private GameObject UI_Inventory;

        //lưu ý Coroutine phải tham chiếu nếu không sẽ không stop được
        private Coroutine DragState;

        private void Awake()
        {
            // tên object là slot_xxx lấy từ xxx trở đi
            index = convertNametoIndex(name);

            // lấy UI_Inventory
            UI_Inventory = transform.parent.gameObject;
            if (index >= Inventory_Control.N) UI_Inventory = UI_Inventory.transform.parent.gameObject;

            // lấy vị trí của temporary_Image là cuối cùng của UI_Inventory
            temporary_Image = UI_Inventory.transform.GetChild(UI_Inventory.transform.childCount - 1).gameObject;

            //vị trí của Status_Item_Menu trong UI_Inventory luôn phải ở thứ 2 từ cuối lên
            Status_Item_Menu = UI_Inventory.transform.GetChild(UI_Inventory.transform.childCount - 2).gameObject;

            // lấy cha của cha của game object hiện tại, nếu nhỏ hơn N thì lấy cha thêm 1 lầm nữa (slot -> UI_Chest -> UI_Inventory -> UI_Canvas_Start..)
            if (index < Inventory_Control.N) Inv = transform.parent.transform.parent.GetComponent<Inventory_Control>();
            else
                Inv = transform.parent.transform.parent.transform.parent.GetComponent<Inventory_Control>();
        }

        private int convertNametoIndex(string str)
        {
            bool startConvert = false;
            int index = 0;
            str.ToLower();
            foreach (var c in str)
            {
                if (c >= '0' && c <= '9') startConvert = true;
                if (startConvert)
                {
                    if (c < '0' || c > '9') return index;
                    index = index * 10 + (c - '0');
                }
            }
            return index;
        }


        //lưu ý Coroutine phải tham chiếu nếu không sẽ không stop được
        IEnumerator StartDrag()
        {
            yield return new WaitForSeconds(0.08f);
            isDraging = true;
            temporary_Image.SetActive(true);
            temporary_Image.transform.position = transform.position;
            temporary_Image.GetComponent<Image>().sprite = transform.GetChild(0).GetComponent<Image>().sprite;

            //tạm thời ẩn hình của slot hiện tại đi
            gameObject.transform.GetChild(0).GetComponent<Image>().enabled = false;
        }

        private void activeStatus_Item_Menu()
        {
            // cái này để tránh bị dính lỗi khi di chuyển item quá nhanh
            {
                if (isDraging)
                {
                    Inv.ChangeSlot(this.gameObject, this.gameObject);
                    temporary_Image.SetActive(false);
                    isDraging = false;
                }
                if (DragState != null) StopCoroutine(DragState);
            }

            //Debug.Log("đã active");
            Status_Item_Menu.SetActive(true);
            //con đầu tiên của Status_Item_Menu là GroupButton chịu tránh nhiệm hiển thị các nút lệnh, cần chuyển nó về hướng ô người chơi bấm vào
            Status_Item_Menu.transform.GetChild(0).transform.position = transform.position;
            //con thứ 2 của Status_Item_Menu là Inform chịu tránh nhiệm hiển thị thông tin
            Status_Item_Menu.transform.GetChild(1).transform.GetChild(0).GetComponent<Text>().text = ListItem.getDscr(Inv.Inventory[index].item.index);
            //Status_Item_Menu.GetComponent<Status_Item_Menu>().Invoke(index);
        }


        public void OnPointerDown(PointerEventData eventData)
        {
            if (!Inv.Inventory[index].empty())
            {
                //sau khi giữ 0.3s, hàm này sẽ bắt đầu việc cho ảnh tạm thời hoạt động và di chuyển , để !isDraging để tránh dí lỗi vô tình ném đồ đi
                if (click != -1 && Time.time - click < 0.5f && !isDraging) activeStatus_Item_Menu();
                click = Time.time;

                //lưu ý Coroutine phải tham chiếu nếu không sẽ không stop được
                DragState = StartCoroutine(StartDrag());
            }
        }

        public void OnDrag(PointerEventData pointerEvent)
        {
            if (isDraging) temporary_Image.transform.position = pointerEvent.position;
            else
                if (DragState != null) StopCoroutine(DragState);

        }

        public void OnPointerUp(PointerEventData eventData)
        {
            //thay đổi 2 gameoject thả và được thả này this.gameOject -> CurrentDrop
            if (isDraging)
            {
                //lấy gameoject của slot được drop hiện tại
                var CurrentDrop = GetCurrentSlotDrop(eventData);
                Inv.ChangeSlot(this.gameObject, CurrentDrop);
                temporary_Image.SetActive(false);
                isDraging = false;
            }
            if (DragState != null) StopCoroutine(DragState);
        }

        private GameObject GetCurrentSlotDrop(PointerEventData eventData)
        {
            List<RaycastResult> raycastResults = new List<RaycastResult>();
            EventSystem.current.RaycastAll(eventData, raycastResults);

            GameObject CurretDrop = raycastResults[0].gameObject;
            Debug.Log(CurretDrop.name);
            while (!CurretDrop.name.Contains("UI_Inventory") && !CurretDrop.name.Contains("slot"))
            {
                CurretDrop = CurretDrop.transform.parent.gameObject;
            }

            return CurretDrop;
        }
    }
}