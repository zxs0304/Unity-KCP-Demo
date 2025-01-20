using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine;

namespace LockstepTutorial{    
    public enum EColliderLayer {
        Static,
        Enemy,
        Hero,
        EnumCount
    }
    public class HeroManager : UnityBaseManager {
        public static HeroManager Instance;
        public override void DoStart(){ }
        public static GameObject InstantiateEntity(Player entity, int prefabIndex, LVector3 position){
            var prefab = ResourceManager.LoadPrefab(prefabIndex);
            var config = ResourceManager.GetPlayerConfig(prefabIndex);
            var obj = UnityEntityService.CreateEntity(entity, prefabIndex, position, prefab, config);
            return obj;
        }


        public override void DoUpdate(LFloat deltaTime){
            foreach (var player in GameManager.allPlayers) {
                player.DoUpdate(deltaTime);
            }
        }
    }
}