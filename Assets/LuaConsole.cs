using UnityEngine;
using System.Collections.Generic;

using NLua;
using System.IO;
using System.Text.RegularExpressions;
using System;

public class LuaConsole : MonoBehaviour {

    // CONSTANTS
    const int MARGIN = 10;
    const int EDITOR_HEIGHT = 160;
    const int HISTORY_LABEL_HEIGHT = 22;

    const int MAX_HISTORY = 500;
    const int AVERAGE_HISTORY = 300;

    const int CONSOLE_HEIGHT = 20;

    public Lua lua { get; private set; }

    // SINGLETON INSTANCE
    public static LuaConsole luaConsole;

    // HISTORY
    List<string> history;
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
    string scriptBase;

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

	void Start () {
        // Assure uniqueness
        if(luaConsole != null)
        {
            Debug.Log("There is already a LuaConsole");
            Destroy(this);
            return;
        }
        luaConsole = this;

        history = new List<string>();
        historyRank = -1;
        commandSave = "";
        logScroll = Vector2.zero;

        lua = new Lua();
        lua.LoadCLRPackage();

        currentConsoleCode = new List<string>();
        parDepth = 0;
        currentConsoleCommand = "";

        codeEditor = "";
        uri = null;
        scriptBase = null;

        showLuaConsole = true;
        showLuaEditor = true;
        showLuaHistory = true;

        mainBox = new Rect();
        historyBox = new Rect();
        consoleBox = new Rect();
        editorBox = new Rect();
        updatePositions();
        cacheScreenHeight = Screen.height;
        cacheScreenWidth = Screen.width;

        runString(@"import 'System'
                    import 'UnityEngine'");
        registerAllCommands();
    }
    void OnGUI()
    {
        if(Event.current.Equals(Event.KeyboardEvent("escape")))
        {
            if(showLuaConsole)
            {
                hideConsole();
            }
            else
            {
                showConsole();
            }
            Event.current.Use();
        }

        if (showLuaConsole)
        {
            // listen to change in screen size
            if(Screen.width != cacheScreenWidth || Screen.height != cacheScreenHeight)
            {
                updatePositions();
                cacheScreenHeight = Screen.height;
                cacheScreenWidth = Screen.width;
            }

            // Moving in history
            if (Event.current.Equals(Event.KeyboardEvent("up")))
            {
                getPreviousCommand();
            }
            else if (Event.current.Equals(Event.KeyboardEvent("down")))
            {
                getNextCommand();
            }

            if (Event.current.Equals(Event.KeyboardEvent("return")) && GUI.GetNameOfFocusedControl() == "console")
            {
                runCurrentCommand();
            }

            GUI.Box(mainBox, "");
            GUI.BeginGroup(mainBox);

            // Command prompt
            GUI.SetNextControlName("console");
            currentConsoleCommand = GUI.TextField(consoleBox, currentConsoleCommand);

            // Log history
            if (showLuaHistory)
            {
                logScroll = GUI.BeginScrollView(historyBox, logScroll, new Rect(0, 0, historyBox.width, history.Count * HISTORY_LABEL_HEIGHT), GUIStyle.none, GUIStyle.none);
                for (int i = 0; i < history.Count; i++)
                {
                    GUI.Label(new Rect(0, i * HISTORY_LABEL_HEIGHT, historyBox.width, HISTORY_LABEL_HEIGHT), history[i]);
                }
                GUI.EndScrollView();
            }

            // Editor
            if (showLuaEditor)
            {
                codeEditor = GUI.TextArea(editorBox, codeEditor);
            }

            GUI.EndGroup();
        }
    }

    #region GUI Positionning
    void updatePositions()
    {
        // main box
        if(showLuaEditor || showLuaHistory)
        {
            mainBox.x = MARGIN;
            mainBox.y = Screen.height - 4 * MARGIN - EDITOR_HEIGHT - CONSOLE_HEIGHT;
            mainBox.width = Screen.width - 2 * MARGIN;
            mainBox.height = 3 * MARGIN + EDITOR_HEIGHT + CONSOLE_HEIGHT;
        }
        else
        {
            mainBox.x = MARGIN;
            mainBox.y = Screen.height - 3*MARGIN - CONSOLE_HEIGHT;
            mainBox.width = Screen.width - 2 * MARGIN;
            mainBox.height = 2 * MARGIN + CONSOLE_HEIGHT;
        }

        // editor and history
        if(showLuaEditor && showLuaHistory)
        {
            historyBox.x = MARGIN;
            historyBox.y = MARGIN;
            historyBox.width = (mainBox.width - 3 * MARGIN) / 2;
            historyBox.height = EDITOR_HEIGHT;

            editorBox.x = 2 * MARGIN + (mainBox.width - 3 * MARGIN) / 2;
            editorBox.y = MARGIN;
            editorBox.width = (mainBox.width - 3 * MARGIN) / 2;
            editorBox.height = EDITOR_HEIGHT;
        }
        else if(showLuaEditor)
        {
            editorBox.x = MARGIN;
            editorBox.y = MARGIN;
            editorBox.width = (mainBox.width - 2 * MARGIN);
            editorBox.height = EDITOR_HEIGHT;
        }
        else if(showLuaHistory)
        {
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
    public void showConsole()
    {
        showLuaConsole = true;
        updatePositions();
    }
    public void hideConsole()
    {
        showLuaConsole = false;
    }
    public void showEditor()
    {
        showLuaEditor = true;
        updatePositions();
    }
    public void hideEditor()
    {
        showLuaEditor = false;
        updatePositions();
    }
    public void showHistory()
    {
        showLuaHistory = true;
        updatePositions();
    }
    public void hideHistory()
    {
        showLuaHistory = false;
        updatePositions();
    }
    #endregion

    #region Commands
    void runCurrentCommand()
    {
        int If = Regex.Matches(currentConsoleCommand, @"([\n\ \t]|^)if([\n\ \t]|$)").Count;
        int While = Regex.Matches(currentConsoleCommand, @"([\n\ \t]|^)while([\n\ \t]|$)").Count;
        int For = Regex.Matches(currentConsoleCommand, @"([\n\ \t]|^)for([\n\ \t]|$)").Count;
        int Function = Regex.Matches(currentConsoleCommand, @"([\n\ \t]|^)function ").Count;
        int End = Regex.Matches(currentConsoleCommand, @"([\n\ \t]|^)end([\n\ \t]|$)").Count;

        int Open = Regex.Matches(currentConsoleCommand, @"{").Count;
        int Close = Regex.Matches(currentConsoleCommand, @"}").Count;

        parDepth += If + While + For + Function - End;
        acoDepth += Open - Close;

        if (parDepth >= 0)
        {
            currentConsoleCode.Add(currentConsoleCommand);

            if (parDepth > 0 || acoDepth > 0)
            {
                printCommandStep(currentConsoleCommand);
            }
            else if(acoDepth < 0)
            {
                printError("Incorrect code : too many brackets");
                currentConsoleCode.Clear();
                parDepth = 0;
                acoDepth = 0;
                currentConsoleCommand = "";
                resetCommandHistory();
            }
            else
            {
                printCommand(currentConsoleCommand);
                string code = "";
                foreach (string line in currentConsoleCode)
                {
                    code += line + "\n";
                }
                runString(code);
                currentConsoleCode.Clear();
                parDepth = 0;
                acoDepth = 0;
            }
            currentConsoleCommand = "";
            resetCommandHistory();
        }
        else
        {
            printError("Incorrect code : end is unexpected");
            currentConsoleCode.Clear();
            parDepth = 0;
            acoDepth = 0;
            currentConsoleCommand = "";
            resetCommandHistory();
        }
    }

    void clear()
    {
        history.Clear();
        resetCommandHistory();
    }
    void resetCommandHistory()
    {
        historyRank = -1;
        commandSave = "";
    }
    void getPreviousCommand()
    {
        if (historyRank >= 0)
        {
            if (historyRank > 0)
            {
                int tmp = historyRank;
                historyRank--;
                while (historyRank > 0
                    && (history[historyRank][0] == ' ' || history[historyRank][0] == '!'))
                {
                    historyRank--;
                }
                if (historyRank > 0 || (history[historyRank][0] != ' ' && history[historyRank][0] != '!'))
                {
                    currentConsoleCommand = history[historyRank].Substring(2);
                }
                else
                {
                    historyRank = tmp;
                }
            }
        }
        else
        {
            if (history.Count > 0)
            {
                historyRank = history.Count - 1;
                while (historyRank > 0
                    && (history[historyRank][0] == ' ' || history[historyRank][0] == '!'))
                {
                    historyRank--;
                }
                if (historyRank > 0 || (history[historyRank][0] != ' ' && history[historyRank][0] != '!'))
                {
                    commandSave = currentConsoleCommand;
                    currentConsoleCommand = history[historyRank].Substring(2);
                }
                else
                {
                    historyRank = -1;
                }
            }
        }
    }
    void getNextCommand()
    {
        if (historyRank >= 0)
        {
            historyRank++;
            while(historyRank < history.Count 
                && (history[historyRank][0] == ' ' || history[historyRank][0] == '!'))
            {
                historyRank++;
            }
            if(historyRank == history.Count)
            {
                historyRank = -1;
                currentConsoleCommand = commandSave;
            }
            else
            {
                currentConsoleCommand = history[historyRank].Substring(2);
            }
        }
    }
    void printCommand(string cmd)
    {
        history.Add("# " + cmd);
        logScroll.y = float.PositiveInfinity;
    }
    void printCommandStep(string cmd)
    {
        history.Add("> " + cmd);
        logScroll.y = float.PositiveInfinity;
    }
    void printMessage(string msg)
    {
        history.Add("  " + msg);
        logScroll.y = float.PositiveInfinity;
    }
    void printError(string error)
    {
        history.Add("! " + error);
        logScroll.y = float.PositiveInfinity;
    }
    new void print(object obj)
    {
        if (obj == null)
        {
            printMessage("null");
        }
        else
        {
            printMessage(obj.ToString());
        }
    }
    #endregion

    #region Scripts
    public void setScriptRoot(string URI)
    {
        scriptBase = URI;
    }
    void loadScript(string URI)
    {
        string path = getCorrectURI(URI);
        if (path != null)
        {
            codeEditor = File.ReadAllText(path);
            uri = URI;
        }
    }
    void saveScript(string URI)
    {
        string path = getCorrectURI(URI);
        if (path != null)
        {
            File.WriteAllText(path, codeEditor);
        }
    }
    void saveCurrentScript()
    {
        saveScript(uri);
    }

    void runScript(string URI)
    {
        string path = getCorrectURI(URI);
        if (path != null)
        {
            runString(File.ReadAllText(path));
        }
    }
    string getCorrectURI(string uri)
    {
        if (Path.IsPathRooted(uri))
        {
            return uri;
        }
        else
        {
            if (scriptBase == null)
            {
                printError("No script root initialized");
                return null;
            }
            else
            {
                return scriptBase + '/' + uri;
            }
        }
    }
    void runCurrentScript()
    {
        runString(codeEditor);
    }
    void runString(string code)
    {
        try
        {
            lua.DoString(code);
        }
        catch (NLua.Exceptions.LuaException e)
        {
            printError(FormatException(e));
			//throw e;
		}
        catch(Exception e)
        {
            printError(e.Message);
        }
    }
    #endregion

    #region Registration
    void registerAllCommands()
    {
        registerCommand("show_editor", "showEditor");
        registerCommand("hide_editor", "hideEditor");
        registerCommand("show_history", "showHistory");
        registerCommand("hide_history", "hideHistory");
        registerCommand("hide", "hideConsole");

        registerCommand("clear", "clear");

        registerCommand("print", "print");

        registerCommand("root", "setScriptRoot");
        registerCommand("load", "loadScript");
        registerCommand("save", "saveCurrentScript");
        registerCommand("save_to_new", "saveScript");

        registerCommand("run", "runScript");
        registerCommand("run_editor", "runCurrentScript");
    }
    void registerCommand(string consoleName, string functionName)
    {
        lua.RegisterFunction(consoleName, this, GetType().GetMethod(functionName, System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public));
    }
    public void registerObject(string nom, object obj)
    {
        lua[nom] = obj;
    }
    public void registerGameObject(GameObject obj)
    {
        lua[obj.name] = obj;
    }
    public void registerNamespace(string ns)
    {
        runString(@"import '" + ns + @"'");
    }
    #endregion

    #region helpers
    static string FormatException(NLua.Exceptions.LuaException e)
    {
        string source = (string.IsNullOrEmpty(e.Source)) ? "<no source>" : e.Source.Substring(0, e.Source.Length - 2);
        return string.Format("{0}\nLua (at {2})", e.Message, string.Empty, source);
    }
    Texture2D MakeTex(int width, int height, Color col)
    {
        Color[] pix = new Color[width * height];

        for (int i = 0; i < pix.Length; i++)
            pix[i] = col;

        Texture2D result = new Texture2D(width, height);
        result.SetPixels(pix);
        result.Apply();

        return result;
    }
    #endregion
}