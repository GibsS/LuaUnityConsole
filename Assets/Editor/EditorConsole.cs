using UnityEngine;
using System.Collections;
using UnityEditor;
using System;
using System.Collections.Generic;

public class EditorConsole : EditorWindow {

    const int CONSOLE_HEIGHT = 20;

    [SerializeField]
    LoggerModel loggerModel;
    [SerializeField]
    ConsoleModel consoleModel;

    [SerializeField]
    bool showEditor;
    [SerializeField]
    bool showHistory;
    [SerializeField]
    bool showLog;
    [SerializeField]
    bool showAll;

    [SerializeField]
    int parf, paro, parDepth, acoDepth;

    [SerializeField]
    int historyRank;
    [SerializeField]
    string commandSave;

    [SerializeField]
    List<string> currentConsoleCode;
    [SerializeField]
    string currentConsoleCommand;

    [SerializeField]
    string uri;
    [SerializeField]
    string editorCode;

    [SerializeField]
    Vector2 editorScroll;
    [SerializeField]
    Vector2 historyScroll;
    [SerializeField]
    Vector2 logScroll;

    [SerializeField]
    bool newPrint;

    int windowCount { get { return (showEditor ? 1 : 0) + (showHistory ? 1 : 0) + (showLog ? 1 : 0); } }
    int editorWidth { get { return (int) (position.width / windowCount); } }
    int historyWidth { get { return (int) (position.width / windowCount); } }
    int logWidth { get { return (int) (position.width / windowCount); } }

    [MenuItem ("Window/Lua console")]
    static void Init() {
        EditorConsole window = (EditorConsole)EditorWindow.GetWindow(typeof(EditorConsole));
        window.Show ();
    }

    void onPlayModeChanged () {

    }
    void OnEnable () {
        if(consoleModel == null) {
            consoleModel = LuaConsole.getEditorConsoleModel ();
            loggerModel = LuaConsole.getLoggerModel ();
            historyRank = -1;
            commandSave = null;
            currentConsoleCode = new List<string> ();
        }
        consoleModel.enable ();

        //model.onClear += ;
        consoleModel.onHideConsole += (() => showEditor = showHistory = false);
        consoleModel.onHideEditor += (() => showEditor = false);
        consoleModel.onHideHistory += (() => showHistory = false);
        consoleModel.onLoadScript += (v => editorCode = v);
        consoleModel.onNewMessage += (s => newPrint = true);
        consoleModel.onRunCurrent += (() => consoleModel.runString (editorCode));
        consoleModel.onSaveCurrent += (() => consoleModel.saveScriptWithCode (uri, editorCode));
        consoleModel.onSaveToNew += (newURI => consoleModel.saveScriptWithCode (newURI, editorCode));
        consoleModel.onShowAll += (() => showEditor = showHistory = true);
        consoleModel.onShowEditor += (() => showEditor = true);
        consoleModel.onShowHistory += (() => showHistory = true);
        
        EditorApplication.playmodeStateChanged += onPlayModeChanged;
    }

    void OnDisable() {
        consoleModel.disable ();
    }

    void OnGUI() {
        LuaConsole.setLoggerModel (loggerModel);
        
        Repaint ();
        
        if (Event.current.type == EventType.Layout) {
            if (GUI.GetNameOfFocusedControl () == "console") {
                if (Event.current.keyCode == KeyCode.Return) {
                    Debug.Log ("run command");
                    consoleModel.runCurrentCommand (currentConsoleCode, currentConsoleCommand, ref parDepth, ref acoDepth);
                    currentConsoleCommand = "";
                    historyRank = -1;
                    commandSave = null;

                    EditorGUI.FocusTextInControl ("console");
                }
            }

            //if (Event.current.keyCode == KeyCode.UpArrow) {
            //    model.getPreviousCommand (ref historyRank, ref currentConsoleCommand, ref commandSave);
            //} else if (Event.current.keyCode == KeyCode.DownArrow) {
            //    model.getNextCommand (ref historyRank, ref currentConsoleCommand, ref commandSave);
            //}
        }

        if (consoleModel == null) {
            OnEnable ();
        }
        // TOOLBAR
        EditorGUILayout.BeginHorizontal (EditorStyles.toolbar);

        showEditor = GUILayout.Toggle (showEditor, "Editor", EditorStyles.toolbarButton);
        showHistory = GUILayout.Toggle (showHistory, "History", EditorStyles.toolbarButton);
        showLog = GUILayout.Toggle (showLog, "Log", EditorStyles.toolbarButton);

        showAll = showEditor && showHistory && showLog;
        bool newShowAll = GUILayout.Toggle (showAll, "Show all", EditorStyles.toolbarButton);
        if(newShowAll != showAll) {
            showEditor = showHistory = showLog = showAll = newShowAll;
        }

        GUILayout.FlexibleSpace ();

        if (showLog) {
            for (int i = 0; i < loggerModel.typeOn.Length; i++) {
                bool tmp = GUILayout.Toggle (loggerModel.typeOn[i], loggerModel.logTypeNames[i], EditorStyles.toolbarButton);
                if(tmp && !loggerModel.typeOn[i]) {
                    loggerModel.enableType ((LogType)i);
                } else if(!tmp && loggerModel.typeOn[i]) {
                    loggerModel.disableType ((LogType) i);
                }
            }
        }

        EditorGUILayout.EndHorizontal ();

        EditorGUILayout.BeginHorizontal (EditorStyles.toolbar);

        if (showLog) {
            foreach(string channel in loggerModel.channels) {
                bool oldVal = loggerModel.visibleChannels.Contains(channel);
                bool newVal = GUILayout.Toggle (oldVal, channel, EditorStyles.toolbarButton);
                if (newVal && !oldVal) {
                    loggerModel.enableChannel (channel);
                } else if (!newVal && oldVal) {
                    loggerModel.disableChannel (channel);
                }
                //bool tmp = GUILayout.Toggle (loggerModel.channelOn[channel], channel, EditorStyles.toolbarButton);
                //if(tmp && !loggerModel.channelOn[channel]) {
                //    loggerModel.enableChannel (channel);
                //} else if (!tmp && loggerModel.channelOn[channel]) {
                //    loggerModel.disableChannel (channel);
                //}
            }
        }
        GUILayout.FlexibleSpace ();

        EditorGUILayout.EndHorizontal ();

        EditorGUILayout.BeginVertical ();
         
        // MAIN AREA
        EditorGUILayout.BeginHorizontal ();

        if (showEditor) {
            EditorGUILayout.BeginVertical (GUILayout.Width(editorWidth), GUILayout.Height(position.height - 60));

            editorScroll = GUILayout.BeginScrollView (editorScroll);
            editorCode = EditorGUILayout.TextArea (editorCode, GUILayout.MinHeight (position.height - 65));
            GUILayout.EndScrollView ();

            EditorGUILayout.EndVertical ();
        }

        if (showHistory) {
            EditorGUILayout.BeginVertical (GUILayout.Width(historyWidth), GUILayout.Height (position.height - 60));

            historyScroll = GUILayout.BeginScrollView (historyScroll);
            for (int i = 0; i < consoleModel.history.Count; i++) {
                EditorGUILayout.LabelField (consoleModel.history[i]);
            }

            if (newPrint) {
                historyScroll.y = float.PositiveInfinity;
                newPrint = false;
            }

            GUILayout.EndScrollView ();
            
            EditorGUILayout.EndVertical ();
        }

        if(showLog) {
            EditorGUILayout.BeginVertical (GUILayout.Width (logWidth), GUILayout.Height (position.height - 60));

            logScroll = GUILayout.BeginScrollView (logScroll);
            foreach(Log log in loggerModel.getLogs()) {
                EditorGUILayout.LabelField ("[" + log.target + ":" + log.channel + "] " + log.msg);
            }
            GUILayout.EndScrollView ();
            EditorGUILayout.EndVertical ();
        }

        EditorGUILayout.EndHorizontal ();

        GUILayout.FlexibleSpace ();

        // CONSOLE
        GUI.SetNextControlName ("console");
        currentConsoleCommand = EditorGUILayout.TextField (currentConsoleCommand, GUILayout.Height(CONSOLE_HEIGHT));

        EditorGUILayout.EndVertical ();

    }
}
