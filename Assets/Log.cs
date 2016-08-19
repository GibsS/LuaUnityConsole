using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable]
public class Log {

    const string patternStack = " in (?<file>.*):line (?<line>[0-9]*)";
    const string patternUnityError = "\\(at Assets\\/(?<file>[^:]*):(?<line>[0-9]*)\\)";
    const string patternUnityCompilationError = "^Assets\\/(?<file>[^(]*)\\((?<line>[0-9]*)[^(warning)]*error";
    const string patternUnityCompilationWarning = "^Assets\\/(?<file>[^(]*)\\((?<line>[0-9]*).*warning";

    [SerializeField]
    public string msg { get; private set; }

    public object target { get; private set; }

    [SerializeField]
    public List<string> stack { get; private set; }

    [SerializeField]
    public LogType type { get; private set; }
    [SerializeField]
    public string channel { get; private set; }
    
    public Log(LogType type, string msg, List<string> stack) {
        this.msg = msg;
        this.target = null;
        this.stack = stack;
        this.type = type;
        this.channel = "";
    }
    public Log(LogType type, object target, string msg, List<string> stack) {
        this.msg = msg;
        this.target = target;
        this.stack = stack;
        this.type = type;
        this.channel = "";
    }
    public Log(LogType type, object target, string channel, string msg, List<string> stack) {
        this.msg = msg;
        this.target = target;
        this.stack = stack;
        this.type = type;
        this.channel = channel;
    }

    public void getLineAndFile(out int line, out string file) {
        Match m = Regex.Match(stack[0], patternUnityError);
        if (m.Success) {
            file = Application.dataPath + "/" + m.Groups["file"].Value;
            line = Convert.ToInt32 (m.Groups["line"].Value);
            return;
        }
        m = Regex.Match (msg, patternUnityCompilationError);
        if (m.Success) {
            file = Application.dataPath + "/" + m.Groups["file"].Value;
            line = Convert.ToInt32 (m.Groups["line"].Value);
            type = LogType.error;
            return;
        }
        m = Regex.Match (msg, patternUnityCompilationWarning);
        if (m.Success) {
            file = Application.dataPath + "/" + m.Groups["file"].Value;
            line = Convert.ToInt32 (m.Groups["line"].Value);
            type = LogType.warning;
            return;
        }
        m = Regex.Match (stack[0], patternStack);
        if (m.Success) {
            file = m.Groups["file"].Value;
            line = Convert.ToInt32 (m.Groups["line"].Value);
            return;
        }
        line = -1;
        file = null;
    }

    public void getLineAndFile(int stackLine, out int line, out string file) {
        Match m = Regex.Match(stack[stackLine], patternUnityError);
        if (m.Success) {
            file = Application.dataPath + "/" + m.Groups["file"].Value;
            line = Convert.ToInt32 (m.Groups["line"].Value);
            return;
        }
        m = Regex.Match (stack[stackLine], patternStack);
        if (m.Success) {
            file = m.Groups["file"].Value;
            line = Convert.ToInt32 (m.Groups["line"].Value);
            return;
        }
        line = -1;
        file = null;
    }

    public override string ToString() {
        return "[" + (target != null ? target + ":" : "") + (channel != "" ? channel : "Default") + "] " + msg;
    }
}

public enum LogType {
    info = 0,
    error = 1,
    warning = 2,
    test = 3,
    exception = 4
}