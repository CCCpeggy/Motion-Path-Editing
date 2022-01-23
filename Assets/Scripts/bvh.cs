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
        cameraDropdown.onValueChanged.AddListener(delegate {ChangeCameraFollower();});
    }

    public void LoadBVH() {
        var bvhFilePath = StandaloneFileBrowser.OpenFilePanel("Open BVH File", ".",  new []{new ExtensionFilter("BVH", "bvh")}, false)[0];
        LoadBVH(bvhFilePath);
    }
    public void LoadBVH(string path) {
        try
        {
            BVH.BVHObject bvhObject = BVH.BVHObject.CreateBVHObject(path).GetComponent<BVH.BVHObject>();
            AddBVHObj(bvhObject);
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

    public void AddBVHObj (BVH.BVHObject bvhObject) {
        BVHObjects.Add(bvhObject);
        cameraDropdown.AddOptions(new List<string> {bvhObject.name});
    }
    public void Blend() {
        LoadBVH(@"D:\workplace\3D遊戲\P1\bvh_sample_files\cowboy.bvh");
        LoadBVH(@"D:\workplace\3D遊戲\P1\bvh_sample_files\sexywalk.bvh");
        var obj1 = BVHObjects[0];
        var obj2 = BVHObjects[1];
        var blendObj = new BVH.TimeWarping(obj1, obj2).Blend();
        blendObj.name = obj1.name + "_" + obj2.name;
        AddBVHObj(blendObj);
    }
    public void Concat() {
        LoadBVH(@"D:\workplace\3D遊戲\P1\bvh_sample_files\cowboy.bvh");
        LoadBVH(@"D:\workplace\3D遊戲\P1\bvh_sample_files\sexywalk.bvh");
        var obj1 = BVHObjects[0];
        var obj2 = BVHObjects[1];
        var concatObj = new BVH.TimeWarping(obj1, obj2).Concat();
        concatObj.name = obj1.name + "_" + obj2.name;
        AddBVHObj(concatObj);
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