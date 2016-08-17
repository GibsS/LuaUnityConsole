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
    Vector2 logScroll;

    // MULTI LINE COMMAND
    List<string> currentConsoleCode;
    int parDepth;
    int acoDepth;
    string currentConsoleCommand;

    string uri;
    string codeEditor;

    // SHOW/HIDE
    bool showLuaConsole;
    bool showLuaEditor;
    bool showLuaHistory;

    // GUI ELEMENT POSITIONNING
    Rect mainBox;
    Rect historyBox;
    Rect consoleBox;
    Rect editorBox;

    int cacheScreenWidth;
    int cacheScreenHeight;

    // EDITOR
    Vector2 editorScroll;

    ConsoleModel model;

    void Start () {
        model = LuaConsole.getGameConsoleModel ();
        model.enable ();
        //model.onClear += 
        model.onHideConsole += hideConsole;
        model.onHideEditor += hideEditor;
        model.onHideHistory += hideHistory;
        model.onLoadScript += (v => codeEditor = v);
        //model.onNewMessage += 
        model.onRunCurrent += (() => model.runString (codeEditor));
        model.onSaveCurrent += (() => model.saveScriptWithCode (uri, codeEditor));
        model.onSaveToNew += (v => model.saveScriptWithCode (v, codeEditor));
        model.onShowAll += showConsole;
        model.onShowEditor += showEditor;
        model.onShowHistory += showHistory;
        
        // Assure uniqueness
        if (instance != null) {
            model.printError ("There is already a console in game");
            Debug.Log ("There is already a console in game");
            Destroy (this);
            return;
        }
        instance = this;
        
        historyRank = -1;
        commandSave = "";
        logScroll = Vector2.zero;

        currentConsoleCode = new List<string> ();
        parDepth = 0;
        currentConsoleCommand = "";

        codeEditor = "";
        uri = null;

        showLuaConsole = true;
        showLuaEditor = true;
        showLuaHistory = true;

        mainBox = new Rect ();
        historyBox = new Rect ();
        consoleBox = new Rect ();
        editorBox = new Rect ();
        updatePositions ();
        cacheScreenHeight = Screen.height;
        cacheScreenWidth = Screen.width;
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
                    model.runCurrentCommand(currentConsoleCode, currentConsoleCommand, ref parDepth, ref acoDepth);
                    currentConsoleCommand = "";
                    logScroll.y = float.PositiveInfinity;
                    historyRank = -1;
                    commandSave = null;
                }

                if (Event.current.Equals (Event.KeyboardEvent ("up"))) {
                    model.getPreviousCommand (ref historyRank, ref currentConsoleCommand, ref commandSave);
                } else if (Event.current.Equals (Event.KeyboardEvent ("down"))) {
                    model.getNextCommand (ref historyRank, ref currentConsoleCommand, ref commandSave);
                }
            }

            GUI.Box (mainBox, "");
            GUI.BeginGroup (mainBox);

            // Command prompt
            GUI.SetNextControlName ("console");
            currentConsoleCommand = GUI.TextField (consoleBox, currentConsoleCommand);

            // Log history
            if (showLuaHistory) {
                logScroll = GUI.BeginScrollView (historyBox, logScroll, new Rect (0, 0, historyBox.width, model.history.Count * HISTORY_LABEL_HEIGHT), GUIStyle.none, GUIStyle.none);
                for (int i = 0; i < model.history.Count; i++) {
                    GUI.Label (new Rect (0, i * HISTORY_LABEL_HEIGHT, historyBox.width, HISTORY_LABEL_HEIGHT), model.history[i]);
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

    void updatePositions () {
        // main box
        if (showLuaEditor || showLuaHistory) {
            mainBox.x = MARGIN;
            mainBox.y = Screen.height - 4 * MARGIN - EDITOR_HEIGHT - CONSOLE_HEIGHT;
            mainBox.width = Screen.width - 2 * MARGIN;
            mainBox.height = 3 * MARGIN + EDITOR_HEIGHT + CONSOLE_HEIGHT;
        } else {
            mainBox.x = MARGIN;
            mainBox.y = Screen.height - 3 * MARGIN - CONSOLE_HEIGHT;
            mainBox.width = Screen.width - 2 * MARGIN;
            mainBox.height = 2 * MARGIN + CONSOLE_HEIGHT;
        }

        // editor and history
        if (showLuaEditor && showLuaHistory) {
            historyBox.x = MARGIN;
            historyBox.y = MARGIN;
            historyBox.width = (mainBox.width - 3 * MARGIN) / 2;
            historyBox.height = EDITOR_HEIGHT;

            editorBox.x = 2 * MARGIN + (mainBox.width - 3 * MARGIN) / 2;
            editorBox.y = MARGIN;
            editorBox.width = (mainBox.width - 3 * MARGIN) / 2;
            editorBox.height = EDITOR_HEIGHT;
        } else if (showLuaEditor) {
            editorBox.x = MARGIN;
            editorBox.y = MARGIN;
            editorBox.width = (mainBox.width - 2 * MARGIN);
            editorBox.height = EDITOR_HEIGHT;
        } else if (showLuaHistory) {
            historyBox.x = MARGIN;
            historyBox.y = MARGIN;
            historyBox.width = (mainBox.width - 2 * MARGIN);
            historyBox.height = EDITOR_HEIGHT;
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