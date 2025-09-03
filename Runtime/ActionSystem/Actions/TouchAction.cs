using System;
using AgentActionSystem;
using HumanoidInteraction;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class TouchAction : AgentAction
{
    [SerializeField] private EffectorType effectorType;
    [SerializeField] private Interactable target;

    [SerializeField] private InteractionSystem interactionSystem;

    public TouchAction(Agent agent, EffectorType effectorType, Interactable target)
    {
        this.interactionSystem = agent.InteractionSystem;
        this.effectorType = effectorType;
        this.target = target;
        
        Assert.IsNotNull(this.interactionSystem);
        Assert.IsNotNull(target);
    }
    
    internal override void Setup()
    {
        if(!target.CanInteract)
            Debug.LogWarning("Not interactable at the moment");
        if(interactionSystem.GetEffector(effectorType).IsInteracting())
            Debug.LogWarning("Effector is already interacting with something");
    }

    internal override void OnStart()
    {
        Debug.Log("Touch started");
        
        if (target != null && target.CanInteract)
        {
            Interaction interaction = interactionSystem.StartSimpleTouchInteraction(target, effectorType);
            
            interaction.OnInteractionStarted += OnInteractionStarted;
            interaction.OnInteractionCompleted += OnInteractionCompleted;
            interaction.OnInteractionFailed += OnInteractionFailed;
        }
    }

    internal override void OnUpdate()
    {
        //Debug.Log("Touch updating");
    }

    internal override void OnComplete()
    {
        Debug.Log("Touch completed");
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
        interaction.OnInteractionStarted -= OnInteractionStarted;
        interaction.OnInteractionCompleted -= OnInteractionCompleted;
        interaction.OnInteractionFailed -= OnInteractionFailed;
        
        this.SetState(ActionState.Failed);
    }
}
