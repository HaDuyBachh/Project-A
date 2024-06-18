using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HaDuyBach
{
    public class Inventory_Control : MonoBehaviour
    {
        // 0 -> 29 là túi đồ người chơi, 30 -> 60 là túi đồ bên ngoài
        public static int N = 30;
        public SlotItem[] Inventory = new SlotItem[N * 2];
        public GameObject UI_Inventory;
        private EquipMenuControl Equip_Menu;
        private GameObject item_image;
        private ChestControl Chst;
        private GameObject UI_Chest;


        //đặt là start vì Inventory sẽ phải khởi động sau Equip menu nếu không hàm Equip_Menu.AddItemInQuickSlot() sẽ bị lỗi vì chưa khởi tạo slot
        private void Start()
        {
            //0 là thứ tự của Image trong UI tổng
            item_image = transform.GetChild(0).gameObject;

            //Debug.Log(item_image.gameObject.name);

            Equip_Menu = gameObject.GetComponent<EquipMenuControl>();
            Chst = gameObject.GetComponent<ChestControl>();
            UI_Chest = UI_Inventory.transform.GetChild(N + 1).gameObject;

            Debug.Log(UI_Chest.name);

            // khởi tạo
            UI_Chest.gameObject.SetActive(false);
            for (int i = 0; i < N; i++)
            {
                Inventory[i] = new SlotItem();
                Set_Slot(i);
            }
            for (int i = N; i < 2 * N; i++)
            {
                UI_Chest.transform.GetChild(i - N).gameObject.SetActive(false);
            }

            int n = 1;
          //  AddItem(ListItem.getItem(0), ref n);
            n = 1;
            AddItem(ListItem.getItem(1), ref n);
            n = 1;
            AddItem(ListItem.getItem(2), ref n);
            n = 1;
            AddItem(ListItem.getItem(3), ref n);
        }

        public int GetN()
        {
            return N;
        }


        public void InitChestSlot(Chest_Inv I)
        {
            //Debug.Log(I.NumOfSlot); 
            for (int i = N; i < N + I.NumOfSlot; i++)
            {
                //Debug.Log(i + "    " + UI_Inventory.transform.GetChild(N + 1).transform.GetChild(i - N).name);
                Inventory[i] = I.GetSlot(i - N);
                UI_Inventory.transform.GetChild(N + 1).transform.GetChild(i - N).gameObject.SetActive(true);
                Set_Slot(i);
            }
        }

        public void CloseUI_Inventory()
        {
            UI_Inventory.SetActive(false);
            UI_Chest.SetActive(false);

            if (!Chst.HaveChest()) return;
            Chst.ChangeItemInInventory();

            for (int i = N; i < 2 * N; i++)
            {
                UI_Inventory.transform.GetChild(N + 1).transform.GetChild(i - N).gameObject.SetActive(false);
            }
        }

        void Set_Slot(int i)
        {
            if (i < 6)
            {
                if (!Inventory[i].empty()) Equip_Menu.AddItemInQuickSlot(Inventory[i].item.index, i);
                else Equip_Menu.EraseItemQuickSlot(i);
            }

            // nếu nhỏ hơn 30 lấy slot ở ngoài lớn hơn thì lấy slot ở trong UI_Chest
            var Obj = (i < N) ? UI_Inventory.transform.GetChild(i + 1) : UI_Chest.transform.GetChild(i - N);

            // image là null
            Obj.gameObject.transform.GetChild(0).GetComponent<Image>().sprite = item_image.transform.GetChild(Mathf.Max(Inventory[i].item.index, 0)).GetComponent<Image>().sprite;

            // không hiển thị ảnh
            Obj.gameObject.transform.GetChild(0).GetComponent<Image>().enabled = !Inventory[i].empty();

            // không hiển thị số
            Obj.gameObject.transform.GetChild(1).GetComponent<Text>().text = Inventory[i].number > 0 ? Inventory[i].number.ToString() : "";
        }

        bool canHand(Item I)
        {
            return 0 < (int)I.type && (int)I.type <= 5;
        }

        public void ChangeSlot(GameObject A, GameObject Z)
        {
            int ai = new(), zi = new();

            //cái này để kiếm tra coi có cùng trong túi đồ không hay ở túi khác
            if (A.transform.parent.name.Contains("UI_Inventory") || A.transform.parent.name.Contains("UI_Chest"))
            {
                ai = A.GetComponent<SlotControl>().index;
            }
            else
            {
                ai = A.transform.parent.GetComponent<SlotControl>().index;
            }

            if (Z.name.Contains("UI_Inventory"))
            {
                /*Inventory[ai] = new SlotItem();
                Debug.Log("Đã vứt ra ngoài");*/

                Debug.Log("Không hợp lệ");
                Set_Slot(ai);
                return;
            }

            if (Z.transform.parent.name.Contains("UI_Inventory") || Z.transform.parent.name.Contains("UI_Chest"))
            {
                zi = Z.GetComponent<SlotControl>().index;
            }
            else
            {
                zi = Z.transform.parent.GetComponent<SlotControl>().index;
            }

            // nếu giống nhau thì chỉ active lại slot bấm vào
            if (A == Z)
            {
                Set_Slot(ai);
                return;
            }

            //nếu là vứt ra ngoài



            if (Inventory[ai].item.index != Inventory[zi].item.index)
            {
                // phải cầm được thì mới cho vào ô cầm được
                if (zi > 6 || canHand(Inventory[ai].item))
                {
                    var T = Inventory[ai];
                    Inventory[ai] = Inventory[zi];
                    Inventory[zi] = T;
                }
            }
            else
            {
                Inventory[zi].Add(Inventory[ai].item, ref Inventory[ai].number);
            }

            Debug.Log(ai + "   " + zi);
            Set_Slot(ai);
            Set_Slot(zi);
        }

        // cộng dồn vào nếu đã tồn tại
        public void AddItem(Item I, ref int n)
        {
            // nếu là Item null thì không thêm vào
            if (I.index == 0)
            {
                n = 0;
                Debug.LogWarning("Không nhận Item có idex = 0");
                return;
            }

            // thử xem coi Item này đã tồn tại chưa và cộng dồn vào
            for (int i = 0; i < N; i++)
            {
                //Debug.Log(Inventory[i].item.index);
                if (Inventory[i].item.index == I.index)
                {
                    // nếu có thay đổi thì chỉnh lại số trong ô ở UI
                    if (Inventory[i].Add(I, ref n))
                    {          
                        Set_Slot(i);
                    }

                    // cộng dồn hết thì thoát không thì chạy ở dưới
                    if (n == 0) return;
                }
            }
            // nếu cộng dồn vẫn chưa hết Item thì cho Item vào ô mới 
            for (int i = 0; i < N; i++)
            {
                if (Inventory[i].empty())
                {
                    if (canHand(I) || i >= 6)
                    {
                        if (Inventory[i].Add(I, ref n))
                        {
                            Set_Slot(i);
                        }
                        if (i < 6) Equip_Menu.AddItemInQuickSlot(I.index, i);
                    }
                    if (n == 0) return;
                }
            }
            if (n > 0)
            {
                Debug.Log("Full Inventory");
                return;
            }

        }

    }
}