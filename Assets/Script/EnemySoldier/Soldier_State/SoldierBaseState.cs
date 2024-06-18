using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public abstract class SoldierBaseState
    {
        private bool _isRootState = false;
        private SoldierControl _ctx;
        private SoldierFactory _factory;
        private SoldierBaseState _currentSuperState;
        private SoldierBaseState _currentSubState;
        private bool isExit = false;

        protected bool IsRootState { set { _isRootState = value; } }
        protected SoldierControl Ctx { get { return _ctx; } }
        protected SoldierFactory Factory { get { return _factory; } }
        protected SoldierBaseState CurrentSuperState { get { return _currentSuperState; } }
        protected SoldierBaseState(SoldierControl currentContext, SoldierFactory StateFactory)
        {
            _ctx = currentContext;
            _factory = StateFactory;
        }
        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
        public abstract void CheckSwitchState();
        public abstract void InitializaSubState();

        public void EnterStates()
        {
            //Debug.Log("Đang vào " + this);
            EnterState();
            isExit = false;
            if (_currentSubState != null)
            {
                _currentSubState.EnterStates();
            }
        }
        public void UpdateStates()
        {
            UpdateState();
            if (_currentSubState != null && !isExit)
            {
                _currentSubState.UpdateStates();
            }
        }
        public void ExitStates()
        {
            //Debug.Log("Đang thoát " + this);
            ExitState();
            if (_currentSubState != null)
            {
                _currentSubState.ExitStates();
            }

            _currentSubState = null;
            isExit = true;
        }
        protected void SwitchState(SoldierBaseState newState)
        {
            ExitStates();

            newState.EnterStates();

            if (_isRootState)
            {
                Ctx.CurrentState = newState;
            }
            else
            if (_currentSuperState != null)
            {
                _currentSuperState.SetSubState(newState);
            }

        }
        protected void SetSuperState(SoldierBaseState NewSuperState)
        {
            _currentSuperState = NewSuperState;
        }
        protected void SetSubState(SoldierBaseState NewSubState)
        {
            _currentSubState = NewSubState;
            NewSubState.SetSuperState(this);
        }
    }
}
