using System;
using UnityEngine;
using UnityEngine.Animations.Rigging;
using System.Collections;
using System.Collections.Generic;

namespace HumanoidInteraction
{
    /// <summary>
    /// Manages Animation Rigging components and provides effector control
    /// </summary>
    [RequireComponent(typeof(InteractionSystem))]
    public class AnimationRiggingController : MonoBehaviour
    {
        [Header("Rigging Components")]
        [SerializeField] private RigBuilder rigBuilder;
        [SerializeField] private Rig rightArmRig;
        [SerializeField] private Rig leftArmRig;
        [SerializeField] private Rig rightLegRig;
        [SerializeField] private Rig leftLegRig;
        [SerializeField] private Rig headRig;
        [SerializeField] private Rig spineRig;

        [Header("IK Targets")]
        [SerializeField] private Transform rightHandTarget;
        [SerializeField] private Transform leftHandTarget;
        [SerializeField] private Transform rightFootTarget;
        [SerializeField] private Transform leftFootTarget;
        [SerializeField] private Transform lookAtTarget;

        [Header("Rest Positions")]
        [SerializeField] private Transform rightHandRest;
        [SerializeField] private Transform leftHandRest;
        [SerializeField] private Transform rightFootRest;
        [SerializeField] private Transform leftFootRest;
        [SerializeField] private Transform lookAtRest;
        
        [Header("Effector Attachments")]
        [SerializeField] private Transform rightHandAttach;
        [SerializeField] private Transform leftHandAttach;
        /*[SerializeField] private Transform rightFootAttach;
        [SerializeField] private Transform leftFootAttach;*/

        /*[Header("Hand Bones")]
        [SerializeField] private Transform rightHandBone;
        [SerializeField] private Transform leftHandBone;*/

        [Header("Animation Settings")]
        [SerializeField] private float blendSpeed = 2.0f;
        /*[SerializeField] private float moveSpeed = 3.0f;
        [SerializeField] private float lookSpeed = 5.0f;*/

        private InteractionEffector rightHandEffector;
        private InteractionEffector leftHandEffector;
        private InteractionEffector rightFootEffector;
        private InteractionEffector leftFootEffector;
        
        private Dictionary<InteractionEffector, Rig> rigMap;

        private InteractionSystem interactionSystem;
        
        private void Awake()
        {
            InitializeEffectorMaps();
            SetAllRigWeights(0f);
        }

        private void Start()
        {
            interactionSystem = this.GetComponent<InteractionSystem>();
        }

        private void InitializeEffectorMaps()
        {
            rightHandEffector = new InteractionEffector(EffectorType.RightHand, rightHandTarget, rightHandRest, rightHandAttach);
            leftHandEffector = new InteractionEffector(EffectorType.LeftHand, leftHandTarget, leftHandRest, leftHandAttach);
            rightFootEffector = new InteractionEffector(EffectorType.RightFoot, rightFootTarget, rightFootRest, null);
            leftFootEffector = new InteractionEffector(EffectorType.LeftFoot, leftFootTarget, leftFootRest, null);

            rigMap = new Dictionary<InteractionEffector, Rig>
            {
                { rightHandEffector, rightArmRig },
                { leftHandEffector, leftArmRig },
                { rightFootEffector, rightLegRig },
                { leftFootEffector, leftLegRig }
            };
        }

        /// <summary>
        /// Move an effector to a target position with animation
        /// </summary>
        public IEnumerator MoveEffectorTargetToDestination(EffectorType effectorType, Transform destination, float duration, AnimationCurve curve)
        {
            if (interactionSystem.GetEffector(effectorType) == null)
            {
                Debug.LogError($"Effector {effectorType} not found in effector map!");
                yield break;
            }

            // Enable the rig
            StartCoroutine(BlendRigWeight(rigMap[interactionSystem.GetEffector(effectorType)], 1.0f));
            // Move the target
            yield return StartCoroutine(MoveTarget(interactionSystem.GetEffector(effectorType).TargetTransform, destination, duration, curve));

            // WHY???
            // Disable the rig
            // yield return StartCoroutine(BlendRigWeight(rigMap[effector], 0.0f));
        }

        /// <summary>
        /// Move an effector back to its rest position
        /// </summary>
        public IEnumerator ReturnEffectorTargetToRest(EffectorType effectorType, float duration, AnimationCurve curve)
        {
            if (interactionSystem.GetEffector(effectorType) == null)
            {
                Debug.LogError($"Effector {effectorType} not found in effector map!");
                yield break;
            }

            // WHY???
            // Enable the rig
            // yield return StartCoroutine(BlendRigWeight(rigMap[effector], 1.0f));

            // Disable the rig
            StartCoroutine(BlendRigWeight(rigMap[interactionSystem.GetEffector(effectorType)], 0.0f));
            // Move back to rest
            yield return StartCoroutine(MoveTarget(interactionSystem.GetEffector(effectorType).TargetTransform, interactionSystem.GetEffector(effectorType).RestTransform, duration, curve));
        }

        /// <summary>
        /// Make the character look at a target
        /// </summary>
        public IEnumerator LookAtTarget(Transform target, float duration)
        {
            if (headRig == null || lookAtTarget == null) yield break;

            StartCoroutine(BlendRigWeight(headRig, 1.0f));
            yield return StartCoroutine(MoveTarget(lookAtTarget, target, duration, AnimationCurve.EaseInOut(0, 0, duration, 1)));
        }

        /// <summary>
        /// Return gaze to rest position
        /// </summary>
        public IEnumerator ReturnGazeToRest(float duration)
        {
            if (headRig == null || lookAtTarget == null || lookAtRest == null) yield break;

            StartCoroutine(BlendRigWeight(headRig, 0.0f));
            yield return StartCoroutine(MoveTarget(lookAtTarget, lookAtRest, duration, AnimationCurve.EaseInOut(0, 0, duration, 1)));
        }

        private IEnumerator BlendRigWeight(Rig rig, float targetWeight)
        {
            if (rig == null) yield break;

            float startWeight = rig.weight;
            float elapsed = 0f;
            float duration = Mathf.Abs(targetWeight - startWeight) / blendSpeed;
            if (duration <= 0) duration = 0.1f;

            while (elapsed < duration)
            {
                float t = elapsed / duration;
                rig.weight = Mathf.Lerp(startWeight, targetWeight, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            rig.weight = targetWeight;
        }

        private IEnumerator MoveTarget(Transform target, Transform destination, float duration, AnimationCurve curve)
        {
            if (target == null || destination == null) yield break;

            Vector3 startPos = target.position;
            Quaternion startRot = target.rotation;
            Vector3 endPos = destination.position;
            Quaternion endRot = destination.rotation;

            float elapsed = 0f;

            while (elapsed < duration)
            {
                float t = curve.Evaluate(elapsed / duration);
                target.position = Vector3.Lerp(startPos, endPos, t);
                target.rotation = Quaternion.Slerp(startRot, endRot, t);
                elapsed += Time.deltaTime;
                yield return null;
            }

            target.position = endPos;
            target.rotation = endRot;
        }

        private void SetAllRigWeights(float weight)
        {
            if (rightArmRig != null) rightArmRig.weight = weight;
            if (leftArmRig != null) leftArmRig.weight = weight;
            if (rightLegRig != null) rightLegRig.weight = weight;
            if (leftLegRig != null) leftLegRig.weight = weight;
            if (headRig != null) headRig.weight = weight;
            if (spineRig != null) spineRig.weight = weight;
        }

        /// <summary>
        /// Get the desired effector of the rig
        /// </summary>
        public InteractionEffector GetEffector(EffectorType type)
        {
            switch (type)
            {
                case EffectorType.RightHand:
                    return rightHandEffector;
                case EffectorType.LeftHand:
                    return leftHandEffector;
                case EffectorType.RightFoot:
                    return rightFootEffector;
                case EffectorType.LeftFoot:
                    return leftFootEffector;
                default:
                    return null;
            }
        }

        /// <summary>
        /// Check if an effector is currently being controlled by IK
        /// </summary>
        public bool IsEffectorActive(EffectorType type)
        {
            InteractionEffector effector = GetEffector(type);
            if (effector != null)
            {
                return rigMap[effector].weight > 0.1f;
            }
            return false;
        }
    }
} 