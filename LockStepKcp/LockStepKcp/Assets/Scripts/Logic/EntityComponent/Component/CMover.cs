using System;
using Lockstep.Collision2D;
using Lockstep.Logic;
using Lockstep.Math;

namespace LockstepTutorial {
    
    [Serializable]
    public partial class CMover : Component {
        public Player player { get;  set; }
        public PlayerInput input => player.input;
        static LFloat _sqrStopDist = new LFloat(true, 40);
        public LFloat speed => player.moveSpd;
        public bool needMove = true;

        public override void BindEntity(BaseEntity entity)
        {
            base.BindEntity(entity);
            player = (Player) entity;
        }
        
        public override void DoUpdate(LFloat deltaTime)
        {

            needMove = input.inputUV.sqrMagnitude > new LFloat(true, 10);
            if (needMove && !player.skillBox.isFiring)
            {
                LVector2 dir = new LVector2(input.inputUV.x,LFloat.zero);
                transform.pos = transform.pos + dir * speed * deltaTime;
                LFloat targetDeg = transform.deg;

                //如果dir为(0,0)时targetDeg保持不变
                dir.ToDeg_2D(ref targetDeg);

                //2D情况下直接转向
                transform.deg = targetDeg;
            }

            if (input.isJump && player.rigidbody.isOnFloor)
            {
                player.rigidbody.AddImpulse(new LVector3(true, 0, 6000, 0));
            }

        }
    }
}