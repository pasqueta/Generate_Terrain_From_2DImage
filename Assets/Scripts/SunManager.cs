using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SunManager : MonoBehaviour
{
    float time = 0.0f;

	// Use this for initialization
	void Start ()
    {
		
	}
	
	// Update is called once per frame
	void Update ()
    {
        time += Time.deltaTime * 0.5f;

        transform.Rotate(new Vector3(0, Time.deltaTime, 0));
	}
}
