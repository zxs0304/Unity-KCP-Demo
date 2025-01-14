// FileName：NewClass.cs
// Author：duole-15
// Date：2025/1/14
// Des：描述
using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;

namespace LockstepTutorial
{
	public class BulletView : MonoBehaviour, IBulletView
    {
        public Bullet entity;

        public virtual void BindEntity(BaseEntity entity)
        {
            this.entity = entity as Bullet;
            this.entity.BulletView = this;
            transform.position = this.entity.transform.Pos3.ToVector3_2D();
        }

        private void Update()
        {
            var pos = entity.transform.Pos3.ToVector3_2D();
            transform.position = Vector3.Lerp(transform.position, pos ,0.3f);
        }

    }
}

