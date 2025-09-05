using AgentActionSystem;
using HumanoidInteraction;
using UnityEngine;

public class SimpleAgent : Agent
{
    [SerializeField] private bool enableDebugLogging;
    
    public AgentAction Touch(Interactable target, EffectorType effectorType)
    {
        if (enableDebugLogging)
            Debug.Log($"Adding TouchAction with {target.Desc}");

        AgentAction action = new TouchAction(this, effectorType, target);
        this.EnqueueAction(action);

        return action;
    }
    
    public AgentAction Pick(Pickable target, EffectorType effectorType)
    {
        if (enableDebugLogging)
            Debug.Log($"Adding PickAction with {target.Desc}");

        AgentAction action = new PickAction(this, effectorType, target);
        this.EnqueueAction(action);

        return action;
    }
    
    public AgentAction Drop(Pickable pickableObj, Transform dropTransform, EffectorType effectorType)
    {
        if (enableDebugLogging)
            Debug.Log($"Adding DropAction with {pickableObj.Desc}");

        AgentAction action = new DropAction(this, pickableObj, dropTransform, effectorType);
        this.EnqueueAction(action);

        return action;
    }
    
    public AgentAction Walk(Transform destination)
    {
        if (enableDebugLogging)
            Debug.Log($"Adding WalkAction to {destination}");

        AgentAction action = new WalkAction(this, destination);
        this.EnqueueAction(action);

        return action;
    }
}
