using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class bvh : MonoBehaviour
{
    float frame = 0;
    BVH.BVHObject BVHObject = null;
    void Start()
    {
        BVHObject = new BVH.BVHObject(@"D:\workplace\3D遊戲\P1\bvh_sample_files\bvh_sample_files\dance02.bvh");
    }

    void Update()
    {
        if (BVHObject != null) {
            BVHObject.ApplyFrame(Time.deltaTime);
        }
    }

}
