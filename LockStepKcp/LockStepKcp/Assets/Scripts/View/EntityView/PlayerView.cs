using System.Collections;
using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;
using UnityEngine.Analytics;
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
            player.OnStartHurt += PlayerOnStartHurt;
            player.OnStartSkill += PlayerOnStartSkill;
        }

        public override void Update()
        {
            base.Update();

            animator.SetBool("Walking", player.mover.needMove);
            animator.SetBool("OnFloor", player.rigidbody.isOnFloor);

            if (player.input.isJump)
            {
                animator.Play("Jump");
            }



        }

        public void PlayerOnStartSkill(string skillName)
        {
            animator.Play(skillName);
        }

        public void PlayerOnLand()
        {
            animator.Play("Idle");
        }
        public void PlayerOnEndHurt()
        {
            animator.Play("Idle");
        }
        public void PlayerOnStartHurt()
        {
            animator.Play("Hurt");
            StartCoroutine(PlayerOnStartHurtReally()) ;
        }

        IEnumerator PlayerOnStartHurtReally()
        {
            var render = GetComponent<SpriteRenderer>();

            render.material.color = Color.red;

            yield return new WaitForSecondsRealtime(0.15f);

            render.material.color = Color.white;
        }

    }
}