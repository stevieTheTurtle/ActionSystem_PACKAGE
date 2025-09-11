using System;
using AgentActionSystem;
using HumanoidInteraction;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class PickAction : AgentAction
{
    [SerializeField] private EffectorType effectorType;
    [SerializeField] private Pickable pickableObj;

    [SerializeField] private InteractionSystem interactionSystem;

    [SerializeField] private Interaction interaction;
    
    [SerializeField] private Vector3 stoppedPosition = Vector3.zero;
    
    public PickAction(Agent agent, EffectorType effectorType, Pickable pickableObj)
    {
        Assert.IsNotNull(agent.InteractionSystem);
        Assert.IsNotNull(pickableObj);
        
        this.interactionSystem = agent.InteractionSystem;
        this.effectorType = effectorType;
        this.pickableObj = pickableObj;
    }
    
    internal override void Setup()
    {
        if (!pickableObj.CanInteract)
        {
            SetLog("Not pickable at the moment");
            SetState(ActionState.Failed);
            return;
        }

        if (interactionSystem.GetEffector(effectorType).IsInteracting())
        {
            SetLog("Effector is already interacting with something");
            SetState(ActionState.Failed);
            return;
        }
        
        SetState(ActionState.Updating);
    }

    internal override void OnStart()
    {
        //Debug.Log("Pick started");
        
        if (!pickableObj.IsBeingCarried)
        {
            interaction = interactionSystem.StartPickInteraction(pickableObj, effectorType);
            
            interaction.OnInteractionStarted += OnInteractionStarted;
            interaction.OnInteractionCompleted += OnInteractionCompleted;
            interaction.OnInteractionFailed += OnInteractionFailed;
        }
        else
        {
            SetLog($"{pickableObj} is already being carried");
            SetState(ActionState.Failed);
        }
    }

    internal override void OnUpdate()
    {
        if(! interactionSystem.IsEffectorTotallyActive(effectorType))
            return;
        
        //EffectorRig is totally blended in, so i can check if the effector is actually able to reach the target
        if (interactionSystem.IsConstraintTipAwayFromTarget(effectorType))
        {
            stoppedPosition = interactionSystem.GetEffector(interaction.effectorType).stoppedPosition;
            interactionSystem.StopInteraction(interaction);
            SetState(ActionState.Failed);
        }
    }

    internal override void OnComplete()
    {
        //Debug.Log("Pick completed");
    }

    private void OnInteractionStarted(Interaction interaction)
    { 
        interaction.OnInteractionStarted -= OnInteractionStarted;
    }
    private void OnInteractionCompleted(Interaction interaction)
    {
        interaction.OnInteractionCompleted -= OnInteractionCompleted;
        interaction.OnInteractionFailed -= OnInteractionFailed;
        this.SetState(ActionState.Completed);
    }
    private void OnInteractionFailed(Interaction interaction)
    {
        interaction.OnInteractionFailed -= OnInteractionFailed;
    }
}
