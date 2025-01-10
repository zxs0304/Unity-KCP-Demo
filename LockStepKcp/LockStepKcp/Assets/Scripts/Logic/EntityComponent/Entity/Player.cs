using System;
using System.Collections.Generic;
using Lockstep;
using Lockstep.Collision2D;
using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace LockstepTutorial {
    [Serializable]
    public partial class Player : Entity {
        public int localId;
        public PlayerInput input = new PlayerInput();
        public CMover mover = new CMover();

        public Player(){
            RegisterComponent(mover);
            OnTriggerEvent = OnCollision;
        }

        public override void DoUpdate(LFloat deltaTime){
            base.DoUpdate(deltaTime);
            if (input.skillId != -1) {
                Fire(input.skillId);
            }
            UnityEngine.Debug.Log($"{skillBox.isFiring}  { skillBox.GetHashCode()}");
        }

        public void OnCollision(ColliderProxy other, ECollisionEvent type)
        {
            if (type == ECollisionEvent.Enter && other.LayerType == (int) EColliderLayer.Enemy)
            {
                Debug.Log($"碰撞进入 ,isFireing:{skillBox.isFiring}  {skillBox.GetHashCode()}");

                if (skillBox.curSkill?.AnimName == "sprint" && skillBox.isFiring)
                {
                    other.Entity.rigidbody.AddImpulse(new LVector3(true,transform.forward.x * 4000,6000,0));
                }
            }
            if (type == ECollisionEvent.Stay && other.LayerType == (int)EColliderLayer.Enemy)
            {
                Debug.Log($"碰撞stay ,isFireing:{skillBox.isFiring}");

                //if (skillBox.curSkill?.AnimName == "sprint" && skillBox.isFiring)
                //{
                //    other.Entity.rigidbody.AddImpulse(new LVector3(true, transform.forward.x * 4000, 6000, 0));
                //}
            }

        }

    }
}