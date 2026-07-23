using UnityEngine;
public class Bird_Movement : StateMachineBehaviour
{
    float timer = 0.9f;
    BirdBoss boss;
    Vector3 pos;

    // OnStateEnter is called when a transition starts and the state machine starts to evaluate this state
    override public void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        boss = animator.GetComponentInParent<BirdBoss>();
        timer = 0.9f;
    }

    // OnStateUpdate is called on each Update frame between OnStateEnter and OnStateExit callbacks
    override public void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        timer -= Time.deltaTime;
        if (!boss.isMoving && timer <= 0)
        {
            pos = GameObject.FindWithTag("Player").transform.position;
            Vector3 moveDir = (pos - animator.transform.position).normalized;
            boss.StartCoroutine(boss.Dash(moveDir));
            animator.SetTrigger("Stop");
        }
    }

    // OnStateExit is called when a transition ends and the state machine finishes evaluating this state
    override public void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex)
    {
        animator.ResetTrigger("Stop");
    }
}
