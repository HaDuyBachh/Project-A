using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class checkRigd : MonoBehaviour
{
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log("Va chạm với " + other.gameObject);
    }

    private void OnTriggerExit(Collider other)
    {
        Debug.Log("Thoát va chạm với " + other.gameObject);
    }
}
