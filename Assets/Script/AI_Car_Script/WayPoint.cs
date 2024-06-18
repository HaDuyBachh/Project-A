using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WayPoint : MonoBehaviour
{
    public GameObject[] Next;
    public Vector3[] local;
    public uint CarIn;
    public uint CarOut;

    /// <summary>
    /// Hàm tạo một waypoint mới
    /// </summary>
    /// <param name="pos">sử dụng để lấy vị trí của waypoint mới cần thêm vào</param>
    /// <param name="Parent">sử dụng để xác định object chung chứa các waypoint</param>
    /// <returns></returns>
    public static void NewWayPoint(Vector3 pos,Transform Parent,out WayPoint p)
    {
        GameObject g = new GameObject();
        g.transform.SetParent(Parent);
        g.transform.SetAsLastSibling();
        g.transform.position = pos;
        g.name = "Waypoint_" + (g.transform.parent.childCount - 1);
        g.AddComponent<WayPoint>();
        g.GetComponent<WayPoint>().Next = new GameObject[4];
        g.GetComponent<WayPoint>().local = new Vector3[4];
        p = g.GetComponent<WayPoint>();
        p.CarIn = 0;
        p.CarOut = 0;
    }
    public static void NewWayPoint(Vector3 pos, Transform Parent)
    {
        GameObject g = new GameObject();
        g.transform.SetParent(Parent);
        g.transform.SetAsLastSibling();
        g.transform.position = pos;
        g.name = "Waypoint_" + (g.transform.parent.childCount - 1);
        g.AddComponent<WayPoint>();
        var p = g.GetComponent<WayPoint>();
        p.Next = new GameObject[4];
        p.local = new Vector3[4];
        p.CarIn = 0;
        p.CarOut = 0;
    }

    /// <summary>
    /// Thêm 1 WayPoint mới từ WayPoint hiện tại theo hướng cho trước
    /// </summary>
    /// <param name="direction">lấy hướng cần thêm vào, hướng sẽ đi theo chiều kim đồng hồ 0: trước, 1: phải, 2: sau, 3: trái.</param>
    /// <param name="point">Transform của WayPoint được thêm vào </param>
    /// <returns></returns>
    public void Add(int direction, GameObject point)
    {
        Next[direction] = point;
    }  
    public int getNumOfNext()
    {
        return ((Next[0] == null) ? 0 : 1) + ((Next[1] == null) ? 0 : 1) + ((Next[2] == null) ? 0 : 1) + ((Next[3] == null) ? 0 : 1);
    }    

    public void popMyOrder()
    {
        CarOut++;
        if (CarOut >= CarIn)
        {
            CarOut = CarIn = 0;
        } 
            
    }

    public uint getMyOrder()
    {
        CarIn++;
        return CarIn;
    }    

    public bool isMyOrder(uint k)
    {
        return (k == CarOut + 1);
    }

    public int CarRemain()
    {
        return (int)Mathf.Max(CarIn - CarOut,0f);
    }    
}
