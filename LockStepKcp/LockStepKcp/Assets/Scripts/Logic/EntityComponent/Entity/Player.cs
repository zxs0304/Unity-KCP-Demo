using System;
using System.Collections;
using System.Collections.Generic;
using Lockstep;
using Lockstep.Collision2D;
using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;


namespace LockstepTutorial {
    [Serializable]
    public partial class Player : Entity {
        public int localId;
        public PlayerInput input = new PlayerInput();
        public CMover mover = new CMover();
        public bool isHurting = false;
        public LFloat hurtTime = new LFloat(true,150);
        public LFloat hurtTimer = 0;
        public Action OnEndHurt;
        public Action OnStartHurt;

        public Player(){
            RegisterComponent(mover);
            OnTriggerEvent = OnCollision;
        }

        public override void DoUpdate(LFloat deltaTime){

            if (isHurting)
            {
                hurtTimer += deltaTime;
                if (hurtTimer >= hurtTime)
                {
                    isHurting = false;
                    hurtTimer = 0;
                    OnEndHurt();
                }
                else
                {
                    return;
                }
            }

            base.DoUpdate(deltaTime);
            if (input.skillId != -1) {
                Fire(input.skillId);
            }


        }

        public void OnCollision(ColliderProxy other, ECollisionEvent type)
        {
          
            if (type == ECollisionEvent.Enter )
            {
                Debug.Log($"Player碰撞enter ");
                if (skillBox.curSkill?.AnimName == "Sprint" && skillBox.isFiring)
                {
                    other.Entity.TakeDamage(2,transform.Pos3,false);
                    other.Entity.rigidbody.AddImpulse(new LVector3(true,0,5000,0));
                }
            }
            if (type == ECollisionEvent.Stay && other.LayerType == (int)EColliderLayer.Enemy)
            {
                Debug.Log($"Player碰撞stay");

            }
            if (type == ECollisionEvent.Exit && other.LayerType == (int)EColliderLayer.Enemy)
            {
                Debug.Log($"player碰撞exit ");
            }

        }

        protected override void OnTakeDamage(int amount, LVector3 hitPoint,bool pauseFrame)
        {
            base.OnTakeDamage(amount, hitPoint, pauseFrame);

            if (pauseFrame)
            {
                OnStartHurt?.Invoke();
                GameManager.Instance.HitPause(7);
                GameManager.Instance.CameraShake(0.1f, 0.1f);
            }


            isHurting = true;
            hurtTimer = 0;
        }

    }
}