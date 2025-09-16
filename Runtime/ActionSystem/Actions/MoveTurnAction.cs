using System;
using UnityEngine;
using UnityEngine.Assertions;

namespace ActionSystem
{
    [Serializable]
    public class MoveTurnAction : AgentAction
    {
        [SerializeField] private Transform destination;
        [SerializeField] private Transform reqDestination;
        [SerializeField] private Transform suggestedDestination;
        [SerializeField] private Vector3 turnToPointGlobal;
        [SerializeField] private Transform origin;
        [SerializeField] private LocomotionSystem locomotionSystem;

        [SerializeField] private Vector3 reachableDestination;
        public Vector3 ReachableDestination => reachableDestination;

        public MoveTurnAction(Agent agent, Transform origin, Vector3 movement, Vector3 turnToPoint)
        {
            Assert.IsNotNull(origin);
            Assert.IsNotNull(agent.LocomotionSystem);

            this.locomotionSystem = agent.LocomotionSystem;
            this.origin = origin;
            this.turnToPointGlobal = origin.TransformPoint(turnToPoint);

            this.reqDestination = GameObject.Find("WalkAction_ReqDestination").transform;
            reqDestination.position = origin.TransformPoint(movement);
            
            //reqDestination.transform.up = Vector3.up;
        }

        protected internal override void Setup()
        {
            suggestedDestination = GameObject.Find("WalkAction_SuggestedDestination").transform;
            //if(IsPointNear(destination.position, locomotionSystem.transform.position, ))

            if (!locomotionSystem.CanReach(reqDestination.position))
            {
                Vector3 reachPosition;
                if (locomotionSystem.CanReachNearPoint(reqDestination.position, 5f,
                        out reachPosition)) //TODO: Distanza di check arbitraria!!!
                {
                    suggestedDestination.position = reachPosition;
                    reachableDestination = origin.InverseTransformPoint(reachPosition);
                    SetState(ActionState.Stopped); //TODO: al momento la considero Stopped e non Failed!!!
                    SetLog($"Destination unreachable but {reachableDestination} is the nearest reachablePosition");
                }
                else
                {
                    SetState(ActionState.Failed);
                    SetLog($"Destination totally unreachable");
                }
            }
            else
            {
                Vector3 turnToPointProjectedOnXZ = new Vector3(
                    turnToPointGlobal.x, 
                    locomotionSystem.transform.transform.position.y, 
                    turnToPointGlobal.z);
                reqDestination.LookAt(turnToPointProjectedOnXZ, Vector3.up);

                float dotProduct = Vector3.Dot(reqDestination.transform.up, Vector3.up);
                if (dotProduct < 0.99f)
                    Debug.LogError($"This should not happen : reqDestination.transform.up is not up!\nDotProduct={dotProduct}");
                
                SetState(ActionState.Updating);
            }
        }

        protected internal override void OnStart()
        {
            Debug.Log("Move started");

            this.destination = GameObject.Find("WalkAction_Destination").transform;
            destination.position = reqDestination.position;
            destination.rotation = reqDestination.rotation;
            
            locomotionSystem.SetDestination(destination);
            locomotionSystem.OnDestinationArrival += OnDestinationArrival;
        }

        protected internal override void OnUpdate()
        {
            //throw new NotImplementedException();
        }

        protected internal override void OnComplete()
        {
            Debug.Log("Move completed");
        }

        private void OnDestinationArrival()
        {
            locomotionSystem.OnDestinationArrival -= OnDestinationArrival;
            SetState(ActionState.Completed);
        }

        // private bool IsPointNear(Vector3 point, Vector3 target, float distance)
        // {
        //     if((target-point).magnitude <= distance)
        //         return true;
        //     else
        //         return false;
        // }
    }
}