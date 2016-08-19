using UnityEngine;
using System.Collections.Generic;

using NLua;
using System.IO;
using System.Text.RegularExpressions;
using System;

public class GameConsole : MonoBehaviour {

    // CONSTANTS
    const int MARGIN = 10;
    const int EDITOR_HEIGHT = 160;
    const int HISTORY_LABEL_HEIGHT = 22;

    const int CONSOLE_HEIGHT = 20;

    static GameConsole instance;

    // HISTORY
    int historyRank;
    string commandSave;
    Vector2 historyScroll;

    // MULTI LINE COMMAND
    List<string> currentConsoleCode;
    int parDepth;
    int acoDepth;
    string currentConsoleCommand;

    string uri;
    string codeEditor;

    // SHOW/HIDE
    public bool showLuaConsole;
    public bool showLuaEditor;
    public bool showLuaHistory;
    public bool showLuaLog;

    // GUI ELEMENT POSITIONNING
    Rect mainBox;
    Rect historyBox;
    Rect consoleBox;
    Rect editorBox;
    Rect logBox;

    int cacheScreenWidth;
    int cacheScreenHeight;

    // EDITOR
    Vector2 editorScroll;

    ConsoleModel consoleModel;
    LoggerModel loggerModel;

    // LOG
    Vector2 logScroll;

    bool newPrint;
    bool newLog;

    void Start () {
        consoleModel = LuaConsole.getGameConsoleModel ();
        consoleModel.enable ();
        //model.onClear += 
        consoleModel.onHideConsole += hideConsole;
        consoleModel.onHideEditor += hideEditor;
        consoleModel.onHideHistory += hideHistory;
        consoleModel.onHideLog += hideLog;
        consoleModel.onLoadScript += (v => codeEditor = v);
        consoleModel.onNewMessage += (m => newPrint = true);
        consoleModel.onRunCurrent += (() => consoleModel.runString (codeEditor));
        consoleModel.onSaveCurrent += (() => consoleModel.saveScriptWithCode (uri, codeEditor));
        consoleModel.onSaveToNew += (v => consoleModel.saveScriptWithCode (v, codeEditor));
        consoleModel.onShowAll += showConsole;
        consoleModel.onShowEditor += showEditor;
        consoleModel.onShowHistory += showHistory;
        consoleModel.onShowLog += showLog;
        
        // Assure uniqueness
        if (instance != null) {
            consoleModel.printError ("There is already a console in game");
            Debug.Log ("There is already a console in game");
            Destroy (this);
            return;
        }
        instance = this;
        
        historyRank = -1;
        commandSave = "";
        historyScroll = Vector2.zero;

        currentConsoleCode = new List<string> ();
        parDepth = 0;
        currentConsoleCommand = "";

        codeEditor = "";
        uri = null;

        mainBox = new Rect ();
        historyBox = new Rect ();
        consoleBox = new Rect ();
        editorBox = new Rect ();
        logBox = new Rect ();
        updatePositions ();
        cacheScreenHeight = Screen.height;
        cacheScreenWidth = Screen.width;

        logScroll = Vector2.zero;
    }

    void OnGUI () {
        if (Event.current.Equals (Event.KeyboardEvent ("escape"))) {
            if (showLuaConsole) {
                hideConsole ();
            } else {
                showConsole ();
            }
            Event.current.Use ();
        }

        if (showLuaConsole) {
            // listen to change in screen size
            if (Screen.width != cacheScreenWidth || Screen.height != cacheScreenHeight) {
                updatePositions ();
                cacheScreenHeight = Screen.height;
                cacheScreenWidth = Screen.width;
            }

            if (GUI.GetNameOfFocusedControl () == "console") {
                if (Event.current.Equals (Event.KeyboardEvent ("return"))) {
                    consoleModel.runCurrentCommand(currentConsoleCode, currentConsoleCommand, ref parDepth, ref acoDepth);
                    currentConsoleCommand = "";
                    logScroll.y = float.PositiveInfinity;
                    historyRank = -1;
                    commandSave = null;
                }

                if (Event.current.Equals (Event.KeyboardEvent ("up"))) {
                    consoleModel.getPreviousCommand (ref historyRank, ref currentConsoleCommand, ref commandSave);
                } else if (Event.current.Equals (Event.KeyboardEvent ("down"))) {
                    consoleModel.getNextCommand (ref historyRank, ref currentConsoleCommand, ref commandSave);
                }
            }

            GUI.Box (mainBox, "");
            GUI.BeginGroup (mainBox);

            // Command prompt
            GUI.SetNextControlName ("console");
            currentConsoleCommand = GUI.TextField (consoleBox, currentConsoleCommand);

            // Log history
            if (showLuaLog) {
                LoggerModel logger = LuaConsole.getLoggerModel ();
                if(logger != loggerModel) {
                    if (loggerModel != null) {
                        foreach (Log log in loggerModel.logs) {
                            logger.addLog (log);
                        }
                    }
                    loggerModel = logger;
                    loggerModel.onLog += (log => newLog = true);
                    loggerModel.onHide += (() => showLuaLog = false);
                    loggerModel.onShow += (() => showLuaLog = true);
                }
                logScroll = GUI.BeginScrollView (logBox, logScroll, new Rect (0, 0, logBox.width, logger.logs.Count * HISTORY_LABEL_HEIGHT), GUIStyle.none, GUIStyle.none);
                for (int i = 0; i < logger.logs.Count; i++) {
                    Log log = logger.logs[i];
                    GUI.Label (new Rect (0, i * HISTORY_LABEL_HEIGHT, logBox.width, HISTORY_LABEL_HEIGHT), log.ToString());
                }

                if(newLog) {
                    logScroll.y = float.PositiveInfinity;
                    newLog = false;
                }

                GUI.EndScrollView ();
            }

            // Log history
            if (showLuaHistory) {
                historyScroll = GUI.BeginScrollView (historyBox, historyScroll, new Rect (0, 0, historyBox.width, consoleModel.history.Count * HISTORY_LABEL_HEIGHT), GUIStyle.none, GUIStyle.none);
                for (int i = 0; i < consoleModel.history.Count; i++) {
                    GUI.Label (new Rect (0, i * HISTORY_LABEL_HEIGHT, historyBox.width, HISTORY_LABEL_HEIGHT), consoleModel.history[i]);
                }

                if (newPrint) {
                    historyScroll.y = float.PositiveInfinity;
                    newPrint = false;
                }

                GUI.EndScrollView ();
            }

            // Editor
            if (showLuaEditor) {
                codeEditor = GUI.TextArea (editorBox, codeEditor);
            }

            GUI.EndGroup ();
        }
    }

    #region GUI Positionning

    public void updatePositions () {
        if(cacheScreenHeight == 0) {
            cacheScreenHeight = Screen.height;
        }
        if (cacheScreenWidth == 0) {
            cacheScreenWidth = Screen.width;
        }

        // main box
        mainBox.x = MARGIN;
        mainBox.width = cacheScreenWidth - 2 * MARGIN;

        if (showLuaEditor || showLuaHistory || showLuaLog) {
            mainBox.y = cacheScreenHeight - 4 * MARGIN - EDITOR_HEIGHT - CONSOLE_HEIGHT;
            mainBox.height = 3 * MARGIN + EDITOR_HEIGHT + CONSOLE_HEIGHT;
        } else {
            mainBox.y = cacheScreenHeight - 3 * MARGIN - CONSOLE_HEIGHT;
            mainBox.height = 2 * MARGIN + CONSOLE_HEIGHT;
        }

        // editor, history and log
        historyBox.y = MARGIN;
        historyBox.height = EDITOR_HEIGHT;
        editorBox.y = MARGIN;
        editorBox.height = EDITOR_HEIGHT;
        logBox.y = MARGIN;
        logBox.height = EDITOR_HEIGHT;

        int count = (showLuaLog?1:0) + (showLuaHistory?1:0) + (showLuaEditor?1:0);
        float width = (mainBox.width - (1 + count) * MARGIN) / count;
        float[] x = new float[3];
        x[0] = MARGIN;
        x[1] = 2 * MARGIN + width;
        x[2] = 3 * MARGIN + 2 * width;

        int current = 0;
        if(showLuaHistory) {
            historyBox.x = x[current];
            historyBox.width = width;
            current++;
        } 
        if(showLuaLog) {
            logBox.x = x[current];
            logBox.width = width;
            current++;
        }
        if(showLuaEditor) {
            editorBox.x = x[current];
            editorBox.width = width;
            current++;
        }

        consoleBox.x = MARGIN;
        consoleBox.y = mainBox.height - MARGIN - CONSOLE_HEIGHT;
        consoleBox.width = mainBox.width - 2 * MARGIN;
        consoleBox.height = CONSOLE_HEIGHT;
    }

    #endregion

    #region Show/Hide

    public void showConsole () {
        showLuaConsole = true;
        updatePositions ();
    }
    public void hideConsole () {
        showLuaConsole = false;
    }
    public void showEditor () {
        showLuaEditor = true;
        updatePositions ();
    }
    public void hideEditor () {
        showLuaEditor = false;
        updatePositions ();
    }
    public void showHistory () {
        showLuaHistory = true;
        updatePositions ();
    }
    public void hideHistory () {
        showLuaHistory = false;
        updatePositions ();
    }
    public void showLog() {
        showLuaLog = true;
        updatePositions ();
    }
    public void hideLog() {
        showLuaLog = false;
        updatePositions ();
    }

    #endregion
    
    #region helpers

    Texture2D MakeTex (int width, int height, Color col) {
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