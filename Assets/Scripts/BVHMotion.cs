using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnityEngine.Assertions;


namespace BVH {
    public class BVHMotion{
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
        public float[, ] motionData;
        public GameObject CurveGameObject;
        public BVHMotion Clone(){
            BVHMotion bVHMotion = new BVHMotion();
            bVHMotion.frameCount = frameCount;
            bVHMotion.frameTime = frameTime;
            bVHMotion.motionData = motionData.Clone() as float[,];
            Debug.Log(bVHMotion);
            bVHMotion.CurveGameObject = CurveGameObject.GetComponent<Curve>().Clone().gameObject;
            return bVHMotion;
        }
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

        public void FitPathCurve(BVHObject obj) {
            int n = frameCount;

            int xIdx=-1, zIdx=-1;
            for(int i = 0; i < obj.ChannelDatas.Count; i++){
                var partObj = obj.ChannelDatas[i].Item1;
                int posAndRotIdx = obj.ChannelDatas[i].Item2;
                if (partObj == obj.Root) {
                    if (posAndRotIdx == 3) xIdx = i;
                    if (posAndRotIdx == 5) zIdx = i;
                }
            }

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
                        b[i, 0] += B[i] * motionData[t, xIdx];
                    if (zIdx >= 0)
                        b[i, 1] += B[i] * motionData[t, zIdx];
                }
            }
            var x = A.Solve(b);
            
            CurveGameObject = Curve.CreateCurve(x, n, "Path");
            Curve curve = CurveGameObject.GetComponent<Curve>();
            
            for(int t = 0; t < n; t++){
                Vector3 curPos = curve.GetPos((float)t / n);
                if (xIdx >= 0)
                    motionData[t, xIdx] -= curPos.x;
                if (zIdx >= 0)
                    motionData[t, zIdx] -= curPos.z;
            }
        }

        public void ApplyFrame(float frameIdx, BVHObject obj) {
            BVHPartObject lastObj = null;
            int previousFrameIdx = (int)frameIdx;
            int nextFrameIdx = (previousFrameIdx + 1) % frameCount;
            Vector3 lastPos = CurveGameObject.GetComponent<Curve>().GetPos((float)previousFrameIdx / frameCount);
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
                float lastMotionValue = motionData[previousFrameIdx, i];
                float nextMotionValue = motionData[nextFrameIdx, i];
                if (partObj == obj.Root && posAndRotIdx >= 3) {
                    lastMotionValue += lastPos[posAndRotIdx - 3];
                    nextMotionValue += nextPos[posAndRotIdx - 3];
                }
                float alpha = frameIdx - previousFrameIdx;
                float thisMotionValue = Utility.GetAngleAvg(lastMotionValue, nextMotionValue, alpha);
                partObj.setPosOrRot(posAndRotIdx, thisMotionValue);
            }
            obj.UpdateLines();
        }
        public void ResetMotionInfo(int frameCount, float frameTime) {
            this.frameCount = frameCount;
            this.frameTime = frameTime;
            this.motionData = new float[frameCount, 57];
        }
        public float getMotion(float frameIdx, int valueIdx, Tuple<BVHPartObject, int> chData) {
            int previousFrameIdx = (int)frameIdx;
            int nextFrameIdx = (previousFrameIdx + 1) % frameCount;
            float lastMotionValue = motionData[previousFrameIdx, valueIdx];
            float nextMotionValue = motionData[nextFrameIdx, valueIdx];
            var partObj = chData.Item1;
            int posAndRotIdx = chData.Item2;
            if (partObj.Parent == null && posAndRotIdx >= 3) {
                Vector3 lastPos = CurveGameObject.GetComponent<Curve>().GetPos((float)previousFrameIdx / frameCount);
                Vector3 nextPos = CurveGameObject.GetComponent<Curve>().GetPos((float)nextFrameIdx / frameCount);
                lastMotionValue += lastPos[posAndRotIdx - 3];
                nextMotionValue += nextPos[posAndRotIdx - 3];
            }
            float alpha = frameIdx - previousFrameIdx;
            return Utility.GetAngleAvg(lastMotionValue, nextMotionValue, alpha);
        }
        public void ResetMotionFrame(int frameIdx, ref BVHPartObject[] Part) {
            Vector3 curPos = CurveGameObject.GetComponent<Curve>().GetPos((float)frameIdx / FrameCount);
            motionData[frameIdx, 0] = Part[0].transform.position.x - curPos.x;
            motionData[frameIdx, 1] = Part[0].transform.position.y;
            motionData[frameIdx, 2] = Part[0].transform.position.z - curPos.z;
            for(int i = 0; i < 18; i++) {
                if(i==0)Debug.Log(Part[i].transform.localRotation);
                float y = Part[i].transform.localRotation.y;
                Part[i].transform.localRotation *= Quaternion.Euler(0, -y, 0);
                float x = Part[i].transform.localRotation.x;
                Part[i].transform.localRotation *= Quaternion.Euler(-x, 0, 0);
                float z = Part[i].transform.localRotation.z;
                Part[i].transform.localRotation *= Quaternion.Euler(0, 0, -z);
                motionData[frameIdx, i * 3 + 0] = z * Mathf.Rad2Deg;
                motionData[frameIdx, i * 3 + 1] = x * Mathf.Rad2Deg;
                motionData[frameIdx, i * 3 + 2] = y * Mathf.Rad2Deg;
                motionData[frameIdx, i * 3 + 0] = 0;
                motionData[frameIdx, i * 3 + 1] = 0;
                Debug.Log(x * Mathf.Rad2Deg+", "+y * Mathf.Rad2Deg+", "+z);
            }
        }
    }

}