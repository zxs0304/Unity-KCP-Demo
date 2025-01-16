using System;
using Lockstep.Collision2D;
using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;
namespace LockstepTutorial
{
    [Serializable]
    public class Bullet : Entity
    {
        public Entity Owner { get; private set; }//创建者
        public CBulletFly Cfly = new CBulletFly();

        public Bullet(Entity owner)
        {
            this.Owner = owner;
            OnTriggerEvent = OnCollision;
            RegisterComponent(Cfly);
        }

        public override void DoStart()
        {
            base.DoStart();
        }
        public override void DoUpdate(LFloat deltaTime)
        {
            base.DoUpdate(deltaTime);
            if (transform.pos.x.Abs() > 100)
            {
                OnDead();
                EntityView.OnDead();
            }
        }

        protected override void OnDead()
        {
            BulletManager.Instance.RemoveBullet(this);
            CollisionManager.Instance.RemoveCollider(this);
        }


        public void OnCollision(ColliderProxy other, ECollisionEvent type)
        {
            if (type == ECollisionEvent.Enter)
            {
                other.Entity.TakeDamage(damage,other.Entity.transform.Pos3);
                other.Entity.rigidbody.AddImpulse(new LVector3(0,3,0));
                Debug.Log($"bullet碰撞enter ");
            }
            if (type == ECollisionEvent.Stay )
            {
                Debug.Log($"bullet碰撞stay ");
            }
            if (type == ECollisionEvent.Exit)
            {
                Debug.Log($"bullet碰撞exit ");
            }

        }
    }

}