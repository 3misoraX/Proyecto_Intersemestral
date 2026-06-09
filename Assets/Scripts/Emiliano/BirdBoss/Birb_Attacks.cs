using UnityEngine;

public class Birb_Attacks : StateMachineBehaviour
{
    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(animator.GetInteger("Attack") == 1)
        {
            animator.GetComponent<BirdBoss>().FallingAttack();
            animator.SetTrigger("Stop");
        }
        if(animator.GetInteger("Attack") == 2)
        {
            animator.GetComponent<BirdBoss>().ShotgunAttack();
            animator.SetTrigger("Stop");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("Stop");
    }
}
