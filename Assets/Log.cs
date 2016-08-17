using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class Log {

    [SerializeField]
    public string msg { get; private set; }

    public object target { get; private set; }

    [SerializeField]
    public string stack { get; private set; }

    [SerializeField]
    public LogType type { get; private set; }
    [SerializeField]
    public string channel { get; private set; }
    
    public Log(LogType type, string msg, string stack) {
        this.msg = msg;
        this.target = null;
        this.stack = stack;
        this.type = type;
        this.channel = "";
    }
    public Log(LogType type, object target, string msg, string stack) {
        this.msg = msg;
        this.target = target;
        this.stack = stack;
        this.type = type;
        this.channel = "";
    }
    public Log(LogType type, object target, string channel, string msg, string stack) {
        this.msg = msg;
        this.target = target;
        this.stack = stack;
        this.type = type;
        this.channel = channel;
    }

    public void getLineAndFile(out int line, out string file) {
        line = 0;
        file = "";
    }
}

public enum LogType {
    info = 0,
    error = 1,
    warning = 2,
    test = 3,
    exception = 4
}