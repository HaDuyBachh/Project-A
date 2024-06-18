using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public abstract class PlayerBaseState
    {
        private bool _isRootState = false;
        private ThirdPersonController _ctx;
        private PlayerStateFactory _factory;
        private PlayerBaseState _currentSuperState;
        private PlayerBaseState _currentSubState;
        /// <summary>
        /// Sử dụng để xác định chương trình đang trong lượt thoát để không thực hiện update nữa <br></br>
        /// Xem lại phần giải thích BUG trong ExitStates để biết thêm
        /// </summary>
        private bool isExit = false;

        protected bool IsRootState {  set { _isRootState = value; } }
        protected ThirdPersonController Ctx { get { return _ctx; } }
        protected PlayerStateFactory Factory {  get { return _factory; } }
        protected PlayerBaseState CurrentSuperState { get { return _currentSuperState; } }
        protected PlayerBaseState(ThirdPersonController currentContext, PlayerStateFactory playerStateFactory)
        {
            _ctx = currentContext;
            _factory = playerStateFactory;
        }
        public abstract void EnterState();
        public abstract void UpdateState();
        public abstract void ExitState();
        public abstract void CheckSwitchState();
        public abstract void InitializaSubState();

        public void EnterStates()
        {
            Debug.Log("Đang vào " + this);

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
            if (_currentSubState != null & !isExit)
            {
                _currentSubState.UpdateStates();
            }    
        }
        public void ExitStates()
        {
            Debug.Log("Đang thoát " + this);

            ExitState();

            if (_currentSubState != null)
            {
                _currentSubState.ExitStates();
            }

            #region Giải thích lí do đặt bằng null ở đây (BUG quan trọng trong thiết kế này)
            // Giả sử đây là state gốc,
            // sau khi ta update() ở vị trí nào thì sẽ ngừng thực hiện tiến độ UpdateStates() lớn của chuỗi update() luôn
            // tránh việc sang nhầm state cũ và bị lỗi lặp đi lặp lại việc chuyển qua lại state ấy
            // - Lưu ý: Câu lệnh này cực kỳ quan trọng trong thiết kế này, nếu không có câu lệnh thiết kế vẫn hoạt động
            //          nhưng sai hoàn toàn bản chất dẫn đến lầm tưởng sai lầm trong quá trình thực hiện các hành động khác nhau!
            #endregion
            _currentSubState = null;
            isExit = true;
        }
        protected void SwitchState(PlayerBaseState newState) 
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
        protected void SetSuperState(PlayerBaseState NewSuperState) 
        {
            _currentSuperState = NewSuperState;
        }
        protected void SetSubState(PlayerBaseState NewSubState) 
        {
            _currentSubState = NewSubState;
            NewSubState.SetSuperState(this);
        }
    }
}
