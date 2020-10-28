using System.Collections.Generic;
using UnityEngine;

namespace RealtimeDebug
{
    public class Console : MonoBehaviour
    {
        struct ConsoleMessage
        {
            public readonly string message;
            public readonly string stackTrace;
            public readonly LogType type;        

            public ConsoleMessage(string message, string stackTrace, LogType type)
            {
                this.message = message;
                this.stackTrace = stackTrace;
                this.type = type;            
            }
        }

        List<ConsoleMessage> entries = new List<ConsoleMessage>();
        static ConsoleMessageFilter filter = new ConsoleMessageFilter();
        Vector2 scrollPos;
        bool show;
        bool collapse;
        bool showOnError;

        // Visual elements:

        const int margin = 20;
        Rect windowRect = new Rect(margin, margin, Screen.width - (2 * margin), Screen.height - (2 * margin));

        GUIContent clearLabel = new GUIContent("Clear", "Clear the contents of the console.");
        GUIContent collapseLabel = new GUIContent("Collapse", "Hide repeated messages.");
        GUIContent autoShowLabel = new GUIContent("AutoShow", "Auto show on errors");

        bool canToggle = true;

        private void OnEnable()
        {
            Application.logMessageReceived += this.HandleLog;
        }

        private void OnDisable()
        {
            Application.logMessageReceived -= this.HandleLog;
        }

        void Update()
        {
            if (canToggle && (Input.GetKeyDown(KeyCode.Space) || Input.touchCount > 3))
            {
                show = !show;
                canToggle = false;
            }

            if(Input.touchCount < 4)
            {
                canToggle = true;
            }
        }

        void OnGUI()
        {
            if (!show)
            {
                return;
            }
        
            windowRect = GUILayout.Window(123456, windowRect, ConsoleWindow, "Console");
        }

        /// <summary>
        /// A window displaying the logged messages.
        /// </summary>
        /// <param name="windowID">The window's ID.</param>
        void ConsoleWindow(int windowID)
        {
            scrollPos = GUILayout.BeginScrollView(scrollPos);

            // Go through each logged entry
            for (int i = 0; i < entries.Count; i++)
            {
                ConsoleMessage entry = entries[i];

                // If this message is the same as the last one and the collapse feature is chosen, skip it
                if (collapse && i > 0 && entry.message == entries[i - 1].message)
                {
                    continue;
                }

                // Change the text colour according to the log type
                switch (entry.type)
                {
                    case LogType.Assert:
                    case LogType.Error:
                    case LogType.Exception:
                        GUI.contentColor = Color.red;
                        GUILayout.Label(entry.message + "\n" + entry.stackTrace);                                       

                        if(GUILayout.Button("Skip", GUILayout.ExpandWidth(false)))
                        {
                            filter.SetFilter(entry.message, entry.stackTrace, true);
                        }

                        show = true;
                        break;

                    case LogType.Warning:
                        GUI.contentColor = Color.yellow;
                        GUILayout.Label(entry.message);
                        break;

                    default:
                        GUI.contentColor = Color.white;
                        GUILayout.Label(entry.message);
                        break;
                }
            }

            GUI.contentColor = Color.white;

            GUILayout.EndScrollView();

            GUILayout.BeginHorizontal();

            // Clear button
            if (GUILayout.Button(clearLabel))
            {
                entries.Clear();
            }

            // Auto show on error toggle
            if(showOnError != GUILayout.Toggle(showOnError, autoShowLabel, GUILayout.ExpandWidth(false)))
            {
                showOnError = !showOnError;
            }

            // Collapse toggle
            collapse = GUILayout.Toggle(collapse, collapseLabel, GUILayout.ExpandWidth(false));

            GUILayout.EndHorizontal();

            // Set the window to be draggable by the top title bar
            GUI.DragWindow(new Rect(0, 0, 10000, 20));
        }

        /// <summary>
        /// Logged messages are sent through this callback function.
        /// </summary>
        /// <param name="message">The message itself.</param>
        /// <param name="stackTrace">A trace of where the message came from.</param>
        /// <param name="type">The type of message: error/exception, warning, or assert.</param>
        void HandleLog(string message, string stackTrace, LogType type)
        {
#if !UNITY_EDITOR
        if(showOnError 
            && (type == LogType.Error || type == LogType.Exception|| type == LogType.Assert) 
            && !filter.IsFiltered(message, stackTrace))
        {
            show = true;
        }
#endif

            if (message.Length > 1000)
                message = message.Substring(0, 1000);
            ConsoleMessage entry = new ConsoleMessage(message, stackTrace, type);
            entries.Add(entry);
        }
    }
}