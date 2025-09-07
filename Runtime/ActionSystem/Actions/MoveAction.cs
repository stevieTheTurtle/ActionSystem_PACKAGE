using System;
using AgentActionSystem;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class MoveAction : AgentAction
{
    [SerializeField] private Transform destination;
    [SerializeField] private Transform origin;
    [SerializeField] private Vector3 movement;
    [SerializeField] private LocomotionSystem locomotionSystem;

    public MoveAction(Agent agent, Transform origin, Vector3 movement)
    {
        Assert.IsNotNull(origin);
        Assert.IsNotNull(agent.LocomotionSystem);
        
        this.locomotionSystem = agent.LocomotionSystem;
        this.origin = origin;
        this.movement = movement;
        
        this.destination = GameObject.Find("WalkAction_Destination").transform;
        destination.position = GetGlobalPosition();
        destination.LookAt(destination.position, agent.transform.up);
    }

    internal override void Setup()
    {
        if (!locomotionSystem.CanReach(destination.position))
        {
            Vector3 reachPosition;
            if (locomotionSystem.CanReachNearPoint(destination.position, 5f, out reachPosition)) //TODO: Distanza di check arbitraria!!!
            {
                SetState(ActionState.Stopped); //TODO: al momento la considero Stopped e non Failed!!!
                SetLog($"Destination unreachable but {origin.InverseTransformPoint(reachPosition)} is the nearest reachablePosition");
            }
            else
            {
                SetState(ActionState.Failed);
                SetLog($"Destination totally unreachable");
            }
        }
        else
        {
            SetState(ActionState.Updating);
        }
    }

    internal override void OnStart()
    {
        Debug.Log("Move started");
        
        locomotionSystem.SetDestination(destination);
        locomotionSystem.OnDestinationArrival += OnDestinationArrival;
    }

    internal override void OnUpdate()
    {
        //throw new NotImplementedException();
    }

    internal override void OnComplete()
    {
        Debug.Log("Move completed");
    }

    private void OnDestinationArrival()
    {
        locomotionSystem.OnDestinationArrival -= OnDestinationArrival;
        SetState(ActionState.Completed);
    }

    private Vector3 GetGlobalPosition()
    {
        return origin.position + origin.TransformVector(movement);
    }
}
