using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class CarControl : MonoBehaviour
    {
        [Header("Wheel Control")]
        public WheelCollider FrontLeftWheel;
        public WheelCollider FrontRightWheel;
        public WheelCollider RearLeftWheel;
        public WheelCollider RearRightWheel;

        public Transform FrontLeftTrans;
        public Transform FrontRightTrans;
        public Transform RearLeftTrans;
        public Transform RearRightTrans;

        [Header("Car Setting")]
        public float brakeAcceleration;
        public float maxAcceleration;
        public float maxSteerAngle;
        public float MaxSpeed = 35.0f;
        public float BrakeSpeed = 20.0f;
        public Vector3 _centerOfMass = new Vector3(0, 0.4f, 0);

        [Header("Sit & Leave")]
        public Transform Sit;
        public Transform Leave;

        private float _moveInput;
        private float _steerInput;
        private float brakeInput;
        private float speed;
        private bool _isPlayerOut = true;

        #region Setter & Getter

        public float MoveInput { get { return _moveInput; } set { _moveInput = value; } }
        public float SteerInput { get { return _steerInput; } set { _steerInput = value; } }
        public bool IsPlayerOut { get { return _isPlayerOut; } set { _isPlayerOut = value; } }

        #endregion

        Rigidbody CarRb;

        private void Start()
        {
            CarRb = GetComponent<Rigidbody>();
            CarRb.centerOfMass = _centerOfMass;
        }

        private void FixedUpdate()
        {
            speed = CarRb.velocity.magnitude;
            HandleMotor();
            HandleSteering();
            HandleBrake();
            UpdateWheels();
        }

        public void GetPCInput()
        {
            _moveInput = Input.GetAxis("Vertical");
            _steerInput = Input.GetAxis("Horizontal");
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

        void HandleMotor()
        {
            if (speed > MaxSpeed) return;
            RearLeftWheel.motorTorque = _moveInput * maxAcceleration;
            RearRightWheel.motorTorque = _moveInput * maxAcceleration;
        }

        void HandleBrake()
        {
            if ((speed > MaxSpeed) || (_moveInput == -1 && isMovingForward()) || (_moveInput == 1 && isMovingBackward()) || (speed > BrakeSpeed && _steerInput != 0))
            {
                Debug.Log("đang brake");
                brakeInput = (speed > MaxSpeed) ? 0.5f : 1f;
            }
            else brakeInput = 0;

            if (_isPlayerOut) brakeInput = 10;

            FrontLeftWheel.brakeTorque = brakeInput * brakeAcceleration;
            FrontRightWheel.brakeTorque = brakeInput * brakeAcceleration;
            RearLeftWheel.brakeTorque = brakeInput * brakeAcceleration;
            RearRightWheel.brakeTorque = brakeInput * brakeAcceleration;
        }

        void HandleSteering()
        {
            FrontLeftWheel.steerAngle = _steerInput * maxSteerAngle;
            FrontRightWheel.steerAngle = _steerInput * maxSteerAngle;
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

    }
}