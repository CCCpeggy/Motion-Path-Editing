using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class bvh : MonoBehaviour
{
    float frame = 0;
    BVH.BVHObject BVHObject;
    void Start()
    {
        BVHObject = new BVH.BVHObject(@"D:\workplace\3D遊戲\P1\bvh_sample_files\bvh_sample_files\walk_loop.bvh");
    }

    void Update()
    {
        BVHObject.ApplyFrame(Time.deltaTime);
    }

}
