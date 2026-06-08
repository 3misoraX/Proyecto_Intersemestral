using UnityEngine;
using UnityEngine.UIElements;
using static UnityEditor.PlayerSettings;

public class Bird_Movement : StateMachineBehaviour
{
    BirdBoss boss;
    Vector3 pos;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        boss = animator.GetComponent<BirdBoss>();
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        if (!boss.isMoving)
        {
            pos = GameObject.FindWithTag("Player").transform.position;
            Vector3 moveDir = (pos - animator.transform.position).normalized;
            boss.StartCoroutine(boss.Dash(moveDir));
        }
        animator.SetTrigger("Stop");
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("Stop");
    }
}
