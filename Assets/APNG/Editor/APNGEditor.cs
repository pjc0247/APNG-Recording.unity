using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(APNG))]
public class APNGEditor : Editor {

	public override void OnInspectorGUI()
	{
		base.OnInspectorGUI();

		GUILayout.Space(20);
		if (GUILayout.Button("Record"))
		{
			((APNG)target).BeginRecord();
		}
	}
}
