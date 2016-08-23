using UnityEngine;
using System.Collections;
using System;

public class Test : MonoBehaviour {

	// Use this for initialization
	void Start () {
        Shell.init ();
        Shell.setScriptRoot (Application.dataPath + "/TestScript");

        StartCoroutine (test ());

        Shell.info ("in g");
        f ();
	}

    IEnumerator test () {
        while(true) {
            Shell.info ("test");
            yield return new WaitForSeconds (1);
            Shell.info ("test", this);
            yield return new WaitForSeconds (1);
            Shell.info ("test", this, "network");
            yield return new WaitForSeconds (1);
            Debug.Log ("un message unity");
        }
    }

    void f () {
        g ();
    }
    void g() {
        Shell.info ("in g");
    }
}
