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
             

            // for(int i = 0; i < obj.ChannelDatas.Count; i++){
            //     Utility.GetB03();
            //     Matrix<double> A = DenseMatrix.OfArray(new double[,] {
            //         {1,1,1,1},
            //         {1,2,3,4},
            //         {4,3,2,1}}
            //     );
            //     var a = Matrix<double>.Build.Random(500, 500);
            //     var b = Vector<double>.Build.Random(500);
            //     var x = a.Solve(b);
            //     Debug.Log(x);
            // }
        }
    
        public void FitPathCurve() {
            
        }
    }

}