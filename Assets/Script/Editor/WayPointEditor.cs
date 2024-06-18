using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
[CustomEditor(typeof(WayPoint))]
public class WayPointEditor : Editor
{
    Vector3[] directions = new Vector3[4] { Vector3.forward, Vector3.right, -Vector3.forward, Vector3.left };
    private void OnSceneGUI()
    {
        var thisP = (WayPoint)target;
        var mul = thisP.GetComponentInParent<MultiPathScript>();
        var n = mul.transform.childCount;

        // khi bấm vào sẽ trở về Multi Path tổng
        Handles.color = Color.clear;
        if (Handles.Button(thisP.transform.position, Quaternion.identity, mul.pointScale + 1f, mul.pointScale + 1f, Handles.SphereHandleCap) == true)
        {
            Selection.activeObject = thisP.transform.parent;
        }
        for (int i = 0; i < n; i++)
        {
            var waypointTras = mul.transform.GetChild(i);
            if (waypointTras == thisP.transform) continue;
            if (Handles.Button(waypointTras.position, Quaternion.identity, mul.pointScale + 1f, mul.pointScale + 1f, Handles.SphereHandleCap) == true)
            {
                Selection.activeObject = mul.transform.GetChild(i);
            }
        }

        // tìm trong các waypoint trong danh sách, nếu có điểm thỏa mãn sẽ nối vào   
        for (int i=0; i<n; i++)
        {
            var p = mul.transform.GetChild(i).GetComponent<WayPoint>();
            if ( p.transform == thisP.transform) continue;

            bool kt = false;
            
            if (thisP.Next == null) break;


            // kiểm tra xem cạnh đang xét có trùng với cạnh tiếp theo của điểm hiện tại hay không
            for (int d = 0; d < 4; d++)
            {
                if (thisP.Next[d] == p.gameObject)
                {
                    kt = true;
                    break;
                }
            }
            if (kt) continue;


            //sẽ hiển thị việc nối khi hợp lệ là màu đỏ
            if ((p.transform.position - thisP.transform.position).magnitude <= mul.distance*9/10f)
            {
                for (int d = 0; d<4; d++)
                {
                    if (SameSide(p.transform,directions[d],thisP.transform,directions[(d+2)%4]) && p.Next[d] == null && thisP.Next[(d + 2) % 4] == null)
                    {
                        // kiểm tra hướng chuyển
                        /*Handles.color = Color.red;
                        Handles.DrawLine(p.transform.position,p.transform.position + directions[d] * 8f);
                        Handles.DrawLine(thisP.transform.position,thisP.transform.position + directions[(d + 2) % 4]*8f);*/
                        if (Mathf.Abs(Vector3.Angle(p.transform.position - (p.transform.position+directions[d]),p.transform.position-thisP.transform.position))<=45.0f)
                        {
                            Handles.color = Color.red;
                            Handles.DrawLine(p.transform.position, thisP.transform.position);
                            var local = thisP.transform.position + directions[(d + 2) % 4] * (mul.pointScale + 2f);
                            if (Handles.Button(local, Quaternion.Euler(90, 0, 0), Mathf.Min(1.5f, mul.pointScale), Mathf.Min(1.5f, mul.pointScale), Handles.RectangleHandleCap) == true)
                            {
                                thisP.Add((d + 2) % 4, p.gameObject);
                                p.Add(d, thisP.gameObject);
                                break;
                            }    
                        }    
                    }
                }
                break;
            }
        }
    }

    private bool SameSide(Transform a,Vector3 ad, Transform b, Vector3 bd)
    {
        return ((a.position + ad * 0.001f) - (b.position + bd * 0.001f)).magnitude < ((a.position + bd * 0.001f) - (b.position + ad * 0.001f)).magnitude;
    }    
}
