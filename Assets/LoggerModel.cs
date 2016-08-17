using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class StringToBoolDict : SerializableDictionary<string, bool> { }

[Serializable]
public class LoggerModel {

    const int MAX_HISTORY = 500;
    const int AVERAGE_HISTORY = 300;
    
    [SerializeField]
    public List<Log> logs { get; private set; }
    
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
        channels = new List<string> ();
        visibleChannels = new List<string> ();
        typeOn = new bool[Enum.GetValues(typeof(LogType)).Length];
        for(int i = 0; i < typeOn.Length; i++) {
            typeOn[i] = true;
        }
        logTypeNames = new string[] { "info", "error", "warning", "test", "exception" };
    }

    public void addLog(Log log) {
        if (log.channel != "" && !channels.Contains (log.channel)) {
            channels.Add (log.channel);
            visibleChannels.Add (log.channel);
        }

        logs.Add (log);
        if(logs.Count > MAX_HISTORY) {
            logs.RemoveRange (0, logs.Count - AVERAGE_HISTORY);
        }
    }
    public void clear() {
        logs.Clear ();
    }
    public IEnumerable getLogs() {
        foreach(Log log in logs) {
            if ((log.channel == "" || visibleChannels.Contains(log.channel)) && typeOn[(int) log.type]) {
                yield return log;
            }
        }
    }

    public void enableType(LogType type) {
        typeOn[(int) type] = true;
    }
    public void disableType(LogType type) {
        typeOn[(int) type] = false;
    }
    public void enableChannel(string channel) {
        visibleChannels.Add (channel);
    }
    public void disableChannel(string channel) {
        visibleChannels.Remove (channel);
    }
}