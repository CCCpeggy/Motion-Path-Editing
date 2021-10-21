using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class bvh : MonoBehaviour
{
    double time = 0;
    int frame = 0;
    BVH.BVHPartObject BVHObject;
    void Start()
    {
        BVHObject = BVH.Reader.LoadFile(@"D:\workplace\3D遊戲\P1\bvh_sample_files\bvh_sample_files\walk_loop.bvh");
        // BVHObject.applyFrame(20);
    }

    void Update()
    {
        time += Time.deltaTime;
        if (time > BVHObject.frameTime) {
          BVHObject.applyFrame(frame);
          time = 0;
          frame += 1;
          frame %= BVHObject.frameCount;
        }
    }

}
