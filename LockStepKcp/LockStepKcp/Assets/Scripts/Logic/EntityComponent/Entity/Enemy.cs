using Lockstep.Math;
using System;
using UnityEngine;
using UnityEngine.Windows;
using Debug = Lockstep.Logging.Debug;

namespace LockstepTutorial {
    [Serializable]
    public class Enemy : Entity {
        public CBrain brain = new CBrain();

        public bool isHurting = false;
        public LFloat hurtTime = new LFloat(true, 150);
        public LFloat hurtTimer = 0;
        public Action OnEndHurt;
        public Action OnStartHurt;
        public Enemy(){
            moveSpd = 2;
            turnSpd = 150;
            RegisterComponent(brain);
        }

        public override void DoUpdate(LFloat deltaTime)
        {
            //if (isHurting)
            //{
            //    hurtTimer += deltaTime;
            //    if (hurtTimer >= hurtTime)
            //    {
            //        isHurting = false;
            //        hurtTimer = 0;
            //        OnEndHurt();
            //    }
            //    else
            //    {
            //        return;
            //    }
            //}
            //base.DoUpdate(deltaTime);

        }

        protected override void OnDead(){
            EnemyManager.Instance.RemoveEnemy(this);
        }

        protected override void OnTakeDamage(int amount, LVector3 hitPoint, bool pauseFrame)
        {
            base.OnTakeDamage(amount, hitPoint, pauseFrame);
            if (pauseFrame)
            {
                OnStartHurt?.Invoke();

                GameManager.Instance.HitPause(8);
                GameManager.Instance.CameraShake(0.1f, 0.1f);

            }


            isHurting = true;
            hurtTimer = 0;
        }
    }
}