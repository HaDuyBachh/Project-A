using UnityEngine;

namespace HaDuyBach
{
    public class Chest_Inv : MonoBehaviour
    {
        private SlotItem[] SlotChest;
        public int NumOfSlot;
        public string Name;

        private void Awake()
        {
            Debug.Log(NumOfSlot);
            SlotChest = new SlotItem[NumOfSlot];
            for (int i = 0; i < NumOfSlot; i++)
            {
                SlotChest[i] = new SlotItem();
            }
            //hoặc lấy dữ liệu ở Database...
        }

        public SlotItem GetSlot(int index)
        {
            return SlotChest[index];
        }

        public void SetSlot(int index, SlotItem I)
        {
            SlotChest[index] = I;
        }

        private void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                //lấy UI_Canvas_StarterAssetsInputs_TouchZones
                collision.transform.parent
                       .transform.GetChild(collision.transform.parent.childCount - 1)
                          .GetComponent<ChestControl>().activeButton(gameObject.GetComponent<Chest_Inv>());
            }
        }

        private void OnTriggerExit(Collider collision)
        {
            if (collision.gameObject.CompareTag("Player"))
            {
                //lấy UI_Canvas_StarterAssetsInputs_TouchZones
                collision.transform.parent
                       .transform.GetChild(collision.transform.parent.childCount - 1)
                          .GetComponent<ChestControl>().inactiveButton();
            }
        }
    }

}
