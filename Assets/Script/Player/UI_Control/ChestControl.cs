using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class ChestControl : MonoBehaviour
    {
        public GameObject Open_Chest_Button;
        // gameoject Tạm để lưu object chest
        private Chest_Inv T;
        private Inventory_Control Inv;
        private bool haveChest = false;

        private void Awake()
        {
            Inv = GetComponent<Inventory_Control>();
        }
        public void activeButton(Chest_Inv I)
        {
            T = I;
            haveChest = true;
            Open_Chest_Button.SetActive(true);
        }

        public void inactiveButton()
        {
            Open_Chest_Button.SetActive(false);
        }

        public void Inventory_InitChestSlot()
        {
            Inv.InitChestSlot(T);
        }

        public bool HaveChest()
        {
            return haveChest;
        }

        public void ChangeItemInInventory()
        {
            for (int i = 0; i < T.NumOfSlot; i++)
                T.SetSlot(i, Inv.Inventory[i + Inventory_Control.N]);
            haveChest = false;
        }
    }

}
