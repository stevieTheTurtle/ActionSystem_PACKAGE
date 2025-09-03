using System;
using System.Collections.Generic;
using HumanoidInteraction;
using UnityEngine;

namespace AgentActionSystem
{
    [RequireComponent(typeof(InteractionSystem))]
    public class Agent : MonoBehaviour
    {
        [SerializeField] protected AgentAction currentAction;
        [SerializeField] protected List<AgentAction> actionsQueue = new List<AgentAction>();
        [SerializeField] protected List<AgentAction> pastActions = new List<AgentAction>();
        
        [SerializeField] protected InteractionSystem interactionSystem;
        [SerializeField] protected LocomotionSystem locomotionSystem;
        
        public InteractionSystem InteractionSystem => interactionSystem;
        public LocomotionSystem LocomotionSystem => locomotionSystem;

        protected void Start()
        {
            this.interactionSystem = this.GetComponent<InteractionSystem>();
            this.locomotionSystem = this.GetComponent<LocomotionSystem>();
        }
        
        protected void Update()
        {
            if (currentAction == null && actionsQueue.Count == 0)
                return;

            if (currentAction != null)
                UpdateCurrentAction();
            else
                LoadNextAction();
        }

        public bool EnqueueAction(AgentAction action)
        {
            if (action != null)
            {
                actionsQueue.Add(action);
                return true;
            }
            else
                return false;
        }

        private bool LoadNextAction()
        {
            pastActions.Add(currentAction);
            currentAction = null;
            
            if (actionsQueue.Count == 0)
                return false;
            else{
                currentAction = actionsQueue[0];
                actionsQueue.RemoveAt(0);
                return true;
            }
        }

        protected void UpdateCurrentAction()
        {
            switch (currentAction?.State)
            {
                case ActionState.Idle:
                    currentAction.Setup();
                    if (currentAction.State != ActionState.Failed)
                    {
                        currentAction.OnStart();
                        currentAction.SetState(ActionState.Updating);
                        break;
                    }
                    break;
                case ActionState.Updating:
                    currentAction.OnUpdate();
                    break;
                case ActionState.Completed:
                    currentAction.OnComplete();
                    LoadNextAction();
                    break;
                case ActionState.Stopped:
                    currentAction.OnStop();
                    LoadNextAction();
                    break;
                case ActionState.Failed:
                    currentAction.OnFail();
                    LoadNextAction();
                    break;
            }
        }

        public void StopCurrentAction()
        {
            currentAction.SetState(ActionState.Stopped);
        }

        
    }
}