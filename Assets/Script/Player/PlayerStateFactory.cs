using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class PlayerStateFactory
    {
        ThirdPersonController _context;

        public enum PlayerState
        {
            Drive,
            Jump,
            Grounded,
            Fall,
            Crouch,
            Idle,
            Walk,
            Run,
            Sprint,
            Melee,
            HandGun,
            Rifles,
            NoAttack,
            ReadyAttack,
            ReadyGunAttack,
            Attack,
            GunAttack,
        }

        readonly Dictionary<PlayerState, PlayerBaseState> S = new();
        public PlayerStateFactory(ThirdPersonController currentContext)
        {
            _context = currentContext;

            //state root
            S[PlayerState.Drive] = new PlayerDriveState(_context, this);
            S[PlayerState.Jump] = new PlayerJumpState(_context, this);
            S[PlayerState.Grounded] = new PlayerGroundedState(_context, this);
            S[PlayerState.Fall] = new PlayerFallState(_context, this);
            S[PlayerState.Crouch] = new PlayerCrouchState(_context, this);

            //state 1
            S[PlayerState.Idle] = new PlayerIdleState(_context, this);
            S[PlayerState.Walk] = new PlayerWalkState(_context, this);
            S[PlayerState.Run] = new PlayerRunState(_context, this);
            S[PlayerState.Sprint] = new PlayerSprintState(_context, this);

            //state 2
            S[PlayerState.Melee] = new PlayerMeleeState(_context, this);
            S[PlayerState.HandGun] = new PlayerHandGunState(_context, this);
            S[PlayerState.Rifles] = new PlayerRiflesState(_context, this);

            //state 3
            S[PlayerState.NoAttack] = new PlayerNoAttackState(_context, this);
            S[PlayerState.ReadyAttack] = new PlayerReadyAttackState(_context, this);
            S[PlayerState.Attack] = new PlayerAttackState(_context, this);
            S[PlayerState.GunAttack] = new PlayerGunAttackState(_context, this);
            S[PlayerState.ReadyGunAttack] = new PlayerReadyGunAttackState(_context, this);
        }

        public PlayerBaseState Idle()
        {
            return S[PlayerState.Idle];
        }
        public PlayerBaseState Walk()
        {
            return S[PlayerState.Walk];
        }
        public PlayerBaseState Run()
        {
            return S[PlayerState.Run];
        }
        public PlayerBaseState Jump()
        {
            return S[PlayerState.Jump];
        }
        public PlayerBaseState Grounded()
        {
            return S[PlayerState.Grounded];
        }
        public PlayerBaseState Fall()
        {
            return S[PlayerState.Fall];
        }
        public PlayerBaseState Crouch()
        {
            return S[PlayerState.Crouch];
        }
        public PlayerBaseState Melee()
        {
            return S[PlayerState.Melee];
        }
        public PlayerBaseState HandGun()
        {
            return S[PlayerState.HandGun];
        }
        public PlayerBaseState Rifles()
        {
            return S[PlayerState.Rifles];
        }
        public PlayerBaseState NoAttack()
        {
            return S[PlayerState.NoAttack];
        }
        public PlayerBaseState Attack()
        {
            return S[PlayerState.Attack];
        }
        public PlayerBaseState ReadyAttack()
        {
            return S[PlayerState.ReadyAttack];
        }
        public PlayerBaseState GunAttack()
        {
            return S[PlayerState.GunAttack];
        }
        public PlayerBaseState ReadyGunAttack()
        {
            return S[PlayerState.ReadyGunAttack];
        }
        public PlayerBaseState Drive()
        {
            return S[PlayerState.Drive];
        }
        public PlayerBaseState Sprint()
        {
            return S[PlayerState.Sprint];
        }
    }
}
