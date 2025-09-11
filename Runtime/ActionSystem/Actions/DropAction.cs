using System;
using AgentActionSystem;
using HumanoidInteraction;
using UnityEngine;
using UnityEngine.Assertions;

[Serializable]
public class DropAction : AgentAction
{
    [SerializeField] private EffectorType effectorType;
    [SerializeField] private Pickable pickableObj;
    [SerializeField] private Transform dropTransform;

    [SerializeField] private InteractionSystem interactionSystem;

    [SerializeField] private Interaction interaction;

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
            SetLog("Pickable object is not being carried right now");
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

        interaction = interactionSystem.StartReachInteraction(dropTransform, effectorType);
        
        interaction.OnInteractionStarted += OnInteractionStarted;
        interaction.OnInteractionHolded += OnInteractionHolded;
        interaction.OnInteractionCompleted += OnInteractionCompleted;
        interaction.OnInteractionFailed += OnInteractionFailed;
    }

    internal override void OnUpdate()
    {
        if(! interactionSystem.IsEffectorTotallyActive(effectorType))
            return;
        
        //EffectorRig is totally blended in, so i can check if the effector is actually able to reach the target
        if (interactionSystem.IsConstraintTipAwayFromTarget(effectorType))
        {
            interactionSystem.StopInteraction(interaction);
            SetState(ActionState.Failed);
        }
    }

    internal override void OnComplete()
    {
        //Debug.Log("Drop completed");
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
