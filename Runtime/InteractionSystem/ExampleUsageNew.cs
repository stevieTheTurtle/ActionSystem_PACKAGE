using System.Collections.Generic;
using AgentActionSystem;
using UnityEngine;
using UnityEngine.UI;

namespace HumanoidInteraction
{
    /// <summary>
    /// Example script demonstrating how to use the interaction system
    /// </summary>
    public class ExampleUsageNew : MonoBehaviour
    {
        [Header("Components")]
        [SerializeField] private SimpleAgent agent;

        [Header("Test Objects")]
        [SerializeField] private Interactable interactableObj;
        [SerializeField] private Pickable pickableObj;
        [SerializeField] private Transform dropTransform;
        
        [Header("Debug")]
        [SerializeField] private bool enableDebugLogging = true;

        [SerializeField] private AgentAction lastAction = null;
        
        private void Start()
        {
            if (agent == null)
                agent = GetComponent<SimpleAgent>();
        }

        private void Update()
        {
            HandleKeyboardInput();
        }

        private void HandleKeyboardInput()
        {
            // Touch action
            if (Input.GetKeyDown(KeyCode.T))
            {
                TestTouchAction();
            }
            
            // Pick action
            if (Input.GetKeyDown(KeyCode.P))
            {
                TestPickAction();
            }
            
            // Drop action
            if (Input.GetKeyDown(KeyCode.D))
            {
                TestDropAction();
            }
            
            // Walk action
            if (Input.GetKeyDown(KeyCode.W))
            {
                TestWalkAction();
            }

            // Stop current action
            if (Input.GetKeyDown(KeyCode.Backspace))
            {
                StopCurrentAction();
            }
        }

        /// <summary>
        /// Example: Simple touch interaction
        /// </summary>
        public void TestTouchAction()
        {
                agent.Touch(interactableObj, EffectorType.RightHand);
        }

        /// <summary>
        /// Example: Pick action
        /// </summary>
        public void TestPickAction()
        {
            agent.Pick(pickableObj, EffectorType.LeftHand);
        }
        
        /// <summary>
        /// Example: Drop action
        /// </summary>
        public void TestDropAction()
        {
            agent.Drop(pickableObj, dropTransform, EffectorType.LeftHand);
        }
        
        /// <summary>
        /// Example: Walk action
        /// </summary>
        public void TestWalkAction()
        {
            GameObject destinationGO = GameObject.Find("WalkAction_Destination");
            if (destinationGO == null)
                destinationGO = new GameObject("WalkAction_Destination");
            
            agent.Walk(destinationGO.transform);
        }

        public void StopCurrentAction()
        {
            agent.StopCurrentAction();
        }

        private void OnInteractionStarted(Interaction interaction)
        {
            if (enableDebugLogging)
                Debug.Log($"Interaction started: {interaction.interactionType} with {interaction.target.Desc} using {interaction.effectorType}");
            
            interaction.OnInteractionStarted -= OnInteractionStarted;
        }

        private void OnInteractionCompleted(Interaction interaction)
        {
            if (enableDebugLogging)
                Debug.Log($"Interaction completed: {interaction.interactionType} with {interaction.target.Desc}");
            
            interaction.OnInteractionCompleted -= OnInteractionCompleted;
        }

        private void OnInteractionFailed(Interaction interaction)
        {
            if (enableDebugLogging)
                Debug.LogWarning($"Interaction failed: {interaction.interactionType} with {interaction.target.Desc}");
            
            interaction.OnInteractionFailed -= OnInteractionFailed;
        }
    }
} 