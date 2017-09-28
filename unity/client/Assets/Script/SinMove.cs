using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SinMove : MonoBehaviour {

	// Use this for initialization
	void Start () {
		
	}
	
	// Update is called once per frame
	void Update () {
        Vector2 pos = new Vector2(Mathf.Sin(Time.time * 2.92f), Mathf.Cos(Time.time * 2.311f));
        transform.localPosition  = pos;
	}
}
