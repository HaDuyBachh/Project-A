using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace HaDuyBach
{

    public class StatusItemMenu : MonoBehaviour, IPointerDownHandler
    {
        private Inventory_Control Inv;

        private void Awake()
        {
            Inv = gameObject.transform.parent.transform.parent.GetComponent<Inventory_Control>();
        }

        public void OnPointerDown(PointerEventData eventData)
        {

        }

        public void showInfo()
        {

        }
    }
}