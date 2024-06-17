using System.Collections;
using System.IO;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

namespace Assets.Scripts
{

    public enum MonsterState
    {
        Following,
        Patrolling,
    }
    public class MonsterController : MonoBehaviour
    {
        [SerializeField] private Transform[] waypoints;
        [SerializeField] private int currentWaypointIndex = 0;
        [SerializeField] private float changeWaypointCooldown = 10.0f;
        [SerializeField] private float changeWaypointElapsedTime = 0.0f;
        [SerializeField] private MonsterState state;

        [SerializeField] private Transform targetPlayer;
        [SerializeField] private float maxRayFollowingRange = 15.0f;
        [SerializeField] private float maxNoVisionFollowingRange = 15.0f;
        [SerializeField] private Vector3 lastSeenPlayerPosition;
        [SerializeField] private NavMeshPath followPlayerPath;
        [SerializeField] private float calculatePathElapsedTime = 0.0f;
        [SerializeField] private float calculatePathCooldown = 1.0f;

        [SerializeField] private float patrollingMoveSpeed = 1.0f;
        [SerializeField] private float followingMoveSpeed = 3.0f;




        [SerializeField] NavMeshAgent navMeshAgent;

        bool IsPlayerVisible()
        {
            Vector3 playerDirection = (targetPlayer.position - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, playerDirection, out hit, maxRayFollowingRange))
            {
                Debug.Log(hit.collider.tag);
                Debug.DrawRay(transform.position, playerDirection * hit.distance, Color.red);
                if (hit.collider.CompareTag("Player"))
                {
                    lastSeenPlayerPosition = targetPlayer.position;
                    return true;
                }
            }
            return false;
        }

        void ChangeState(MonsterState newState)
        {
            if (state == newState)
            {
                return;
            }
            else if (newState == MonsterState.Following)
            {
                navMeshAgent.speed = followingMoveSpeed;
            }
            else if (newState == MonsterState.Patrolling)
            {
                navMeshAgent.speed = patrollingMoveSpeed;
            }
            state = newState;
        }

        float CalculatePathToPlayer ()
        {
            if(calculatePathElapsedTime > calculatePathCooldown)
            {
                calculatePathElapsedTime = 0.0f;
                navMeshAgent.CalculatePath(targetPlayer.transform.position, followPlayerPath);
            }
            float totalDistance = 0.0f;
            for (int i = 1; i < followPlayerPath.corners.Length; i++)
            {
                totalDistance += Vector3.Distance(followPlayerPath.corners[i - 1], followPlayerPath.corners[i]);
            }
            return totalDistance;
        }

        void UpdateTimeVariables()
        {
            calculatePathElapsedTime += Time.deltaTime;
            changeWaypointElapsedTime += Time.deltaTime;

        }

        void UpdateState()
        {
            if (state == MonsterState.Patrolling)
            {
                if (changeWaypointElapsedTime > changeWaypointCooldown)
                {
                    changeWaypointElapsedTime = 0.0f;
                    int newWaypointIndex = currentWaypointIndex;
                    while (newWaypointIndex == currentWaypointIndex)
                    {
                        newWaypointIndex = Random.Range(0, waypoints.Length);
                    }
                    currentWaypointIndex = newWaypointIndex;
                }
                navMeshAgent.SetDestination(waypoints[currentWaypointIndex].position);

                if (IsPlayerVisible())
                {
                    ChangeState(MonsterState.Following);
                }
            } 
            else if (state == MonsterState.Following)
            {
                navMeshAgent.SetDestination(lastSeenPlayerPosition);
                if (!IsPlayerVisible())
                {
                    float distanceToPlayer = CalculatePathToPlayer();
                    if (distanceToPlayer > maxNoVisionFollowingRange)
                    {
                        ChangeState(MonsterState.Patrolling);
                    }
                }
            }
        }

        void Start()
        {
            currentWaypointIndex = Random.Range(0, waypoints.Length);

            changeWaypointElapsedTime = 0.0f;
            calculatePathElapsedTime = 0.0f;

            ChangeState(MonsterState.Patrolling);
            followPlayerPath = new NavMeshPath();
        }

        void Update()
        {
            UpdateTimeVariables();
            UpdateState();
        }
    }
}