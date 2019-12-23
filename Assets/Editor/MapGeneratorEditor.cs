using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(MapGenerator))]
public class MapGeneratorEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        MapGenerator myTarget = (MapGenerator)target;

        if (GUILayout.Button("Save"))
        {
            myTarget.SaveTerrain();
        }

        /*if (GUILayout.Button("Load"))
        {
            if (myTarget.dataTerrain != null)
            {
                myTarget.LoadTerrain(myTarget.dataTerrain);
            }
        }*/
    }
}
