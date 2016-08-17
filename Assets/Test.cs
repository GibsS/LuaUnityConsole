using UnityEngine;
using System.Collections;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        LuaConsole.init ();
        LuaConsole.setScriptRoot (Application.dataPath + "/TestScript");

        LuaConsole.print ("test");

        LuaConsole.runCode ("print 'test'");

        StartCoroutine (test ());
	}

    IEnumerator test () {
        while(true) {
            LuaConsole.info ("test");
            yield return new WaitForSeconds (1);
            LuaConsole.info ("test", this);
            yield return new WaitForSeconds (1);
            LuaConsole.info ("test", this, "network");
            yield return new WaitForSeconds (1);
        }
    }
}
