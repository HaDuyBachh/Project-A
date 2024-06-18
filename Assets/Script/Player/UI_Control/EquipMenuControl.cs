using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace HaDuyBach
{
    public class EquipMenuControl : MonoBehaviour
    {
        public GameObject Quick_Equip_Menu;
        private GameObject[] Slot;
        private GameObject[] Image_Slot;
        private GameObject item_image;
        private UIControllerInput UI_Control;
        private Inventory_Control Inv;

        //phải đặt là Awake, lí do thì xem phần Start ở Inventory_Control
        public void Awake()
        {
            //0 là thứ tự của Image trong UI tổng
            item_image = transform.GetChild(0).gameObject;

            Inv = gameObject.GetComponent<Inventory_Control>();

            UI_Control = this.gameObject.GetComponentInParent<UIControllerInput>();

            //gán các gameobject vào các slot theo thứ tự
            Slot = new GameObject[6];
            for (int i = 0; i < 6; i++)
            {
                Slot[i] = Quick_Equip_Menu.transform.GetChild(i + 1).gameObject;
                //Debug.Log(Slot[i].gameObject.name);
            }

            Image_Slot = new GameObject[6];
            for (int i = 0; i < 6; i++)
                Image_Slot[i] = Slot[i].transform.GetChild(0).gameObject;
        }

        bool canHand(Item I)
        {
            return 0 < (int)I.type && (int)I.type <= 5;
        }

        public void ChangeEquipNow(int slot)
        {
            // nếu có thể cầm thì Active Equip thành 
            if (canHand(Inv.Inventory[slot].item)) UI_Control.ActiveEquip(Inv.Inventory[slot].item);
            else
                //Nếu không phải vật phẩm có thể cầm thì ta sẽ không Active
                UI_Control.ActiveEquip(new());
        }

        public void EraseItemQuickSlot(int i)
        {
            Image_Slot[i].GetComponent<Image>().enabled = false;
            ChangeEquipNow(i);
        }

        public void AddItemInQuickSlot(int index, int i)
        {
            Image_Slot[i].GetComponent<Image>().enabled = true;
            Image_Slot[i].GetComponent<Image>().sprite = item_image.transform.GetChild(index).gameObject.GetComponent<Image>().sprite;
        }

        public void Equip_Menu_Active()
        {
            Quick_Equip_Menu.SetActive(true);
        }
        public void Equip_Menu_UnActive()
        {
            Quick_Equip_Menu.SetActive(false);
        }
        public void Click_Slot_None()
        {
            UI_Control.ActiveEquip(new());
        }    
        public void Click_Slot_0()
        {
            ChangeEquipNow(0);
        }

        public void Click_Slot_1()
        {
            ChangeEquipNow(1);
        }
        public void Click_Slot_2()
        {
            ChangeEquipNow(2);
        }
        public void Click_Slot_3()
        {
            ChangeEquipNow(3);
        }
        public void Click_Slot_4()
        {
            ChangeEquipNow(4);
        }
        public void Click_Slot_5()
        {
            ChangeEquipNow(5);
        }
    }
}
