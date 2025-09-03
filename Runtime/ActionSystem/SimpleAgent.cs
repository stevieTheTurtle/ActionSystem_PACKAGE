using AgentActionSystem;
using HumanoidInteraction;
using UnityEngine;

public class SimpleAgent : Agent
{
    [SerializeField] private bool enableDebugLogging;
    
    public void Touch(Interactable target, EffectorType effectorType)
    {
        if (enableDebugLogging)
            Debug.Log($"Adding TouchAction with {target.Desc}");

        this.EnqueueAction(new TouchAction(this, effectorType, target));
    }
    
    public void Pick(Pickable target, EffectorType effectorType)
    {
        if (enableDebugLogging)
            Debug.Log($"Adding PickAction with {target.Desc}");
        
        this.EnqueueAction(new PickAction(this, effectorType, target));
    }
    
    public void Drop(Pickable pickableObj, Transform dropTransform, EffectorType effectorType)
    {
        if (enableDebugLogging)
            Debug.Log($"Adding DropAction with {pickableObj.Desc}");
        
        this.EnqueueAction(new DropAction(this, pickableObj, dropTransform, effectorType));
    }
    
    public void Walk(Transform destination)
    {
        if (enableDebugLogging)
            Debug.Log($"Adding WalkAction to {destination}");
            
        this.EnqueueAction(new WalkAction(this, destination));
    }
}
