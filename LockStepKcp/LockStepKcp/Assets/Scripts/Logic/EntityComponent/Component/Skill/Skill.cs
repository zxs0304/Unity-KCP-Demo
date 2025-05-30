#define OPEN_DEBUG_SKILL
#if OPEN_DEBUG_SKILL && UNITY_EDITOR
#define DEBUG_SKILL
#endif
using System;
using System.Collections.Generic;
using Lockstep.Collision2D;
using Lockstep.Logic;
using Lockstep.Math;
using LockstepTutorial;
#if UNITY_EDITOR
using UnityEngine;
#endif
using Debug = Lockstep.Logging.Debug;

namespace LockstepTutorial {
    public interface ISkillEventHandler {
        void OnSkillStart(Skill skill);
        void OnSkillDone(Skill skill);
        void OnSkillPartStart(SkillPart skill);
    }

    [Serializable]
    public class Skill {
        public enum ESkillState {
            Idle,
            Firing,
        }

        private static readonly HashSet<ColliderProxy> _tempTargets = new HashSet<ColliderProxy>();

        public ISkillEventHandler eventHandler;
        public Entity entity { get; private set; }
        public SkillInfo SkillInfo;
        public LFloat CD => SkillInfo.CD;
        public LFloat DoneDelay => SkillInfo.doneDelay;
        public List<SkillPart> Parts => SkillInfo.parts;
        public int TargetLayer => SkillInfo.targetLayer;
        public LFloat MaxPartTime => SkillInfo.maxPartTime;
        public string AnimName => SkillInfo.animName;

        public LFloat CdTimer;
        public ESkillState _state;
        private LFloat _skillTimer;
        private SkillPart _curPart;

#if DEBUG_SKILL
        private float _showTimer;
#endif

        public void ForceStop(){ }

        public void DoStart(Entity entity, SkillInfo info, ISkillEventHandler eventHandler){
            this.entity = entity;
            this.SkillInfo = info;
            this.eventHandler = eventHandler;
            _skillTimer = MaxPartTime;
            _state = ESkillState.Idle;
            _curPart = null;
        }


        public bool Fire(){
            if (CdTimer <= 0 && _state == ESkillState.Idle) {
                CdTimer = CD;
                _skillTimer = LFloat.zero;
                foreach (var part in Parts) {
                    part.counter = 0;
                }

                _state = ESkillState.Firing;
                entity.animator?.Play(AnimName);
                ((Player) entity).mover.needMove = false;
                OnFire();
                return true;
            }

            return false;
        }

        public void OnFire(){
            eventHandler.OnSkillStart(this);
        }

        public void Done(){
            eventHandler.OnSkillDone(this);
            _state = ESkillState.Idle;
            entity.animator?.Play(AnimDefine.Idle);
        }

        public void DoUpdate(LFloat deltaTime){
            CdTimer -= deltaTime;
            _skillTimer += deltaTime;
            if (_skillTimer < MaxPartTime) {
                foreach (var part in Parts) {
                    CheckSkillPart(part);
                }

                if (_curPart != null && _curPart.moveSpd != 0) {
                    entity.transform.pos += _curPart.moveSpd * deltaTime * entity.transform.forward;
                }
            }
            else {
                _curPart = null;
                if (_state == ESkillState.Firing) {
                    Done();
                }
            }
        }

        void CheckSkillPart(SkillPart part){
            if (part.counter > part.otherCount) return;
            if (_skillTimer > part.NextTriggerTimer()) {
                TriggerPart(part);
                part.counter++;
            }
        }

        void TriggerPart(SkillPart part){
            eventHandler.OnSkillPartStart(part);
            _curPart = part;
#if DEBUG_SKILL
            _showTimer = Time.realtimeSinceStartup + 0.1f;
#endif

            var col = part.collider;
            if (col.radius > 0) {
                //circle
                CollisionManager.QueryRegion(TargetLayer, entity.transform.TransformPoint(col.pos), col.radius,
                    _OnTriggerEnter);
            }
            else {
                //aabb
                CollisionManager.QueryRegion(TargetLayer, entity.transform.TransformPoint(col.pos), col.size,
                    entity.transform.forward,
                    _OnTriggerEnter);
            }

            foreach (var other in _tempTargets) {
                other.Entity.TakeDamage(_curPart.damage, other.Entity.transform.pos.ToLVector3XY(),true);
            }

            //add force
            if (part.needForce) {
                var force = part.impulseForce;
                var forward = entity.transform.forward;
                force.x = forward.x * force.x;
                foreach (var other in _tempTargets) {
                    other.Entity.rigidbody.AddImpulse(force);
                }
            }

            if (part.isResetForce) {
                foreach (var other in _tempTargets) {
                    other.Entity.rigidbody.ResetSpeed(new LFloat(3));
                }
            }

            _tempTargets.Clear();
        }


        private void _OnTriggerEnter(ColliderProxy other){
            if (other.Entity == entity)
            {
                return;
            }
            if (_curPart.collider.IsCircle && _curPart.collider.deg > 0) {
                var deg = (other.Transform2D.pos - entity.transform.pos).ToDeg();
                var playerDeg = entity.transform.deg;
                var degDiff = playerDeg - deg;
                if (degDiff > 180)
                {
                    degDiff -= 360;
                }
                else if (degDiff < -180)
                {
                    degDiff += 360;
                }
                if (LMath.Abs(degDiff) <= _curPart.collider.deg) {
                    _tempTargets.Add(other);
                }
            }
            else {
                _tempTargets.Add(other);
            }
        }

        public void OnDrawGizmos(){
#if UNITY_EDITOR && DEBUG_SKILL
            float tintVal = 0.3f;
            Gizmos.color = new Color(0, 1.0f - tintVal, tintVal, 0.5f);
            if (Application.isPlaying)
            {
                if (entity == null) return;
                if (_curPart == null) return;
                if (_showTimer < Time.realtimeSinceStartup)
                {
                    return;
                }

                ShowPartGizmons(_curPart);
            }
            else
            {
                foreach (var part in Parts)
                {
                    if (part._DebugShow)
                    {
                        ShowPartGizmons(part);
                    }
                }
            }

            Gizmos.color = Color.red;
#endif
        }

        private void ShowPartGizmons(SkillPart part){
#if UNITY_EDITOR
            var col = part.collider;
            if (col.radius > 0)
            {
                //circle
                var pos = entity?.transform.TransformPoint(col.pos) ?? col.pos;
                Gizmos.DrawSphere(pos.ToVector3_2D(), col.radius.ToFloat());
            }
            else
            {
                //aabb
                Gizmos.color = Color.red;
                var pos = entity?.transform.TransformPoint(col.pos) ?? col.pos;

                //绘制技能范围，根据当前角色的方向
                DebugExtension.DebugLocalCube(Matrix4x4.TRS(
                        pos.ToVector3_2D(),
                        Quaternion.Euler(0,0, 0),
                        Vector3.one),
                    col.size.ToVector3_2D() * 2, Color.green);
                //Gizmos.DrawCube(pos.ToVector3_2D(), new Vector3(col.size.x, col.size.y, 1) * 2);


                //TEST2D
                //绘制技能范围，但不根据当前角色的方向
                //Gizmos.DrawCube(pos.ToVector3XZ(LFloat.one), col.size.ToVector3XZ(LFloat.one) * 2);
                //绘制技能范围，但根据当前角色的方向
                //DebugExtension.DebugLocalCube(Matrix4x4.TRS(
                //        pos.ToVector3XZ(LFloat.one),
                //        Quaternion.Euler(0, entity.transform.deg.ToFloat(), 0),
                //        Vector3.one),
                //    col.size.ToVector3XZ(LFloat.one) * 2, Color.green);
            }
#endif
        }
    }
}