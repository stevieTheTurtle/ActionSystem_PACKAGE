using System.Collections.Generic;
using HumanoidInteraction;
using UnityEngine;

namespace ActionSystem
{
    public class SimpleAgent : Agent
    {
        [SerializeField] protected bool enableDebugLogging;

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

        public AgentAction Move(Transform cameraPov, Vector3 movement)
        {
            if (enableDebugLogging)
                Debug.Log($"Adding MoveAction to move of {movement}");

            AgentAction action = new MoveAction(this, cameraPov, movement);
            this.EnqueueAction(action);

            return action;
        }

        public AgentAction MoveTurn(Transform cameraPov, Vector3 movement, Vector3 turnToPoint)
        {
            if (enableDebugLogging)
                Debug.Log($"Adding MoveTurn-Action to move of {movement} look at {turnToPoint}");

            AgentAction action = new MoveTurnAction(this, cameraPov, movement, turnToPoint);
            this.EnqueueAction(action);

            return action;
        }

        public AgentCompositeAction MoveTurnAndTouch(Transform cameraPov, Vector3 movement, Vector3 turnToPoint,
            Interactable target, EffectorType effectorType)
        {
            if (enableDebugLogging)
                Debug.Log($"Adding MoveTurnAndTouch-CompositeAction to move of {movement} look at {turnToPoint} and touch {target}");

            List<AgentAction> subActions = new List<AgentAction>();
            subActions.Add(new MoveTurnAction(this, cameraPov, movement, turnToPoint));
            subActions.Add(new TouchAction(this, effectorType, target));
            AgentCompositeAction compositeAction = new AgentCompositeAction(subActions);
            
            this.EnqueueAction(compositeAction);

            return compositeAction;
        }
        
        public AgentAction ToggleLook(Transform lookTarget)
        {
            if (enableDebugLogging)
                Debug.Log($"Adding LookAction to look at {lookTarget}");

            AgentAction action = new LookAction(this, lookTarget);
            this.EnqueueAction(action);

            return action;
        }
    }
}