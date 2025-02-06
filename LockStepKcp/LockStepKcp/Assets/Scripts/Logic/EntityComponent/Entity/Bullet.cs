using System;
using Lockstep.Collision2D;
using Lockstep.ECS.ECDefine;
using Lockstep.Game;
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
        private LFloat aliveTime = 0;
        public LFloat deadTime;
        public string deadEffectName;
        public LVector2 deadEffectPosition;
        private bool isCollided = false;
        // 用于config构造
        public Bullet()
        {
            RegisterComponent(Cfly);
        }

        public Bullet(Entity owner)
        {
            this.Owner = owner;
            OnTriggerEvent = OnCollision;
            transform.forward = owner.transform.forward;
            RegisterComponent(Cfly);
        }

        public override void DoStart()
        {
            base.DoStart();
        }
        public override void DoUpdate(LFloat deltaTime)
        {
            base.DoUpdate(deltaTime);
            aliveTime += deltaTime;
            if (TryDead())
            {
                OnDead();
            }

        }

        public virtual bool TryDead()
        {
            if (deadTime > 0)
            {
                if (aliveTime >= deadTime)
                {
                    return true;
                }
            }
            else
            {
                if (transform.pos.x.Abs() > 70 || isCollided)
                {
                    return true;
                }
            }
            return false;

            //switch (PrefabId)
            //{
            //    case 20:
            //        if (transform.pos.x.Abs() > 70 || isCollided)
            //        {
            //            return true;
            //        }
            //        break;
            //    case 21:
            //        if (aliveTime >= deadTime)
            //        {
            //            return true;
            //        }
            //        break;
            //    case 21:
            //        if (aliveTime >= deadTime)
            //        {
            //            return true;
            //        }
            //        break;
            //    default:
            //        if (transform.pos.x.Abs() > 70 || isCollided)
            //        {
            //            return true;
            //        }
            //        break;

            //}

            //return false;
        }

        protected override void OnDead()
        {
            BulletManager.Instance.RemoveBullet(this);
            CollisionManager.Instance.RemoveCollider(this);
            EntityView.OnDead();
        }


        public void OnCollision(ColliderProxy other, ECollisionEvent type)
        {
            if (type == ECollisionEvent.Enter)
            {
                if (other.Entity == Owner)
                {
                    return;
                }

                other.Entity.TakeDamage(damage, other.Entity.transform.Pos3, true);
                other.Entity.rigidbody.AddImpulse(new LVector3(1, 3, 0) * Cfly.flyDir);
                ////deadTime == 0说明是由碰撞来控制何时死亡
                //if (deadTime ==0)
                //{
                    isCollided = true;
                //}


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