using Lockstep.Logic;
using Lockstep.Math;
using LockstepTutorial;
using UnityEngine;

public class AnimatorView : MonoBehaviour, IAnimatorView {
    public Animation animComp;
    public Transform rootTrans;
    public AnimationState animState;
    private CAnimator cAnim;
    private Animator animator;
    public LFloat speed;
    public Entity entity;
    void Start(){
        rootTrans = transform;
        animator = GetComponent<Animator>();
        if (animComp == null) {
            animComp = GetComponent<Animation>();
            if (animComp == null) {
                animComp = GetComponentInChildren<Animation>();
            }
        }
    }

    public virtual void BindEntity(BaseEntity entity)
    {
        UnityEngine.Debug.Log("Animator Bind");
        this.entity = entity as Entity;
        cAnim = this.entity.animator;
        cAnim.view = this;
    }

    public void SetInteger(string name, int val){
        animator.SetInteger(name, val);
    }

    public void SetTrigger(string name){
        animator.SetTrigger(name);
    }

    public void Play(string name, bool isCross){
        //animState = animComp[name];
        var state = animator.HasState(0,Animator.StringToHash(name));
        if (state)
        {
            if (isCross) {
                //animComp.CrossFade(name);
                animator.CrossFade(name, 0, 0);
            }
            else {
                //animComp.Play(name);
                animator.Play(name);
            }
        }
    }

    public void LateUpdate(){
        if (cAnim.curAnimBindInfo != null && cAnim.curAnimBindInfo.isMoveByAnim) {
            rootTrans.localPosition = Vector3.zero;
        }
    }

    public void Sample(LFloat time){
        if (Application.isPlaying) {
            return;
        }
        if (animState == null) return;
        if (!Application.isPlaying) {
            animComp.Play();
        }

        animState.enabled = true;
        animState.weight = 1;
        animState.time = time.ToFloat();
        animComp.Sample();
        if (!Application.isPlaying) {
            animState.enabled = false;
        }
    }
}