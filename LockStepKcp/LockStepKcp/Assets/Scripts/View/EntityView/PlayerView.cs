using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;
using UnityEngine.UI;

namespace LockstepTutorial {
    public partial class PlayerView : EntityView, IPlayerView
    {
        public Player player;
        public override void BindEntity(BaseEntity entity)
        {
            base.BindEntity(entity);
            player = entity as Player;
            player.rigidbody.OnLandEvent += PlayerOnLand;
            player.OnEndHurt += PlayerOnEndHurt;
        }

        public override void Update()
        {
            base.Update();

            if (player.isHurting)
            {
                animator.Play("Hurt");
            }
            animator.SetBool("Walking", player.mover.needMove);
            animator.SetBool("OnFloor", player.rigidbody.isOnFloor);

            if (player.input.isJump)
            {
                animator.Play("Jump");
            }
            if (player.input.skillId != -1)
            {
                switch (player.input.skillId)
                {
                    case 0:
                        animator.Play("Attack");
                        break;
                    case 1:
                        animator.Play("Sprint");
                        break;
                    case 2:
                        animator.Play("FireBall");
                        break;
                }
            }



        }

        public void PlayerOnLand()
        {
            animator.Play("Idle");
        }
        public void PlayerOnEndHurt()
        {
            animator.Play("Idle");
        }

    }
}