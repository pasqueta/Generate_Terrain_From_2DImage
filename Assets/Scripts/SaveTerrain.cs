using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SaveTerrain : MonoBehaviour
{
    MapGenerator generator;

	// Use this for initialization
	void Start ()
    {
        generator = GetComponent<MapGenerator>();
	}
	
	// Update is called once per frame
	void Update ()
    {
        generator.SaveTerrain();
	}

    public void Save()
    {
        generator.SaveTerrain();
    }
    public void Load()
    {
        //generator.LoadTerrain();
    }
}
