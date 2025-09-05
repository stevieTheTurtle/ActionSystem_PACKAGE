using System;
using System.IO;
using MxM;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class LocomotionSystem : MonoBehaviour
{
    private NavMeshAgent navAgent;
    private MxMAnimator mxmAnim;
    private MxMTIPExtension tipModule;

    [Range(0f,0.30f)]
    public float angleDiffThres = 0.15f;
    
    public Action OnDestinationArrival;

    protected void Start()
    {
        navAgent = this.GetComponent<NavMeshAgent>();
        
        mxmAnim = this.GetComponent<MxMAnimator>();
        tipModule = this.GetComponent<MxMTIPExtension>();

        tipModule.TIPVector = this.transform.forward;
    }
    
    public bool SetDestination(Transform destination)
    {
        tipModule.TIPVector = destination.transform.forward;
        return SetDestination(destination.position);
    }
    public bool SetDestination(Vector3 destinationPos)
    {
        if (CanReach(destinationPos))
        {
            navAgent.SetDestination(destinationPos);
            return true;
        }
        else 
            return false;
    }

    public bool CanReach(Vector3 position)
    {
        NavMeshHit navHit;
        NavMeshPath navPath = new NavMeshPath();
        
        bool isOnNavMesh = NavMesh.SamplePosition(position, out navHit, 0.1f, NavMesh.AllAreas); //TODO: Distanza di check arbitraria!!!
        bool hasReachablePath =
            NavMesh.CalculatePath(navAgent.transform.position, position, NavMesh.AllAreas, navPath);

        return (isOnNavMesh && hasReachablePath);
    }

    public bool CanReachNearPoint(Vector3 position, float maxDistance, out Vector3 reachPos)
    {
        NavMeshHit navHit;
        NavMeshPath navPath = new NavMeshPath();
        
        NavMesh.SamplePosition(position, out navHit, maxDistance, NavMesh.AllAreas);
        reachPos = navHit.position;

        return CanReach(navHit.position);
    }

    private void LateUpdate()
    {
        if (IsInPlace() && IsTurnedRight())
        {
            OnDestinationArrival?.Invoke(); 
        }
    }

    private bool IsInPlace()
    {
        // Check if the agent has a path and is not in the process of calculating one.
        if (!navAgent.pathPending)
        {
            // Check if the agent has a complete path.
            if (navAgent.pathStatus == NavMeshPathStatus.PathComplete)
            {
                // Check if the agent is close enough to the destination.
                if (navAgent.remainingDistance <= navAgent.stoppingDistance)
                {
                    // Optional: Check if the agent has stopped moving.
                    if (!navAgent.hasPath || navAgent.velocity.sqrMagnitude == 0f)
                    {
                        //Debug.Log("Agent has arrived at the destination!");
                        return true;
                    }
                }
            }
        }

        return false;
    }

    private bool IsTurnedRight()
    {
        if (Vector3.Dot(this.transform.forward, tipModule.TIPVector) > (1f - angleDiffThres))
            return true;
        else
            return false;
    }
}
