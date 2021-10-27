using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

[ExecuteInEditMode]
public class bvh : MonoBehaviour
{
    public List<BVH.BVHObject> BVHObjects = new List<BVH.BVHObject>();
    void Start()
    {
        // BVH.BVHObject.CreateBVHObject(@"D:\workplace\3D遊戲\P1\bvh_sample_files\bvh_sample_files\walk_loop.bvh");
    }

}
