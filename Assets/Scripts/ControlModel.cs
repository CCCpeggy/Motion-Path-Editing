using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using UnityEngine;

public class ControlModel : MonoBehaviour
{
    private Animator anim;
    public GameObject model;
    public BVH.BVHObject bVHObject;

    [Serializable]
    public struct BoneMap
    {
        public int index;
        public Quaternion tPos;
        public HumanBodyBones humanoid_bone;
        public BoneMap(Animator anim, int index, HumanBodyBones humanoid_bone)
        {
            this.index = index;
            this.humanoid_bone = humanoid_bone;
            this.tPos = anim.GetBoneTransform(humanoid_bone).transform.rotation;
        }
    }
    public List<BoneMap> bonemaps = new List<BoneMap>();
    void Start()
    {
        anim = model.GetComponent<Animator>();
        bonemaps.Add(new BoneMap(anim, 0, HumanBodyBones.Hips));
        bonemaps.Add(new BoneMap(anim, 1, HumanBodyBones.Chest));
        bonemaps.Add(new BoneMap(anim, 2, HumanBodyBones.Neck));
        bonemaps.Add(new BoneMap(anim, 3, HumanBodyBones.Head));
        bonemaps.Add(new BoneMap(anim, 4, HumanBodyBones.RightShoulder));
        bonemaps.Add(new BoneMap(anim, 5, HumanBodyBones.RightUpperArm));
        bonemaps.Add(new BoneMap(anim, 6, HumanBodyBones.RightLowerArm));
        bonemaps.Add(new BoneMap(anim, 7, HumanBodyBones.RightHand));
        bonemaps.Add(new BoneMap(anim, 8, HumanBodyBones.LeftShoulder));
        bonemaps.Add(new BoneMap(anim, 9, HumanBodyBones.LeftUpperArm));
        bonemaps.Add(new BoneMap(anim, 10, HumanBodyBones.LeftLowerArm));
        bonemaps.Add(new BoneMap(anim, 11, HumanBodyBones.LeftHand));
        bonemaps.Add(new BoneMap(anim, 12, HumanBodyBones.RightUpperLeg));
        bonemaps.Add(new BoneMap(anim, 13, HumanBodyBones.RightLowerLeg));
        bonemaps.Add(new BoneMap(anim, 14, HumanBodyBones.RightFoot));
        bonemaps.Add(new BoneMap(anim, 15, HumanBodyBones.LeftUpperLeg));
        bonemaps.Add(new BoneMap(anim, 16, HumanBodyBones.LeftLowerLeg));
        bonemaps.Add(new BoneMap(anim, 17, HumanBodyBones.LeftFoot));
    }

    void Update()
    {
        if (bVHObject)
        {
            foreach (BoneMap bm in bonemaps)
            {
                Transform currBone = anim.GetBoneTransform(bm.humanoid_bone);
                currBone.rotation = bVHObject.Part[bm.index].transform.rotation * bm.tPos;
            }
            // anim.GetBoneTransform(HumanBodyBones.Hips).transform.position = FindObjectOfType<BVH.BVHObject>().Root.transform.position;
        }
    }
}
