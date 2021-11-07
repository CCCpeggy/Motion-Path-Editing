using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class bvh : MonoBehaviour
{
    public List<BVH.BVHObject> BVHObjects = new List<BVH.BVHObject>();
    void Start() {
        BVHObjects.Clear();
    }
}

[CustomEditor(typeof(bvh))]
public class bvhEditor : Editor
{
    string bvhFilePath;
    public override void OnInspectorGUI()
    {
        bvh myBvh = (bvh)target;
        serializedObject.Update();
        bvhFilePath = EditorGUILayout.TextField("Bvh File Path", bvhFilePath);
        if(GUILayout.Button("Create"))
        {
            try
            {
                var tmp = BVH.BVHObject.CreateBVHObject(bvhFilePath);
                myBvh.BVHObjects.Add(tmp.GetComponent<BVH.BVHObject>());
            }
            catch
            {
                Debug.Log("Something Error");
            }
        }
        if(GUILayout.Button("Blend"))
        {
            BVH.Blend.Do2(myBvh.BVHObjects);
        }
        if (GUILayout.Button("Registrationi Curves"))
        {
            BVH.Blend.Do(myBvh.BVHObjects);
        }
        serializedObject.ApplyModifiedProperties();
    }
}