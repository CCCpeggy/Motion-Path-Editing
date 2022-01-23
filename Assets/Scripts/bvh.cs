using System.Collections.Generic;
using UnityEngine;
using SFB;
using UnityEngine.UI;

public class bvh : MonoBehaviour
{
    public List<BVH.BVHObject> BVHObjects = new List<BVH.BVHObject>();
    public CameraFollow cameraFollow;
    public Camera UpToDownCamera;
    public Dropdown cameraDropdown;
    public ControlModel controlModel;
    public bool useModel = true;
    private bool editMode = false;
    public Material lineMaterial, CurveMaterial, CurveControlMaterial, SelectedMaterial;
    int preSelectedIdx = 0;
    void Start()
    {
        cameraDropdown.onValueChanged.AddListener(delegate { ChangeCameraFollower(); });
    }
    public void LoadBVH()
    {
        var bvhFilePath = StandaloneFileBrowser.OpenFilePanel("Open BVH File", ".", new[] { new ExtensionFilter("BVH", "bvh") }, false)[0];
        LoadBVH(bvhFilePath);
    }
    public void LoadBVH(string path)
    {
        try
        {
            BVH.BVHObject bvhObject = BVH.BVHObject.CreateBVHObject(path).GetComponent<BVH.BVHObject>();
            AddBVHObj(bvhObject);
            cameraDropdown.gameObject.SetActive(true);
            if (cameraFollow && BVHObjects.Count == 1)
            {
                cameraFollow.target = bvhObject.Root.transform;
                cameraDropdown.value = 0;
                controlModel.bVHObject = bvhObject;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError(e);
        }
    }
    public void AddBVHObj(BVH.BVHObject bvhObject)
    {
        bvhObject.SetLineMaterial(BVHObjects.Count == 0 ? SelectedMaterial: lineMaterial);
        Curve curve = bvhObject.Motion.CurveGameObject.GetComponent<Curve>();
        curve.SetMaterial(CurveControlMaterial, CurveMaterial);
        curve.SetLineVisible(false);
        BVHObjects.Add(bvhObject);
        cameraDropdown.AddOptions(new List<string> { bvhObject.name });
    }
    public void Blend()
    {
        var obj1 = BVHObjects[0];
        var obj2 = BVHObjects[1];
        var blendObj = new BVH.TimeWarping(obj1, obj2).Blend();
        blendObj.name = obj1.name + "_" + obj2.name;
        AddBVHObj(blendObj);
    }
    public void Concat()
    {
        var obj1 = BVHObjects[0];
        var obj2 = BVHObjects[1];
        var concatObj = new BVH.TimeWarping(obj1, obj2).Concat();
        concatObj.name = obj1.name + "_" + obj2.name;
        AddBVHObj(concatObj);
    }

    public void ChangeCameraFollower()
    {
        int index = cameraDropdown.value;
        cameraFollow.target = BVHObjects[index].Root.transform;
        controlModel.bVHObject = BVHObjects[index];
        BVHObjects[preSelectedIdx].SetLineMaterial(lineMaterial);
        BVHObjects[index].SetLineMaterial(SelectedMaterial);
        if(editMode) {
            Edit(preSelectedIdx);
            Edit();
        }

        preSelectedIdx = index;
    }

    public void Edit(int index=-1) {
        if(index<0) index = cameraDropdown.value;
        var bvhObject = BVHObjects[index];
        Curve curve = bvhObject.Motion.CurveGameObject.GetComponent<Curve>();
        if (editMode) {
            UpToDownCamera.gameObject.SetActive(false);
            curve.SetLineVisible(false);
            editMode = false;
        }
        else {
            UpToDownCamera.transform.position = bvhObject.Root.transform.position;
            UpToDownCamera.gameObject.SetActive(true);
            curve.SetLineVisible(true);
            editMode = true;
        }
    }

    // public void OpenOrCloseModel() {
    //     useModel = !useModel;
    //     controlModel.gameObject.SetActive(useModel);
    // }
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