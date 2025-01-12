using Lockstep.Math;
using System;
using UnityEngine;
using UnityEngine.Windows;
using Debug = Lockstep.Logging.Debug;

namespace LockstepTutorial {
    [Serializable]
    public class Enemy : Entity {
        public CBrain brain = new CBrain();

        public Enemy(){
            moveSpd = 2;
            turnSpd = 150;
            RegisterComponent(brain);
        }

        public override void DoUpdate(LFloat deltaTime)
        {
            base.DoUpdate(deltaTime);
        }

        protected override void OnDead(){
            EnemyManager.Instance.RemoveEnemy(this);
        }
    }
}