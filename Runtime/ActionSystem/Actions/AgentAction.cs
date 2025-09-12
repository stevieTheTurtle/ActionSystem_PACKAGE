using System;
using UnityEngine;

using HumanoidInteraction;

namespace ActionSystem
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
        [SerializeField] private string LOG = "None";

        public Action<Interaction> OnActionStarted;
        public Action<Interaction> OnActionReached;
        public Action<Interaction> OnActionHolded;
        public Action<Interaction> OnActionCompleted;
        public Action<Interaction> OnActionStopped;
        public Action<Interaction> OnActionFailed;
        
        protected internal abstract void Setup();
        
        protected internal abstract void OnStart();
        protected internal abstract void OnUpdate();
        protected internal abstract void OnComplete();

        protected internal  void OnStop()
        {
            //Debug.LogError("TO IMPLEMENT");
        }

        protected internal void OnFail()
        {
            //Debug.LogError("TO IMPLEMENT");
        }

        protected internal void SetState(ActionState newState) => state = newState;
        public ActionState State => state;

        protected internal void SetLog(string log) => LOG = log;
        public string Log => LOG;
    }
}
