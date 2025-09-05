using System;
using System.Collections.Generic;
using UnityEngine;

using HumanoidInteraction;

namespace AgentActionSystem
{
    [Serializable]
    public enum ActionState
    {
        Idle,
        Updating,
        Completed,
        Stopped,
        Failed,
    }

    [Serializable]
    public abstract class AgentAction
    {
        [SerializeField] private ActionState state = ActionState.Idle;
        internal ActionState State => state;

        public Action<Interaction> OnActionStarted;
        public Action<Interaction> OnActionReached;
        public Action<Interaction> OnActionHolded;
        public Action<Interaction> OnActionCompleted;
        public Action<Interaction> OnActionStopped;
        public Action<Interaction> OnActionFailed;
        
        internal abstract void Setup();
        
        internal abstract void OnStart();
        internal abstract void OnUpdate();
        internal abstract void OnComplete();

        internal  void OnStop()
        {
            Debug.LogError("TO IMPLEMENT");
        }

        internal void OnFail()
        {
            //Debug.LogError("TO IMPLEMENT");
        }

        internal void SetState(ActionState newState)
        {
            state = newState;
        }

        public ActionState GetState()
        {
            return state;
        }
    }
}
