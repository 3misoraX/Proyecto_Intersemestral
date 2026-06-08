using UnityEngine;

public class Bird_Idle : StateMachineBehaviour
{
    float timer;
    int ctrl;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        ctrl = Random.Range(0, 3);
        timer = 3;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if(ctrl == 0 && timer <= 0)
        {
            animator.SetTrigger("Move");
        }
        else if(ctrl != 0 && timer == 0)
        {
            timer = 3f;
            animator.SetInteger("Attack", ctrl);
            animator.SetTrigger("AttackT");
        }
        timer -= Time.deltaTime;
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("AttackT");
        animator.ResetTrigger("Move");
    }
}
