using UnityEngine;
using System.Collections;
using System;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        LuaConsole.init ();
        LuaConsole.setScriptRoot (Application.dataPath + "/TestScript");

        StartCoroutine (test ());

        LuaConsole.info ("in g");
        f ();
	}

    IEnumerator test () {
        while(true) {
            LuaConsole.info ("test");
            yield return new WaitForSeconds (1);
            LuaConsole.info ("test", this);
            yield return new WaitForSeconds (1);
            LuaConsole.info ("test", this, "network");
            yield return new WaitForSeconds (1);
            Debug.Log ("un message unity");
        }
    }

    void f () {
        g ();
    }
    void g() {
        LuaConsole.info ("in g");
    }
}
