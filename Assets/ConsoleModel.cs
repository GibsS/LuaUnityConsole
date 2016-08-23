using NLua;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class ConsoleModel {

    public delegate void OnNewMessage (string msg);
    public delegate void OnShow ();
    public delegate void OnHide ();
    public delegate void OnClear ();
    public delegate void OnLoadScript (string text);
    public delegate void OnSaveCurrent ();
    public delegate void OnSaveToNew (string URI);
    public delegate void OnRunCurrent ();

    public event OnNewMessage onNewMessage;

    public event OnShow onShowAll;
    public event OnShow onShowEditor;
    public event OnShow onShowHistory;
    public event OnShow onShowLog;

    public event OnHide onHideConsole;
    public event OnHide onHideEditor;
    public event OnHide onHideHistory;
    public event OnHide onHideLog;

    public event OnClear onClear;

    public event OnLoadScript onLoadScript;

    public event OnSaveToNew onSaveToNew;
    public event OnSaveCurrent onSaveCurrent;
    public event OnRunCurrent onRunCurrent;

    const int MAX_HISTORY = 500;
    const int AVERAGE_HISTORY = 300;

    public Lua lua { get; private set; }

    [SerializeField]
    public List<string> history;

    [SerializeField]
    string scriptBase;

    [SerializeField]
    bool isInit;
    
    public void init() {
        history = new List<string> ();
        scriptBase = null;
        isInit = true;
    }

    public void enable () {
        if(!isInit) {
            init ();
        }
        lua = new Lua ();
        lua.LoadCLRPackage ();
        
        runString (@"import 'System'
                    import 'UnityEngine'");
        registerAllCommands ();
    }

    public void disable () {

    }

    #region Scripts

    public void setScriptRoot (string URI) {
        scriptBase = URI;
    }
    public void loadScript (string URI) {
        string path = getCorrectURI(URI);
        if (onLoadScript != null) {
            if (path != null) {
                onLoadScript(File.ReadAllText (path));
            } else {
                onLoadScript (null);
            }
        }
    }
    public void saveScript (string URI) {
        if(onSaveToNew != null) {
            onSaveToNew (URI);
        }
    }
    public void saveScriptWithCode(string URI, string code) {
        string path = getCorrectURI(URI);
        if (path != null) {
            File.WriteAllText (path, code);
        }
    }
    public void saveCurrentScript () {
        if(onSaveCurrent != null) {
            onSaveCurrent ();
        }
    }

    public void runScript (string URI) {
        string path = getCorrectURI(URI);
        if (path != null) {
            runString (File.ReadAllText (path));
        }
    }
    public string getCorrectURI (string uri) {
        if (Path.IsPathRooted (uri)) {
            return uri;
        } else {
            if (scriptBase == null) {
                printError ("No script root initialized");
                return null;
            } else {
                return scriptBase + '/' + uri;
            }
        }
    }
    public void runString (string code) {
        try {
            lua.DoString (code);
        } catch (NLua.Exceptions.LuaException e) {
            printError (FormatException (e));
        } catch (Exception e) {
            printError (e.Message);
        }
    }
    public void runCurrentScript () {
        if(onRunCurrent != null) {
            onRunCurrent ();
        }
    }
    #endregion
    
    #region Commands

    public void runCurrentCommand (List<string> currentConsoleCode, string currentConsoleCommand, ref int parDepth, ref int acoDepth) {
        int If = Regex.Matches(currentConsoleCommand, @"([\n\ \t]|^)if([\n\ \t]|$)").Count;
        int While = Regex.Matches(currentConsoleCommand, @"([\n\ \t]|^)while([\n\ \t]|$)").Count;
        int For = Regex.Matches(currentConsoleCommand, @"([\n\ \t]|^)for([\n\ \t]|$)").Count;
        int Function = Regex.Matches(currentConsoleCommand, @"([\n\ \t]|^)function ").Count;
        int End = Regex.Matches(currentConsoleCommand, @"([\n\ \t]|^)end([\n\ \t]|$)").Count;

        int Open = Regex.Matches(currentConsoleCommand, @"{").Count;
        int Close = Regex.Matches(currentConsoleCommand, @"}").Count;

        parDepth += If + While + For + Function - End;
        acoDepth += Open - Close;

        if (parDepth >= 0) {
            currentConsoleCode.Add (currentConsoleCommand);

            if (parDepth > 0 || acoDepth > 0) {
                printCommandStep (currentConsoleCommand);
            } else if (acoDepth < 0) {
                printError ("Incorrect code : too many brackets");
                currentConsoleCode.Clear ();
                parDepth = 0;
                acoDepth = 0;
                currentConsoleCommand = "";
            } else {
                printCommand (currentConsoleCommand);
                string code = "";
                foreach (string line in currentConsoleCode) {
                    code += line + "\n";
                }
                runString (code);
                currentConsoleCode.Clear ();
                parDepth = 0;
                acoDepth = 0;
            }
            currentConsoleCommand = "";
        } else {
            printError ("Incorrect code : end is unexpected");
            currentConsoleCode.Clear ();
            parDepth = 0;
            acoDepth = 0;
            currentConsoleCommand = "";
        }
    }

    public void clear () {
        history.Clear ();
        if(onClear != null) {
            onClear ();
        }
    }
    public void getPreviousCommand (ref int historyRank, ref string currentConsoleCommand, ref string commandSave) {
        if (historyRank >= 0) {
            if (historyRank > 0) {
                int tmp = historyRank;
                historyRank--;
                while (historyRank > 0
                    && (history[historyRank][0] == ' ' || history[historyRank][0] == '!')) {
                    historyRank--;
                }
                if (historyRank > 0 || (history[historyRank][0] != ' ' && history[historyRank][0] != '!')) {
                    currentConsoleCommand = history[historyRank].Substring (2);
                } else {
                    historyRank = tmp;
                }
            }
        } else {
            if (history.Count > 0) {
                historyRank = history.Count - 1;
                while (historyRank > 0
                    && (history[historyRank][0] == ' ' || history[historyRank][0] == '!')) {
                    historyRank--;
                }
                if (historyRank > 0 || (history[historyRank][0] != ' ' && history[historyRank][0] != '!')) {
                    commandSave = currentConsoleCommand;
                    currentConsoleCommand = history[historyRank].Substring (2);
                } else {
                    historyRank = -1;
                }
            }
        }
    }
    public void getNextCommand (ref int historyRank, ref string currentConsoleCommand, ref string commandSave) {
        if (historyRank >= 0) {
            historyRank++;
            while (historyRank < history.Count
                && (history[historyRank][0] == ' ' || history[historyRank][0] == '!')) {
                historyRank++;
            }
            if (historyRank == history.Count) {
                historyRank = -1;
                currentConsoleCommand = commandSave;
            } else {
                currentConsoleCommand = history[historyRank].Substring (2);
            }
        }
    }
    public void newPrint (string msg) {
        history.Add (msg.Replace('\n', ' '));
        if (onNewMessage != null) {
            onNewMessage (msg.Replace ('\n', ' '));
        }
    }

    public void printCommand (string cmd) {
        newPrint ("# " + cmd);
    }
    public void printCommandStep (string cmd) {
        newPrint ("> " + cmd);
    }
    public void printMessage (string msg) {
        newPrint ("  " + msg);
    }
    public void printError (string error) {
        newPrint ("! " + error);
    }
    public void print (object obj) {
        if (obj == null) {
            printMessage ("null");
        } else {
            printMessage (obj.ToString ());
        }
    }

    #endregion

    #region UI Command

    public void showEditor() { if(onShowEditor != null) { onShowEditor (); } }
    public void hideEditor () { if(onHideEditor != null) { onHideEditor (); } }
    public void showHistory () { if(onShowHistory != null) { onShowHistory (); } }
    public void hideHistory () { if(onHideHistory != null) { onHideHistory (); } }
    public void hideConsole () { if(onHideConsole != null) { onHideConsole (); } }
    public void showLog () { if (onShowLog != null) { onShowLog (); } }
    public void hideLog () { if (onHideLog != null) { onHideLog (); } }

    #endregion

    #region Registration

    void registerAllCommands () {
        registerCommand ("show_log", "showLog");
        registerCommand ("hide_log", "hideLog");
        registerCommand ("show_editor", "showEditor");
        registerCommand ("hide_editor", "hideEditor");
        registerCommand ("show_history", "showHistory");
        registerCommand ("hide_history", "hideHistory");
        registerCommand ("hide", "hideConsole");

        registerCommand ("clear", "clear");

        registerCommand ("print", "print");

        registerCommand ("root", "setScriptRoot");
        registerCommand ("load", "loadScript");
        registerCommand ("save", "saveCurrentScript");
        registerCommand ("save_to_new", "saveScript");

        registerCommand ("run", "runScript");
        registerCommand ("run_editor", "runCurrentScript");
    }
    void registerCommand (string consoleName, string functionName) {
        lua.RegisterFunction (consoleName, this, GetType ().GetMethod (functionName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public));
    }
    public void registerObject (string nom, object obj) {
        lua[nom] = obj;
    }
    public void registerGameObject (GameObject obj) {
        lua[obj.name] = obj;
    }
    public void registerNamespace (string ns) {
        runString (@"import '" + ns + @"'");
    }

    #endregion

    #region Helpers
    
    static string FormatException (NLua.Exceptions.LuaException e) {
        string source = (string.IsNullOrEmpty(e.Source)) ? "<no source>" : e.Source.Substring(0, e.Source.Length - 2);
        return string.Format ("{0}\nLua (at {2})", e.Message, string.Empty, source);
    }

    #endregion
}