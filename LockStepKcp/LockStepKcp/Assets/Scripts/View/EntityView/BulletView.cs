// FileName：NewClass.cs
// Author：duole-15
// Date：2025/1/14
// Des：描述
using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;

namespace LockstepTutorial
{
	public class BulletView : EntityView, IBulletView
    {
        public Bullet bullet;
        public override void BindEntity(BaseEntity entity)
        {
            base.BindEntity(entity);
            bullet = entity as Bullet;
        }

        public override void OnDead()
        {
            base.OnDead();
            SpawnEffect();
        }

        public void SpawnEffect()
        {
            if (bullet.deadEffectName != "")
            {
                var effectObj = Instantiate(Resources.Load<GameObject>($"Prefabs/Effect/{bullet.deadEffectName}"));
                effectObj.GetComponent<SpriteRenderer>().flipX = sprite.flipX;
                effectObj.transform.position = transform.position + bullet.transform.TransformDirection(bullet.deadEffectPosition).ToVector3();
            }
           
        }
    }
}

