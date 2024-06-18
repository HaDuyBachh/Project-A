using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;

namespace HaDuyBach
{
    [CustomEditor(typeof(SoldierGroup))]
    public class SoldierGroupEditor : Editor
    {

        Ray SceneViewPointToWorld;
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
            SoldierGroup Group = (SoldierGroup)target;

            #region Tạo & chỉnh sửa vị trí, số lượng, trang bị lính

            // nếu muốn tạo khoảng trống linh hoạt có thể sử dụng GUILayout.FlexibleSpace();
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("ReCreate Group", GUILayout.Height(30)))
            {
                Group.Creating();
                Group.Hide();
                SceneView.RepaintAll();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            if (GUILayout.Button("Reload", GUILayout.Height(30)))
            {
                Group.Reload();
                EditorSceneManager.MarkSceneDirty(EditorSceneManager.GetActiveScene());
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Show Soldier", GUILayout.Height(32)))
            {
                Group.Show();
            }
            if (GUILayout.Button("Hide Soldier", GUILayout.Height(32)))
            {
                Group.Hide();
            }
            GUILayout.EndHorizontal();

            GUILayout.Space(10);

            #endregion

            if (Group.HaveGuardLocate)
            {
                #region Tạo & chỉnh sửa điểm canh gác

                GUILayout.BeginHorizontal();
                //nếu không edit
                if (!Group.EditGruardLocate)
                {
                    if (GUILayout.Button("Edit Gruard Locate", GUILayout.Height(32)))
                    {
                        Group.EditGruardLocate = true;
                        SceneView.RepaintAll();
                    }
                }
                //nếu có edit
                else
                {
                    if (GUILayout.Button("Save & Exit Edit", GUILayout.Height(32)))
                    {
                        Group.EditGruardLocate = false;
                        SceneView.RepaintAll();
                    }
                    if (GUILayout.Button("Add", GUILayout.Height(32)))
                    {
                        RaycastHit Hit;
                        if (Physics.Raycast(SceneViewPointToWorld, out Hit))
                        {
                            Group.AddGuard(Hit.point);
                            SceneView.RepaintAll();
                        }
                    }
                }
                GUILayout.EndHorizontal();

                #endregion
            }
        }
        public void OnSceneGUI()
        {
            #region Require - Những lệnh bắt buộc thực hiện

            SoldierGroup Group = (SoldierGroup)target;

            //Lấy tia nhìn từ camera scene view ở giữa màn hình
            SceneViewPointToWorld = SceneView.lastActiveSceneView.camera.ScreenPointToRay(new Vector3(Screen.width / 2f, Screen.height / 2f, 0));

            // Vẽ vòng màu tím xung quanh
            Handles.color = Color.magenta;
            Handles.CircleHandleCap(0, Group.transform.position, Group.transform.rotation * Quaternion.LookRotation(Vector3.up),
                Group.Large, EventType.Repaint);

            #endregion

            #region Chỉ thực hiện việc edit các điểm canh gác

            if (Group.EditGruardLocate)
            {
                for (int i = 0; i < Group.GuardLocate.Count; i++)
                {
                    Handles.color = Color.blue;
                    if (Handles.Button(Group.GuardLocate[i], Quaternion.identity, 0.4f, 0.4f, Handles.SphereHandleCap))
                    {
                        var p = new GameObject();
                        Selection.activeObject = p;
                        p.AddComponent<GuardLocateScript>().New(Group, i, Group.GuardLocate[i]);
                        return;
                    }
                }
                return;
            }

            #endregion

            #region Thực hiện việc trở về nhóm lớn khi bấm vào


            Handles.color = Color.magenta;
            //Từ nhóm nhỏ trở về nhóm lớn nếu đang không bấm chuột giữa
            if (Event.current.button != 2 && Handles.Button(Group.transform.position, Group.transform.rotation * Quaternion.LookRotation(Vector3.up),
               Group.Large, Group.Large, Handles.CircleHandleCap))
            {
                Selection.activeObject = Group.transform.parent.gameObject;
            }

            #endregion

        }
    }
}
