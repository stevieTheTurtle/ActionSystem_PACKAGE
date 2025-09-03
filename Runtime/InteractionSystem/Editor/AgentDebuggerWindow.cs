using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Reflection;
using AgentActionSystem;

public class AgentDebuggerWindow : EditorWindow
{
    private Agent selectedAgent;
    private int selectedAgentInstanceId; // To store the instance ID
    private Vector2 scrollPosition;
    private bool showCurrentActionDetails = true;
    private readonly Dictionary<int, bool> showActionDetails = new Dictionary<int, bool>();
    private readonly Dictionary<int, bool> showPastActionDetails = new Dictionary<int, bool>();

    // Key to save the agent's ID
    private const string SelectedAgentIdKey = "AgentDebugger_SelectedAgentId";

    // Method to create the menu item and open the window
    [MenuItem("Window/Agent Action Debugger")]
    public static void ShowWindow()
    {
        GetWindow<AgentDebuggerWindow>("Agent Action Debugger");
    }

    // This method is called when the window is enabled
    private void OnEnable()
    {
        // Retrieve the saved instance ID
        selectedAgentInstanceId = SessionState.GetInt(SelectedAgentIdKey, 0);
        if (selectedAgentInstanceId != 0)
        {
            // Find the agent in the scene using the saved ID
            selectedAgent = EditorUtility.InstanceIDToObject(selectedAgentInstanceId) as Agent;
        }
    }

    // This method is called to draw the window's GUI
    void OnGUI()
    {
        // Add a title label
        GUILayout.Label("Agent Action Inspector", EditorStyles.boldLabel);

        // Create an object field to select an Agent from the scene
        EditorGUI.BeginChangeCheck();
        selectedAgent = (Agent)EditorGUILayout.ObjectField("Selected Agent", selectedAgent, typeof(Agent), true);
        if (EditorGUI.EndChangeCheck())
        {
            // If the selection changes, save the new instance ID
            if (selectedAgent != null)
            {
                selectedAgentInstanceId = selectedAgent.GetInstanceID();
                SessionState.SetInt(SelectedAgentIdKey, selectedAgentInstanceId);
            }
            else
            {
                // If the agent is deselected, clear the saved ID
                SessionState.EraseInt(SelectedAgentIdKey);
            }
        }


        // Add a separator
        EditorGUILayout.Space();

        // If no agent is selected, display a help message
        if (selectedAgent == null)
        {
            EditorGUILayout.HelpBox("Select an Agent to inspect its action queue.", MessageType.Info);
            return;
        }

        // Create a scroll view for the action list
        scrollPosition = EditorGUILayout.BeginScrollView(scrollPosition);

        // Use reflection to get the private fields for the current action, the action queue and past actions
        FieldInfo currentActionField = typeof(Agent).GetField("currentAction", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo actionsQueueField = typeof(Agent).GetField("actionsQueue", BindingFlags.NonPublic | BindingFlags.Instance);
        FieldInfo pastActionsField = typeof(Agent).GetField("pastActions", BindingFlags.NonPublic | BindingFlags.Instance);


        if (currentActionField != null)
        {
            AgentAction currentAction = (AgentAction)currentActionField.GetValue(selectedAgent);
            if (currentAction != null)
            {
                // Display the current action
                EditorGUILayout.LabelField("Current Action", EditorStyles.boldLabel);
                showCurrentActionDetails = EditorGUILayout.Foldout(showCurrentActionDetails, currentAction.GetType().Name, true);
                if (showCurrentActionDetails)
                {
                    EditorGUI.indentLevel++;
                    DisplayActionDetails(currentAction);
                    EditorGUI.indentLevel--;
                }
            }
        }

        EditorGUILayout.Space();

        if (actionsQueueField != null)
        {
            // Get the list of actions from the agent's queue
            List<AgentAction> actionsQueue = (List<AgentAction>)actionsQueueField.GetValue(selectedAgent);

            if (actionsQueue != null && actionsQueue.Count > 0)
            {
                // Display the queued actions
                EditorGUILayout.LabelField("Actions Queue", EditorStyles.boldLabel);
                for (int i = 0; i < actionsQueue.Count; i++)
                {
                    if (!showActionDetails.ContainsKey(i))
                    {
                        showActionDetails[i] = true;
                    }
                    showActionDetails[i] = EditorGUILayout.Foldout(showActionDetails[i], $"Action {i}: {actionsQueue[i].GetType().Name}", true);

                    if (showActionDetails[i])
                    {
                        EditorGUI.indentLevel++;
                        DisplayActionDetails(actionsQueue[i]);
                        EditorGUI.indentLevel--;
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("Action queue is empty.");
            }
        }
        
        EditorGUILayout.Space();

        if (pastActionsField != null)
        {
            // Get the list of past actions from the agent
            List<AgentAction> pastActions = (List<AgentAction>)pastActionsField.GetValue(selectedAgent);

            if (pastActions != null && pastActions.Count > 0)
            {
                // Display the past actions
                EditorGUILayout.LabelField("Past Actions", EditorStyles.boldLabel);
                for (int i = 0; i < pastActions.Count; i++)
                {
                    if(pastActions[i] == null) continue;
                    
                    if (!showPastActionDetails.ContainsKey(i))
                    {
                        showPastActionDetails[i] = false; // Default to collapsed
                    }
                    showPastActionDetails[i] = EditorGUILayout.Foldout(showPastActionDetails[i], $"Action {i}: {pastActions[i].GetType().Name}", true);

                    if (showPastActionDetails[i])
                    {
                        EditorGUI.indentLevel++;
                        DisplayActionDetails(pastActions[i]);
                        EditorGUI.indentLevel--;
                    }
                }
            }
            else
            {
                EditorGUILayout.LabelField("No past actions recorded.");
            }
        }


        EditorGUILayout.EndScrollView();
    }

    // Helper method to display the details of a single AgentAction
    private void DisplayActionDetails(AgentAction action)
    {
        if (action == null) return;

        // Use reflection to get the state of the action
        FieldInfo stateField = typeof(AgentAction).GetField("state", BindingFlags.NonPublic | BindingFlags.Instance);
        if (stateField != null)
        {
            ActionState state = (ActionState)stateField.GetValue(action);
            EditorGUILayout.LabelField("State", state.ToString());
        }

        // Get all fields of the action's specific type to display its parameters
        FieldInfo[] fields = action.GetType().GetFields(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
        foreach (FieldInfo field in fields)
        {
            // Skip fields from the base class to avoid redundancy, unless they are specific parameters
            if (field.DeclaringType != typeof(AgentAction) || field.IsPublic)
            {
                object value = field.GetValue(action);
                string valueStr = value != null ? value.ToString() : "null";

                // For Unity objects, create an object field to allow easy inspection
                if (value is Object unityObject)
                {
                     EditorGUILayout.ObjectField(field.Name, unityObject, field.FieldType, true);
                }
                else
                {
                    EditorGUILayout.LabelField(field.Name, valueStr);
                }
            }
        }
    }

    // This method is called frequently to update the window
    void OnInspectorUpdate()
    {
        // Repaint the window to see changes in real-time during play mode
        Repaint();
    }
}
