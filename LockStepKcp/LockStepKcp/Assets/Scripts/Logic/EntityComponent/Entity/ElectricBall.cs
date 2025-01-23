
using Lockstep.Math;

namespace LockstepTutorial
{
    public class ElectricBall : Bullet
    {
        public override bool TryDead()
        {
            if (transform.pos.x.Abs() > 80)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
    }
}