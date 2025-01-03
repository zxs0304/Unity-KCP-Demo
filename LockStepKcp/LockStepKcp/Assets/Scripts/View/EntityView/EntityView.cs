using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;

namespace LockstepTutorial {
    public abstract class EntityView : MonoBehaviour, IEntityView {
        public UIFloatBar uiFloatBar;
        public Entity entity;
        protected bool isDead => entity?.isDead ?? true;

        public virtual void BindEntity(BaseEntity entity){
            this.entity = entity as Entity;
            this.entity.EntityView = this;
            uiFloatBar = FloatBarManager.CreateFloatBar(transform, this.entity.curHealth, this.entity.maxHealth);
            transform.position = this.entity.transform.Pos3.ToVector3();
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
            var pos = entity.transform.Pos3.ToVector3();
            transform.position = Vector3.Lerp(transform.position, pos, 0.3f);
            var deg = entity.transform.deg.ToFloat();
            //deg = Mathf.Lerp(transform.rotation.eulerAngles.y, deg, 0.3f);
            transform.rotation = Quaternion.Lerp(transform.rotation, Quaternion.Euler(0, deg, 0), 0.3f);
        }    
        
        private void OnDrawGizmos(){
            if (entity.skillBox.isFiring) {
                var skill = entity.skillBox.curSkill;
                skill?.OnDrawGizmos();
            }
        }
    }
}