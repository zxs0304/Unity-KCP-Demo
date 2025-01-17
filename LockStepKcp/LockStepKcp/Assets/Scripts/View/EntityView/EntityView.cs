using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;

namespace LockstepTutorial {
    //Entity里面只有数据，EntityView 就是把Entity的数据给呈现出来
    public abstract class EntityView : MonoBehaviour, IEntityView {
        public UIFloatBar uiFloatBar;
        public Entity entity;
        protected bool isDead => entity?.isDead ?? true;

        public virtual void BindEntity(BaseEntity entity){
            this.entity = entity as Entity;
            this.entity.EntityView = this;
            if (this.entity.showFloatBar)
            {
                uiFloatBar = FloatBarManager.CreateFloatBar(transform, this.entity.curHealth, this.entity.maxHealth);
            }
            //TestRigidBody
            //transform.position = this.entity.transform.Pos3.ToVector3();
            transform.position = this.entity.transform.Pos3.ToVector3_2D();
        }

        public virtual void OnTakeDamage(int amount, LVector3 hitPoint){
            uiFloatBar.UpdateHp(entity.curHealth, entity.maxHealth);
            FloatTextManager.CreateFloatText(hitPoint.ToVector3(), -amount);
        }

        public virtual void OnDead(){
            if (uiFloatBar != null) FloatBarManager.DestroyText(uiFloatBar);
            GameObject.Destroy(gameObject);
        }

        private void Update(){
            //TestR
            //var pos = entity.transform.Pos3.ToVector3();
            var pos = entity.transform.Pos3.ToVector3_2D();


            transform.position = Vector3.Lerp(transform.position, pos, 0.37f);


            //TEST2d
            //var deg = entity.transform.deg.ToFloat();
            //transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, deg, 0), 0.3f);
        }

        private void OnDrawGizmos(){
            if (entity.skillBox.isFiring) {
                var skill = entity.skillBox.curSkill;
                skill?.OnDrawGizmos();
            }
        }
    }
}