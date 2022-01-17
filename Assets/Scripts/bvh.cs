using System.Collections.Generic;
using UnityEngine;
using SFB;
using UnityEngine.UI;

public class bvh : MonoBehaviour
{
    public List<BVH.BVHObject> BVHObjects = new List<BVH.BVHObject>();
    public CameraFollow cameraFollow;
    public Dropdown cameraDropdown;
    void Start() {
        BVHObjects.Clear();
    }

    public void LoadBVH() {
        var bvhFilePath = StandaloneFileBrowser.OpenFilePanel("Open BVH File", ".",  new []{new ExtensionFilter("BVH", "bvh")}, false)[0];
        try
        {
            BVH.BVHObject bvhObject = BVH.BVHObject.CreateBVHObject(bvhFilePath).GetComponent<BVH.BVHObject>();
            
            BVHObjects.Add(bvhObject);
            cameraDropdown.AddOptions(new List<string> {bvhObject.name});
            cameraDropdown.gameObject.SetActive(true);
            if (cameraFollow) {
                cameraFollow.target = bvhObject.Root.transform;
                cameraDropdown.value = BVHObjects.Count - 1;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }

    public void ChangeCameraFollower() {
        int index = cameraDropdown.value;
        cameraFollow.target = BVHObjects[index].Root.transform;
    }
}

// [CustomEditor(typeof(bvh))]
// public class bvhEditor : Editor
// {
//     string bvhFilePath;
//     public override void OnInspectorGUI()
//     {
//         bvh myBvh = (bvh)target;
//         serializedObject.Update();
//         bvhFilePath = EditorGUILayout.TextField("Bvh File Path", bvhFilePath);
//         if(GUILayout.Button("Create"))
//         {
//             try
//             {
                
//             }
//             catch
//             {
//                 Debug.Log("Something Error");
//             }
//         }
//         if(GUILayout.Button("Blend"))
//         {
//             BVH.Blend.Do2(myBvh.BVHObjects);
//         }
//         if (GUILayout.Button("Registrationi Curves"))
//         {
//             BVH.Blend.Do(myBvh.BVHObjects);
//         }
//         serializedObject.ApplyModifiedProperties();
//     }
// }