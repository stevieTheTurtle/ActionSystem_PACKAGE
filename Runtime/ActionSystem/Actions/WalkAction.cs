using System;
using AgentActionSystem;
using HumanoidInteraction;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class WalkAction : AgentAction
{
    [SerializeField] private Transform destination;
    [SerializeField] private LocomotionSystem locomotionSystem;

    public WalkAction(Agent agent, Transform destination)
    {
        Assert.IsNotNull(destination);
        Assert.IsNotNull(agent.LocomotionSystem);
        
        this.destination = destination;
        this.locomotionSystem = agent.LocomotionSystem;
    }

    internal override void Setup()
    {
        if (!locomotionSystem.CanReach(destination.position))
        {
            Vector3 reachPosition;
            if (locomotionSystem.CanReachNearPoint(destination.position, 5f, out reachPosition)) //TODO: Distanza di check arbitraria!!!
            {
                SetState(ActionState.Stopped); //TODO: al momento la considero Stopped e non Failed!!!
                SetLog($"Destination unreachable but {reachPosition} is the nearest reachablePosition");
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
        Debug.Log("Walk started");
        
        locomotionSystem.SetDestination(destination);
        locomotionSystem.OnDestinationArrival += OnDestinationArrival;
    }

    internal override void OnUpdate()
    {
        //throw new NotImplementedException();
    }

    internal override void OnComplete()
    {
        Debug.Log("Walk completed");
    }

    private void OnDestinationArrival()
    {
        locomotionSystem.OnDestinationArrival -= OnDestinationArrival;
        SetState(ActionState.Completed);
    }
}
