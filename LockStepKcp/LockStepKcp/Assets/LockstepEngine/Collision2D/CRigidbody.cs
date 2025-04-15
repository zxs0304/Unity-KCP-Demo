using System;
using Lockstep.Collision2D;
using Lockstep.Logging;
using Lockstep.Math;
using UnityEngine;

namespace Lockstep.Logic {
    public delegate void OnFloorResultCallback(bool isOnFloor);
    public delegate void OnLandEvent();
    [Serializable]
    public class CRigidbody {
        public CTransform2D transform { get; private set; }
        public static LFloat G = new LFloat(12);
        public static LFloat MinSleepSpeed = new LFloat(true, 100);
        public static LFloat FloorFriction = new LFloat(20);
        public static LFloat MinYSpd = new LFloat(-10);
        public static LFloat FloorY = LFloat.zero;
        
        public OnFloorResultCallback OnFloorEvent;
        public OnLandEvent OnLandEvent;

        public LVector3 Speed;
        public LFloat Mass = LFloat.one;
        public bool isEnable = true;
        public bool isSleep = false;
        public bool isOnFloor;
        public bool lastFrameOnFloor; //上一帧是否在地面上

        public void Init(CTransform2D transform2D){
            this.transform = transform2D;
        }

        //private int __id;
        //private static int __idCount;
        public void DoStart(){
            //__id = __idCount++;
            LFloat y = LFloat.zero;
            isOnFloor = TestOnFloor(transform.Pos3, ref y);
            Speed = LVector3.zero;
            isSleep = isOnFloor;
        }

        public void DoUpdate(LFloat deltaTime){
            if (!isEnable) return;
            if (!TestOnFloor(transform.Pos3)) {
                isSleep = false;
            }

            if (!isSleep) {
                if (!isOnFloor) {
                    Speed.y -= G * deltaTime;
                    Speed.y = LMath.Max(MinYSpd, Speed.y);
                }

                var pos = transform.Pos3;
                pos += Speed * deltaTime;
                LFloat y = pos.y;
                //Test floor
                isOnFloor = TestOnFloor(transform.Pos3, ref y);
                if (isOnFloor && Speed.y <=0) {
                    Speed.y = LFloat.zero;
                }

                if (Speed.y <= 0) {
                    pos.y = y;
                }

                //Test walls
                if (TestOnWall(ref pos)) {
                    Speed.x = LFloat.zero;
                    Speed.z = LFloat.zero;
                }
                if (isOnFloor)
                {
                    var speedVal = Speed.magnitude - FloorFriction * deltaTime;
                    speedVal = LMath.Max(speedVal, LFloat.zero);
                    Speed = Speed.normalized * speedVal;
                    if (speedVal < MinSleepSpeed) {
                        isSleep = true;
                    }
                }


                if (isOnFloor && !lastFrameOnFloor)
                {
                    UnityEngine.Debug.Log("落地");
                    OnLandEvent?.Invoke();
                }
                lastFrameOnFloor = isOnFloor;

                transform.Pos3 = pos;
            }
        }


        public void AddImpulse(LVector3 force){
            isSleep = false;
            Speed += force / Mass;
            //Debug.Log(__id+ " AddImpulse " + force  +" after " + Speed);
        }
        public void ResetSpeed(LFloat ySpeed){
            Speed = LVector3.zero;
            Speed.y = ySpeed;
        }
        public void ResetSpeed(){
            Speed = LVector3.zero;
        }

        private bool TestOnFloor(LVector3 pos, ref LFloat y){
            var onFloor = pos.y <= 0;//TODO check with scene
            if (onFloor) {
                y = LFloat.zero;
            }

            LayerMask mask = LayerMask.GetMask("Floor");
            // 定义射线的起点和方向
            UnityEngine.Ray2D ray = new UnityEngine.Ray2D(transform.pos.ToVector2(),Vector2.down); // 使用2D射线
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, .5f, mask); // 射线检测

            if (hit.collider != null)
            {
                UnityEngine.Debug.Log("Hit: " + hit.collider.name);
                onFloor = true;
            }

            return onFloor;
        }

        private bool TestOnFloor(LVector3 pos){
            var onFloor = pos.y <= 0;//TODO check with scene

            LayerMask mask = LayerMask.GetMask("Floor");
            // 定义射线的起点和方向
            UnityEngine.Ray2D ray = new UnityEngine.Ray2D(transform.pos.ToVector2(), Vector2.down); // 使用2D射线
            RaycastHit2D hit = Physics2D.Raycast(ray.origin, ray.direction, .5f, mask); // 射线检测

            if (hit.collider != null)
            {
                UnityEngine.Debug.Log("在地面");
                onFloor = true;
            }

            return onFloor;
        }

        private bool TestOnWall(ref LVector3 pos){
            return false;//TODO check with scene
        }
        
    }
}