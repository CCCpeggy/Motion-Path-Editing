using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;


namespace BVH {
    class BVHMotion{
        private int frameCount;
        public int FrameCount{
            get{
                return frameCount;
            }
        }
        private float frameTime;
        public float FrameTime{
            get{
                return frameTime;
            }
        }
        private float[, ] motionData;

        public static BVHMotion readMotion(ref IEnumerator<string> bvhDataIter, BVHObject obj) {
            BVHMotion motion = new BVHMotion();
            Utility.IterData.CheckAndNext(ref bvhDataIter, "Frames:");
            motion.frameCount = int.Parse(Utility.IterData.GetAndNext(ref bvhDataIter));
            Utility.IterData.CheckAndNext(ref bvhDataIter, "Frame");
            Utility.IterData.CheckAndNext(ref bvhDataIter, "Time:");
            motion.frameTime = float.Parse(Utility.IterData.GetAndNext(ref bvhDataIter));
            motion.motionData = new float[motion.frameCount, obj.ChannelDatas.Count];
            for (int i = 0; i < motion.frameCount; i++) {
                for (int j = 0; j < obj.ChannelDatas.Count; j++) {
                    float num = float.Parse(Utility.IterData.GetAndNext(ref bvhDataIter));
                    motion.motionData[i, j] = num;
                }
            }
            
            int n = motion.frameCount;

            List<double> tmp=new List<double>();
            double d = 0;
            tmp.Add(0);
            for(int i = 0; i < n-1; i++){
                Vector2 v1 = new Vector2(motion.motionData[i, 0],  motion.motionData[i, 2]);
                Vector2 v2 = new Vector2(motion.motionData[i+1, 0],  motion.motionData[i+1, 2]);
                d += Vector2.Distance(v1, v2);
                tmp.Add(d);
            }
            for(int i = 0; i < n; i++){
                tmp[i] /= d;
            }

            Matrix<double> A = new DenseMatrix(4, 4);
            Matrix<double> b = new DenseMatrix(4, 2);
            for(int t = 0; t < n; t++){
                double[] B = 
                {
                    Curve.GetB0((double)tmp[t]),
                    Curve.GetB1((double)tmp[t]),
                    Curve.GetB2((double)tmp[t]),
                    Curve.GetB3((double)tmp[t])
                };
                for (int i = 0; i < 4; i++) {
                    for (int j = 0; j < 4; j++) {
                        A[i, j] += B[i] * B[j];
                    }
                    b[i, 0] += B[i] * motion.motionData[t, 0];
                    b[i, 1] += B[i] * motion.motionData[t, 2];
                }
            }
            var x = A.Solve(b);

            Debug.Log(A);
            Debug.Log(b);
            Debug.Log(x);
            
            Curve.CreateCurve(x, n, "Path");
            return motion;
            
        }
        
        public void ApplyFrame(float frameIdx, BVHObject obj) {
            BVHPartObject lastObj = null;
            for(int i = 0; i < obj.ChannelDatas.Count; i++){
                var partObj = obj.ChannelDatas[i].Item1;
                int posAndRotIdx = obj.ChannelDatas[i].Item2;
                if (lastObj != partObj) {
                    partObj.transform.localPosition = Vector3.zero;
                    partObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    partObj.transform.localPosition += partObj.Offset;
                    lastObj = partObj;
                }
                int lastFrameIdx = (int)frameIdx;
                float lastMotionValue = motionData[lastFrameIdx, i];
                float nextMotionValue = motionData[(lastFrameIdx+1)%frameCount, i];
                float alpha = frameIdx - lastFrameIdx;
                float thisMotionValue = Utility.GetAngleAvg(lastMotionValue, nextMotionValue, alpha);
                partObj.setPosOrRot(posAndRotIdx, thisMotionValue);
            }
            obj.UpdateLines();
        }
    
        public void FitPathCurve() {
            
        }
    }

}