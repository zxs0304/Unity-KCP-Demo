using Lockstep.Collision2D;
using Lockstep.Logic;
using Lockstep.Math;
using Lockstep.Util;
using UnityEngine;

namespace LockstepTutorial {
    public class UnityEntityService {
        public static GameObject CreateEntity(BaseEntity entity, int prefabId, LVector3 position, GameObject prefab,
            object config){
            var obj = (GameObject) GameObject.Instantiate(prefab, position.ToVector3(), Quaternion.identity);
            entity.engineTransform = obj.transform;
            entity.transform.Pos3 = position;
            config.CopyFiledsTo(entity);
            //当config为EntityConfig时，会导致Player.localId重置为0，导致外面后续用到localId来设置摄像头就会出错
            //config.CopyTo(entity); 
            entity.PrefabId = prefabId;
            CollisionManager.Instance.RegisterEntity(prefab, obj, entity);
            entity.DoAwake();
            entity.DoStart();
            var views = obj.GetComponents<IView>();
            foreach (var view in views) {
                view.BindEntity(entity);
            }
            return obj;
        }
    }
}