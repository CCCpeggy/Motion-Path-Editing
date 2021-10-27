using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnityEditor;
using UnityEditor.UIElements;

[ExecuteInEditMode]
public class bvh : MonoBehaviour
{
    public List<BVH.BVHObject> BVHObjects = new List<BVH.BVHObject>();

}

[CustomEditor(typeof(bvh))]
public class bvhEditor : Editor
{
    string bvhFilePath = @"D:\workplace\3D遊戲\P1\bvh_sample_files\bvh_sample_files\walk_loop.bvh";
    public override void OnInspectorGUI()
    {
        bvh myBvh = (bvh)target;
        serializedObject.Update();
        bvhFilePath = EditorGUILayout.TextField("Bvh File Path", bvhFilePath);
        if(GUILayout.Button("Create"))
        {
            myBvh.BVHObjects.Add(BVH.BVHObject.CreateBVHObject(bvhFilePath).GetComponent<BVH.BVHObject>());
        }
        serializedObject.ApplyModifiedProperties();
    }
}