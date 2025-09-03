using System;
using AgentActionSystem;
using HumanoidInteraction;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class DropAction : AgentAction
{
    [SerializeField] private Pickable pickableObj;
    [SerializeField] private Transform dropTransform;
    [SerializeField] private EffectorType effectorType;

    [SerializeField] private InteractionSystem interactionSystem;

    public DropAction(Agent agent, Pickable pickableObj, Transform dropTransform, EffectorType effectorType)
    {
        Assert.IsNotNull(agent.InteractionSystem);
        Assert.IsNotNull(pickableObj);
        Assert.IsNotNull(dropTransform);
        
        this.interactionSystem = agent.InteractionSystem;
        this.pickableObj = pickableObj;
        this.dropTransform = dropTransform;
        this.effectorType = effectorType;
    }
    
    internal override void Setup()
    {
        if (!pickableObj.IsBeingCarried)
        {
            Debug.LogWarning("Pickable object is not being carried right now");
            SetState(ActionState.Failed);
        }

        if (interactionSystem.GetEffector(effectorType).IsInteracting())
        {
            Debug.LogWarning("Effector is already interacting with something");
            SetState(ActionState.Failed);
        }
    }

    internal override void OnStart()
    {
        Debug.Log("Pick started");

        Interaction interaction = interactionSystem.StartReachInteraction(dropTransform, effectorType);
        
        interaction.OnInteractionStarted += OnInteractionStarted;
        interaction.OnInteractionHolded += OnInteractionHolded;
        interaction.OnInteractionCompleted += OnInteractionCompleted;
        interaction.OnInteractionFailed += OnInteractionFailed;
    }

    internal override void OnUpdate()
    {
        //Debug.Log("Touch updating");
    }

    internal override void OnComplete()
    {
        Debug.Log("Drop completed");
    }

    private void OnInteractionStarted(Interaction interaction)
    { 
        interaction.OnInteractionStarted -= OnInteractionStarted;
    }

    private void OnInteractionHolded(Interaction interaction)
    {
        interaction.OnInteractionHolded -= OnInteractionHolded;
        
        pickableObj.SetBeingCarried(false);
        pickableObj.transform.SetParent(null,true);
    }
    
    private void OnInteractionCompleted(Interaction interaction)
    {
        interaction.OnInteractionCompleted -= OnInteractionCompleted;
        interaction.OnInteractionHolded -= OnInteractionHolded;
        interaction.OnInteractionFailed -= OnInteractionFailed;
        
        this.SetState(ActionState.Completed);
    }
    private void OnInteractionFailed(Interaction interaction)
    {
        interaction.OnInteractionCompleted -= OnInteractionCompleted;
        interaction.OnInteractionHolded -= OnInteractionHolded;
        interaction.OnInteractionFailed -= OnInteractionFailed;
        
        this.SetState(ActionState.Failed);
    }
}
