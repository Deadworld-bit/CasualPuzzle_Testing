using UnityEngine;
using System.Collections;

[RequireComponent(typeof(Rigidbody))]
public class EnemyController : MonoBehaviour
{
    [SerializeField] private Transform[] waypoints;
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float waypointThreshold = 0.5f;
    [SerializeField] private bool moveIn2D = true;

    private Rigidbody rb;
    private Animator animator;
    private int currentWaypointIndex = 0;
    private bool isWaiting = false;

    private void Awake()
    {
        rb = GetComponent<Rigidbody>();
        animator = GetComponent<Animator>();
    }

    private void FixedUpdate()
    {
        if (waypoints == null || waypoints.Length == 0) return;
        Patrol();
    }

    private void Update()
    {
        if (animator != null)
        {
            animator.SetBool("IsMoving", !isWaiting);
        }
    }

    private void Patrol()
    {
        if (!isWaiting)
        {
            Vector3 targetPosition = waypoints[currentWaypointIndex].position;
            float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

            if (distanceToTarget < waypointThreshold)
            {
                StartCoroutine(WaitAtWaypoint());
            }
            else
            {
                Vector3 direction = (targetPosition - transform.position).normalized;
                if (moveIn2D)
                {
                    direction.y = 0;
                    direction = direction.normalized;
                }
                Vector3 nextPosition = transform.position + direction * moveSpeed * Time.deltaTime;
                rb.MovePosition(nextPosition);

                if (direction != Vector3.zero)
                {
                    transform.rotation = Quaternion.LookRotation(direction);
                }
            }
        }
    }

    private IEnumerator WaitAtWaypoint()
    {
        isWaiting = true;
        yield return new WaitForSeconds(2f);
        currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
        isWaiting = false;
    }
}