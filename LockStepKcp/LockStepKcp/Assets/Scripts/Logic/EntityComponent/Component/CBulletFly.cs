using System;
using Lockstep.Collision2D;
using Lockstep.Logic;
using Lockstep.Math;


namespace LockstepTutorial
{
    [Serializable]
    public class CBulletFly : Component
    {
        public Bullet bullet { get; private set; }
        public LVector2 flyDir;
        public LFloat flySpeed;

        //BindEntity的时候，entity还没赋值属性，最好此时只做绑定这一个事，不要做一些数值操作
        public override void BindEntity(BaseEntity entity)
        {
            base.BindEntity(entity);
            bullet = (Bullet)entity;
        }

        public override void DoStart()
        {
            base.DoStart();
            flySpeed = bullet.moveSpd;
            flyDir = bullet.Owner.transform.forward;
        }

        public override void DoUpdate(LFloat deltaTime)
        {
            transform.pos = transform.pos + flyDir * flySpeed * deltaTime;
        }
    }
}