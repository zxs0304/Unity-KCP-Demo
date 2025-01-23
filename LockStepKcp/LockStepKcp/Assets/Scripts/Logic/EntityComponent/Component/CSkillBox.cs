using System;
using System.Collections.Generic;
using Lockstep.ECS.ECDefine;
using Lockstep.Logging;
using Lockstep.Logic;
using Lockstep.Math;
using UnityEngine.UIElements;

namespace LockstepTutorial {
    [Serializable]
    public class CSkillBox : Component, ISkillEventHandler {
        public SkillBoxConfig config;
        public bool isFiring;
        public Skill curSkill;
        private int curSkillIdx = 0;
        public bool canMove = true;
#if UNITY_EDITOR
        [UnityEngine.SerializeField]
#endif
        public List<Skill> skills;

        public override void DoStart(){
            base.DoStart();
            skills = new List<Skill>();
            if (config != null) {
                config.CheckInit();
                foreach (var info in config.skillInfos) {
                    var skill = new Skill();
                    skill.DoStart(entity, info, this);
                    skills.Add(skill);
                }
            }
        }

        public override void DoUpdate(LFloat deltaTime){
            foreach (var skill in skills) {
                skill.DoUpdate(deltaTime);
            }

        }

        public bool Fire(int idx){
            if (idx < 0 || idx >= skills.Count) {
                return false;
            }

            //Debug.Log("TryFire " + idx);

            if (isFiring)
            {
                UnityEngine.Debug.Log("已经在技能了 " + curSkill.AnimName);
                return false; //
            }

            var skill = skills[idx];
            if (skill.Fire()) {
                curSkillIdx = idx;
                return true;
            }

            Debug.Log($"TryFire failure {idx} {skill.CdTimer}  {skill._state}");
            return false;
        }

        public void ForceStop(int idx = -1){
            if (idx == -1) {
                idx = curSkillIdx;
            }

            if (idx < 0 || idx > skills.Count) {
                return;
            }

            if (curSkill != null) {
                if (curSkill == skills[idx]) {
                    curSkill.ForceStop();
                }
            }
        }

        public void OnSkillStart(Skill skill){
            Debug.Log("OnSkillStart " + skill.SkillInfo.animName);
            curSkill = skill;
            isFiring = true;
            entity.isInvincible = false;
            canMove = skill.SkillInfo.canMove;

            (entity as Player).OnStartSkill(skill.SkillInfo.animName);
        }

        public void OnSkillDone(Skill skill){
            Debug.Log("OnSkillDone " + skill.SkillInfo.animName);
            curSkill = null;
            curSkillIdx = -1;
            isFiring = false;
            entity.isInvincible = false;
            canMove = true;
        }

        public void OnSkillPartStart(Skill skill){
            Debug.Log("OnSkillPartStart " + skill.SkillInfo.animName);
            if (skill.SkillInfo.animName == "FireBall")
            {
                UnityEngine.Debug.Log("发射火球");
                var bullet = BulletManager.InstantiateEntity(20, entity, entity.transform.Pos3);
                BulletManager.Instance.AddBullet(bullet as Bullet);
            }
        }

        public void OnDrawGizmos(){
#if UNITY_EDITOR
            foreach (var skill in skills) {
                skill.OnDrawGizmos();
            }
#endif
        }
    }
}