using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace HaDuyBach
{

    public class AICarController : MonoBehaviour
    {
        [Header("Car Setup (if have Car_Controller, no need to set)")]
        public WheelCollider FrontLeftWheel;
        public WheelCollider FrontRightWheel;
        public WheelCollider RearLeftWheel;
        public WheelCollider RearRightWheel;
        [Space(10)]
        public Transform FrontLeftTrans;
        public Transform FrontRightTrans;
        public Transform RearLeftTrans;
        public Transform RearRightTrans;
        [Space(10)]
        public float brakeAcceleration;
        public float maxAcceleration;
        public float maxSteerAngle;
        [Space(10)]
        public Vector3 CenterOfMass = Vector3.zero;

        [Header("AI Car Setup")]
        public float MaxSpeed = 20.0f;
        public float BrakeSpeed = 15.0f;
        [Space(10)]
        [Tooltip("Khoảng cách để tránh xe")]
        public float VehicleAvoidDistance;
        [Tooltip("khoảng cách cảm biến của xe đối với xe khác")]
        public float VehicleSensorDistance;
        [Tooltip("khoảng cách cảm biết của xe đối với tường")]
        public float SensorDistance;
        [Space(10)]
        [Tooltip("điểm bắt đầu front back của cảm biến")]
        public float startPointZ;
        [Tooltip("điểm bắt đầu up down của cảm biến")]
        public float startPointY;
        [Tooltip("độ rộng của 2 cảm biến 2 bên")]
        public float WidthSensor;
        [Tooltip("góc của 2 cảm biến 2 bên")]
        public float AngleSensors;
        [Tooltip("góc của 2 sideway")]
        public float SideWayAngleSensors;
        [Tooltip("góc quay tối đa khi nhận cảm biến")]
        public float MaxSteerSensors;
        [Space(10)]
        [Tooltip("giới hạn khoảng cách để xe chuyển hướng sang WayPoint mới")]
        public float disFromPath;
        [Tooltip("giới hạn khoảng cách xe có thể tới")]
        public float LimitMinDistance = -1;
        [Space(10)]
        [Tooltip("Sử dụng cho Path")]
        public int currentPathObj;

        [Tooltip("Sử dụng cho Multi Path")]
        public WayPoint currentWayPoint;
        public WayPoint NextWayPoint;
        public int direct;
        public Vector3 TargetLocal;
        private float speed;

        [Space(10)]
        private float moveInput;
        private float brakeInput;
        private CarControl CarOption;

        [Header("Type of Path")]
        public PathScript Path = null;
        public MultiPathScript mulPath = null;

        private Transform[] path = null;
        Rigidbody CarRb;

        private void SetupCarValue()
        {
            FrontLeftWheel = CarOption.FrontLeftWheel;
            FrontRightWheel = CarOption.FrontRightWheel;
            RearLeftWheel = CarOption.RearLeftWheel;
            RearRightWheel = CarOption.RearRightWheel;

            FrontLeftTrans = CarOption.FrontLeftTrans;
            FrontRightTrans = CarOption.FrontRightTrans;
            RearLeftTrans = CarOption.RearLeftTrans;
            RearRightTrans = CarOption.RearRightTrans;

            brakeAcceleration = CarOption.brakeAcceleration;
            maxAcceleration = CarOption.maxAcceleration;
            maxSteerAngle = CarOption.maxSteerAngle;

            CarRb.centerOfMass = CenterOfMass;
        }
        private void Awake()
        {
            CarRb = transform.GetComponent<Rigidbody>();
            if (transform.TryGetComponent(out CarOption))
            {
                SetupCarValue();
                CarOption.enabled = false;
            }
            direct = -1;

            // trường hợp không cho gameobject vào
            if (Path == null && mulPath == null) GetPath();

            if (mulPath != null)
            {
                if (currentWayPoint == null) getMulPath();
                if (currentWayPoint == null) getRandomMulPath();
                if (currentWayPoint == null)
                {
                    Debug.Log("không có đường");
                    haveWay = false;
                    return;
                }
                setupFirstWayPoint();
            }

            brakeInput = 0;
            moveInput = 1;
        }

        private void getMulPath()
        {
            var min = Mathf.Infinity;
            var n = mulPath.transform.childCount;
            for (int i = 0; i < n; i++)
            {
                if ((mulPath.transform.GetChild(i).transform.position - transform.position).magnitude < min)
                {
                    min = (mulPath.transform.GetChild(i).transform.position - transform.position).magnitude;
                    currentWayPoint = mulPath.transform.GetChild(i).GetComponent<WayPoint>();
                }
            }

            if (LimitMinDistance != -1 && min > LimitMinDistance)
            {
                currentWayPoint = null;
                return;
            }
        }

        private void getRandomMulPath()
        {
            float Min = Mathf.Infinity;
            var Mp_objs = FindObjectsOfType<MultiPathScript>();
            foreach (var mp_obj in Mp_objs)
            {
                var n = mp_obj.transform.childCount;
                for (int i = 0; i < n; i++)
                {
                    var waypoint = mp_obj.transform.GetChild(i);
                    if ((waypoint.transform.position - transform.position).magnitude <= Min)
                    {
                        Min = (waypoint.transform.position - transform.position).magnitude;
                        currentWayPoint = waypoint.GetComponent<WayPoint>();
                    }
                }
            }
        }

        private int getCurrentDirect(float r)
        {
            int direct = 0;
            for (int i = 1; i <= 3; i++)
            {
                if (45 + 90 * (i - 1) < r && r <= 45 + 90 * i)
                {
                    direct = i;
                }
            }
            return direct;
        }

        private void setupFirstWayPoint()
        {
            direct = getCurrentDirect(transform.rotation.eulerAngles.y);
            var same = direct;
            while (currentWayPoint.Next[direct] == null)
            {
                direct = (direct + 1) % 4;
                if (same == direct)
                {
                    Debug.Log("không tìm được đường");
                    return;
                }
            }


            Debug.Log(direct + "  " + currentWayPoint.name);
            NextWayPoint = currentWayPoint.Next[direct].GetComponent<WayPoint>();
            TargetLocal = currentWayPoint.local[(direct - direct - direct * 3 + 4 * 4) % 4];

            myOrder = currentWayPoint.getMyOrder();

        }

        private void GetPath()
        {
            mulPath = GameObject.FindGameObjectWithTag("Path").GetComponent<MultiPathScript>();
            path = null;

            if (mulPath == null) path = GameObject.FindGameObjectWithTag("Path").GetComponent<PathScript>().Path;
        }

        void Update()
        {
            if (path == null && mulPath == null)
            {
                Debug.LogError("Thiếu đường đi, hãy thêm đường đi");
                return;
            }

            if (!haveWay)
            {
                brakeInput = 10;
                moveInput = 0;
                //Debug.Log("không có đường đâu pé ơi");
                return;
            }

            HandleSteer();
            HandleMotor();
            HandleBrake();
            UpdateWheels();
            HandleSteerAvoidAICar();
        }

        public bool isMovingForward()
        {
            var velocity = CarRb.velocity;
            var localVel = transform.InverseTransformDirection(velocity);
            return (localVel.z > 1);
        }

        public bool isMovingBackward()
        {
            var velocity = CarRb.velocity;
            var localVel = transform.InverseTransformDirection(velocity);
            return (localVel.z < -1);
        }

        private bool haveWay = true;
        private Vector3 steerVector;
        private bool isCarReverse = false;
        private bool StopForAnotherCar = false;
        public uint myOrder = 0;

        bool junctionOrIntersection(WayPoint P)
        {
            int dem = 0;
            for (int i = 0; i < 4; i++)
            {
                if (P.Next[i] != null) dem++;
            }
            return (dem > 2);
        }
        void HandleSteer()
        {
            // Single Path
            if (Path != null)
            {
                if (currentPathObj < path.Length)
                {
                    var steerVector = transform.InverseTransformPoint(new Vector3(path[currentPathObj].position.x, transform.position.y, path[currentPathObj].position.z));
                    var newSteer = maxSteerAngle * (steerVector.x / steerVector.magnitude);
                    FrontLeftWheel.steerAngle = newSteer;
                    FrontRightWheel.steerAngle = newSteer;

                    var speed = CarRb.velocity.magnitude;

                    if (newSteer != 0 && speed > BrakeSpeed)
                    {
                        brakeInput = 5.0f;
                        moveInput = 0;
                    }
                    else
                    {
                        brakeInput = 0;
                        moveInput = 1;
                    }

                    if (steerVector.magnitude <= disFromPath)
                    {
                        currentPathObj++;
                    }
                }
                else
                {
                    currentPathObj = 0;
                    brakeInput = 1;
                    moveInput = 0;
                }
            }


            // Multi Path
            if (mulPath != null)
            {
                float newSteer = 0;
                steerVector = transform.InverseTransformPoint(new Vector3(TargetLocal.x, transform.position.y, TargetLocal.z));

                if (flag == 0)
                {
                    newSteer = maxSteerAngle * (steerVector.x / steerVector.magnitude);
                    FrontLeftWheel.steerAngle = newSteer;
                    FrontRightWheel.steerAngle = newSteer;
                }

                speed = CarRb.velocity.magnitude;

                if (!currentWayPoint.isMyOrder(myOrder) && junctionOrIntersection(currentWayPoint)) StopForAnotherCar = true;

                if (((newSteer != 0 || flag != 0) && speed > BrakeSpeed) || StopForAnotherCar == true)
                {
                    brakeInput = Mathf.Max(brakeInput, 1f);
                    moveInput = 0;
                }
                else
                {
                    brakeInput = 0;
                    moveInput = isCarReverse ? -2 : 1;
                }


                // quyết định hướng tiếp theo di chuyển
                if (steerVector.magnitude <= disFromPath)
                {
                    //hàm chọn đường đi tránh tắc đường
                    int Min = 40;
                    for (int i = 0; i < 4; i++)
                    {
                        if (NextWayPoint.Next[i] != null && NextWayPoint.Next[i].GetComponent<WayPoint>().CarRemain() < Min && NextWayPoint.Next[i] != currentWayPoint.gameObject)
                        {
                            Min = NextWayPoint.Next[i].GetComponent<WayPoint>().CarRemain();
                        }
                    }

                    var nextdirect = UnityEngine.Random.Range(0, 3);
                    var same = nextdirect;
                    while (NextWayPoint.Next[nextdirect] == null || NextWayPoint.Next[nextdirect] == currentWayPoint.gameObject ||
                              NextWayPoint.Next[nextdirect].GetComponent<WayPoint>().CarRemain() > Min)
                    {
                        nextdirect = (nextdirect + 1) % 4;
                        if (same == nextdirect)
                        {
                            haveWay = false;
                            return;
                        }
                    }

                    currentWayPoint.popMyOrder();

                    currentWayPoint = NextWayPoint;
                    NextWayPoint = NextWayPoint.Next[nextdirect].GetComponent<WayPoint>();
                    if (nextdirect == (direct - 1 + 4) % 4 && currentWayPoint.getNumOfNext() >= 3)
                    {
                        TargetLocal = currentWayPoint.local[((nextdirect - direct - direct * 3 + 4 * 4) % 4 + 1) % 4];
                        //Debug.Log(((nextdirect - direct - direct * 3 + 4 * 4) % 4 + 1) % 4 + "  hê hê  ");
                    }

                    else
                    {
                        TargetLocal = currentWayPoint.local[(nextdirect - direct - direct * 3 + 4 * 4) % 4];
                        //Debug.Log((nextdirect - direct - direct * 3 + 4 * 4) % 4);
                    }

                    myOrder = currentWayPoint.getMyOrder();


                    direct = nextdirect;
                }
            }

        }

        private bool isNextOrEqual(WayPoint p, WayPoint xet)
        {
            if (p == xet) return true;
            for (int i = 0; i < 4; i++)
            {
                if (xet.Next[i] == p.gameObject) return true;
            }
            return false;
        }

        private float flag = 0;

        public LayerMask Layer;
        private void HandleSteerAvoidAICar()
        {
            flag = 0;
            StopForAnotherCar = false;
            float avoidSensitivy = 0;

            Vector3 pos;
            RaycastHit Hit;
            Vector3 Angle;
            LayerMask layer;

            #region  ---- WALL SENSOR ----

            layer = ~Layer;

            //Wall Front Left
            pos = transform.position + transform.up * startPointY + transform.forward * startPointZ - transform.right * WidthSensor;
            if (Physics.Raycast(pos, transform.forward, out Hit, SensorDistance, layer))
            {
                // cho xe lùi lại
                if (Hit.distance < SensorDistance * 0.2f)
                {
                    isCarReverse = true;
                }
                else
                if (Hit.distance > SensorDistance * 0.6f) isCarReverse = false;

                flag++;
                avoidSensitivy += 1f;
                Debug.DrawLine(pos, Hit.point, Color.white);
            }
            else
            {
                //Wall Left Angle
                pos = transform.position + transform.up * startPointY + transform.forward * startPointZ - transform.right * WidthSensor;
                Angle = Quaternion.AngleAxis(360 - AngleSensors, transform.up) * transform.forward;
                if (Physics.Raycast(pos, Angle, out Hit, SensorDistance * 0.5f, layer))
                {
                    // cho xe lùi lại
                    if (Hit.distance < SensorDistance * 0.2f)
                    {
                        isCarReverse = true;
                    }
                    else
                    if (Hit.distance > SensorDistance * 0.6f) isCarReverse = false;

                    flag++;
                    avoidSensitivy += 0.5f;
                    Debug.DrawLine(pos, Hit.point, Color.white);
                }
            }


            //Wall Front Right
            pos = transform.position + transform.up * startPointY + transform.forward * startPointZ + transform.right * WidthSensor;
            if (Physics.Raycast(pos, transform.forward, out Hit, SensorDistance, layer))
            {
                // cho xe lùi lại
                if (Hit.distance < SensorDistance * 0.2f)
                {
                    isCarReverse = true;
                }
                else
                if (Hit.distance > SensorDistance * 0.6f) isCarReverse = false;

                flag++;
                avoidSensitivy -= 1f;
                Debug.DrawLine(pos, Hit.point, Color.white);
            }
            else
            {
                //Wall Right Angle
                pos = transform.position + transform.up * startPointY + transform.forward * startPointZ + transform.right * WidthSensor;
                Angle = Quaternion.AngleAxis(AngleSensors, transform.up) * transform.forward;
                if (Physics.Raycast(pos, Angle, out Hit, SensorDistance * 0.5f, layer))
                {
                    // cho xe lùi lại
                    if (Hit.distance < SensorDistance * 0.2f)
                    {
                        isCarReverse = true;
                    }
                    else
                    if (Hit.distance > SensorDistance * 0.6f) isCarReverse = false;

                    flag++;
                    avoidSensitivy -= 0.5f;
                    Debug.DrawLine(pos, Hit.point, Color.white);
                }
            }


            //Wall Left Side
            pos = transform.position + transform.up * startPointY - transform.right * (WidthSensor);
            var sideAngle = Quaternion.AngleAxis(SideWayAngleSensors, transform.up) * transform.right;
            if (Physics.Raycast(pos, sideAngle, out Hit, SensorDistance * 0.3f, layer))
            {
                flag++;
                avoidSensitivy += 0.5f;
                Debug.DrawLine(pos, Hit.point, Color.white);
            }

            //Wall Right Side
            pos = transform.position + transform.up * startPointY + transform.right * (WidthSensor);
            sideAngle = Quaternion.AngleAxis(360 - SideWayAngleSensors, transform.up) * -transform.right;
            if (Physics.Raycast(pos, sideAngle, out Hit, SensorDistance * 0.3f, layer))
            {
                flag++;
                avoidSensitivy -= 0.5f;
                Debug.DrawLine(pos, Hit.point, Color.white);
            }

            //Wall front
            if (flag > 0 && avoidSensitivy == 0)
            {

                pos = transform.position + transform.up * startPointY;
                if (Physics.Raycast(pos, transform.forward, out Hit, SensorDistance, layer))
                {
                    if (Mathf.Abs(Hit.normal.y) <= 0.5f)
                    {
                        flag++;
                        if (Vector3.SignedAngle(Hit.normal, transform.forward, Vector3.up) < 0f)
                        {
                            avoidSensitivy += 1;
                        }
                        else avoidSensitivy -= 1;
                    }

                    //Debug.Log(Hit.normal +  "  =>  normal");
                    //Debug.Log(Vector3.SignedAngle(Hit.normal, transform.forward,Vector3.up));
                    //Debug.DrawLine(pos, Hit.point);
                    //Debug.DrawRay(Hit.point, Hit.normal,Color.yellow);
                }
            }

            #endregion

            #region  ---- VEHICLES SENSOR ----

            layer = Layer;

            //Vehicle Front Left
            pos = transform.position + transform.up * startPointY + transform.forward * startPointZ - transform.right * WidthSensor;
            if (Physics.Raycast(pos, transform.forward, out Hit, VehicleSensorDistance, layer))
            {

                // nếu va chạm thì lùi lại
                if (Hit.distance <= VehicleAvoidDistance * 0.5f) isCarReverse = true;
                else
                    if (Hit.distance >= VehicleAvoidDistance) isCarReverse = false;

                //tránh xe
                if (Hit.distance <= VehicleAvoidDistance)
                {
                    flag++;
                    avoidSensitivy += 1f;
                    Debug.DrawLine(pos, Hit.point, Color.yellow);
                }

                // nếu cùng chiều và cùng waypoint đã từng qua thì nhường đường cho xe đi trước
                if (!StopForAnotherCar)
                {
                    var FrontCar = Hit.collider.GetComponentInParent<AICarController>();
                    if (FrontCar != null && isNextOrEqual(FrontCar.currentWayPoint, currentWayPoint))
                        if (Hit.distance <= VehicleSensorDistance * 0.6f && getCurrentDirect(Hit.collider.transform.rotation.eulerAngles.y) == getCurrentDirect(transform.rotation.eulerAngles.y))
                        {
                            StopForAnotherCar = true;
                        }

                    //Debug.Log(FrontCar.currentWayPoint.name + "    " + currentWayPoint.name + "    " + isNextOrEqual(FrontCar.currentWayPoint, currentWayPoint));
                }
            }
            else
            {
                pos = transform.position + transform.up * startPointY + transform.forward * startPointZ - transform.right * WidthSensor;
                Angle = Quaternion.AngleAxis(360 - AngleSensors, transform.up) * transform.forward;
                if (Physics.Raycast(pos, Angle, out Hit, VehicleSensorDistance * 0.7f, layer))
                {
                    // nếu va chạm thì lùi lại
                    if (Hit.distance <= VehicleAvoidDistance * 0.5f) isCarReverse = true;
                    else
                        if (Hit.distance >= VehicleAvoidDistance) isCarReverse = false;

                    // tránh xe
                    if (Hit.distance <= VehicleAvoidDistance)
                    {
                        flag++;
                        avoidSensitivy += 0.5f;
                        Debug.DrawLine(pos, Hit.point, Color.yellow);
                    }

                    // nếu cùng chiều và cùng waypoint đã từng qua thì nhường đường cho xe đi trước
                    if (!StopForAnotherCar)
                    {
                        var FrontCar = Hit.collider.GetComponentInParent<AICarController>();
                        if (FrontCar != null && isNextOrEqual(FrontCar.currentWayPoint, currentWayPoint))
                            if (Hit.distance <= VehicleSensorDistance * 0.5f && getCurrentDirect(Hit.collider.transform.rotation.eulerAngles.y) == getCurrentDirect(transform.rotation.eulerAngles.y))
                            {
                                StopForAnotherCar = true;
                            }
                    }

                }
            }


            //Vehicle Front Right
            pos = transform.position + transform.up * startPointY + transform.forward * startPointZ + transform.right * WidthSensor;
            if (Physics.Raycast(pos, transform.forward, out Hit, VehicleSensorDistance, layer))
            {
                // nếu va chạm thì lùi lại
                if (Hit.distance <= VehicleAvoidDistance * 0.6f) isCarReverse = true;
                else
                      if (Hit.distance >= VehicleAvoidDistance) isCarReverse = false;

                //tránh xe
                if (Hit.distance <= VehicleAvoidDistance)
                {
                    flag++;
                    avoidSensitivy -= 1f;
                    Debug.DrawLine(pos, Hit.point, Color.yellow);
                }

                // nếu cùng chiều và cùng waypoint đã từng qua thì nhường đường cho xe đi trước
                if (!StopForAnotherCar)
                {
                    var FrontCar = Hit.collider.GetComponentInParent<AICarController>();

                    if (FrontCar != null && isNextOrEqual(FrontCar.currentWayPoint, currentWayPoint))
                        if (Hit.distance <= VehicleSensorDistance * 0.5f && getCurrentDirect(Hit.collider.transform.rotation.eulerAngles.y) == getCurrentDirect(transform.rotation.eulerAngles.y))
                        {
                            StopForAnotherCar = true;
                        }

                }
            }
            else
            {
                pos = transform.position + transform.up * startPointY + transform.forward * startPointZ + transform.right * WidthSensor;
                Angle = Quaternion.AngleAxis(AngleSensors, transform.up) * transform.forward;
                if (Physics.Raycast(pos, Angle, out Hit, VehicleSensorDistance * 0.7f, layer))
                {
                    // nếu va chạm thì lùi lại
                    if (Hit.distance <= VehicleAvoidDistance * 0.5f) isCarReverse = true;
                    else
                         if (Hit.distance >= VehicleAvoidDistance) isCarReverse = false;

                    //tránh xe
                    if (Hit.distance <= VehicleAvoidDistance)
                    {
                        flag++;
                        avoidSensitivy -= 0.5f;
                        Debug.DrawLine(pos, Hit.point, Color.yellow);
                    }

                    // nếu cùng chiều và cùng waypoint đã từng qua thì nhường đường cho xe đi trước
                    if (!StopForAnotherCar)
                    {
                        var FrontCar = Hit.collider.GetComponentInParent<AICarController>();

                        if (FrontCar != null && isNextOrEqual(FrontCar.currentWayPoint, currentWayPoint))

                            if (Hit.distance <= VehicleSensorDistance * 0.5f && getCurrentDirect(Hit.collider.transform.rotation.eulerAngles.y) == getCurrentDirect(transform.rotation.eulerAngles.y))
                            {
                                StopForAnotherCar = true;
                            }
                    }
                }
            }

            if (flag > 0 && avoidSensitivy == 0)
            {
                pos = transform.position + transform.up * startPointY;
                if (Physics.Raycast(pos, transform.forward, out Hit, SensorDistance, layer))
                {
                    if (Hit.distance <= VehicleAvoidDistance && Mathf.Abs(Hit.normal.y) <= 0.5f)
                    {
                        flag++;
                        if (Vector3.SignedAngle(Hit.normal, transform.forward, Vector3.up) < 0f)
                        {
                            avoidSensitivy += 1;
                        }
                        else avoidSensitivy -= 1;
                    }
                }
            }

            #endregion


            if (flag != 0)
            {
                var newSteer = (isCarReverse || isMovingBackward() ? -1 : 1) * MaxSteerSensors * avoidSensitivy;
                FrontLeftWheel.steerAngle = newSteer;
                FrontRightWheel.steerAngle = newSteer;

                //Debug.Log(newSteer + "    " + avoidSensitivy);
            }
            else isCarReverse = false;
        }

        private void HandleMotor()
        {
            RearLeftWheel.motorTorque = moveInput * maxAcceleration;
            RearRightWheel.motorTorque = brakeInput * maxAcceleration;
        }

        private void HandleBrake()
        {
            FrontLeftWheel.brakeTorque = brakeInput * brakeAcceleration;
            FrontRightWheel.brakeTorque = brakeInput * brakeAcceleration;
            RearLeftWheel.brakeTorque = brakeInput * brakeAcceleration;
            RearRightWheel.brakeTorque = brakeInput * brakeAcceleration;
        }

        private void UpdateWheels()
        {
            UpdateWheelPos(FrontLeftWheel, FrontLeftTrans);
            UpdateWheelPos(FrontRightWheel, FrontRightTrans);
            UpdateWheelPos(RearLeftWheel, RearLeftTrans);
            UpdateWheelPos(RearRightWheel, RearRightTrans);
        }

        private void UpdateWheelPos(WheelCollider wheelCollider, Transform trans)
        {
            Vector3 pos;
            Quaternion rot;
            wheelCollider.GetWorldPose(out pos, out rot);
            trans.rotation = rot;
            trans.position = pos;
        }

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(TargetLocal, 3f);
        }
    }

}