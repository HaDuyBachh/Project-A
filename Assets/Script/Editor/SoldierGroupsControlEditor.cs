using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace HaDuyBach
{
    [CustomEditor(typeof(SoldierGroupsControl))]
    public class SoldierGroupsControlEditor : Editor
    {
        private Ray SceneViewPointToWorld;
        public override void OnInspectorGUI()
        {

            DrawDefaultInspector();
            SoldierGroupsControl soldierGroups = (SoldierGroupsControl)target;
            GUILayout.BeginHorizontal();
            // tạo group mới ở giữa tầm nhìn của người chơi
            if (GUILayout.Button("Create Group Local", GUILayout.Height(30)))
            {
                RaycastHit Hit;
                if (Physics.Raycast(SceneViewPointToWorld, out Hit))
                {
                    soldierGroups.CreateNewGroupLocal(Hit.point);
                    SceneView.RepaintAll();
                    EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
                }
                else
                {
                    Debug.LogError("Không tìm thấy mặt phẳng để tạo group");
                }

            }
            if (GUILayout.Button("Relist Groups", GUILayout.Height(30)))
            {
                soldierGroups.ReListGroups();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            GUILayout.EndHorizontal();
        }

        public void OnSceneGUI()
        {
            SoldierGroupsControl soldierGroups = (SoldierGroupsControl)target;

            //kiểm tra tính thống nhất về số lượng
            if (soldierGroups.groups.Count != soldierGroups.groups.Count)
            {
                soldierGroups.ReListGroups();
            }

            //Lấy tia nhìn từ camera scene view ở giữa màn hình
            SceneViewPointToWorld = SceneView.lastActiveSceneView.camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));

            if (soldierGroups.transform.childCount != soldierGroups.groups.Count) soldierGroups.ReListGroups();

            //Lựa chọn các nhóm khi đang ở trong group lớn
            foreach(var gr in soldierGroups.groups)
            {
                var grd = gr.GetComponent<SoldierGroup>();
                Handles.color = Color.magenta;
                if (Handles.Button(grd.transform.position, Quaternion.Euler(90, 0, 0), grd.Large,grd.Large,Handles.CircleHandleCap))
                {
                    Selection.activeObject = gr.gameObject;
                }    
            }    
        }
    }
}
