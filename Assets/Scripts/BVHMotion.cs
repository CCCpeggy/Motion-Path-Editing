using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnityEngine.Assertions;


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
        public GameObject CurveGameObject;

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

            int xIdx=-1, zIdx=-1;
            for(int i = 0; i < obj.ChannelDatas.Count; i++){
                var partObj = obj.ChannelDatas[i].Item1;
                int posAndRotIdx = obj.ChannelDatas[i].Item2;
                if (partObj == obj.Root) {
                    if (posAndRotIdx == 3) xIdx = i;
                    if (posAndRotIdx == 5) zIdx = i;
                }
            }
            // Assert.IsTrue(xIdx >= 0);
            // Assert.IsTrue(zIdx >= 0);

            // 讓曲線的每一段是以距離去切割
            // List<double> tmp=new List<double>();
            // double d = 0;
            // tmp.Add(0);
            // for(int i = 0; i < n-1; i++){
            //     Vector2 v1 = new Vector2(motion.motionData[i, 0],  motion.motionData[i, 2]);
            //     Vector2 v2 = new Vector2(motion.motionData[i+1, 0],  motion.motionData[i+1, 2]);
            //     d += Vector2.Distance(v1, v2);
            //     tmp.Add(d);
            // }
            // for(int i = 0; i < n; i++){
            //     tmp[i] /= d;
            // }

            Matrix<double> A = new DenseMatrix(4, 4);
            Matrix<double> b = new DenseMatrix(4, 2);
            for(int t = 0; t < n; t++){
                double[] B = 
                {
                    Curve.GetB0((double)t/n),
                    Curve.GetB1((double)t/n),
                    Curve.GetB2((double)t/n),
                    Curve.GetB3((double)t/n)
                };
                for (int i = 0; i < 4; i++) {
                    for (int j = 0; j < 4; j++) {
                        A[i, j] += B[i] * B[j];
                    }
                    if (xIdx >= 0)
                        b[i, 0] += B[i] * motion.motionData[t, xIdx];
                    if (zIdx >= 0)
                        b[i, 1] += B[i] * motion.motionData[t, zIdx];
                }
            }
            var x = A.Solve(b);

            // Debug.Log(A);
            // Debug.Log(b);
            // Debug.Log(x);
            
            motion.CurveGameObject = Curve.CreateCurve(x, n, "Path");
            Curve curve = motion.CurveGameObject.GetComponent<Curve>();
            
            for(int t = 0; t < n; t++){
                Vector3 curPos = curve.GetPos((float)t / n);
                if (xIdx >= 0)
                    motion.motionData[t, xIdx] -= curPos.x;
                if (zIdx >= 0)
                    motion.motionData[t, zIdx] -= curPos.z;
            }

            return motion;
            
        }

        public void ApplyFrame(float frameIdx, BVHObject obj) {
            BVHPartObject lastObj = null;
            int lastFrameIdx = (int)frameIdx;
            int nextFrameIdx = (lastFrameIdx + 1) % frameCount;
            Vector3 lastPos = CurveGameObject.GetComponent<Curve>().GetPos((float)lastFrameIdx / frameCount);
            Vector3 nextPos = CurveGameObject.GetComponent<Curve>().GetPos((float)nextFrameIdx / frameCount);
            for(int i = 0; i < obj.ChannelDatas.Count; i++){
                var partObj = obj.ChannelDatas[i].Item1;
                int posAndRotIdx = obj.ChannelDatas[i].Item2;
                if (lastObj != partObj) {
                    partObj.transform.localPosition = Vector3.zero;
                    partObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    partObj.transform.localPosition += partObj.Offset;
                    lastObj = partObj;
                }
                float lastMotionValue = motionData[lastFrameIdx, i];
                float nextMotionValue = motionData[nextFrameIdx, i];
                if (partObj == obj.Root && posAndRotIdx >= 3) {
                    lastMotionValue += lastPos[posAndRotIdx - 3];
                    nextMotionValue += nextPos[posAndRotIdx - 3];
                }
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