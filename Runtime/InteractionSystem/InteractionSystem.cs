using UnityEngine;
using System.Collections;

namespace HumanoidInteraction
{
    /// <summary>
    /// Main interaction system that provides simple commands for NPC interactions
    /// This is the primary interface for commanding NPCs to interact with objects
    /// </summary>
    public class InteractionSystem : MonoBehaviour
    {
        [Header("Core Components")]
        [SerializeField] private AnimationRiggingController riggingController;
        
        [Header("Default Settings")]
        [SerializeField] private float defaultReachDuration = 1.0f;
        [SerializeField] private float defaultHoldDuration = 0.1f;
        [SerializeField] private float defaultReturnDuration = 1.0f;
        [SerializeField] private AnimationCurve defaultReachCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        [SerializeField] private AnimationCurve defaultReturnCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

        [Header("Carried Objects")]
        [SerializeField] private Pickable rightHandCarriedObj;
        [SerializeField] private Pickable leftHandCarriedObj;

        public Pickable RightHandCarriedObj => rightHandCarriedObj;
        public Pickable LeftHandCarriedObj => rightHandCarriedObj;
        
        private Coroutine rightHandCoroutine;
        private Coroutine leftHandCoroutine;
        private Coroutine rightFootCoroutine;
        private Coroutine leftFootCoroutine;

        private void Awake()
        {
            if (riggingController == null)
                riggingController = GetComponent<AnimationRiggingController>();
        }
        
        /// <summary>
        /// SIMPLE FUNCTION: start touching the object
        /// </summary>
        /// <param name="target">The object to touch</param>
        /// <param name="effectorType">Which body part to use (hand/foot)</param>
        public Interaction StartSimpleTouchInteraction(IInteractable target, EffectorType effectorType)
        {
            var simpleTouchInteraction = new Interaction(target, InteractionType.Touch, effectorType)
            {
                reachDuration = defaultReachDuration,
                holdDuration = defaultHoldDuration,
                returnDuration = defaultReturnDuration,
                reachCurve = defaultReachCurve,
                returnCurve = defaultReturnCurve
            };

            StartInteraction(simpleTouchInteraction);

            return simpleTouchInteraction;
        }

        /// <summary>
        /// PICK FUNCTION: start picking the object
        /// </summary>
        /// <param name="target">The object to pick</param>
        /// <param name="effectorType">Which body part to use (hand/foot)</param>
        public Interaction StartPickInteraction(Pickable target, EffectorType effectorType)
        {
            if (target.IsBeingCarried){
                Debug.LogWarning($"{target} is already being carried");
                return null;
            }

            Interaction pickInteraction =
                new Interaction(target, InteractionType.Pick, effectorType);

            pickInteraction.OnInteractionHolded += interaction =>
            {
                InteractionEffector effector = GetEffector(effectorType);
                (target).transform.SetParent(effector.AttachTransform);
                (target).transform.localPosition = Vector3.zero;
                (target).transform.localRotation = Quaternion.identity;
                
                this.SetCarriedObj(target,effectorType);
                
                target.OnPickup();
            }; 
            
            StartInteraction(pickInteraction);

            return pickInteraction;
        }
        
        /// <summary>
        /// SIMPLE FUNCTION: start reaching the target
        /// </summary>
        /// <param name="target">The target to reach</param>
        /// <param name="effectorType">Which body part to use (hand/foot)</param>
        public Interaction StartReachInteraction(Transform target, EffectorType effectorType)
        {
            Interactable dummyInteractable;
            dummyInteractable = target.GetComponent<Interactable>();
            
            if (dummyInteractable == null)
                dummyInteractable = target.gameObject.AddComponent<Interactable>();
            
            Interaction reachInteraction = new Interaction(dummyInteractable, InteractionType.Touch, effectorType)
            {
                reachDuration = defaultReachDuration,
                holdDuration = defaultHoldDuration,
                returnDuration = defaultReturnDuration,
                reachCurve = defaultReachCurve,
                returnCurve = defaultReturnCurve
            };

            reachInteraction.OnInteractionCompleted += (Interaction interaction) => Destroy((Interactable) interaction.target);
            
            StartInteraction(reachInteraction);

            return reachInteraction;
        }
        
        /// <summary>
        /// Command an NPC to interact with an object using custom settings
        /// </summary>
        /// <param name="interaction">The interaction with all settings</param>
        public void StartInteraction(Interaction interaction)
        {
            if (interaction.currentState!= InteractionState.Idle)
            {
                Debug.LogWarning($"{interaction.effectorType} is already performing an interaction. Cannot start a new one.");
                interaction.OnInteractionFailed?.Invoke(interaction);
                return;
            }

            if (interaction.target == null)
            {
                Debug.LogError($"{interaction.target} interaction target is null!");
                interaction.OnInteractionFailed?.Invoke(interaction);
                return;
            }

            /*if (!command.target.CanInteract)
            {
                Debug.LogWarning($"Target {command.target.InteractionName} cannot be interacted with.");
                OnInteractionFailed?.Invoke(command);
                return;
            }*/
            interaction.currentCoroutine = StartCoroutine(ExecuteInteractionCoroutine(interaction));
        }

        /// <summary>
        /// Stop the current interaction and return to rest position
        /// </summary>
        public void StopInteraction(Interaction interaction)
        {
            if (interaction.currentCoroutine != null)
            {
                StopCoroutine(interaction.currentCoroutine);
                interaction.currentCoroutine = null;
            }

            if (interaction.currentState != InteractionState.Idle)
            {
                StartCoroutine(ReturnPhase(interaction));
                interaction.currentState = InteractionState.Idle;
                interaction.currentCoroutine = null;
            }
        }
        
        /*/// <summary>
        /// Stop the current interaction and return to rest position
        /// </summary>
        public void StopInteraction(InteractionEffector effector)
        {
            StopInteraction(effector.currentInteraction);
        }*/

        private IEnumerator ExecuteInteractionCoroutine(Interaction interaction)
        {
            interaction.OnInteractionStarted?.Invoke(interaction);

            bool interactionSucceeded = true;
            
            // Phase 1: REACH - Move effector to target
            interaction.currentState = InteractionState.Reaching;
            yield return StartCoroutine(ReachPhase(interaction));
            interaction.OnInteractionReached?.Invoke(interaction);
            
            // Phase 2: HOLD - Stay at target position
            interaction.currentState = InteractionState.Holding;
            yield return StartCoroutine(HoldPhase(interaction));
            interaction.OnInteractionHolded?.Invoke(interaction);

            // Phase 3: RETURN - Move back to rest position
            interaction.currentState = InteractionState.Returning;
            yield return StartCoroutine(ReturnPhase(interaction));
            
            // Interaction completed
            interaction.OnInteractionCompleted?.Invoke(interaction);
            interaction.currentState = InteractionState.Idle;
            interaction.currentCoroutine = null;
        }

        private IEnumerator ReachPhase(Interaction interaction)
        {
            Debug.Log($"REACH: {interaction.effectorType} -> {interaction.target.Desc}");

            // Look at the target if enabled
            if (interaction.useLookAt)
            {
                StartCoroutine(riggingController.LookAtTarget(interaction.target.InteractionPoint, interaction.reachDuration * 0.75f));
            }

            // Move effector to target
            yield return StartCoroutine(riggingController.MoveEffectorTargetToDestination(
                interaction.effectorType,
                interaction.target.InteractionPoint,
                interaction.reachDuration,
                interaction.reachCurve
            ));
        }

        private IEnumerator HoldPhase(Interaction interaction)
        {
            Debug.Log($"HOLD: staying at {interaction.target.Desc} for {interaction.holdDuration}s");

            float elapsed = 0f;
            while (elapsed < interaction.holdDuration)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }
        }

        private IEnumerator ReturnPhase(Interaction interaction)
        {
            Debug.Log($"RETURN: {interaction.effectorType} -> rest position");

            // Return gaze to rest
            if (interaction.useLookAt)
            {
                StartCoroutine(riggingController.ReturnGazeToRest(interaction.returnDuration));
            }

            // Return effector to rest
            yield return StartCoroutine(riggingController.ReturnEffectorTargetToRest(
                interaction.effectorType,
                interaction.returnDuration,
                interaction.returnCurve
            ));
        }
        
        /// <summary>
        /// Get the rigging controller
        /// </summary>
        public AnimationRiggingController RiggingController => riggingController;

        /// <summary>
        /// Get the rigging controller
        /// </summary>
        public InteractionEffector GetEffector(EffectorType type)
        {
            return riggingController.GetEffector(type);
        }
        
        /// <summary>
        /// Set the object being carried by the selected effector
        /// </summary>
        public void SetCarriedObj(Pickable pickableObj, EffectorType effectorType)
        {
            switch (effectorType)
            {
                case EffectorType.RightHand:
                    rightHandCarriedObj = pickableObj;
                    break;
                case EffectorType.LeftFoot:
                    leftHandCarriedObj = pickableObj;
                    break;
            }
        }

        /// <summary>
        /// Remove the object being carried by the selected effector
        /// </summary>
        public void RemoveCarriedObj(EffectorType effectorType)
        {
            SetCarriedObj(null,effectorType);
        }
    }
} 