using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    /// <summary>
    /// Điều khiển toàn bộ các nhóm hiện tại có mặt trên bản đồ, khi người chơi đi khỏi khu vực sẽ luân chuyển các đối tượng cho nhau <br></br>
    /// Giới hạn số linh có mặt trên toàn bộ bản đồ là 10 lính 
    /// </summary>
    public class SoldierGroupsControl : MonoBehaviour
    {
        /// <summary>
        /// Giới hạn số lượng lính có thể xuất hiện
        /// </summary>
        public const int SoldierLimit = 12;

        #region Visual Modify
        [Header("Soldier Template")]
        public GameObject Soldier;
        [Header("Organization ID")]
        public int OrgID;
        [Header("Option")]
        [Tooltip("Hành động của Lính đối với người chơi \n " +
                 "- Friendly: Thái độ thân thiện không tấn công lại khi bị tấn công\n" +
                 "- Normal: Thái độ bình thường, chỉ tấn công lại khi bị tấn công\n" +
                 "- Danger: Thái độ thù địch, chỉ cần gặp là tấn công")]
        [SerializeField]
        SoldierControl.Behavr _behavior = SoldierControl.Behavr.Normal;

        [Tooltip("Số lượng lính trong lần spawn tiếp theo \n  (một nhóm canh gác có tối thiểu 1 lính và tối đa 5 lính)")]
        [Range(1, 5, order = 3)]
        public int NumberSoldier;
        [Tooltip("Độ lớn của khu vực kiểm soát trong lần spawn tiếp theo")]
        [Range(2.0f, 20.0f)]
        public float LocalLarge = 3.0f;
        [Tooltip("Danh sách các Soldier Group")]
        public List<GameObject> groups = new();
        /// <summary>
        /// Lưu trữ các đơn vị lính để phục vụ cho việc tối ưu game
        /// </summary>
        [SerializeField]
        public int SoldierStorageCount;
        public GameObject[] SoldierStorage = new GameObject[SoldierLimit];
        #endregion

        #region General
        private ThirdPersonController _player;
        #endregion

        #region Getter & Setter
        public ThirdPersonController Player { get { return _player; } }
        #endregion

        #region Tool
        public void CreateNewGroupLocal(Vector3 p)
        {
            var group = new GameObject();
            group.AddComponent<SoldierGroup>();
            group.GetComponent<SoldierGroup>().SoldierGroupCreate(NumberSoldier, LocalLarge, Soldier, _behavior, this);
            group.transform.SetParent(transform);
            group.transform.position = p;
            groups.Add(group);


            ReListGroups();
        }
        public void ReListGroups()
        {
            groups.Clear();
            for (int i = 0; i < transform.childCount; i++)
            {
                groups.Add(transform.GetChild(i).gameObject);
                groups[i].name = "group" + "_" + i;
            }
        }
        public void ShowSoldier()
        {

        }
        public void HideSoldier()
        {

        }
        public bool SoldierSpawn()
        {
            if (SoldierStorageCount > SoldierLimit) return false;

            var s = Instantiate<GameObject>(Soldier);
            s.transform.name = "Soldier_" + OrgID;
            AddLastSoldier(s);
            return true;
        }

        #endregion

        #region In Game Function
        public void Awake()
        {
            _player = FindObjectOfType<ThirdPersonController>();
        }
        public void AddLastSoldier(GameObject g)
        {
            if (SoldierStorageCount > SoldierLimit) throw new System.Exception("Không chỗ để thêm vào trong SoldierStorage");
            SoldierStorage[SoldierStorageCount++] = g;
        }
        public GameObject PopLastSoldier()
        {
            if (SoldierStorageCount == 0) throw new System.Exception("Không còn phần tử để lấy ra trong SoldierStorage");
            var s = GetLastSoldier();
            SoldierStorage[SoldierStorageCount--] = null;
            return s;
        }
        public GameObject GetLastSoldier()
        {
            if (SoldierStorageCount == 0) throw new System.Exception("Không còn phần tử để lấy ra trong SoldierStorage");
            return SoldierStorage[SoldierStorageCount - 1];
        }

        #endregion
    }
}