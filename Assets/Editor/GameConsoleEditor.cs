using UnityEditor;
using UnityEngine;

[CustomEditor (typeof (GameConsole))]
public class GameConsoleEditor : Editor {

    public override void OnInspectorGUI () {
        GameConsole console = (GameConsole)target;

        bool newShowConsole = EditorGUILayout.BeginToggleGroup("show console", console.showLuaConsole);
        if (newShowConsole && !console.showLuaConsole) {
            console.showConsole ();
        } else if (!newShowConsole && console.showLuaConsole) {
            console.hideConsole ();
        }

        bool newShowLog = EditorGUILayout.Toggle("show log", console.showLuaLog);
        if (newShowLog && !console.showLuaLog) {
            Debug.Log ("show log");
            console.showLog ();
        } else if (!newShowLog && console.showLuaLog) {
            Debug.Log ("hide log");
            console.hideLog ();
        }

        bool newShowHistory = EditorGUILayout.Toggle("show history", console.showLuaHistory);
        if (newShowHistory && !console.showLuaHistory) {
            console.showHistory ();
        } else if (!newShowHistory && console.showLuaHistory) {
            console.hideHistory ();
        }

        bool newShowEditor = EditorGUILayout.Toggle("show editor", console.showLuaEditor);
        if (newShowEditor && !console.showLuaEditor) {
            console.showEditor ();
        } else if (!newShowEditor && console.showLuaEditor) {
            console.hideEditor ();
        }
        
        EditorGUILayout.EndToggleGroup ();
    }
}