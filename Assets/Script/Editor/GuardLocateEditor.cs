using UnityEngine;
using UnityEditor;

namespace HaDuyBach
{
    [CustomEditor(typeof(GuardLocateScript))]
    public class GuardLocateEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            var p = (GuardLocateScript)target;
            if (GUILayout.Button("Delete", GUILayout.Height(32)))
            {
                p.Group.DelGuard(p.id);
                DestroyImmediate(p.gameObject);
                Selection.activeObject = p.Group.gameObject;
                SceneView.RepaintAll();
            }
        }    
        private void OnSceneGUI()
        {
            var p = (GuardLocateScript)target;
            if (p != null) 
                p.Group.GuardLocate[p.id] = p.transform.position;
            else 
                return;

            //Trở về group chính và xóa gameOject hiện tại
            Handles.color = Color.green;
            if (Handles.Button(p.transform.position, Quaternion.identity, 0.4f, 0.4f, Handles.SphereHandleCap))
            {
                Selection.activeObject = p.Group.gameObject;
                DestroyImmediate(p.gameObject);
                return;
            }

            //In ra các Point khác và lựa chọn giữa những Point đó
            Handles.color = Color.blue;
            for (int i=0; i<p.Group.GuardLocate.Count; i++)
            {
                if (i!=p.id)
                {
                    //lựa chọn các điểm khác nếu chuột giữa đang không sử dụng (chuột giữa là để kéo, tránh việc đang kéo bị bấm nhầm)
                    if (Event.current.button != 2 && Handles.Button(p.Group.GuardLocate[i],Quaternion.identity,0.4f,0.4f,Handles.SphereHandleCap))
                    {
                        var p_next = new GameObject();
                        p_next.AddComponent<GuardLocateScript>().New(p.Group, i, p.Group.GuardLocate[i]);
                        Selection.activeObject = p_next;
                    }    
                }    
            }       

            if (Selection.activeObject != p.gameObject )
            {
                DestroyImmediate(p.gameObject);
                return;
            }    
        }
    }

}
