using System;
using Lockstep.Collision2D;
using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;
namespace LockstepTutorial
{
    [Serializable]
    public class Bullet : BaseEntity
    {
        public IBulletView BulletView;
        public LFloat moveSpd = 10;
        public int damage = 10;
        public Entity owner; //创建者

        public Bullet()
        {
            OnTriggerEvent = OnCollision;
        }

        public override void DoStart()
        {
            base.DoStart();
        }
        public override void DoUpdate(LFloat deltaTime)
        {
            base.DoUpdate(deltaTime);
        }

        protected virtual void OnTakeDamage(int amount, LVector3 hitPoint) { }


        public void OnCollision(ColliderProxy other, ECollisionEvent type)
        {
            Debug.Log($"bullet碰撞 ");
            if (type == ECollisionEvent.Enter)
            {
                Debug.Log($"player碰撞enter ");
            }
            if (type == ECollisionEvent.Stay )
            {
                Debug.Log($"player碰撞exit ");
            }
            if (type == ECollisionEvent.Exit)
            {
                Debug.Log($"player碰撞exit ");
            }

        }
    }

}