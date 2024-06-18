using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class Open_Car_UI : MonoBehaviour
    {
        public Transform CamTrans;
        private UI_Car_Control UI;
        private void Awake()
        {
            UI = FindObjectOfType<UI_Car_Control>();
        }

        private void OnTriggerEnter(Collider col)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                UI.GetComponent<UI_Car_Control>().invoke_veh(this.gameObject);
            }
        }

        public Transform getCameraRoot()
        {
            return CamTrans.transform;
        }

        private void OnTriggerExit(Collider col)
        {
            if (col.gameObject.CompareTag("Player"))
            {
                UI.GetComponent<UI_Car_Control>().invoke_veh(null);
            }
        }

    }
}