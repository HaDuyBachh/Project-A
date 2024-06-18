using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

[CustomEditor(typeof(MultiPathScript))]
public class MultiPathEditorControl : Editor
{
    MultiPathScript m_Target;
    Vector3[] directions = new Vector3[4] { Vector3.forward, Vector3.right, -Vector3.forward, Vector3.left };
    private void OnSceneGUI()
    {
        m_Target = (MultiPathScript)target;

        if (m_Target.WayPointObjs != m_Target.transform.childCount ) return;

        var n = m_Target.transform.childCount;
        for (int i = 0; i < n; i++)
        {
            var p = m_Target.transform.GetChild(i).GetComponent<WayPoint>();

            ///select - thay đổi chọn Multi Path hay chọn WayPoint
            Handles.color = Color.clear;
            if (Handles.Button(p.transform.position, Quaternion.identity, m_Target.pointScale+1f, m_Target.pointScale+1f, Handles.SphereHandleCap))
            {
                Selection.activeObject = p.transform.gameObject;
            }

            // hiện các nút chọn đường đi
            Handles.color = Color.cyan;
            for (int d = 0; d < 4; d++)
            {
                if (p.Next[d] != null) continue;
                var local = p.transform.position + directions[d] * (m_Target.pointScale + 2f);

                /// tạo các đường đi
                if (Handles.Button(local, Quaternion.Euler(90,0,0) ,Mathf.Min(1.5f,m_Target.pointScale), Mathf.Min(1.5f,m_Target.pointScale), Handles.RectangleHandleCap) == true)
                {
                    WayPoint NewWayPoint;
                    WayPoint.NewWayPoint(local + directions[d] * m_Target.distance, m_Target.transform,out NewWayPoint);
                    p.Add(d, NewWayPoint.gameObject);
                    NewWayPoint.Add((d + 2) % 4, p.gameObject);
                    m_Target.WayPointObjs++;
                    EditorSceneManager.MarkSceneDirty(SceneManager.GetActiveScene());
                }
            }
        }
    }
}
