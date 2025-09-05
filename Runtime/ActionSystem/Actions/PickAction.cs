using System;
using AgentActionSystem;
using HumanoidInteraction;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class PickAction : AgentAction
{
    [SerializeField] private EffectorType effectorType;
    [SerializeField] private Pickable target;

    [SerializeField] private InteractionSystem interactionSystem;

    public PickAction(Agent agent, EffectorType effectorType, Pickable target)
    {
        Assert.IsNotNull(agent.InteractionSystem);
        Assert.IsNotNull(target);
        
        this.interactionSystem = agent.InteractionSystem;
        this.effectorType = effectorType;
        this.target = target;
    }
    
    internal override void Setup()
    {
        if (!target.CanInteract)
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
        
        if (!target.IsBeingCarried)
        {
            Interaction interaction = interactionSystem.StartPickInteraction(target, effectorType);
            
            interaction.OnInteractionStarted += OnInteractionStarted;
            interaction.OnInteractionCompleted += OnInteractionCompleted;
            interaction.OnInteractionFailed += OnInteractionFailed;
        }
        else
        {
            SetLog($"{target} is already being carried");
            SetState(ActionState.Failed);
        }
    }

    internal override void OnUpdate()
    {
        //Debug.Log("Touch updating");
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
