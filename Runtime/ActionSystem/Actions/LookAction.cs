using HumanoidInteraction;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using UnityEngine.Assertions;

namespace ActionSystem
{
    public class LookAction : AgentAction
    {
        [SerializeField] private InteractionSystem interactionSystem;

        [SerializeField] private MultiAimConstraint aimConstraint;
        [SerializeField] private Transform lookTarget;

        public LookAction(Agent agent, Transform lookTarget)
        {
            Assert.IsNotNull(agent.InteractionSystem);

            this.interactionSystem = agent.InteractionSystem;
            this.lookTarget = lookTarget;
        }

        protected internal override void Setup()
        {
            aimConstraint = interactionSystem.RiggingController.GetRig(EffectorType.HeadLook)
                .GetComponentInChildren<MultiAimConstraint>();
            
            SetState(ActionState.Updating);
        }

        protected internal override void OnStart()
        {
            if(lookTarget != null)
                interactionSystem.StartLook(lookTarget);
            else
                interactionSystem.StopLook();
        }

        protected internal override void OnUpdate()
        {
            if (lookTarget != null)
            {
                if ((aimConstraint.data.sourceObjects[0].transform.position - lookTarget.position).magnitude < 0.01f)
                    SetState(ActionState.Completed);
            }
            else 
                if (interactionSystem.IsEffectorTotallyInactive(EffectorType.HeadLook))
                    SetState(ActionState.Completed);
        }

        protected internal override void OnComplete()
        {
            //Debug.Log("Pick completed");
        }
    }
}
