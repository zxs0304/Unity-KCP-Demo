using System.Collections;
using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;

namespace LockstepTutorial {
    public class EnemyView : EntityView, IEnemyView
    {
        public Enemy enemy;
        public override void BindEntity(BaseEntity entity)
        {
            base.BindEntity(entity);
            enemy = entity as Enemy;
            enemy.rigidbody.OnLandEvent += EnemyOnLand;
            enemy.OnEndHurt += EnemyOnEndHurt;
            enemy.OnStartHurt += EnemyOnStartHurt;
        }

        public override void Update()
        {
            base.Update();
        }

        public void EnemyOnLand()
        {

        }
        public void EnemyOnEndHurt()
        {

        }
        public void EnemyOnStartHurt()
        {
            StartCoroutine(EnemyOnStartHurtReally());
        }

        IEnumerator EnemyOnStartHurtReally()
        {
            var renders = GetComponentsInChildren<MeshRenderer>();
            foreach (var render in renders)
            {
                render.material.color = Color.red;
            }
            yield return new WaitForSecondsRealtime(0.15f);
            foreach (var render in renders)
            {
                render.material.color = Color.white;
            }
        }

    }
}