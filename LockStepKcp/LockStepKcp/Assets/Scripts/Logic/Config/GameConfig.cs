using System;
using System.Collections.Generic;
using System.Reflection;
using Lockstep.Logic;
using Lockstep.Math;
using Lockstep.Util;
using UnityEngine;

namespace LockstepTutorial {
    [Serializable]
    public class EntityConfig {
        public virtual object Entity { get; }
        public string prefabPath;

        public void CopyTo(object dst){
            if (Entity.GetType() != dst.GetType())
            {
                Debug.Log(Entity.GetType());
                Debug.Log(dst.GetType());
                return;
            }
            FieldInfo[] fields = dst.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields) {
                var type = field.FieldType;

                //如果属性名是localId，那么不应该拷贝config的localId，而是应该保持每个对象构造时自己的localId
                if (field.Name == "localId") 
                {
                    continue;
                }
                //如果属性是一个委托类型 (OnTriggerEvent)，那么不应该拷贝config的委托的值，否则到时候调用委托的时候，会触发config对象的函数！
                if (type.IsSubclassOf(typeof(MulticastDelegate)))
                {
                    continue;
                }
                //如果属性是一个BaseEntity (Owner)，那么不应该拷贝config的值
                if (type.IsSubclassOf(typeof(BaseEntity)))
                {
                    continue;
                }

                if (typeof(BaseComponent).IsAssignableFrom(type)
                    || typeof(CRigidbody).IsAssignableFrom(type)
                    || typeof(Transform).IsAssignableFrom(type)
                ) {
                    CopyTo(field.GetValue(dst), field.GetValue(Entity));
                }
                else 
                {
                    field.SetValue(dst, field.GetValue(Entity));
                }
            }
        }

        void CopyTo(object dst, object src){
            if (src.GetType() != dst.GetType()) {
                return;
            }

            FieldInfo[] fields = dst.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public);
            foreach (var field in fields) {
                var type = field.FieldType;
                field.SetValue(dst, field.GetValue(src));
            }
        }
    }

    [Serializable]
    public class EnemyConfig : EntityConfig {
        public override object Entity => entity;
        public Enemy entity = new Enemy();
    }

    [Serializable]
    public class PlayerConfig : EntityConfig {
        public override object Entity => entity;
        public Player entity = new Player();
    }

    [Serializable]
    public class BulletConfig : EntityConfig
    {
        public override object Entity => entity;
        public Bullet entity = new Bullet();
    }

    [CreateAssetMenu(menuName = "GameConfig")]
    public class GameConfig : ScriptableObject {
        public List<EnemyConfig> enemies = new List<EnemyConfig>();
        public List<PlayerConfig> player = new List<PlayerConfig>();
        public List<BulletConfig> bullet = new List<BulletConfig>();

        public EnemyConfig GetEnemyConfig(int id){
            return enemies[id];
        }

        public PlayerConfig GetPlayerConfig(int id){
            return player[id];
        }

        public BulletConfig GetBulletConfig(int id)
        {
            return bullet[id];
        }
    }
}