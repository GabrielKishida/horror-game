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
        [SerializeField] private PlayerController targetPlayerController;
        [SerializeField] private float maxRayFollowingRange = 15.0f;
        [SerializeField] private float maxNoVisionFollowingRange = 15.0f;
        [SerializeField] private Vector3 lastSeenPlayerPosition;
        [SerializeField] private NavMeshPath path;
        [SerializeField] private float calculatePathElapsedTime = 0.0f;
        [SerializeField] private float calculatePathCooldown = 0.5f;

        [SerializeField] private float patrollingMoveSpeed = 1.0f;
        [SerializeField] private float followingMoveSpeed = 3.0f;

        [SerializeField] NavMeshAgent navMeshAgent;
        [SerializeField] SpotlightDetector spotlightDetector;

        bool IsPlayerDetected()
        {
            Vector3 playerDirection = (targetPlayer.position - transform.position).normalized;
            RaycastHit hit;
            if (Physics.Raycast(transform.position, playerDirection, out hit, maxRayFollowingRange))
            {
                Debug.DrawRay(transform.position, playerDirection * hit.distance, Color.red);
                if (hit.collider.CompareTag("Player"))
                {
                   if (Vector3.Dot(playerDirection, transform.forward) > 0 || spotlightDetector.isIlluminated)
                    {
                        lastSeenPlayerPosition = targetPlayer.position;
                        return true;
                    } 
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

        float CalculatePathToPlayer()
        {
            if(calculatePathElapsedTime > calculatePathCooldown)
            {
                calculatePathElapsedTime = 0.0f;
                navMeshAgent.CalculatePath(targetPlayer.position, path);
            }
            float totalDistance = 0.0f;
            for (int i = 1; i < path.corners.Length; i++)
            {
                totalDistance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return totalDistance;
        }

        float CalculatePathToPlayerLastSeen()
        {
            if (calculatePathElapsedTime > calculatePathCooldown)
            {
                calculatePathElapsedTime = 0.0f;
                navMeshAgent.CalculatePath(lastSeenPlayerPosition, path);
            }
            float totalDistance = 0.0f;
            for (int i = 1; i < path.corners.Length; i++)
            {
                totalDistance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
            }
            return totalDistance;
        }

        float CalculatePathToWaypoint()
        {
            {
                if (calculatePathElapsedTime > calculatePathCooldown)
                {
                    calculatePathElapsedTime = 0.0f;
                    navMeshAgent.CalculatePath(waypoints[currentWaypointIndex].position, path);
                }
                float totalDistance = 0.0f;
                for (int i = 1; i < path.corners.Length; i++)
                {
                    totalDistance += Vector3.Distance(path.corners[i - 1], path.corners[i]);
                }
                return totalDistance;
            }
        }

        void GetNewWaypoint()
        {
            changeWaypointElapsedTime = 0.0f;
            int newWaypointIndex = currentWaypointIndex;
            while (newWaypointIndex == currentWaypointIndex)
            {
                newWaypointIndex = Random.Range(0, waypoints.Length);
            }
            currentWaypointIndex = newWaypointIndex;
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
                if (changeWaypointElapsedTime > changeWaypointCooldown || CalculatePathToWaypoint() < 1.0f)
                {
                    GetNewWaypoint();
                }
                navMeshAgent.SetDestination(waypoints[currentWaypointIndex].position);

                if (IsPlayerDetected())
                {
                    ChangeState(MonsterState.Following);
                }
            } 
            else if (state == MonsterState.Following)
            {
                navMeshAgent.SetDestination(lastSeenPlayerPosition);
                if (!IsPlayerDetected())
                {
                    if (targetPlayerController.isFlashlightOn) 
                    {
                        navMeshAgent.SetDestination(targetPlayer.position);
                        float distanceToPlayerPosition = CalculatePathToPlayer();
                        if (distanceToPlayerPosition > maxNoVisionFollowingRange)
                        {
                            ChangeState(MonsterState.Patrolling);
                        }
                    }
                    else
                    {

                        float distanceToPlayerLastSeen = CalculatePathToPlayerLastSeen();
                        Debug.Log(distanceToPlayerLastSeen);
                        if (distanceToPlayerLastSeen < 2.0f)
                        {
                            ChangeState(MonsterState.Patrolling);
                        }
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
            path = new NavMeshPath();
        }

        void Update()
        {
            UpdateTimeVariables();
            UpdateState();
        }
    }
}