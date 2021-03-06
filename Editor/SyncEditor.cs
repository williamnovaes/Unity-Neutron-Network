﻿using UnityEditor;

[CustomEditor(typeof(SyncRigidbody))]
public class SyncEditor : Editor
{
    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        SyncRigidbody eTarget = (SyncRigidbody)target;

        if(eTarget.whenChanging == default) EditorGUILayout.HelpBox("This function synchronize at all times. UPDATE()", MessageType.Warning);
        else if (eTarget.whenChanging == WhenChanging.Position) EditorGUILayout.HelpBox("Only the Position will be synchronized when changing.", MessageType.Info);
        else if (eTarget.whenChanging == WhenChanging.Rotation) EditorGUILayout.HelpBox("Only the Rotation will be synchronized when changing.", MessageType.Info);
        else if (eTarget.whenChanging == WhenChanging.Velocity) EditorGUILayout.HelpBox("Only the Velocity will be synchronized when changing.", MessageType.Info);
        else if (eTarget.whenChanging == (WhenChanging.Position | WhenChanging.Rotation)) EditorGUILayout.HelpBox("Only the Rotation and Position will be synchronized when changing.", MessageType.Info);
        else if (eTarget.whenChanging == (WhenChanging.Velocity | WhenChanging.Position)) EditorGUILayout.HelpBox("Only the Velocity and Position will be synchronized when changing.", MessageType.Info);
        else if (eTarget.whenChanging == (WhenChanging.Velocity | WhenChanging.Rotation)) EditorGUILayout.HelpBox("Only the Rotation and Velocity will be synchronized when changing.", MessageType.Info);
        else if (eTarget.whenChanging == (WhenChanging.Velocity | WhenChanging.Rotation | WhenChanging.Position)) EditorGUILayout.HelpBox("Only the Rotation, Velocity, Position will be synchronized when changing.", MessageType.Info);
        else EditorGUILayout.HelpBox("This function synchronize at any property changed", MessageType.Info);
    }
}
