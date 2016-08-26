using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

public class Shell {

    static bool isInit;

    static LoggerModel loggerModel;

    static ConsoleModel editorModel;
    static ConsoleModel consoleModel;

    public static void setLoggerModel(LoggerModel loggerModel) {
        Shell.loggerModel = loggerModel;
    }
    public static LoggerModel getLoggerModel() {
        if(loggerModel == null) {
            loggerModel = new LoggerModel ();
        }
        return loggerModel;
    }

    public static ConsoleModel getEditorConsoleModel() {
        if(editorModel == null) {
            editorModel = new ConsoleModel();
            editorModel.init ();
        }
        return editorModel;
    }
    public static ConsoleModel getGameConsoleModel() {
        if (consoleModel == null) {
            consoleModel = new ConsoleModel ();
            consoleModel.init ();
            consoleModel.enable ();
        }
        return consoleModel;
    }

    public static void init() {
        if(!isInit) {
            isInit = true;

            if(loggerModel == null) {
                loggerModel = new LoggerModel ();
            }

            if (consoleModel == null) {
                consoleModel = new ConsoleModel ();
                consoleModel.init ();
                consoleModel.enable ();
            }
        }
    }

    public static void registerObject(string name, object obj) {
        if (consoleModel != null) {
            consoleModel.registerObject (name, obj);
        }
        if(editorModel != null) {
            editorModel.registerObject (name, obj);
        }
    }
    public static void registerGameObject(GameObject gameObject) {
        if (consoleModel != null) {
            consoleModel.registerGameObject (gameObject);
        }
        if (editorModel != null) {
            editorModel.registerGameObject (gameObject);
        }
    }
    public static void registerNamespace(string nspace) {
        if(consoleModel != null) {
            consoleModel.registerNamespace (nspace);
        }
        if (editorModel != null) {
            editorModel.registerNamespace (nspace);
        }
    }

    public static void setScriptRoot(string root) {
        if(consoleModel != null) {
            consoleModel.setScriptRoot (root);
        }
        if(editorModel != null) {
            editorModel.setScriptRoot (root);
        }
    }

    public static void print(string msg) {
        if (consoleModel != null) {
            consoleModel.print (msg);
        }
        if (editorModel != null) {
            editorModel.print (msg);
        }
    }
    public static void clear() {
        if (consoleModel != null) {
            consoleModel.clear();
        }
        if (editorModel != null) {
            editorModel.clear ();
        }
    }

    public static void showEditor() {
        if (consoleModel != null) {
            consoleModel.showEditor ();
        }
        if (editorModel != null) {
            editorModel.showEditor ();
        }
    }
    public static void showHistory() {
        if (consoleModel != null) {
            consoleModel.showHistory ();
        }
        if (editorModel != null) {
            editorModel.showHistory ();
        }
    }
    public static void showLog() {
        if(loggerModel != null) {
            loggerModel.show ();
        }
    }
    public static void showAll() {
        showHistory ();
        showEditor ();
        showLog ();
    }
    public static void hideEditor() {
        if (consoleModel != null) {
            consoleModel.hideEditor ();
        }
        if (editorModel != null) {
            editorModel.hideEditor ();
        }
    }
    public static void hideHistory() {
        if (consoleModel != null) {
            consoleModel.hideHistory ();
        }
        if (editorModel != null) {
            editorModel.hideHistory ();
        }
    }
    public static void hideLog() {
        if (loggerModel != null) {
            loggerModel.hide ();
        }
    }
    public static void hideAll() {
        hideEditor ();
        hideHistory ();
        hideLog ();
    }

    public static string loadScript(string URI) {
        if (consoleModel != null) {
            return File.ReadAllText(consoleModel.getCorrectURI (URI));
        }
        if (editorModel != null) {
            return File.ReadAllText (editorModel.getCorrectURI (URI));
        }
        return null;
    }
    public static void saveScript(string URI, string code) {
        if (consoleModel != null) {
            consoleModel.saveScriptWithCode (URI, code);
        }
        if (editorModel != null) {
            editorModel.saveScriptWithCode (URI, code);
        }
    }
    public static void runScript(string URI) {
        if (consoleModel != null) {
            consoleModel.runScript (URI);
        }
        if (editorModel != null) {
            editorModel.runScript (URI);
        }
    }
    public static void runCode(string code) {
        if (consoleModel != null) {
            consoleModel.runString (code);
        }
        if (editorModel != null) {
            editorModel.runString (code);
        }
    }

    static List<string> formatStack(string stack) {
        return stack.Split ('\n')
                    .Except (new List<string> () { "" })
                    .Skip (2)
                    .ToList ();
    }

    public static void info (string msg) {
        if(loggerModel != null) {
            loggerModel.addLog (new Log (LogType.info, null, "Default", msg, formatStack(Environment.StackTrace)));
        }
    }
    public static void info (string msg, object target) {
        if (loggerModel != null) {
            loggerModel.addLog (new Log (LogType.info, target, "Default", msg, formatStack (Environment.StackTrace)));
        }
    }
    public static void info (string msg, object target, string channel) {
        if (loggerModel != null) {
            loggerModel.addLog (new Log (LogType.info, target, channel, msg, formatStack (Environment.StackTrace)));
        }
    }

    public static void error (string msg) {
        if (loggerModel != null) {
            loggerModel.addLog (new Log (LogType.error, null, "Default", msg, formatStack (Environment.StackTrace)));
        }
    }
    public static void error (string msg, object target) {
        if (loggerModel != null) {
            loggerModel.addLog (new Log (LogType.error, target, "Default", msg, formatStack (Environment.StackTrace)));
        }
    }
    public static void error (string msg, object target, string channel) {
        if (loggerModel != null) {
            loggerModel.addLog (new Log (LogType.error, target, channel, msg, formatStack (Environment.StackTrace)));
        }
    }

    public static void warning (string msg) {
        if (loggerModel != null) {
            loggerModel.addLog (new Log (LogType.warning, null, "Default", msg, formatStack (Environment.StackTrace)));
        }
    }
    public static void warning (string msg, object target) {
        if (loggerModel != null) {
            loggerModel.addLog (new Log (LogType.warning, target, "Default", msg, formatStack (Environment.StackTrace)));
        }
    }
    public static void warning (string msg, object target, string channel) {
        if (loggerModel != null) {
            loggerModel.addLog (new Log (LogType.warning, target, channel, msg, formatStack (Environment.StackTrace)));
        }
    }

    public static void exception (string msg) {
        if (loggerModel != null) {
            loggerModel.addLog (new Log (LogType.exception, null, "Default", msg, formatStack (Environment.StackTrace)));
        }
    }
    public static void exception (string msg, object target) {
        if (loggerModel != null) {
            loggerModel.addLog (new Log (LogType.exception, target, "Default", msg, formatStack (Environment.StackTrace)));
        }
    }
    public static void exception (string msg, object target, string channel) {
        if (loggerModel != null) {
            loggerModel.addLog (new Log (LogType.exception, target, channel, msg, formatStack (Environment.StackTrace)));
        }
    }

    public static void test (string msg) {
        if (loggerModel != null) {
            loggerModel.addLog (new Log (LogType.test, null, "Default", msg, formatStack (Environment.StackTrace)));
        }
    }
    public static void test (string msg, object target) {
        if (loggerModel != null) {
            loggerModel.addLog (new Log (LogType.test, target, "Default", msg, formatStack (Environment.StackTrace)));
        }
    }
    public static void test (string msg, object target, string channel) {
        if (loggerModel != null) {
            loggerModel.addLog (new Log (LogType.test, target, channel, msg, formatStack (Environment.StackTrace)));
        }
    }
}