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

    void DrawCurve() {
        Vector<double>[] tmp = {
            new DenseVector(new double[] {0, 1, 3, 7}), 
            new DenseVector(new double[] {-1, 0, 3, -3}), 
        };

        var curve = new GameObject();
        curve.name = "Curve";
        int count = 0;
        for(double i=0; i<10;i+=0.05){
            Vector3 last = BVH.Utility.Curve.GetP(i-0.057, tmp);
            Vector3 p = BVH.Utility.Curve.GetP(i, tmp);
            var line = new GameObject();
            line.name = "Curve_" + count++;
            line.transform.parent = curve.transform;
            LineRenderer lr = line.AddComponent<LineRenderer>();
            lr.SetPosition(0, last);
            lr.SetPosition(1, p);
            lr.startWidth = 0.2f;
            lr.endWidth = 0.2f;
            last = p;
        }
    }
}
