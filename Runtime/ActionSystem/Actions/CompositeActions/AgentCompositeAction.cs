using System;
using System.Collections.Generic;
using UnityEngine;

namespace ActionSystem
{
    [Serializable]
    public class AgentCompositeAction : AgentAction
    {
        [SerializeField] private AgentAction currentSubAction;
        [SerializeField] private List<AgentAction> pastSubActions = new List<AgentAction>();
        [SerializeField] private List<AgentAction> subActionsQueue = new List<AgentAction>();

        private float waitTime;
        private float elapsedTime;
        
        public AgentCompositeAction(List<AgentAction> subActions, float waitTimeBetweenActions = 0.5f)
        {
            subActionsQueue = subActions;
            this.waitTime = waitTimeBetweenActions;
            this.elapsedTime = waitTime; // To not wait on the first SubAction
        }
        
        protected internal override void Setup()
        {
            if (! (subActionsQueue.Count > 0))
            {
                Debug.LogError($"{this} has no sub actions!");
                SetState(ActionState.Failed);
            }
            
            SetState(ActionState.Updating);
        }

        protected internal override void OnStart()
        {
            currentSubAction = subActionsQueue[0];
        }

        protected internal override void OnUpdate()
        {
            if (elapsedTime <= waitTime)
            {
                elapsedTime += Time.deltaTime;
                return;
            }

            UpdateCurrentSubAction();
            if (currentSubAction.State == ActionState.Completed)
            {
                bool hasNext = LoadNextSubAction();
                if(! hasNext)
                    SetState(ActionState.Completed);
            }
            else
                SetState(currentSubAction.State);
        }

        protected internal override void OnComplete()
        {
            //
        }

        private bool LoadNextSubAction()
        {
            pastSubActions.Add(currentSubAction);
            
            if (subActionsQueue.Count == 0)
                return false;
            else{
                currentSubAction = subActionsQueue[0];
                subActionsQueue.RemoveAt(0);
                
                elapsedTime = 0f;
                return true;
            }
        }

        private void UpdateCurrentSubAction()
        {
            switch (currentSubAction?.State)
            {
                case ActionState.Idle:
                    currentSubAction.Setup();
                    if (currentSubAction.State == ActionState.Updating)
                    {
                        currentSubAction.OnStart();
                    }
                    break;
                case ActionState.Updating:
                    currentSubAction.OnUpdate();
                    break;
                case ActionState.Completed:
                    currentSubAction.OnComplete();
                    LoadNextSubAction();
                    break;
                case ActionState.Stopped:
                    currentSubAction.OnStop();
                    LoadNextSubAction();
                    break;
                case ActionState.Failed:
                    currentSubAction.OnFail();
                    LoadNextSubAction();
                    break;
            }
        }

        public void StopCurrentSubAction()
        {
            currentSubAction.SetState(ActionState.Stopped);
        }
        
        // public bool EnqueueSubAction(AgentAction action)
        // {
        //     if (action != null)
        //     {
        //         subActionsQueue.Add(action);
        //         return true;
        //     }
        //     else
        //         return false;
        // }
    }
}
