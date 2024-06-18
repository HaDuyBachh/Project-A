using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace HaDuyBach
{
    public class SoldierControl : Character
    {
        #region Thông số nhập từ bên ngoài

        [Header("Thông số nhập từ bên ngoài")]
        [Tooltip("Nhận những layer nào?")]
        public LayerMask Obstacle_Layer;
        [Tooltip("Vị Trí ô vũ khí")]
        public GameObject Weapon_Block;
        [Tooltip("Vị trí của đầu")]
        public Transform Head;
        [Tooltip("Vị trí của hông")]
        public Transform Hip;
        [Tooltip("Vị trí của cổ " +
            "\n\n Có thể dùng để xác định có bắn vào phía người chơi được hay không")]
        public Transform Neck;
        [Tooltip("Bán kính của nhân vật")]
        public float Radius = 0.25f;

        #endregion

        #region Type
        public enum Type
        {
            None,
            Melee,
            MeleeShield,
            Handgun,
            Rifle,
            Officials
        };
        [Tooltip("Chủng loại Binh lính \n" +
            "None: Lính hậu cần thông thường không có vũ khí \n " +
            "Melee: Lính cận chiến không khiên \n " +
            "MeleeShield: Lính cận chiến có khiên \n" +
            "Handgun: Lính dùng súng lục \n" +
            "Rifle: Lính dùng rifles,..\n" +
            "Officials: Lính quan chức\n")]

        [Header("Soldier Type & Behavior")]

        [SerializeField]
        private Type _soldierType = Type.None;

        /// <summary>
        /// Thể hiện các loại hành vi <br></br>
        /// Friendly: Thái độ thân thiện không tấn công lại khi bị tấn công <br></br>
        /// Normal: Thái độ bình thường, chỉ tấn công lại khi bị tấn công <br></br>
        /// Danger: Thái độ thù địch, chỉ cần gặp là tấn công <br></br>
        /// </summary>
        public enum Behavr
        {
            Friendly,
            Normal,
            Danger
        }
        [SerializeField]
        private Behavr _soldierBehavr = Behavr.Normal;

        #endregion

        #region General
        private ThirdPersonController _player;
        private Rigidbody Rb;
        private SoldierBaseState _currentState = null;
        private Animator _animator;
        private NavMeshAgent _navmeshAgent;
        private SoldierFactory _states;
        private SoldierGroup _group = null;
        private bool _isUsed = false;
        private int _id = -1;
        private bool Init = false;
        private Health_Bar _health_Bar;
        #endregion

        #region Movement
        public readonly float WalkSpeed = 1.5f;
        public readonly float RunSpeed = 5f;
        [SerializeField]
        private short _normMove = 0;
        private float _speed = 0.0f;

        private bool _crouch = false;
        /// <summary>
        /// Vị trí dừng lại tương đối của chủ thể, bằng độ dài đường kính mặc định
        /// </summary>
        public readonly float StopDis = 0.5f;
        private readonly float _meleeAttackDistance = 1.4f;
        private readonly float _handgunAttackDistance = 35.0f;
        private readonly float _gunAttackDistance = 40.0f;
        #endregion

        #region Stat
        [Range(1, 100, order = 1)]
        private float _intel = 50;
        private int _hp = 100;

        #endregion

        #region Behavior
        /// hằng số cho hành động
        public readonly float Gruard_Patrol = 3.0f;
        public readonly float Find_Patrol = 10.0f;
        public readonly float Attack_Find = 6.0f;
        /// <summary>
        /// Thời gian để đuổi theo mục tiêu sau khi đã nhìn thấy mục tiêu
        /// </summary>
        public readonly float TimeFollow = 2.5f;
        /// <summary>
        /// Khoảng cách theo dấu mục tiêu
        /// </summary>
        public readonly float FollowDistance = 15.0f;
        /// <summary>
        /// Thời gian để ghi nhận là bị tấn công
        /// </summary>
        public readonly float TimeRecordAttacked = 0.5f;

        public readonly float Cover_Timeout = 4.0f;
        public readonly float AttackTimeout = 4.0f;
        public readonly float Change_CoverPoint = 4.0f;

        /// bộ đém thời gian
        private float _rateFireTimeout = 0.0f;
        private float _rateFireTimeoutDelta = 0.0f;

        private float _gruard_Patrol_Delta = 0.0f;
        private float _find_Patrol_Delta = 0.0f;
        private float _attack_Find_Delta = 0.0f;
        private float _timeFollowRemainDelta = 0.0f;
        private float _cover_Timeout_Delta = 0.0f;
        private float _attackTimeout_Delta = 0.0f;
        private float _change_CoverPoint_Delta = 0.0f;

        private Transform _coverObject = null;

        /// hành động thể hiện
        private bool _beAttacked = false;
        private bool _suspect = false;
        private Vector3? _suspectLocate = null;
        private bool _attack = false;
        private bool _reckless = false;
        private bool _isRunningToCoverPoint = false;

        // các điểm phòng thủ & tấn công
        private Pair<Vector3, Vector3> _coverPoint = null;
        private Pair<Vector3, Vector3>[] _coverList = null;
        private int _coverCount = 0;
        private bool _isNearAttackPlace = false;

        #endregion

        #region Animation
        public enum Anim
        {
            vX,
            vZ,
            Speed,
            Grounded,
            Jump,
            Attack,
            Weapon,
            Fall,
            Crouch,
            ReadyAttack,
            Rand,
            Cover,
            Turn,
            AttackSpeed,
        }

        public Dictionary<Anim, int> AnimList = new Dictionary<Anim, int>();

        /// <summary>
        /// Sử Dụng để lấy mã hash các animation
        /// </summary>
        private void AssignAnimationIDs()
        {
            AnimList[Anim.vX] = Animator.StringToHash("Velocity x");
            AnimList[Anim.vZ] = Animator.StringToHash("Velocity z");
            AnimList[Anim.Speed] = Animator.StringToHash("Speed");
            AnimList[Anim.Grounded] = Animator.StringToHash("Grounded");
            AnimList[Anim.Jump] = Animator.StringToHash("Jump");
            AnimList[Anim.Weapon] = Animator.StringToHash("Weapon");
            AnimList[Anim.Attack] = Animator.StringToHash("Attack");
            AnimList[Anim.Fall] = Animator.StringToHash("Fall");
            AnimList[Anim.Crouch] = Animator.StringToHash("Crouch");
            AnimList[Anim.ReadyAttack] = Animator.StringToHash("ReadyAttack");
            AnimList[Anim.Rand] = Animator.StringToHash("Rand");
            AnimList[Anim.Cover] = Animator.StringToHash("Cover");
            AnimList[Anim.AttackSpeed] = Animator.StringToHash("AttackSpeed");
        }
        /// <summary>
        /// Sử dụng để đặt lại hành động đúng theo loại lính
        /// Đặt ở Awake hoặc Start vì các câu lệnh của animator sẽ chưa chạy khi bắt đầu chạy tập lệnh này
        /// </summary>
        private void SetupAnimator()
        {
            _animator = GetComponent<Animator>();

            switch (_soldierType)
            {
                case Type.None:
                case Type.Melee:
                    _animator.Play("Base Layer.No Weapon Movement.No Weapon Move", 0, 0);
                    _animator.SetInteger(AnimList[Anim.Weapon], 0);
                    Debug.Log("Đã đặt thành tay đôi"); break;
                
                    //_animator.Play("Base Layer.Melee No Shield Movement.Melee Move", 0, 0);
                    //_animator.SetInteger(AnimList[Anim.Weapon], 1);
                    //Debug.Log("Đã đặt thành cận chiến"); break;
                case Type.Handgun: 
                    _animator.Play("Base Layer.Handgun Movement.Handgun Move", 0, 0); 
                    _animator.SetInteger(AnimList[Anim.Weapon], 2);
                    Debug.Log("Đã đặt thành handgun"); break;
                case Type.Rifle:
                    _animator.Play("Base Layer.Rifle Movement.Rifle Move", 0, 0);
                    _animator.SetInteger(AnimList[Anim.Weapon], 3);
                    Debug.Log("Đã đặt thành Rifle"); break;
            }
        }

        #endregion

        #region Equip
        private int _weaponEquip = -1;
        #endregion

        #region Getter & Setter
        public SoldierBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
        public NavMeshAgent Nav { get { return _navmeshAgent; } }
        public Animator Animator { get { return _animator; } }
        public bool Crouch { get { return _crouch; } set { _crouch = value; } }
        public float Speed { get { return _speed; } set { _speed = value; } }
        public Type SoldierType { get { return _soldierType; } }
        public Behavr SoldierBehavr { get { return _soldierBehavr; } }
        /// <summary>
        /// Thời gian Delta từ lúc canh gác để lúc di chuyển đến điểm khác
        /// </summary>
        public float Gruard_Patrol_Delta { get { return _gruard_Patrol_Delta; } set { _gruard_Patrol_Delta = value; } }
        /// <summary>
        /// Thời gian Delta từ lúc tìm kiếm đến lúc ngừng và di chuyển đến chỗ canh gác khác
        /// </summary>
        public float Find_Patrol_Delta { get { return _find_Patrol_Delta; } set { _find_Patrol_Delta = value; } }
        /// <summary>
        /// Thời gian Delta từ lúc tấn công đến lúc ngừng tấn công và tìm kiếm
        /// </summary>
        public float Attack_Find_Delta { get { return _attack_Find_Delta; } set { _attack_Find_Delta = value; } }
        /// <summary>
        /// Trả về khoảng cách tấn công ( cận chiến hoặc tầm xa tùy theo vũ khí )
        /// </summary>
        public float AttackDistance
        {
            get
            {
                switch (SoldierType)
                {
                    case Type.Officials:
                    case Type.Melee:
                    case Type.MeleeShield:
                    case Type.None:
                        return _meleeAttackDistance;
                    case Type.Rifle:
                        return _gunAttackDistance;
                    case Type.Handgun:
                        return _handgunAttackDistance;
                    default: 
                        return _meleeAttackDistance ;
                }
            }
        }
        /// <summary>
        /// Thể hiện tốc độ chạy được chuẩn hóa: <br></br>
        /// 0: Đứng yên <br></br>
        /// 1: Đi bộ <br></br>
        /// 2: Chạy <br></br>
        /// </summary>
        public short NormMove { get { return _normMove; } set { _normMove = value; } }
        /// <summary>
        /// Trạng thái bị tấn công
        /// </summary>
        public bool BeAttacked { get { return _beAttacked; } }
        /// <summary>
        /// Vũ khí hiện tại của soldeir
        /// </summary>
        public int WeaponEquip { get { return _weaponEquip; } }
        /// <summary>
        /// Nhóm mà Soldier hiện tại đang đăng ký
        /// </summary>
        public SoldierGroup Group { get { return _group; } }
        /// <summary>
        /// Chỉ số của lính trong Group
        /// </summary>
        public int ID { get { return _id; } }
        public bool IsUsed { get { return _isUsed; } }
        /// <summary>
        /// Thể hiện độ nghi ngờ và cần tìm kiếm khi có ai đó thông báo bị tấn công hoặc có tiếng súng
        /// </summary>
        public bool Suspec { get { return _suspect; } set { _suspect = value; } }
        /// <summary>
        /// Địa điểm nghi ngờ cần đến kiểm tra
        /// </summary>
        public Vector3? SusLocate { get { return _suspectLocate; } set { _suspectLocate = value; } }

        /// <summary>
        /// Thể hiện là có thể tấn công hay chưa
        /// </summary>
        public bool Attack { get { return _attack; } set { _attack = value; } }
        public ThirdPersonController Player { get { return _player; } }

        /// <summary>
        /// Thời gian theo dỗi mục tiêu
        /// </summary>
        public float TimeFollowRemainDelta { get { return _timeFollowRemainDelta; } set { _timeFollowRemainDelta = value; } }
        /// <summary>
        /// Vị trí ẩn nấp
        /// </summary>
        public Pair<Vector3, Vector3> CoverPoint { get { return _coverPoint; } set { _coverPoint = value; } }
        /// <summary>
        /// Danh sách các vị trí ẩn nấp
        /// </summary>
        public Pair<Vector3, Vector3>[] CoverList { get { return _coverList; } }
        /// <summary>
        /// Hành vi tấn công có liều lĩnh hay không
        /// </summary>
        public bool Reckless { get { return _reckless; } set { _reckless = value; } }
        /// <summary>
        /// Có đang chạy đến điểm ẩn nấp hay không
        /// </summary>
        public bool IsRunningToCoverPoint { get { return _isRunningToCoverPoint; } set { _isRunningToCoverPoint = value; } }
        /// <summary>
        /// Thời gian để thực hiện tấn công
        /// </summary>
        public float AttackTimeoutDelta { get { return _attackTimeout_Delta; } set { _attackTimeout_Delta = value; } }
        /// <summary>
        /// Thời gian kết thúc ẩn nấp
        /// </summary>
        public float Cover_Timeout_Delta { get { return _cover_Timeout_Delta; } set { _cover_Timeout_Delta = value; } }

        /// <summary>
        /// Thời gian cho lượt bắn/chém tiếp theo của vũ khí
        /// </summary>
        public float RateFireTimeout { get { return _rateFireTimeout; } set { _rateFireTimeout = value; } }
        public float RateFireTimeoutDelta { get { return _rateFireTimeoutDelta; } set { _rateFireTimeoutDelta = value; } }

        /// <summary>
        /// Số lượng điểm phòng thủ trong danh sách điểm phòng thủ
        /// </summary>
        public int CoverCount { get { return _coverCount; } set { _coverCount = value; } }
        public bool IsNearAttackPlace { get { return _isNearAttackPlace; } set { _isNearAttackPlace = value; } }

        /// <summary>
        /// Thời gian thay đổi điểm phòng thủ
        /// </summary>
        public float Change_CoverPoint_Delta { get { return _change_CoverPoint_Delta; } set { _change_CoverPoint_Delta = value; } }
        
        public float HP { get { return _hp; } }
        
        #endregion

        private void OnEnable()
        {
            SetupAnimator();
            ReStart();
        }
        void Update()
        {
            if (_currentState != null) _currentState.UpdateStates();
        }
        private void OnDrawGizmos()
        {
            for (int i = 0; i < _coverCount; i++)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireSphere(CoverList[i].First, 0.5f);
                Gizmos.color = Color.blue;
                Gizmos.DrawWireSphere(CoverList[i].Second, 0.4f);
            }
        }

        #region ------------------------------------------------ ASSIGN SOLDIER  ------------------------------------------------

        /// <summary>
        /// Kiểm tra coi Soldier này có đang được bởi Group khác hay không
        /// </summary>
        public bool IsUsedButNotBy(SoldierGroup S)
        {
            return (IsUsed && _group != S);
        }
        private void InitValue()
        {
            AssignAnimationIDs();

            #region NavMeshAgent Setup

            _navmeshAgent = GetComponent<NavMeshAgent>();
            _navmeshAgent.angularSpeed = 360;
            _navmeshAgent.acceleration = 48;
            _navmeshAgent.autoBraking = false;
            _navmeshAgent.obstacleAvoidanceType = ObstacleAvoidanceType.HighQualityObstacleAvoidance;
            _health_Bar = GetComponentInChildren<Health_Bar>();

            #endregion

            _player = FindObjectOfType<ThirdPersonController>();
            _currentState = null;
        }

        /// <summary>
        /// Thay cho Start vì hàm sẽ thực hiện lại nhiều lần
        /// </summary>
        private void ReStart()
        {
            Debug.Log("Bắt đầu thực hiện Restart");
            // Những thứ k kết thừa monobehavior sẽ được tự xóa khi không có biến nào tham chiếu, vì vậy có thể tạo nhiều lần
            // Về vấn đề hiệu năng của việc này thì đã xử lí bằng lazy init
            _states = new SoldierFactory(this);
            _currentState = _states.Ground();
            _currentState.EnterStates();

        }
        public void ResetAllValue()
        {

            //Tất cả các giá trị có khởi tạo đều phải reset
            _soldierType = Type.None;
            _soldierBehavr = Behavr.Normal;

            //Movement
            _speed = 0.0f;
            _crouch = false;

            //Behavior
            _beAttacked = false;
            _normMove = 0;
            _gruard_Patrol_Delta = 0.0f;
            _suspect = false;
            _attack = false;
            _coverPoint = null;
            _coverCount = 0;
            _reckless = false;
            _isRunningToCoverPoint = false;
            _isNearAttackPlace = false;
            _hp = 100;

            //Equip
            _weaponEquip = -1;
        }

        /// <summary>
        /// Phân chia lại vai trò cho nhóm binh lính <br></br>
        /// - Vấn đề việc các giá trị như OrgID hay GroupID trong quá trình bị thay đổi sẽ được Assign lại khi Soldier dược hiện lại.
        /// </summary>
        /// <param name="OrgID"> ID Tổ chức </param>
        /// <param name="GroupID"> ID Group </param>
        /// <param name="SoldierBehavior"> Các loại hành vi </param>
        /// <param name="SoldierType"></param>
        /// <param name="Equip"> ID Trang bị</param>
        public void AssignRole(Vector3 p, int OrgID, Behavr SoldierBehavior, int Equip, SoldierGroup group, int id)
        {
            if (!Init)
            {
                InitValue();
                Init = true;
            }
            ResetAllValue();

            this.transform.rotation = group.transform.rotation;
            this.transform.position = group.transform.TransformPoint(p);
            this.OrgID = OrgID;
            this._soldierBehavr = SoldierBehavior;
            this._weaponEquip = Equip;
            this._group = group;
            this._id = id;

            _health_Bar.SetMaxHealth(_hp);

            //vũ khí quyết định loại lính
            switch (ListItem.getItem(Equip).type)
            {
                case Item.Type.none: _soldierType = Type.None; break;
                case Item.Type.melee: _soldierType = Type.Melee; break;
                case Item.Type.shiled: _soldierType = Type.MeleeShield; break;
                case Item.Type.pistol: _soldierType = Type.Handgun; break;
                case Item.Type.rifle: _soldierType = Type.Rifle; break;
            }

            _isUsed = true;
            Radius = group.Edge / 2;

            Debug.Log("Đã thực hiện xong Assign Role");
        }
        public void RejectRole()
        {
            this._group = null;
            this.gameObject.SetActive(false);
            _isUsed = false;
            _currentState = null;
        }
        public void SetSoldierIsDead()
        {
            _group.EliminateSoldier(_id);
        }    

        #endregion

        #region ------------------------------------------------ MOVE SYSTEM  ------------------------------------------------
        public void SetSpeedDelta(float value)
        {
            if (Mathf.Abs(_speed - value) <= 0.001f)
            {
                _speed = value;
                return;
            }
            else
                _speed = Mathf.Lerp(_speed, value, 6 * Time.deltaTime);

            _animator.SetFloat(AnimList[Anim.Speed], _speed);
        }
        public bool IsArrived()
        {
            return (Nav.remainingDistance - Nav.stoppingDistance <= 0.1f);
        }

        private Vector3 __StuckLocate;
        private float __stuckTime = 1.5f;
        public bool IsStuck()
        {
            if (NormMove == 0)
            {
                __stuckTime = 1.5f;
            }
            else
            {
                if ((transform.position - __StuckLocate).magnitude <= 0.9f)
                {
                    __StuckLocate = transform.position;
                    __stuckTime -= Time.deltaTime;
                    if (__stuckTime <= 0) return true;
                }
                else
                {
                    __stuckTime = 1.5f;
                }
            }
            return false;
        }

        /// <summary>
        /// Đặt địa điểm để di chuyển đến, hoạt ảnh chỉ thực hiện khi nhân vật đứng yên
        /// </summary>
        /// <param name="v">Địa điểm di chuyển</param>
        /// <param name="speed">Tốc độ di chuyển</param>
        /// <param name="stopdis">Khoảng cách dừng lại</param>
        public void SetDestination(Vector3 v, float stopdis)
        {
            _navmeshAgent.SetDestination(v);
            _navmeshAgent.stoppingDistance = stopdis;
        }

        /// <summary>
        /// xoay sang một hướng nào đó
        /// </summary>
        /// <param name="dir">hướng xoay mới</param>
        /// <param name="value">giá trị xoay theo nhân với delta time</param>
        public void Turn(Vector3 dir, float value)
        {
            var oldDir = transform.forward;
            var newDir = new Vector3(dir.x, transform.position.y, dir.z);
            var cross = Vector3.Cross(oldDir, newDir);
            var angle = Quaternion.Angle(transform.rotation, Quaternion.LookRotation(newDir));

            if (angle > 0.1f)
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(newDir), value * Time.deltaTime);

            //Chỉ thực hiện khi nhân vật đứng yên
            if (_normMove == 0)
            {
                if (angle <= 1f)
                {
                    Animator.SetFloat(AnimList[Anim.vZ],
                        Mathf.Lerp(Animator.GetFloat(AnimList[Anim.vZ]), 0, 1.5f * value * Time.deltaTime));
                    Animator.SetFloat(AnimList[Anim.vX],
                        Mathf.Lerp(Animator.GetFloat(AnimList[Anim.vX]), 0, 1.5f * value * Time.deltaTime));
                }
                else
                {
                    float _v = (angle > 2) ? 0.25f : (angle / 2) * 0.125f;

                    //Xoay phải
                    if (cross.y > 0.005f)
                    {
                        Animator.SetFloat(AnimList[Anim.vZ],
                            Mathf.Lerp(Animator.GetFloat(AnimList[Anim.vZ]), _v, value * Time.deltaTime));

                        Animator.SetFloat(AnimList[Anim.vX],
                            Mathf.Lerp(Animator.GetFloat(AnimList[Anim.vX]), _v, value * Time.deltaTime));
                    }
                    else
                    //Xoay trái
                    if (cross.y < -0.005f)
                    {
                        Animator.SetFloat(AnimList[Anim.vZ],
                            Mathf.Lerp(Animator.GetFloat(AnimList[Anim.vZ]), -_v, value * Time.deltaTime));

                        Animator.SetFloat(AnimList[Anim.vX],
                            Mathf.Lerp(Animator.GetFloat(AnimList[Anim.vX]), _v, value * Time.deltaTime));
                    }
                }
            }
        }

        /// <summary>
        /// Thực hiện Update các hướng di chuyển khi nhân vật không xoay cùng hướng di chuyển
        /// </summary>
        public void UpdateAnimationMoveDir()
        {
            var _v = NormMove switch
            {
                1 => 0.25f,
                2 => 0.5f,
                _ => 0f,
            };

            var normalizedMovement = Nav.desiredVelocity.normalized;

            var forwardVector = Vector3.Project(normalizedMovement, transform.forward);

            var rightVector = Vector3.Project(normalizedMovement, transform.right);

            float forwardVelocity = _v * Vector3.Dot(forwardVector, transform.forward);

            float rightVelocity = _v * Vector3.Dot(rightVector, transform.right);

            Animator.SetFloat(AnimList[Anim.vZ],
                Mathf.Lerp(Animator.GetFloat(AnimList[Anim.vZ]), forwardVelocity, 3 * Time.deltaTime));

            Animator.SetFloat(AnimList[Anim.vX],
                Mathf.Lerp(Animator.GetFloat(AnimList[Anim.vX]), rightVelocity, 3 * Time.deltaTime));

        }
        #endregion

        #region ------------------------------------------------ FUNCTION SYSTEM ---------------------------------------------
        /// <summary>
        /// Tìm điểm phòng thủ mới xuất phát trừ danh sách điểm phòng thủ (CoverList)
        /// </summary>
        /// <returns></returns>
        public bool FindNewCoverPoint()
        {
            if (_coverCount <= 0) return false;

            var k = Rand.RandSpc(0, _coverCount-1, 30052003);

            for (int i = k; i < _coverCount; i++)
            {
                if (CoverList[i] != CoverPoint)
                {
                    if ((BehaviorExt.CanBeSeenByEnemy(CoverList[i].First, transform.up, 0, EnemyPos(), EnemyTag(), true) == -1) &&
                        (BehaviorExt.CanBeSeenByEnemy(CoverList[i].Second, transform.up, 0, EnemyPos(), EnemyTag(), true) != -1))
                    {
                        CoverPoint = CoverList[i];
                        return true;
                    }
                }
            }

            for (int i = k; i >= 0; i--)
            {
                if (CoverList[i] != CoverPoint)
                {
                    if ((BehaviorExt.CanBeSeenByEnemy(CoverList[i].First, transform.up, 0, EnemyPos(), EnemyTag(), true) == -1) &&
                        (BehaviorExt.CanBeSeenByEnemy(CoverList[i].Second, transform.up, 0, EnemyPos(), EnemyTag(), true) != -1))
                    {
                        CoverPoint = CoverList[i];
                        return true;
                    }
                }
            }

            return false;
        }
        /// <summary>
        /// Tìm danh sách các điểm Cover <br></br>
        /// Lưu ý: vị trí tìm xét xuất phát từ hông để đảm bảo khi cúi người có thể che chắn tốt được
        /// </summary>
        public bool FindListCoverPoint(int cnt)
        {
            _coverCount = 0;
            _coverList = new Pair<Vector3, Vector3>[cnt];

            var vPoint = new Vector3(_player.Hip.transform.position.x, Hip.position.y, _player.Hip.transform.position.z);

            //Lưu ý vị trí tìm xét xuất phát từ hông để đảm bảo khi cúi người có thể che chắn tốt được
            var dir = Hip.position - vPoint;
            var ray = new Ray(vPoint, dir);

            //Tìm các điểm chĩa về phía người chơi trên 1 đường tròn với khoảng cách dây cung
            //nhất định rồi chiếu tia để tìm vị trí ẩn nấp
            for (int i = -30; i <= 30; i += 12)
            {
                var cnt_temp = cnt - _coverCount;

                var r = new Ray(ray.origin, Quaternion.AngleAxis(i, Vector3.up) * ray.direction);

                r = new Ray(r.GetPoint(dir.magnitude), vPoint - r.GetPoint(dir.magnitude));

                var _c = BehaviorExt.FindCoverFormItseft(r, ref cnt_temp, 10, 30, dir.magnitude, Obstacle_Layer, true,
                                  (Neck.position - Hip.position).magnitude/2, Radius + 0.2f, _player.Head.position, _player.tag, true);

                foreach (var __c in _c)
                {
                    _coverList[_coverCount++] = __c;
                    if (_coverCount == cnt) break;
                }

                if (_coverCount == cnt) break;
            }

            return _coverCount > 0;
        }
        /// <summary>
        /// Tìm điểm canh gác mới
        /// </summary>
        public int FindNewGuardLocal()
        {
            var k = Random.Range(0, Group.GuardLocate.Count - 1);
            for (int i = 0; i < Group.GuardLocate.Count; i++)
            {
                if (Group.GuardLocateID[(k + i) % Group.GuardLocate.Count] == -1)
                {
                    return (k + i) % Group.GuardLocate.Count;
                }
            }
            return -1;
        }
        /// <summary>
        /// Xóa điểm canh gác cũ và đặt điểm canh gác mới bằng id của soldier hiện tại
        /// </summary>
        /// <param name="ID"></param>
        public void SetNewGuardLocal(int ID)
        {
            for (int i = 0; i < Group.GuardLocate.Count; i++)
            {
                if (Group.GuardLocateID[i] == _id)
                {
                    Group.GuardLocateID[i] = -1;
                    break;
                }
            }

            if (ID == -1)
            {
                SetDestination(Group.transform.TransformPoint(Group.GuardOriginLocate[_id]), StopDis);
                return;
            }
            else
            {
                Group.GuardLocateID[ID] = _id;
                SetDestination(Group.GuardLocate[ID], StopDis);
                return;
            }

        }
        /// <summary>
        /// Lấy random vị trí xung quanh 1 điểm
        /// </summary>
        /// <param name="v">Vị trí mục tiêu</param>
        public Vector3 GetNearTarget(Vector3 v, float randomDis)
        {
            var v2 = Random.insideUnitCircle * Mathf.Max(2.0f, randomDis);
            return v + new Vector3(v2.x, 0, v2.y);
        }
        public void SetWeightLayerDelta(int layer_index, float value, out bool done)
        {
            _animator.SetLayerWeight(layer_index, Mathf.Lerp(_animator.GetLayerWeight(layer_index), value, 2.0f * Time.deltaTime));
            done = (Mathf.Abs(_animator.GetLayerWeight(layer_index) - 0.0f) <= 0.01f);
        }
        public bool FindAndMoveAroundEnemy(float _distance)
        {
            var dir = transform.position - Player.transform.position;
            dir -= new Vector3(0, dir.y, 0);
            var p = Player.transform.position + Player.transform.up * 0.4f;

            int AngleMax = 90;
            int AngleDiv = 5;
            var r = AngleDiv * Rand.RandSpc(0, AngleMax / AngleDiv, 305203);

            for (int i = r; i <= AngleMax; i += AngleDiv)
            {
                var k = Rand.RandPer(50, 100) ? -1 : 1;
                var newDir = new Ray(p, Quaternion.AngleAxis(i * k, transform.up) * dir);

                if (Physics.Raycast(newDir, out var hit, _distance, Obstacle_Layer))
                {
                    Debug.DrawLine(hit.point, hit.point + Vector3.up * 10, Color.magenta);
                    if (hit.distance >= dir.magnitude)
                    {
                        p = newDir.GetPoint(Random.Range(Mathf.Max(dir.magnitude, _distance * 2 / 5f),
                                                          Mathf.Min(hit.distance - 2 * Radius, _distance * 4 / 5f)));
                        SetDestination(p, StopDis);
                        return true;
                    }
                }
                else
                {
                    p = newDir.GetPoint(Random.Range(Mathf.Max(dir.magnitude, _distance * 2 / 5f), _distance * 4 / 5f));
                    SetDestination(p, StopDis);
                    return true;
                }
            }

            for (int i = r-AngleDiv; i >= 0; i -= AngleDiv)
            {
                var k = Rand.RandPer(50, 100) ? -1 : 1;
                var newDir = new Ray(p, Quaternion.AngleAxis(i * k, transform.up) * dir);

                if (Physics.Raycast(newDir, out var hit, _distance, Obstacle_Layer))
                {
                    Debug.DrawLine(hit.point, hit.point + Vector3.up * 10, Color.magenta);
                    if (hit.distance >= dir.magnitude)
                    {
                        p = newDir.GetPoint(Random.Range(Mathf.Max(dir.magnitude, _distance * 2 / 5f),
                                                          Mathf.Min(hit.distance - 2 * Radius, _distance * 4 / 5f)));
                        SetDestination(p, StopDis);
                        return true;
                    }
                }
                else
                {
                    p = newDir.GetPoint(Random.Range(Mathf.Max(dir.magnitude, _distance * 2 / 5f), _distance * 4 / 5f));
                    SetDestination(p, StopDis);
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// Kiểm tra xe có tiếp cận được kẻ thù hay không, nếu có thì đặt luôn đường đi đến kẻ thù
        /// </summary>
        /// <returns></returns>
        public bool CanApproachEnemy()
        {
            NavMeshPath path = new();
            if (_navmeshAgent.CalculatePath(EnemyPos(), path))
            {
                _navmeshAgent.SetPath(path);
                return true;
            }
            return false;
        }    
        #endregion

        #region ------------------------------------------------ INPUT SYSTEM  ------------------------------------------------
        /// <summary>
        /// Kiểm tra tiếng ồn xung quanh
        /// </summary>
        public void RecordNoise()
        {
            if (_player.IsMakingNoise)
            {
                SusLocate = GetNearTarget((Vector3)_player.NoiseLocate, 5.0f);
                Suspec = true;
            }
        }
        /// <summary>
        /// Có nhìn thấy kẻ thù hay không
        /// </summary>
        /// <returns></returns>
        public bool SeeEnemy()
        {
            RaycastHit r;
            var org = Head.position;

            //Nhìn thấy thân người chơi
            var dir = (Player.Spine2.position - Head.position).normalized;

            Debug.DrawRay(org, dir * 100, Color.yellow);

            if (Physics.Raycast(org, dir, out r, 50.0f) && r.collider.gameObject.CompareTag("Player"))
            {
                //Debug.Log("đã nhìn thấy người chơi");
                return true;
            }


            //Nhìn thấy hông người chơi
            dir = (Player.Hip.position - Head.position).normalized;

            Debug.DrawRay(org, dir * 100, Color.yellow);

            if (Physics.Raycast(org, dir, out r, 50.0f) && r.collider.gameObject.CompareTag("Player"))
            {
                //Debug.Log("đã nhìn thấy người chơi");
                return true;
            }

            return false;
        }

        /// <summary>
        /// Kẻ thù sử dụng cận chiến?
        /// </summary>
        /// <returns></returns>
        public bool EnemyUseGun()
        {
            switch (_player.Input.Equiped.type)
            {
                case Item.Type.none:
                case Item.Type.melee:
                    return false;
                default:
                    return true;
            }
        }
        /// <summary>
        /// Vị trí kẻ thù
        /// </summary>
        public Vector3 EnemyPos()
        {
            return _player.Spine2.position;
        }
        public string EnemyTag()
        {
            return (_player.tag);
        }
        /// <summary>
        /// Đang bị kẻ thù nhắm đến
        /// </summary>
        public bool BeingTargetByEnemy()
        {
            var v = _player.transform.position - transform.position;
            var v1 = _player.transform.forward;
            return (Vector3.SignedAngle(v1, v, Vector3.up) < 15);
        }
        /// <summary>
        /// Kiểm tra xem điểm đưa vào có khuất khỏi tầm nhìn của _player hay không
        /// </summary>
        /// <param name="v"></param>
        /// <returns></returns>
        public bool IsThisLocateHideFromPlayer(Vector3 v)
        {
            float r;
            if (_group != null) r = _group.Edge / 2;
            else r = 0.25f;

            var dir = _player.Hip.position - v;

            var point1 = v + Vector3.Cross(dir, transform.up).normalized * r;
            var dir1 = _player.Hip.position - point1;

            Debug.DrawLine(point1, point1 + dir1, Color.green, 2.0f);

            if (Physics.Raycast(point1, dir1, out var Hit, dir1.magnitude) && Hit.collider.transform == _player.transform)
            {
                return false;
            }

            Debug.DrawLine(point1, dir1 + point1, Color.red, 5.0f);

            point1 = v - Vector3.Cross(dir, transform.up).normalized * r;
            dir1 = _player.Hip.position - point1;

            Debug.DrawLine(point1, point1 + dir1, Color.green, 2.0f);

            if (Physics.Raycast(point1, dir1, out Hit, dir1.magnitude) && Hit.collider.transform == _player.transform)
            {
                return false;
            }

            Debug.DrawLine(point1, dir1 + point1, Color.red, 5.0f);

            return true;
        }

        #endregion

        #region ------------------------------------------------ ATTACK SYSTEM  ------------------------------------------------

        /// <summary>
        /// Kiểm tra có còn đi theo người chơi nữa không
        /// </summary>
        public bool StillFollow()
        {
            return ((_player.transform.position - transform.position).magnitude <= AttackDistance + FollowDistance);
        }
        /// <summary>
        /// Kiểm tra xem có thể tấn công kẻ thù ở vị trí hiện tại hay không
        /// </summary>
        /// <returns></returns>
        public bool CanAttackEnemy()
        {
            return ((transform.position - _player.transform.position).magnitude <= AttackDistance) &&
                    (BehaviorExt.CanBeSeenByEnemy(Neck.position, transform.up, 0, EnemyPos(), EnemyTag(), true) != -1);
        }
        /// <summary>
        /// Có thể tấn công kẻ thù ở điểm phòng thủ hay không
        /// </summary>
        /// <returns></returns>
        public bool CanAttackEnemyInAttackPlace()
        {
            if (_coverPoint == null || (transform.position - EnemyPos()).magnitude > AttackDistance) return false;

            if (BehaviorExt.CanBeSeenByEnemy(_coverPoint.Second, transform.up, 0, EnemyPos(), EnemyTag(), true) != -1)
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Ghi nhận là đã bị tấn công
        /// </summary>
        IEnumerator RecordAttack()
        {
            _beAttacked = true;
            yield return new WaitForSeconds(0.5f);
            _beAttacked = false;
        }
        public override void Damaged(int Damage)
        {
            if (!_beAttacked) StartCoroutine(RecordAttack());

            _hp -= Damage;

            _health_Bar.SetHealth(Mathf.Max(_hp,0));
        }
        public void DealMeleeDamage()
        {

        }
        public void DealBulletDamage()
        {

        }

        #endregion
    }
}