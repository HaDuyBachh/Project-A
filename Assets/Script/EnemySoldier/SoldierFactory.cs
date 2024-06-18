using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace HaDuyBach
{
    public class SoldierFactory
    {
        SoldierControl _context;
        private enum State
        {
            Crouch,
            Fall,
            Ground,
            Gruard,
            Patrol,
            Melee,
            Gun,
            Idle,
            Walk,
            Run,
            Shield,
            ShieldCover,
            ShieldAttack,
            GunAttack,
            MeleeAttack,
            NoAttack,
            Find,
            MeleeCover,
            GunCover,
            Dead,
        }

        private readonly Dictionary<State, SoldierBaseState> S = new();

        public SoldierFactory(SoldierControl currentContext)
        {
            _context = currentContext;
        }

        #region Lazy Init
        public SoldierBaseState Crouch()
        {
            if (!S.ContainsKey(State.Crouch)) S[State.Crouch] = new SoldierCrouchState(_context, this);
            return S[State.Crouch];
        }
        public SoldierBaseState Fall()
        {
            if (!S.ContainsKey(State.Fall)) S[State.Fall] = new SoldierFallState(_context, this);
            return S[State.Fall];
        }
        public SoldierBaseState Ground()
        {
            if (!S.ContainsKey(State.Ground)) S[State.Ground] = new SoldierGroundState(_context, this);
            return S[State.Ground];
        }
        public SoldierBaseState Idle()
        {
            if (!S.ContainsKey(State.Idle)) S[State.Idle] = new SoldierIdleState(_context, this);
            return S[State.Idle];
        }
        public SoldierBaseState Walk()
        {
            if (!S.ContainsKey(State.Walk)) S[State.Walk] = new SoldierWalkState(_context, this);
            return S[State.Walk];
        }
        public SoldierBaseState Run()
        {
            if (!S.ContainsKey(State.Run)) S[State.Run] = new SoldierRunState(_context, this);
            return S[State.Run];
        }
        public SoldierBaseState ShieldCover()
        {
            if (!S.ContainsKey(State.ShieldCover)) S[State.ShieldCover] = new SoldierShieldCoverState(_context, this);
            return S[State.ShieldCover];
        }
        public SoldierBaseState GunAttack()
        {
            if (!S.ContainsKey(State.GunAttack)) S[State.GunAttack] = new SoldierGunAttackState(_context, this);
            return S[State.GunAttack];
        }
        public SoldierBaseState MeleeAttack()
        {
            if (!S.ContainsKey(State.MeleeAttack)) S[State.MeleeAttack] = new SoldierMeleeAttackState(_context, this);
            return S[State.MeleeAttack];
        }
        public SoldierBaseState Guard()
        {
            if (!S.ContainsKey(State.Gruard)) S[State.Gruard] = new SoldierGuardState(_context, this);
            return S[State.Gruard];
        }
        public SoldierBaseState Patrol()
        {
            if (!S.ContainsKey(State.Patrol)) S[State.Patrol] = new SoldierPatrolState(_context, this);
            return S[State.Patrol];
        }
        public SoldierBaseState Find()
        {
            if (!S.ContainsKey(State.Find)) S[State.Find] = new SoldierFindEnemyState(_context, this);
            return S[State.Find];
        }
        public SoldierBaseState Melee()
        {
            if (!S.ContainsKey(State.Melee)) S[State.Melee] = new SoldierMeleeState(_context, this);
            return S[State.Melee];
        }
        public SoldierBaseState NoAttack()
        {
            if (!S.ContainsKey(State.NoAttack)) S[State.NoAttack] = new SoldierNoAttackState(_context, this);
            return S[State.NoAttack];
        }
        public SoldierBaseState MeleeCover()
        {
            if (!S.ContainsKey(State.MeleeCover)) S[State.MeleeCover] = new SoldierMeleeCoverState(_context, this);
            return S[State.MeleeCover];
        }
        public SoldierBaseState Gun()
        {
            if (!S.ContainsKey(State.Gun)) S[State.Gun] = new SoldierGunState(_context, this);
            return S[State.Gun];
        }
        public SoldierBaseState GunCover()
        {
            if (!S.ContainsKey(State.GunCover)) S[State.GunCover] = new SoldierGunCoverState(_context, this);
            return S[State.GunCover];
        }

        public SoldierBaseState Dead()
        {
            if (!S.ContainsKey(State.Dead)) S[State.Dead] = new SoldierDeadState(_context, this);
            return S[State.Dead];
        }

        #endregion
    }
}