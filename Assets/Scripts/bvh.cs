using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bvh : MonoBehaviour
{
    double time = 0;
    int frame = 0;
    BVH.BVHObject BVHObject;
    void Start()
    {
        BVHObject = new BVH.BVHObject(@"D:\workplace\3D遊戲\P1\bvh_sample_files\bvh_sample_files\dance3.bvh");
    }

    void Update()
    {
        time += Time.deltaTime;
        if (time > BVHObject.FrameTime) {
          BVHObject.applyFrame(frame);
          time = 0;
          frame += 1;
          frame %= BVHObject.FrameCount;
        }
    }

}
