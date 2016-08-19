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
    Vector2 stackScroll;

    [SerializeField]
    bool newPrint;
    [SerializeField]
    bool newLog;
    
    [SerializeField]
    Log selectedLog;
    [SerializeField]
    Log hoverLog;

    [SerializeField]
    int selectedStack;
    [SerializeField]
    int hoverStack;

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
        if (consoleModel == null) {
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
        consoleModel.onHideLog += (() => showLog = false);
        consoleModel.onLoadScript += (v => editorCode = v);
        consoleModel.onNewMessage += (s => newPrint = true);
        consoleModel.onRunCurrent += (() => consoleModel.runString (editorCode));
        consoleModel.onSaveCurrent += (() => consoleModel.saveScriptWithCode (uri, editorCode));
        consoleModel.onSaveToNew += (newURI => consoleModel.saveScriptWithCode (newURI, editorCode));
        consoleModel.onShowAll += (() => showEditor = showHistory = true);
        consoleModel.onShowEditor += (() => showEditor = true);
        consoleModel.onShowHistory += (() => showHistory = true);
        consoleModel.onShowLog += (() => showLog = true);

        loggerModel.onLog += (log => newLog = true);
        loggerModel.onShow += (() => showLog = true);
        loggerModel.onHide += (() => showLog = false);
        // loggerModel.onClear += ;
        // loggerModel.onTypeChange += ;
        // loggerModel.onChannelChange += ;

        EditorApplication.playmodeStateChanged += onPlayModeChanged;
    }

    void OnDisable() {
        consoleModel.disable ();
    }

    void OnGUI() {
        GUIStyle gray1 = new GUIStyle();
        GUIStyle gray2 = new GUIStyle();
        GUIStyle selected = new GUIStyle();
        GUIStyle hover = new GUIStyle();
        gray1.normal.background = MakeTex (1, 1, new Color (0.75f, 0.75f, 0.75f));
        gray2.normal.background = MakeTex (1, 1, new Color (0.8f, 0.8f, 0.8f));
        selected.normal.background = MakeTex (1, 1, new Color (0.6f, 0.6f, 0.7f));
        hover.normal.background = MakeTex (1, 1, new Color (0.72f, 0.72f, 0.72f));

        GUIStyle[] logTypeFont = new GUIStyle[Enum.GetValues(typeof(LogType)).Length];
        GUIStyle fontStyle = new GUIStyle();
        fontStyle.normal.textColor = Color.red;
        logTypeFont[(int) LogType.error] = fontStyle;
        fontStyle = new GUIStyle();
        fontStyle.normal.textColor = Color.blue;
        logTypeFont[(int) LogType.info] = fontStyle;
        fontStyle = new GUIStyle();
        fontStyle.normal.textColor = Color.yellow;
        logTypeFont[(int) LogType.warning] = fontStyle;
        fontStyle = new GUIStyle();
        fontStyle.normal.textColor = Color.magenta;
        logTypeFont[(int) LogType.exception] = fontStyle;
        fontStyle = new GUIStyle ();
        fontStyle.normal.textColor = Color.white;
        logTypeFont[(int) LogType.test] = fontStyle;

        GUIStyle messagePrint = new GUIStyle();
        messagePrint.normal.textColor = new Color (0.3f, 0.3f, 0.3f);
        GUIStyle errorPrint = new GUIStyle();
        errorPrint.normal.textColor = new Color (1, 0.3f, 0.3f);

        LuaConsole.setLoggerModel (loggerModel);
        
        Repaint ();
        
        if (Event.current.type == EventType.Layout) {
            if (GUI.GetNameOfFocusedControl () == "console") {
                if (Event.current.keyCode == KeyCode.Return) {
                    if (currentConsoleCommand != "") {
                        consoleModel.runCurrentCommand (currentConsoleCode, currentConsoleCommand, ref parDepth, ref acoDepth);
                        currentConsoleCommand = "";
                        historyRank = -1;
                        commandSave = null;
                    }
                }

                if (Event.current.keyCode == KeyCode.UpArrow && Event.current.type == EventType.keyDown) {
                    Debug.Log ("up : " + historyRank);
                    consoleModel.getPreviousCommand (ref historyRank, ref currentConsoleCommand, ref commandSave);
                } else if (Event.current.keyCode == KeyCode.DownArrow) {
                    Debug.Log ("down");
                    consoleModel.getNextCommand (ref historyRank, ref currentConsoleCommand, ref commandSave);
                }

                EditorGUI.FocusTextInControl ("console");
            }
        }

        if (consoleModel == null) {
            OnEnable ();
        }

        // TOOLBAR
        EditorGUILayout.BeginHorizontal (EditorStyles.toolbar);

        showHistory = GUILayout.Toggle (showHistory, "History", EditorStyles.toolbarButton);
        showLog = GUILayout.Toggle (showLog, "Log", EditorStyles.toolbarButton);
        showEditor = GUILayout.Toggle (showEditor, "Editor", EditorStyles.toolbarButton);

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

        GUILayout.FlexibleSpace ();

        if (showLog) {
            foreach(string channel in loggerModel.channels) {
                bool oldVal = loggerModel.visibleChannels.Contains(channel);
                bool newVal = GUILayout.Toggle (oldVal, channel, EditorStyles.toolbarButton);
                if (newVal && !oldVal) {
                    loggerModel.enableChannel (channel);
                } else if (!newVal && oldVal) {
                    loggerModel.disableChannel (channel);
                }
            }

            GUILayout.Space (10);

            if(GUILayout.Button("Clear", EditorStyles.toolbarButton)) {
                loggerModel.clear ();
            }
        }

        EditorGUILayout.EndHorizontal ();

        EditorGUILayout.BeginVertical ();
         
        // MAIN AREA
        EditorGUILayout.BeginHorizontal ();

        if (showHistory) {
            EditorGUILayout.BeginVertical (GUILayout.Width (historyWidth), GUILayout.Height (position.height - 60));

            historyScroll = GUILayout.BeginScrollView (historyScroll);
            for (int i = 0; i < consoleModel.history.Count; i++) {
                if (i % 2 == 0) {
                    EditorGUILayout.BeginHorizontal (gray1);
                } else {
                    EditorGUILayout.BeginHorizontal (gray2);
                }
                if (consoleModel.history[i][0] == ' ') {
                    EditorGUILayout.LabelField (consoleModel.history[i], messagePrint);
                } else if (consoleModel.history[i][0] == '!') {
                    EditorGUILayout.LabelField (consoleModel.history[i], errorPrint);
                } else {
                    EditorGUILayout.LabelField (consoleModel.history[i]);
                }
                EditorGUILayout.EndHorizontal ();
            }

            if (newPrint) {
                historyScroll.y = float.PositiveInfinity;
                newPrint = false;
            }

            GUILayout.EndScrollView ();

            EditorGUILayout.EndVertical ();
        }

        if (showLog) {
            EditorGUILayout.BeginVertical (GUILayout.Width (logWidth), GUILayout.Height (position.height - 60));

            EditorGUILayout.BeginHorizontal ();

            logScroll = GUILayout.BeginScrollView (logScroll);
            int i = 0;
            foreach (Log log in loggerModel.getLogs ()) {
                if (log == selectedLog) {
                    EditorGUILayout.BeginHorizontal (selected);
                } else if (log == hoverLog) {
                    EditorGUILayout.BeginHorizontal (hover);
                } else {
                    if (i % 2 == 0) {
                        EditorGUILayout.BeginHorizontal (gray1);
                    } else {
                        EditorGUILayout.BeginHorizontal (gray2);
                    }
                }

                EditorGUILayout.BeginVertical (GUILayout.Width (70));

                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField (log.type.ToString (), logTypeFont[(int) log.type]);
                EditorGUILayout.EndHorizontal ();

                EditorGUILayout.BeginHorizontal ();
                EditorGUILayout.LabelField ("[" + (log.target != null ? log.target + ":" : "") + (log.channel != "" ? log.channel : "Default") + "]");
                EditorGUILayout.EndHorizontal ();

                EditorGUILayout.EndVertical ();

                EditorGUILayout.BeginVertical ();
                EditorGUILayout.LabelField (log.msg);
                EditorGUILayout.EndVertical ();

                //EditorGUILayout.LabelField (log.ToString());

                EditorGUILayout.EndHorizontal ();

                if (GUILayoutUtility.GetLastRect ().Contains (Event.current.mousePosition)) {
                    hoverLog = log;
                    if (Event.current.type == EventType.mouseDown) {
                        selectedLog = log;
                        selectedStack = -1;
                        hoverStack = -1;
                    }
                    if (Event.current.clickCount >= 2) {
                        string file;
                        int line;
                        log.getLineAndFile (out line, out file);
                        //Debug.Log(file + " " + line);
                        if (line >= 0) {
                            UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal (file, line);
                        }
                    }
                }

                i++;
            }

            if (newLog) {
                logScroll.y = float.PositiveInfinity;
                newLog = false;
            }
            GUILayout.EndScrollView ();

            EditorGUILayout.EndHorizontal ();

            if (selectedLog != null && loggerModel.logs.Contains(selectedLog)) {
                EditorGUILayout.BeginHorizontal (GUILayout.Height (position.height * 1 / 5));
                stackScroll = EditorGUILayout.BeginScrollView (stackScroll);

                for (int j = 0; j < selectedLog.stack.Count; j++) {
                    if (j == selectedStack) {
                        EditorGUILayout.BeginHorizontal (selected);
                    } else if (j == hoverStack) {
                        EditorGUILayout.BeginHorizontal (hover);
                    } else {
                        if (j % 2 == 0) {
                            EditorGUILayout.BeginHorizontal (gray1);
                        } else {
                            EditorGUILayout.BeginHorizontal (gray2);
                        }
                    }
                    EditorGUILayout.LabelField (selectedLog.stack[j]);
                    EditorGUILayout.EndHorizontal ();

                    if (GUILayoutUtility.GetLastRect ().Contains (Event.current.mousePosition)) {
                        hoverStack = j;
                        if (Event.current.type == EventType.mouseDown) {
                            selectedStack = j;
                        }
                        if (Event.current.clickCount >= 2) {
                            string file;
                            int line;
                            selectedLog.getLineAndFile (j, out line, out file);
                            //Debug.Log(file + " " + line);
                            if (line >= 0) {
                                UnityEditorInternal.InternalEditorUtility.OpenFileAtLineExternal (file, line);
                            }
                        }
                    }
                }

                EditorGUILayout.EndScrollView ();
                EditorGUILayout.EndHorizontal ();
            }

            EditorGUILayout.EndVertical ();
        }

        if (showEditor) {
            EditorGUILayout.BeginVertical (GUILayout.Width(editorWidth), GUILayout.Height(position.height - 60));

            editorScroll = GUILayout.BeginScrollView (editorScroll);
            editorCode = EditorGUILayout.TextArea (editorCode, GUILayout.MinHeight (position.height - 65));
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

    #region General helpers
    private Texture2D MakeTex (int width, int height, Color col) {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels (pix);
        result.Apply ();

        return result;
    }
    #endregion
}
