using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class LoggerModel {

    public delegate void OnShow ();
    public delegate void OnHide ();
    public delegate void OnLog (Log log);
    public delegate void OnClear ();
    public delegate void OnTypeChange (LogType type, bool active);
    public delegate void OnChannelChange (string channel, bool active);

    public event OnShow onShow;
    public event OnHide onHide;
    public event OnLog onLog;
    public event OnClear onClear;
    public event OnTypeChange onTypeChange;
    public event OnChannelChange onChannelChange;

    const int MAX_HISTORY = 500;
    const int AVERAGE_HISTORY = 400;

    [SerializeField]
    public List<Log> logs;
    [SerializeField]
    public List<Log> displayedLogs { get; private set; }

    [SerializeField]
    public List<string> channels { get; private set; }
    [SerializeField]
    public List<string> visibleChannels;

    [SerializeField]
    public bool[] typeOn { get; private set; }
    [SerializeField]
    public string[] logTypeNames { get; private set; }

    public LoggerModel() {
        logs = new List<Log> ();
        displayedLogs = new List<Log> ();
        channels = new List<string> ();
        visibleChannels = new List<string> ();
        typeOn = new bool[Enum.GetValues(typeof(LogType)).Length];
        for(int i = 0; i < typeOn.Length; i++) {
            typeOn[i] = true;
        }
        logTypeNames = new string[] { "info", "error", "warning", "test", "exception" };
        
        Application.logMessageReceived += handleUnityLog;
    }

    void handleUnityLog (string condition, string stackTrace, UnityEngine.LogType unityLogType) {
        LogType logType = LogType.info;
        switch (unityLogType) {
            case UnityEngine.LogType.Error:
                logType = LogType.error;
                break;
            case UnityEngine.LogType.Assert:
                logType = LogType.exception;
                break;
            case UnityEngine.LogType.Warning:
                if(logs.Find(log => log.msg == condition) != null) {
                    return;
                }
                logType = LogType.warning;
                break;
            case UnityEngine.LogType.Log:
                logType = LogType.info;
                break;
            case UnityEngine.LogType.Exception:
                logType = LogType.exception;
                break;
        }
        addLog (new Log (logType, null, "Unity", condition, new List<string> () { stackTrace }));
    }

    public void addLog(Log log) {
        if (log.channel != "" && !channels.Contains (log.channel)) {
            channels.Add (log.channel);
            visibleChannels.Add (log.channel);
        }

        logs.Add (log);
        if ((log.channel == "" || visibleChannels.Contains (log.channel)) && typeOn[(int) log.type]) {
            displayedLogs.Add (log);
        }
        if (logs.Count > MAX_HISTORY) {
            logs.RemoveRange (0, logs.Count - AVERAGE_HISTORY);
            recalculateDisplayedLogs ();
        }
        if(onLog != null) {
            onLog (log);
        }
    }
    public void clear() {
        logs.Clear ();
        displayedLogs.Clear ();
        if(onClear != null) {
            onClear ();
        }
    }
    void recalculateDisplayedLogs () {
        displayedLogs.Clear ();

        foreach (Log log in logs) {
            if ((log.channel == "" || visibleChannels.Contains(log.channel)) && typeOn[(int) log.type]) {
                displayedLogs.Add (log);
            }
        }
    }

    public void enableType(LogType type) {
        typeOn[(int) type] = true;
        recalculateDisplayedLogs ();
        if (onTypeChange != null) {
            onTypeChange (type, true);
        }
    }
    public void disableType(LogType type) {
        typeOn[(int) type] = false;
        recalculateDisplayedLogs ();
        if (onTypeChange != null) {
            onTypeChange (type, false);
        }
    }
    public void enableChannel(string channel) {
        visibleChannels.Add (channel);
        recalculateDisplayedLogs ();
        if (onTypeChange != null) {
            onChannelChange (channel, true);
        }
    }
    public void disableChannel(string channel) {
        visibleChannels.Remove (channel);
        recalculateDisplayedLogs ();
        if (onTypeChange != null) {
            onChannelChange (channel, true);
        }
    }

    public void show() {
        if(onShow != null) {
            onShow ();
        }
    }
    public void hide() {
        if(onHide != null) {
            onHide ();
        }
    }
}