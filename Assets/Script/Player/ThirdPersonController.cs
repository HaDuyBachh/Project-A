using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Animations.Rigging;

namespace HaDuyBach
{
    public class ThirdPersonController : Character
    {
        #region Cinemachine Camera
        [Header("Cinemachine Camera")]
        [Tooltip("The follow target set in the Cinemachine Virtual Camera that the camera will follow")]
        public GameObject CinemachineCameraTarget;

        [Tooltip("How far in degrees can you move the camera up")]
        public float TopClamp = 40.0f;

        [Tooltip("How far in degrees can you move the camera down")]
        public float BottomClamp = 30.0f;

        [Tooltip("Độ nhạy xoay camera")]
        public float _sensitive = 12.0f;

        [Tooltip("Vị trí Camera tự động xoay đến")]
        public Vector3 OriginVec = new Vector3(12, 0, 0);

        [Tooltip("Additional degress to override the camera. Useful for fine tuning camera position when locked")]
        public float CameraAngleOverride = 0.0f;

        [Tooltip("For locking the camera position on all axis")]
        public bool LockCameraPosition = false;

        /// <summary>
        /// Tổng hợp các cài đặt Camera được lưu trữ sẵn
        /// </summary>
        private Transform MainCameras;
        /// <summary>
        /// Camera chính của hiện tại
        /// </summary>
        private Transform _mainCamera;
        /// <summary>
        /// Tổng hợp các cài đặt Camera Follow được lưu trữ sẵn
        /// </summary>
        private Transform CameraFollows;
        /// <summary>
        /// Cài đặt camera hiện tại
        /// </summary>
        private Transform _cameraFollow = null;
        private float _cinemachineTargetYaw = 0.0f;
        private float _cinemachineTargetPitch = 0.0f;
        private float _threshold = 0.01f;

        [Space(20)]

        #endregion

        #region Movement

        [Header("Movement")]

        [Tooltip("Walk speed of the character in m/s")]
        public float WalkSpeed = 1.5f;

        [Tooltip("Run speed  of the character in m/s")]
        public float RunSpeed = 5.0f;

        [Tooltip("Sprint speed of the character in m/s")]
        public float SprintSpeed = 8.0f;

        [Tooltip("How fast the character turns to face movement direction")]
        [Range(0.0f, 0.3f)]
        public float RotationSmoothTime = 0.12f;

        [Tooltip("Acceleration and deceleration")]
        public float SpeedChangeRate = 10.0f;

        [Tooltip("Giới Hạn Khoảng Kéo Khi chạy")]
        public float ClampMoveValue = 150;

        private float _targetRotation = 0.0f;
        private float _rotationVelocity;
        private float _animationBlend;
        private float _targetSpeed;
        private float _speed;

        [Space(20)]

        #endregion

        #region Jump 
        [Header("Jump")]
        [Tooltip("Trọng lực trái đất")]
        public float Gravity = -15.0f;

        [Tooltip("The height the player can jump")]
        public float JumpHeight;

        [Tooltip("Time required to pass before being able to jump again. Set to 0f to instantly jump again")]
        public float JumpTimeout;

        [Tooltip("Time required to pass before entering the fall state. Useful for walking down stairs")]
        public float FallTimeout;

        private float _fallTimeoutDelta = 0.0f;
        private float _jumpTimeoutDelta = 0.0f;
        private float _terminalVelocity = 53.0f;
        [Space(20)]
        #endregion

        #region General
        private bool _driving = false;
        private PlayerGetInput _input;
        private Animator _animator;
        private GameObject _UI;
        private CharacterController _controller;
        private PlayerBaseState _currentState;
        private PlayerStateFactory _states;
        private SoldierGroupsControl _soldierGroups;
        private Rigidbody _rb;
        private Vector3 _movement;
        private float _verticalVelocity = -2f;

        #endregion

        #region Weapon Equip
        [SerializeField]
        private GameObject _weaponBlock;
        private bool _changeWeapon = false;
        public readonly float ReadyToAttackTimeout = 0.5f;
        private float _readyToAttackTimeoutDelta = 0.0f;
        private float _rateFireTimeoutDelta;
        [Space(20)]
        #endregion

        #region Combat
        public readonly float ReadyToIdleTimeout = 1.5f;
        public readonly float Noise_Timeout = 0.5f;

        private bool _attackingState = false;
        private float _readyToIdleTimeoutDelta = -1.0f;
        private Vector3? _noiseLocate = null;
        #endregion

        #region Rigging
        private struct RiggedGun
        {
            public MultiAimConstraint HandAim;
            public MultiAimConstraint BodyAim;
            public Transform Target;
            public TwoBoneIKConstraint SecondhandAim;
            public Vector3 OriginOffset;
        }

        [Header("Rigging Auto Setup")]
        [Tooltip("Tự động nối các component bên dưới một cách tự động \n" +
                 "Cần xếp theo đúng trình tự đã có để sử dụng được hoàn thành như sau:\n" +
                 "- Trong 1 cụm điều chỉnh 4 loại: HandAim, BodyAim, Target,SecondHand. \n" +
                 "- Thứ 1: là HandAim \n" +
                 "- Thứ 2: là BodyAim \n" +
                 "- thứ 3: Target là con thứ 0 của BodyAim\n" +
                 "- thứ 4: Secondhand Aim vì Secondhand Aim dùng chung cho các trạng thái của 1 vũ khí"
                )]

        public bool _autoSetupRigComponent;

        [Tooltip("Gameobject chưa các Rig Component")]
        public GameObject Rig1;

        [Header("Sử dụng để định vị vị trí các bộ phận")]
        public Transform Head;
        public Transform Hip;
        public Transform Spine2;
        public LayerMask _layer;

        RiggedGun[] RigState;
        private int _rigCount;
        private int _beforeRigState = -1;
        private int _currentRigState = -1;
        #endregion

        #region Driving

        private CarControl _carControl;

        #endregion

        #region Behavior
        private bool _isMakingNoise = false;
        #endregion

        #region Getter & Setter
        public Transform MainCamera { get { return _mainCamera; } }
        public PlayerBaseState CurrentState { get { return _currentState; } set { _currentState = value; } }
        public PlayerGetInput Input { get { return _input; } }
        public Animator Animator { get { return _animator; } }
        /// <summary>
        /// Vector hướng di chuyển
        /// </summary>
        public Vector3 Movement { get { return _movement; } set { _movement = value; } }
        /// <summary>
        /// Vận tốc dọc, ví dụ khi nhảy Vận tốc dọc = sqrt(H*-2*G) để đạt được H mong muốn
        /// </summary>
        public float VerticalVelocity { get { return _verticalVelocity; } set { _verticalVelocity = value; } }
        /// <summary>
        /// Tốc độ được sử dụng lên nhân vật
        /// </summary>
        public float TargetSpeed { get { return _targetSpeed; } set { _targetSpeed = value; } }

        /// <summary>
        /// Thời gian dể có thể nhảy lại lần nữa 
        /// </summary>
        public float JumpTimeOutDelta { get { return _jumpTimeoutDelta; } set { _jumpTimeoutDelta = value; } }
        /// <summary>
        /// Thời gian rơi, ví dụ khi xuống cầu thang thời gian gian một khoảng mới hiện hoạt ảnh rơi
        /// </summary>
        public float FallTimeOutDelta { get { return _fallTimeoutDelta; } set { _fallTimeoutDelta = value; } }

        public float TerminalVelocity { get { return _terminalVelocity; } }

        /// <summary>
        /// Tốc độ được áp dụng vào <br></br>
        /// </summary>
        public float Speed { get { return _speed; } set { _speed = value; } }
        /// <summary>
        /// Kiểm tra coi có đang ở trên mặt đất hay không
        /// </summary>
        public bool Grounded { get { return _controller.isGrounded; } }

        /// <summary>
        /// Kiểm tra xem Weapon đã bị thay đổi hay chưa
        /// </summary>
        public bool ChangeWeapon { get { return _changeWeapon; } set { _changeWeapon = value; } }

        /// <summary>
        /// Sử dụng để đặt thái đang tấn công
        /// </summary>
        public bool AttackingState { get { return _attackingState; } set { _attackingState = value; } }

        /// <summary>
        /// Thời gian chuyển từ trạng thái Ready Attack -> Idle <br></br>
        /// => Thể hiện việc vẫn trong Ready Attack tránh bị ngắt khi chuyển trạng thái khác InitSubState. <br></br>
        /// Reset khi enter các AttackState và ReadyState
        /// </summary>
        public float ReadyToIdleTimeoutDelta { get { return _readyToIdleTimeoutDelta; } set { _readyToIdleTimeoutDelta = value; } }
        /// <summary>
        /// Thời gian đợi để chuyển từ ready sang tấn công. Đặt lại trong khoảng chuyển từ NoAttack -> ReadyAttack. <br></br>
        /// Reset khi Enter NoAttackState vì chỉ khi bắt đầu tấn công mới cần thời gian để chuyển 
        /// </summary>
        public float ReadyToAttackTimeoutDelta { get { return _readyToAttackTimeoutDelta; } set { _readyToAttackTimeoutDelta = value; } }
        /// <summary>
        /// Thời gian bắn của súng
        /// </summary>
        public float RateFireTimeoutDelta { get { return _rateFireTimeoutDelta; } set { _rateFireTimeoutDelta = value; } }

        public Vector3? NoiseLocate { get { return _noiseLocate; } }

        /// <summary>
        /// Có đang lái xe hay không
        /// </summary>
        public bool Driving { get { return _driving; } set { _driving = value; } }

        /// <summary>
        /// Phương tiện hiện tại mà người chơi điều khiển
        /// </summary>
        public CarControl CarControl { get { return _carControl; } set { _carControl = value; } }

        /// <summary>
        /// Đang gây tiếng ồn
        /// </summary>
        public bool IsMakingNoise { get { return _isMakingNoise; } }
        #endregion

        #region Animation IDs
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
        }

        private readonly float _speedChange = 2.0f;

        #endregion

        #region Auto Setup Rig Component

        /// <summary>
        /// Là số lượng các gameobject hiện ra trong 1 cụm Rigged Gun
        /// </summary>
        const int RigComponent = 3;
        private void AutoSetupRigComponent()
        {
            // vì số lượng phần tử trong 
            _rigCount = Rig1.transform.childCount / RigComponent;
            RigState = new RiggedGun[_rigCount];
            for (int i = 0; i < 4; i++)
            {
                RigState[i].HandAim = Rig1.transform.GetChild(i * RigComponent).GetComponent<MultiAimConstraint>();
                RigState[i].Target = RigState[i].HandAim.transform.GetChild(0);
                RigState[i].BodyAim = Rig1.transform.GetChild(i * RigComponent + 1).GetComponent<MultiAimConstraint>();
                RigState[i].SecondhandAim = Rig1.transform.GetChild(i * RigComponent + 2).GetComponent<TwoBoneIKConstraint>();
                RigState[i].OriginOffset = RigState[i].BodyAim.data.offset;
            }
        }

        /// <summary>
        /// Lấy vị trí Rig hiện tại theo quy ước tự đặt
        /// </summary>
        /// <returns></returns>
        private int RigIndex()
        {
            switch (_input.Equiped.type)
            {
                case Item.Type.pistol:
                    {
                        if (!_input.crouch) return 0;
                        else return 1;
                    }
                case Item.Type.rifle:
                case Item.Type.smg:
                case Item.Type.shotgun:
                case Item.Type.snip:
                    {
                        if (!_input.crouch) return 2;
                        else return 3;
                    }
                default: return -1;
            }
        }

        #endregion

        public void Awake()
        {
            Application.targetFrameRate = 60;

            //Camera
            MainCameras = GameObject.FindGameObjectWithTag("CameraControl").transform;
            CameraFollows = GameObject.FindGameObjectWithTag("CameraFollow").transform;
            ChangeCamera(0);
            ChangeCameraFollow(0);
            ChangeCameraTarget(transform.GetChild(0).gameObject, 5);

            //General
            _UI = GameObject.FindGameObjectWithTag("Main UI Block").GetComponentInChildren<UIControllerInput>().gameObject;
            _animator = GetComponent<Animator>();
            _controller = GetComponent<CharacterController>();
            _input = GetComponent<PlayerGetInput>();
            _soldierGroups = FindObjectOfType<SoldierGroupsControl>();
            _rb = GetComponent<Rigidbody>();

            //Animation Hash
            AssignAnimationIDs();

            //Thêm các Rig Component vào
            if (_autoSetupRigComponent) AutoSetupRigComponent();


        }

        public void Start()
        {
            //Player State Machine
            _states = new PlayerStateFactory(this);
            _currentState = _states.Grounded();
            _currentState.EnterStates();
        }

        public void Update()
        {
            // thứ 1: Lấy đầu vào khi người chơi xoay
            HandleCameraRotateInput();

            // thứ 2: Di chuyển camera theo hướng xoay của nhân vật
            HandleMove();

            // thứ 3: Update các state 
            _currentState.UpdateStates();

        }

        public void LateUpdate()
        {
            if (!_driving) HandleCameraRotation();
        }
        public void FixedUpdate()
        {
            if (_driving) HandleCameraRotation();
        }

        #region ------------------------------------------------ CAMERA CONTROL SYSTEM ------------------------------------------------
        private float Angle = 0.0f;

        /// <summary>
        /// Thay đổi Camera 
        /// </summary>
        public void ChangeCamera(int i)
        {
            if (_mainCamera != null)
            {
                _mainCamera.gameObject.SetActive(false);
            }

            if (i < MainCameras.childCount)
            {
                _mainCamera = MainCameras.GetChild(i);
                _mainCamera.gameObject.SetActive(true);
            }
        }

        /// <summary>
        /// Thay đổi các cài đặt để Camera Follow
        /// </summary>
        public void ChangeCameraFollow(int i)
        {
            if (_cameraFollow != null)
            {
                _cameraFollow.gameObject.SetActive(false);
            }
            if (i < CameraFollows.childCount)
            {
                _cameraFollow = CameraFollows.GetChild(i);
                _cameraFollow.gameObject.SetActive(true);
            }

            else
                Debug.LogError("Nhập quá thứ tự cài đặt trong +  " + this + ".ChangeCameraFollow");
        }

        /// <summary>
        /// Thay đổi gốc camera hiện tại
        /// </summary>
        /// <param name="Target"> Gốc thay đổi </param>
        /// <param name="CameraAngleOverride"> Độ quay cộng thêm</param>
        public void ChangeCameraTarget(GameObject Target, float CameraAngleOverride)
        {
            if (_cameraFollow == null) Debug.LogError("Thực hiện ChangeCameraFollow trước rồi thực hiện ChangeCameraTarget sau!");

            //gắn lại CinemachineCameraTarget bằng Target mới 
            CinemachineCameraTarget = Target;
            _cinemachineTargetYaw = Target.transform.eulerAngles.y;
            _cinemachineTargetPitch = Target.transform.eulerAngles.x;
            this.CameraAngleOverride = CameraAngleOverride;

            //Đặt camera follow theo dõi đối tượng mới
            _cameraFollow.GetComponent<CinemachineVirtualCamera>().Follow = Target.transform;
        }

        /// <summary>
        /// Lấy đầu vào _input.look từ PlayerGetInput của
        /// </summary>
        private void HandleCameraRotateInput()
        {
            // nếu tồn tại đầu vào và camera chưa lock
            if (_input.look.sqrMagnitude >= _threshold && !LockCameraPosition)
            {
                // độ nhạy chạm màn hình
                float deltaTimeMultiplier = _sensitive * Time.deltaTime * 0.3f;

                _cinemachineTargetYaw += _input.look.x * deltaTimeMultiplier;
                _cinemachineTargetPitch += _input.look.y * deltaTimeMultiplier;
            }

            _cinemachineTargetYaw = ClampAngleY(_cinemachineTargetYaw);
            _cinemachineTargetPitch = ClampAngleX(_cinemachineTargetPitch, BottomClamp, TopClamp);

            //điều chỉnh cách rotate của camera, điều chỉnh này gây ảnh hưởng đến xe khi xoay nên ta sẽ có 1 biến _input.Onlooking ghi nhận là đang di chuyển screen
            _input.look = Vector2.zero;
        }
        /// <summary>
        /// Claim góc trục hoành
        /// </summary>
        private static float ClampAngleY(float lfAngle)
        {
            lfAngle %= 360;
            if (lfAngle < 0) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            return lfAngle;
        }

        /// <summary>
        /// Clamp góc trục tung
        /// </summary>
        private static float ClampAngleX(float lfAngle, float Bottom, float Top)
        {
            lfAngle %= 360;
            if (lfAngle < 0) lfAngle += 360f;
            if (lfAngle > 360f) lfAngle -= 360f;
            if (lfAngle <= 180 && lfAngle > Bottom) return Bottom;
            if (lfAngle > 180 && lfAngle < 360 - Top) return 360 - Top;
            return lfAngle;
        }

        /// <summary>
        /// Tự động xoay Camera khi người chơi không xoay
        /// </summary>
        /// <param name="Angle"> Góc cộng thêm </param>
        /// <param name="smoth"> Tốc độ xoay nhân thêm</param>
        void AutoRotateCamera(float Angle, float smoth)
        {
            var EulerAnglesY = (_driving ? _carControl.transform.eulerAngles.y : transform.eulerAngles.y) + Angle;
            var EulerAnglesX = CinemachineCameraTarget.transform.eulerAngles.x;
            var OldRotation = CinemachineCameraTarget.transform.rotation;

            CinemachineCameraTarget.transform.rotation = Quaternion.Slerp(CinemachineCameraTarget.transform.rotation,
                                                        Quaternion.Euler(ClampAngleX(EulerAnglesX, BottomClamp, TopClamp), ClampAngleY(EulerAnglesY), 0.0f),
                                                        smoth * Time.deltaTime);

            // lấy rotation mới trừ rotatio cũ r cộng vào Yaw và Pitch là cách tốt nhất bởi vì hướng xoay của các gameObject khác nhau
            _cinemachineTargetYaw += CinemachineCameraTarget.transform.rotation.eulerAngles.y - OldRotation.eulerAngles.y;
            _cinemachineTargetPitch += CinemachineCameraTarget.transform.rotation.eulerAngles.x - OldRotation.eulerAngles.x;
        }
        private void HandleCameraRotation()
        {
            Quaternion TargerRotation;
            if (!_driving)
            {
                TargerRotation = Quaternion.Euler(CinemachineCameraTarget.transform.GetComponent<OriginValue>().RotationOrigins.x, transform.eulerAngles.y, 0.0f);
            }
            else
            {
                TargerRotation = Quaternion.Euler(CinemachineCameraTarget.transform.GetComponent<OriginValue>().RotationOrigins.x, _carControl.transform.eulerAngles.y, 0.0f);
            }

            // lấy góc quay hiện tại sao với camera
            Angle = Mathf.Abs(Quaternion.Angle(CinemachineCameraTarget.transform.rotation, TargerRotation));

            if (!_input.OnLooking)
            {
                if (!_driving)
                {
                    if (_input.move != Vector2.zero && Angle >= 10 && Angle <= 145)
                    {
                        AutoRotateCamera(0.0f, 0.9f);
                    }
                }
                else
                {
                    if (_carControl.isMovingForward())
                    {
                        AutoRotateCamera(0.0f, _carControl.SteerInput != 0 ? 3.0f : 1.8f);
                    }
                    else
                    if (_carControl.isMovingBackward())
                    {
                        AutoRotateCamera(-180.0f, 1.8f);
                    }
                }

            }

            //Nếu mục tiêu để thực hiện recoil bị hủy thì chuyển dần về Vector3.zero
            if (__currentRecoilTarget != Vector3.zero && __recoilTarget == Vector3.one * -1f)
            {
                __currentRecoilTarget = Vector3.Slerp(__currentRecoilTarget, Vector3.zero, 12.0f * Time.fixedDeltaTime);
            }

            CinemachineCameraTarget.transform.rotation = 
              Quaternion.Slerp(CinemachineCameraTarget.transform.rotation, Quaternion.Euler(
              ClampAngleX(_cinemachineTargetPitch + __currentRecoilTarget.x + CameraAngleOverride, BottomClamp, TopClamp),
                ClampAngleY(_cinemachineTargetYaw + __currentRecoilTarget.y), 
                                                    __currentRecoilTarget.z), 12.0f);
        }
        #endregion

        #region ------------------------------------------------ MOVE SYSTEM ------------------------------------------------
        /// <summary>
        /// Xoay Player theo hướng hình của camera
        /// </summary>
        public void SetSameCameraDirect()
        {
            // smooth rotate camera when attack 
            transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.Euler(0f, CinemachineCameraTarget.transform.eulerAngles.y, 0f), 10.0f * Time.deltaTime);
            //transform.rotation = Quaternion.Euler(0.0f, CinemachineCameraTarget.transform.eulerAngles.y, 0.0f);

        }
        /// <summary>
        /// Sử dụng để cập nhật các hướng và kiểu di chuyển phục vụ cho hoạt ảnh tấn công
        /// </summary>
        public void SetAnimDirect()
        {
            // nếu đang tấn công thì hạn chế tốc độ di chuyển
            _input.moveNormalize.x = Mathf.Clamp(_input.moveNormalize.x, -0.5f, 0.5f);
            _input.moveNormalize.z = Mathf.Clamp(_input.moveNormalize.z, -0.5f, 1f);

            //nếu hướng di chuyển là tiến lên và không quá nghiêng sang trái hoặc phải thì sẽ giảm ready attack đi nhanh hơn trong trường hợp ready attack
            if (_input.moveNormalize.z > 0 && Mathf.Abs(_input.moveNormalize.x) <= 0.4f
                && !_animator.GetBool(AnimList[Anim.Attack]))
            {
                if (_readyToIdleTimeoutDelta > 0) _readyToAttackTimeoutDelta -= 2 * Time.deltaTime;
            }
            // còn không vẫn clamp vận tốc như bình thường
            else
                _input.standardMove = Mathf.Clamp(_input.standardMove, 0, 1);

            _animator.SetFloat(AnimList[Anim.vX], Mathf.Lerp(_animator.GetFloat(AnimList[Anim.vX]), _input.moveNormalize.x, 6 * Time.deltaTime));
            _animator.SetFloat(AnimList[Anim.vZ], Mathf.Lerp(_animator.GetFloat(AnimList[Anim.vZ]), _input.moveNormalize.z, 6 * Time.deltaTime));
        }

        /// <summary>
        /// Sử dụng đặt lại laywerWeight theo delta time
        /// </summary>
        /// <param name="layer_index">Layer cần đặt </param>
        /// <param name="value">Giá trị cần thay đổi theo thời gian</param>
        /// <param name="speedChange">Tốc độ thay đổi </param>
        /// <param name="done">Hoàn thành hay chưa </param>
        public void SetWeightLayerDelta(int layer_index, float value, out bool done)
        {
            Animator.SetLayerWeight(layer_index, Mathf.Lerp(Animator.GetLayerWeight(layer_index), value, _speedChange * Time.deltaTime));
            done = (Mathf.Abs(Animator.GetLayerWeight(layer_index) - 0.0f) <= 0.01f);
        }
        public void SetWeightLayerImm(int layer_index, float value)
        {
            Animator.SetLayerWeight(layer_index, value);
        }    

        /// <summary>
        /// Chuyển Speed theo delta time
        /// </summary>
        /// <param name="value">giá trị chuyển</param>
        public void SetSpeedDelta(float value)
        {
            if (Mathf.Abs(_speed - value) <= 0.001f)
            {
                _speed = value;
                return;
            }
            else
                _speed = Mathf.Lerp(_speed, value, (Input.crouch ? 4 : 6) * Time.deltaTime);

            _animator.SetFloat(AnimList[Anim.Speed], _speed);
        }

        /// <summary>
        /// Tự động chỉnh hướng di chuyển của nhân vật theo hướng xoay camera và các đầu vào người chơi cung cấp
        /// </summary>
        private void HandleMove()
        {
            if (_driving) return;

            _animationBlend = Mathf.Lerp(_animationBlend, _targetSpeed, Time.deltaTime * SpeedChangeRate);
            if (_animationBlend < 0.01f) _animationBlend = 0f;

            // normalise input direction
            Vector3 inputDirection = new Vector3(_input.move.x, 0.0f, _input.move.y).normalized;

            // Việc xoay này có thể thay đổi nếu vào các trạng thái khác như trạng thái bắn súng
            if (_input.move != Vector2.zero)
            {
                _targetRotation = Mathf.Atan2(inputDirection.x, inputDirection.z) * Mathf.Rad2Deg +
                                  _mainCamera.transform.eulerAngles.y;

                //Nếu đang trong trạng tháy ready attack thì không xoay nhân vật
                if (_readyToIdleTimeoutDelta <= 0.0f)
                {
                    float rotation = Mathf.SmoothDampAngle(transform.eulerAngles.y, _targetRotation, ref _rotationVelocity,
                        RotationSmoothTime);

                    // rotate to face input direction relative to camera position
                    // xoay  theo hướng vị trí 
                    transform.rotation = Quaternion.Euler(0.0f, rotation, 0.0f);
                }
            }

            Vector3 targetDirection = Quaternion.Euler(0.0f, _targetRotation, 0.0f) * Vector3.forward;

            // di chuyển nhân vật
            _controller.Move(targetDirection.normalized * (_speed * Time.deltaTime) +
                             new Vector3(0.0f, _verticalVelocity, 0.0f) * Time.deltaTime);
        }

        #endregion

        #region ------------------------------------------------ EQUIP SYSTEM ------------------------------------------------
        private GameObject _fireLine;
        private ParticleSystem _muzzleFire;

        /// <summary>
        /// Không được dùng để thay thế vật phẩm, phải dùng Input.ActiveEquip(...) thay thế vì hàm này không có chức năng lưu lại vật phẩm trước
        /// </summary>
        public void ActiveItemInHand()
        {
            if (_weaponBlock == null)
            {
                Debug.LogError("Chưa có weaponBlock");
                return;
            }

            _changeWeapon = true;

            if (_input.PreEquip != null) _weaponBlock.transform.GetChild(_input.PreEquip.index).gameObject.SetActive(false);
            if (_input.Equiped != null && _input.Equiped.index > 0)
            {
                _weaponBlock.transform.GetChild(_input.Equiped.index).gameObject.SetActive(true);
                // đặt lại cooldown của súng khi đổi vũ khí
                _rateFireTimeoutDelta = -1;
                // lấy đường đạn
                switch (_input.Equiped.type)
                {
                    case Item.Type.pistol:
                    case Item.Type.rifle:
                    case Item.Type.smg:
                    case Item.Type.shotgun:
                    case Item.Type.snip:
                        {
                            _muzzleFire = _weaponBlock.transform.GetChild(_input.Equiped.index).GetChild(0).GetComponent<ParticleSystem>();
                            _fireLine = _weaponBlock.transform.GetChild(_input.Equiped.index).GetChild(1).gameObject;
                            _muzzleFire.gameObject.SetActive(true);
                            break;
                        }
                }

            }
        }
        #endregion

        #region ------------------------------------------------ RIG & FIRE SYSTEM ------------------------------------------------
        private float _fireTimeOut = 0.0f;
        private float _fireTimeoutDelta = 0.0f;
        private int _bulletCount;
        private Vector3 _orgRigValue = Vector3.zero;
        private Vector3 _v_forward;

        private struct RigOffset
        {
            public Vector3 c_f;
            public Vector3 c_b;
            public Vector3 c_l;
            public Vector3 c_r;
            public Vector3 c_fl;
            public Vector3 c_fr;
            public Vector3 c_bl;
            public Vector3 c_br;

            public Vector3 f;
            public Vector3 b;
            public Vector3 l;
            public Vector3 r;
            public Vector3 fl;
            public Vector3 fr;
            public Vector3 bl;
            public Vector3 br;
        }

        /// <summary>
        /// Giá trị cộng thêm để không bị lỗi nhắm bắn <br></br>
        /// 0 : pistol  <br></br>
        /// 1 : rifle
        /// </summary>
        RigOffset[] Rof = new RigOffset[2]
        {
            // Vector 3 với x là hướng đi lên, y là hướng 2 bên
            
            // Pistol
            new RigOffset{
                c_f = new Vector3(-5.6f,0,0),
                c_l = new Vector3(-5.6f,-0.8f,0),
                c_r = new Vector3(-5.6f,-0.3f,0),
                c_b = new Vector3(-5.6f,0,0),
                c_fl = new Vector3(-5.6f,-0.8f,0),
                c_fr = new Vector3(-5.6f,-0.3f,0),
                c_bl = new Vector3(-5.6f,-0.8f,0),
                c_br = new Vector3(-5.6f,-0.3f,0),

                f = new Vector3(0,0,0),
                l = new Vector3(0,0.45f,0),
                r = new Vector3(0,-0.6f,0),
                b = new Vector3(0,0,0),
                fl = new Vector3(0,0.45f,0),
                fr = new Vector3(0,-0.6f,0),
                bl = new Vector3(0,0.50f,0),
                br = new Vector3(0,-0.6f,0),
            },
            // Rifle 
            new RigOffset {
                c_f = new Vector3(-1.6f,-0.2f,0),
                c_l = new Vector3(-3.25f,-0.5f,0),
                c_r = new Vector3(-1.6f,-0.7f,0),
                c_b = new Vector3(-1.6f,-0.2f,0),

                c_fl = new Vector3(-2.425f,-0.35f,0),
                c_fr = new Vector3(-1.6f,-0.45f,0),
                c_bl = new Vector3(-2.425f,-0.35f,0),
                c_br = new Vector3(-1.6f,-0.45f,0),

                f = new Vector3(0,-0.5f,0),
                l = new Vector3(0,0.7f,0),
                r = new Vector3(0,-1.0f,0),
                b = new Vector3(0.3f,-0.5f,0),
                fl = new Vector3(0.5f,0,0),
                fr = new Vector3(0,-1.0f,0),
                bl = new Vector3(0.3f,0,0),
                br = new Vector3(0.3f,-0.5f,0),
            }
        };
        public void OffRigImm()
        {
            RigState[_currentRigState].HandAim.weight = 0;
            RigState[_currentRigState].BodyAim.weight = 0;
            RigState[_currentRigState].SecondhandAim.weight = 0;
        }

        /// <summary>
        /// Thực hiện chức năng <br></br>
        /// - Đặt lại các Rig theo trạng thái đang bắn <br></br>
        /// - Đặt tia đạn bắn
        /// </summary>
        /// <param name="value"> Giá trị chuyển của Weight, nếu giá trị nhỏ hơn 0 tức là đang ngừng bắn  </param>
        /// <param name="bullet">Số đạng cộng thêm, cộng vào số đạng cần bắn ra hiện có để hiển thị _fireLine </param>
        public void SetRigAndFireState(float value, bool bullet)
        {
            //return;

            if (RigIndex() != _currentRigState)
            {
                if (_beforeRigState > -1)
                {
                    RigState[_beforeRigState].HandAim.weight = 0.0f;
                    RigState[_beforeRigState].BodyAim.weight = 0.0f;
                    RigState[_beforeRigState].SecondhandAim.weight = 0.0f;

                    if (_currentRigState > -1)
                        RigState[_currentRigState].BodyAim.data.offset = RigState[_currentRigState].OriginOffset;

                }
                _beforeRigState = _currentRigState;
                _currentRigState = RigIndex();
            }
            else
            {
                if (_beforeRigState > -1)
                {
                    RigState[_beforeRigState].HandAim.weight =
                        Mathf.Lerp(RigState[_beforeRigState].HandAim.weight, 0.0f, 24 * Time.deltaTime);
                    RigState[_beforeRigState].BodyAim.weight =
                        Mathf.Lerp(RigState[_beforeRigState].BodyAim.weight, 0.0f, 24 * Time.deltaTime);
                    RigState[_beforeRigState].SecondhandAim.weight =
                        Mathf.Lerp(RigState[_beforeRigState].SecondhandAim.weight, 0.0f, 24 * Time.deltaTime);

                }

                if (_currentRigState > -1)
                {
                    RigState[_currentRigState].HandAim.weight =
                    Mathf.Lerp(RigState[_currentRigState].HandAim.weight, value, 12 * Time.deltaTime);
                    RigState[_currentRigState].BodyAim.weight =
                        Mathf.Lerp(RigState[_currentRigState].BodyAim.weight, value, 12 * Time.deltaTime);

                    if (_input.Equiped.type == Item.Type.rifle && Grounded)
                    {
                        RigState[_currentRigState].SecondhandAim.weight =
                            Mathf.Lerp(RigState[_currentRigState].SecondhandAim.weight, 1, 2 * Time.deltaTime);
                    }
                    else
                    {
                        RigState[_currentRigState].SecondhandAim.weight =
                            Mathf.Lerp(RigState[_currentRigState].SecondhandAim.weight, 0, 12 * Time.deltaTime);
                    }

                    RigState[_currentRigState].Target.localPosition =
                            RigState[_currentRigState].Target.InverseTransformDirection(
                            CinemachineCameraTarget.transform.forward.normalized * 2.0f);

                    #region Điều chỉnh thủ công

                    var v3 = Vector3.zero;
                    var id = (_input.Equiped.type == Item.Type.pistol) ? 0 : 1;

                    int z = 0;
                    int x = 0;
                    if (_input.moveNormalize.z > 0) z = 1;
                    else
                    if (_input.moveNormalize.z < 0) z = -1;

                    if (_input.moveNormalize.x > 0) x = 1;
                    else
                    if (_input.moveNormalize.x < 0) x = -1;

                    switch (z, x)
                    {
                        case (0, 0): break;
                        case (0, -1): v3 = (_input.crouch ? Rof[id].c_l : Rof[id].l); break;
                        case (0, 1): v3 = (_input.crouch ? Rof[id].c_r : Rof[id].r); break;
                        case (-1, 0): v3 = (_input.crouch ? Rof[id].c_b : Rof[id].b); break;
                        case (1, 0): v3 = (_input.crouch ? Rof[id].c_f : Rof[id].f); break;
                        case (1, -1): v3 = (_input.crouch ? Rof[id].c_fl : Rof[id].fl); break;
                        case (1, 1): v3 = (_input.crouch ? Rof[id].c_fr : Rof[id].fr); break;
                        case (-1, -1): v3 = (_input.crouch ? Rof[id].c_bl : Rof[id].bl); break;
                        case (-1, 1): v3 = (_input.crouch ? Rof[id].c_br : Rof[id].br); break;
                    }

                    ////Thay đổi dần dần theo góc để không bị lệch tâm ngắm
                    // 0 -> 35  ~  0 -> -2.5
                    var deg = 360 - (CinemachineCameraTarget.transform.eulerAngles.x);
                    if (deg <= TopClamp)
                    {
                        v3.x += (deg / (TopClamp - CameraAngleOverride)) * -3.5f;
                    }


                    RigState[_currentRigState].BodyAim.data.offset = Vector3.Lerp(
                        RigState[_currentRigState].BodyAim.data.offset,
                            RigState[_currentRigState].OriginOffset + v3, 15 * Time.deltaTime);

                    #endregion
                }
            }

            if (value > 0)
            {
                //cộng thêm đạn
                if (bullet && Mathf.Abs(RigState[_currentRigState].BodyAim.weight - value) <= 0.0015f)
                {
                    _bulletCount++;
                    _fireTimeOut = _input.Equiped.speed;
                    //nếu đang không có đạn bắn hiện tại
                    if (_fireTimeoutDelta <= 0.0f)
                    {
                        _fireTimeoutDelta = _fireTimeOut;
                        _fireLine.SetActive(true);
                        _muzzleFire.Play();
                        _fireLine.transform.localPosition = Vector3.zero;
                        _v_forward = _fireLine.transform.forward;
                    }
                }

                //hiển thị đường đạn
                if (_bulletCount > 0)
                {
                    //nếu trong thời gian rate of fire thì di chuyển đường đạn
                    if (_fireTimeoutDelta > 0)
                    {
                        _fireTimeoutDelta -= Time.deltaTime;

                        if (_fireLine.transform.localPosition.z < 30)
                        {
                            if (_fireLine.transform.localPosition.z <= 10) 
                                _fireLine.transform.localPosition +=
                                      _fireLine.transform.InverseTransformDirection(_v_forward) * (Random.Range(6f, 10f));
                            else
                                _fireLine.transform.localPosition +=
                                      _fireLine.transform.InverseTransformDirection(_v_forward) * (Random.Range(12f, 16f));

                            if (_fireLine.transform.localPosition.z >= 30)
                            {
                                _fireLine.SetActive(false);
                                if (_bulletCount == 0) _muzzleFire.Stop();
                            }
                        }
                    }
                    //nếu không trừ đạn đi và kiểm tra xem còn cần bắn đạn nữa hay không
                    else
                    {
                        _bulletCount--;
                        if (_bulletCount > 0)
                        {
                            _fireTimeoutDelta = _fireTimeOut;
                            _fireLine.transform.localPosition = Vector3.zero;
                            _fireLine.SetActive(true);
                            _muzzleFire.Play();
                        }
                        else TurnOffFire();
                    }
                }
            }
        }

        /// <summary>
        /// Tắt hoạt ảnh đang bắn hiện tại
        /// </summary>
        public void TurnOffFire()
        {
            _bulletCount = 0;
            _fireTimeoutDelta = 0.0f;
            if (_fireLine != null) _fireLine.SetActive(false);
            if (_muzzleFire != null) _muzzleFire.Stop();
        }

        #endregion

        #region ------------------------------------------------ ATTACK SYSTEM ------------------------------------------------
        private Vector3 __recoilTarget = Vector3.zero;
        private Vector3 __currentRecoilTarget = Vector3.zero;
        private float __returnSpeed = 0;
        private float __snappiness = 0;
        IEnumerator Noise()
        {
            _noiseLocate = transform.position;
            _isMakingNoise = true;
            yield return new WaitForSeconds(Noise_Timeout);
            _isMakingNoise = false;
            _noiseLocate = null;
        }
        public void MakeNoise()
        {
            if (!_isMakingNoise) StartCoroutine(Noise());
        }

        #region Deprecated
        //public void Recoil_SetUp(short value)
        //{
        //    if (value > 0)
        //    {
        //        if (Input.standardMove > 0) value += 3;
        //    }

        //    if (value > 0)
        //    {
        //        //Trục hoành
        //        _recoilTarget.x = _cinemachineTargetYaw + Random.Range(-0.5f, 0.5f) * value * Time.deltaTime * 10f;
        //        _recoilTarget.x = ClampAngleY(_recoilTarget.x);

        //        //Trục tung
        //        _recoilTarget.y = _cinemachineTargetPitch - 2.0f * value * Time.deltaTime * 10f;
        //        _recoilTarget.y = ClampAngleX(_recoilTarget.y, BottomClamp, TopClamp);

        //        LerpDelta = Time.deltaTime * value;
        //        _timeCounterDelta = 0.1f;
        //    }

        //    if (_timeCounterDelta > 0)
        //    {
        //        //trừ dần thời gian
        //        _timeCounterDelta -= _timeCounterDelta > 0 ? Time.deltaTime : 0;

        //        // Bắt exception: nhân vật ở hướng quay Yaw = 0 mà _recoilTarget ở 360 sẽ bị xoay 1 vòng
        //        if ((_cinemachineTargetYaw <= 90 && _recoilTarget.x >= 270) ||
        //            (_cinemachineTargetYaw >= 270 && _recoilTarget.x <= 90))
        //        {
        //            if (_cinemachineTargetYaw <= 90)
        //            {
        //                _cinemachineTargetYaw = Mathf.Lerp(_cinemachineTargetYaw, 0, LerpDelta);
        //                if (_cinemachineTargetYaw <= 0.1f) _cinemachineTargetYaw = 360;
        //            }
        //            else
        //            {
        //                _cinemachineTargetYaw = Mathf.Lerp(_cinemachineTargetYaw, 360, LerpDelta);
        //                if (Mathf.Abs(_cinemachineTargetYaw - 360) <= 0.1f) _cinemachineTargetYaw = 0;
        //            }
        //        }
        //        else
        //            _cinemachineTargetYaw = Mathf.Lerp(_cinemachineTargetYaw, _recoilTarget.x, LerpDelta);


        //        //Vấn đề là trục tung nằm trong khoảng [BottomClaim,0] & [360-TopClamp,360] vì vậy nên cần sửa đổi để không bị lật khung hình
        //        if ((_cinemachineTargetPitch >= 360 - TopClamp && _recoilTarget.y >= 360 - TopClamp) ||
        //           (_cinemachineTargetPitch <= BottomClamp && _recoilTarget.y <= BottomClamp))
        //        {
        //            _cinemachineTargetPitch = Mathf.Lerp(_cinemachineTargetPitch, _recoilTarget.y, LerpDelta);
        //        }
        //        else
        //        //nếu khác chiều
        //        if (_cinemachineTargetPitch <= BottomClamp && _recoilTarget.y >= 360 - TopClamp)
        //        {
        //            _cinemachineTargetPitch = Mathf.Lerp(_cinemachineTargetPitch, 0, LerpDelta);
        //            if (_cinemachineTargetPitch <= 0.2f)
        //            {
        //                // --- Bug --- Bị lỗi ở đây, tôi đặt thành 360 không chạy chỉ chạy ở dưới 360 không biết lỗi ở đâu
        //                //_cinemachineTargetPitch = 359.999f;
        //                _cinemachineTargetPitch = 359.99f;
        //                //Debug.Log("Đã xóa vị trí này  " + _recoilTarget.y + "  " + _cinemachineTargetPitch);
        //            }

        //        }
        //        else
        //        if (_recoilTarget.y <= BottomClamp && _cinemachineTargetPitch >= 360 - TopClamp)
        //        {
        //            _cinemachineTargetPitch = Mathf.Lerp(_cinemachineTargetPitch, 360, LerpDelta);
        //            if (Mathf.Abs(_cinemachineTargetPitch - 360) <= 0.5f)
        //            {
        //                _cinemachineTargetPitch = 0;
        //            }
        //        }


        //        if (Mathf.Abs(_cinemachineTargetYaw - _recoilTarget.x) <= 0.001 && Mathf.Abs(_cinemachineTargetPitch - _recoilTarget.y) <= 0.001)
        //        {
        //            _timeCounterDelta = 0.0f;
        //        }
        //    }
        //}

        //
        #endregion

        /// <summary>
        /// Thực hiện giật khi nhắm bắn
        /// </summary>
        /// <param name="value"></param>
        public void RecoilSetUp(RecoilValue value)
        {
            if (value == null)
            {
                __recoilTarget = Vector3.one * -1;
                return;
            }

            if (value != null )
            {
                if (value != RecoilValue.None)
                {
                    __recoilTarget += value.GetRecoil();
                    __returnSpeed = value.returnSpeed;
                    __snappiness = value.snappiness;
                }
                else
                if (__recoilTarget == Vector3.one * -1)
                {
                    __recoilTarget = Vector3.zero;
                }
            }

            __recoilTarget = Vector3.Lerp(__recoilTarget, Vector3.zero, __returnSpeed * Time.deltaTime);
            __currentRecoilTarget = Vector3.Slerp(__currentRecoilTarget, __recoilTarget, __snappiness * Time.fixedDeltaTime);
        }

        /// <summary>
        /// Gây sát thương tầm xa thông qua bắn súng đến đối tượng trên tầm ngắm
        /// </summary>
        /// <param name="Damgage"></param>
        public void DealBulletDamage(int Damgage)
        {
            //Gây tiếng động ở một khoảng
            MakeNoise();

            var r = new Ray(_mainCamera.position + _mainCamera.forward * 1.5f, _mainCamera.forward);

            Debug.DrawLine(r.origin, r.origin + r.direction * 100, Color.magenta, 50);

            if (Physics.Raycast(r, out var hit, 60.0f, ~(1 << 6)))
            {
                var c = hit.collider.GetComponentInParent<Character>();
                if (c != null) c.Damaged(Damgage * (hit.collider.CompareTag("Weak Point") ? 3 : 1));
            }
        }
        /// <summary>
        /// Gây sát thương cận chiến đến đối tượng ở gần 
        /// </summary>
        /// <param name="Damage"></param>
        public void DealMeleeDamage(float Damage)
        {

        }

        public override void Damaged(int Damage)
        {

        }

        #endregion
    }
}



