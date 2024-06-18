using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace HaDuyBach
{
    public class SoldierGroup : MonoBehaviour
    {
        #region General
        enum SpaceState
        {
            SmallDistance,
            LargeDistance,
        }

        /// <summary>
        /// khi một đối tượng tấn công lính trong group sẽ được thêm vào attack queue và sẽ là mục tiêu tấn công đến khi mất dấu hoặc bị hạ gục
        /// </summary>
        private List<int> AttackQueue = new();
        private float DistanceSoldier = 0.7f;
        /// <summary>
        /// Thành phầm kiểm soát các Group con
        /// </summary>
        private SoldierGroupsControl _soldierGroups = null;
        private bool _editGruardLocate = false;
        private bool _isShowSoldier = false;
        /// <summary>
        /// Lưu tạm vị trí mà Soldier trong SoldierStorage đang được dùng tương ứng với các lính theo thứ tự
        /// </summary>
        private Stack<GameObject> _tempSoldier = new();
        #endregion

        #region Visual Modify

        [Header("Soldier Template")]
        public GameObject SoldierTemp;
        [Header("Function")]
        [Tooltip("Có các điểm canh gác hay không")]
        public bool HaveGuardLocate = false;

        [Space(10)]
        [Header("Customize Group")]
        [Tooltip("Các loại hành vi của lính")]
        public SoldierControl.Behavr Behavior = SoldierControl.Behavr.Normal;
        [Tooltip("Dùng xác định khoảng cách của các lính khi đứng trong vòng\n")]
        [SerializeField]
        SpaceState DistanceBetweenSoldier = SpaceState.LargeDistance;
        [Tooltip("Số lượng lính trong lần spawn tiếp theo \n  (một nhóm canh gác có tối thiểu 1 lính và tối đa 5 lính)")]
        [Range(1, 5, order = 3)]
        public int SoldierCount;
        [Tooltip("Độ lớn của khu vực Group kiểm soát")]
        [Range(2.0f, 20.0f)]
        public float Large;
        [Tooltip("Thời gian ReSpawn khi lính chết chết")]
        public float RespawmTime = 5.0f;
        [Tooltip("Số lượng lính còn lái")]
        public int RemainSoldier = 0;

        [Space(10)]
        [Header("Custom Soldier Slot")]
        [Tooltip("Độ dài và rộng của một ô chứa lính. Default = 0.5f")]
        public float Edge = 0.5f;
        [Tooltip("Độ cao của ô chứa lính. Default = 1.5f")]
        public float Height = 1.5f;
        [Tooltip("Danh sách đứng ban đầu của lính")]
        public List<Vector3> GuardOriginLocate = new();
        [Tooltip("Trang bị của lính")]
        public List<int> SoldierEquip = new();
        [Tooltip("Danh sách các điểm canh gác của lính")]
        public List<Vector3> GuardLocate = new();
        [Tooltip("Tương ứng với GuardLocate là các ID của lính đang ở vị trí đó. Không có thì là -1")]
        public List<int> GuardLocateID = new();

        [Space(10)]
        [Header("Điều chỉnh khoảng cách xuất hiện")]
        [Tooltip("Khoảng cách để ẩn ngay lập tức")]
        public float Distance_Hide_Imm = 70;

        #endregion

        #region Setter & getter
        /// <summary>
        /// Sử dụng cho TOOL: <br></br>
        /// Kiểm tra xem còn trong edit hay không
        /// </summary>
        public bool EditGruardLocate { get { return _editGruardLocate; } set { _editGruardLocate = value; } }
        /// <summary>
        /// Các lính được phân bổ tạm thời cho Group này
        /// </summary>
        public Stack<GameObject> TempSoldier { get { return _tempSoldier; } }
        #endregion

        #region Tool
        public void SoldierGroupCreate(int Number, float Large, GameObject SoldierTemp, SoldierControl.Behavr Behavior, SoldierGroupsControl Gs)
        {
            this.SoldierCount = Number;
            this.Large = Large;
            this.SoldierTemp = SoldierTemp;
            this.Behavior = Behavior;
            this._soldierGroups = Gs;

            Creating();
        }
        public void Hide()
        {
            _isShowSoldier = false;

            if (_soldierGroups == null) _soldierGroups = transform.parent.GetComponent<SoldierGroupsControl>();

            if (_tempSoldier.Count == 0) return;

            for (int i = 0; i < SoldierCount; i++)
            {
                //Hủy vai trò
                _tempSoldier.Peek().GetComponent<SoldierControl>().RejectRole();

                //Tra về lại cho Soldier Storage
                _soldierGroups.AddLastSoldier(_tempSoldier.Pop());
            }
        }
        public void Show()
        {
            _isShowSoldier = true;

            if (_soldierGroups == null) _soldierGroups = transform.parent.GetComponent<SoldierGroupsControl>();

            RemainSoldier = 0;

            for (int i = 0; i < SoldierCount; i++)
            {
                //Trang bị -1 nghĩa là lính đã chết
                if (SoldierEquip[i] < 0) continue;

                RemainSoldier++;

                //Nếu đã hết lính mà không spawn thêm được thì thoát ra
                if (_soldierGroups.SoldierStorageCount == 0 && !_soldierGroups.SoldierSpawn()) return;

                //Thêm lính vào khay nhớ tạm và pop lính được lưu trữ trong Group tổng ra
                _tempSoldier.Push(_soldierGroups.GetLastSoldier());
                _soldierGroups.PopLastSoldier();

                //Khởi tạo chỉ số cho lính
                _tempSoldier.Peek().GetComponent<SoldierControl>()
                    .AssignRole(GuardOriginLocate[i], _soldierGroups.OrgID, Behavior, SoldierEquip[i], this, i);
                _tempSoldier.Peek().name = "Soldier " + i;
                _tempSoldier.Peek().SetActive(true);
            }
        }
        public void Creating()
        {
            //Tạo khoảng cách
            switch (DistanceBetweenSoldier)
            {
                case SpaceState.SmallDistance: DistanceSoldier = Mathf.Sqrt(2 * Edge) + Random.Range(-0.2f, 0.2f); break;
                case SpaceState.LargeDistance: DistanceSoldier = Large / 2; break;
            }

            //Tạo mới danh sách các vị trí đứng
            GuardOriginLocate.Clear();
            SoldierEquip.Clear();

            for (int i = 0; i < SoldierCount; i++)
            {
                //Xét tính time out của đoạn mã
                int T = 1000000;
                for (; T > 0; T--)
                {
                    var kt = false;
                    var p = Random.insideUnitCircle * Random.Range(1.0f, Large + 0.1f);
                    foreach (var s in GuardOriginLocate)
                    {
                        kt = (s - new Vector3(p.x, 0, p.y)).magnitude <= DistanceSoldier;
                        if (kt) break;
                    }
                    // Tạo Soldier nếu chưa có Soldier bị trùng vị trí
                    if (!kt)
                    {
                        GuardOriginLocate.Add(new Vector3(p.x, 0.0f, p.y));

                        // Xét tính cân bằng trang bị sau.
                        SoldierEquip.Add(Random.Range(0, 4));

                        break;
                    }
                }

                if (GuardOriginLocate.Count != i + 1)
                {
                    Debug.LogWarning("Không tạo được vị trí soldier ");
                    return;
                }

            }

            Debug.Log("Đã create xong vị trí Soldier");
        }
        public void Reload()
        {
            Hide();
            Show();
        }

        /// <summary>
        /// Thêm điểm canh gác
        /// </summary>
        public void AddGuard(Vector3 Pos)
        {
            GuardLocate.Add(Pos);
            GuardLocateID.Add(-1);
        }

        /// <summary>
        /// Xóa điểm canh gác
        /// </summary>
        public void DelGuard(int index)
        {
            GuardLocate.RemoveAt(index);
            GuardLocateID.RemoveAt(index);
        }

        public void OnDrawGizmos()
        {
            Gizmos.color = Color.green;
            //xoay gizmos theo vật được vẽ chính
            Gizmos.matrix = this.transform.localToWorldMatrix;
            foreach (var s in GuardOriginLocate)
            {
                Gizmos.DrawWireCube(s + new Vector3(0, Height / 2 + 0.05f, 0), new Vector3(Edge, Height, Edge));
            }
        }

        #endregion

        public void Start()
        {
            Hide();
            RemainSoldier = SoldierCount;
        }
        public void Update()
        {
            if (RemainSoldier == 0 && RespawmTime > 0.0f)
            {
                //if (_isShowSoldier && RespawmTime <= 3.0f) Hide();

                Debug.Log("Running");
                RespawmTime -= Time.deltaTime;
                if (RespawmTime <= 0.0f)
                {
                    for (int i = 0; i < SoldierCount; i++)
                    {
                        SoldierEquip[i] = Random.Range(0, 3);
                    }
                    RemainSoldier = SoldierCount;
                    RespawmTime = 5.0f;
                }
            }
            else
            if (_isShowSoldier)
                CheckForHide();
            else
                CheckForShow();
        }

        #region In Game Function

        public void EliminateSoldier(int id)
        {
            SoldierEquip[id] = -1;
            RemainSoldier--;
        }

        private void CheckForHide()
        {
            //Nếu khoảng cách người chơi nhỏ hơn thì không cần phải kiểm tra để ẩn đi
            var dis = (_soldierGroups.Player.transform.position - transform.position).magnitude;

            if (dis < Distance_Hide_Imm) return;

            Hide();
        }

        private void CheckForShow()
        {
            //Nếu khoảng cách người chơi lớn hơn thì không cần phải trưng ra
            var dis = (_soldierGroups.Player.transform.position - transform.position).magnitude;
            if (dis > Distance_Hide_Imm) return;

            Show();
        }

        #endregion
    }
}