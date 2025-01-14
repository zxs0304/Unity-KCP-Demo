using System;
using Lockstep.Collision2D;
using Lockstep.Logging;
using Lockstep.Logic;
using Lockstep.Math;
namespace LockstepTutorial
{

    public class Bullet : BaseEntity
    {
        public IBulletView BulletView;
        public LFloat moveSpd = 10;
        public int damage = 10;
        public Entity entity; //创建者

        public Bullet()
        {

            OnTriggerEvent = OnCollision;
        }

        public void OnCollision(ColliderProxy other, ECollisionEvent type)
        {

            if (type == ECollisionEvent.Enter && other.LayerType == (int)EColliderLayer.Enemy)
            {

            }
            if (type == ECollisionEvent.Stay && other.LayerType == (int)EColliderLayer.Enemy)
            {

            }
            if (type == ECollisionEvent.Exit && other.LayerType == (int)EColliderLayer.Enemy)
            {
                Debug.Log($"player碰撞exit ");
            }

        }
    }

}