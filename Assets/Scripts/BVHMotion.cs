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
        public class Frame{
            public Vector3 Position = new Vector3();
            public Quaternion[] Rotation = new Quaternion[18];
            public Frame Clone() {
                Frame newFrame = new Frame();
                newFrame.Position = Position;
                newFrame.Rotation = Rotation.Clone() as Quaternion[];
                return newFrame;
            }
        }
        public List<Frame> motionData;
        public GameObject CurveGameObject;
        public BVHMotion Clone(){
            BVHMotion bVHMotion = new BVHMotion();
            bVHMotion.frameCount = frameCount;
            bVHMotion.frameTime = frameTime;
            for(int i = 0; i < motionData.Count; i++)  
                bVHMotion.motionData.Add(motionData[i].Clone());
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
            motion.motionData = new List<Frame>();
            for (int i = 0; i < motion.frameCount; i++) {
                Frame frame = new Frame();
                for (int j = 0; j < 18; j++) {
                    frame.Rotation[j] = Quaternion.Euler(0, 0, 0);
                }
                for (int j = 0; j < obj.ChannelDatas.Count; j++) {
                    float num = float.Parse(Utility.IterData.GetAndNext(ref bvhDataIter));
                    var partObj = obj.ChannelDatas[j].Item1;
                    var infoIdx = obj.ChannelDatas[j].Item2;
                    int partIdx = Utility.GetPartIdxByName(partObj.name);
                    if (partIdx == 0 && infoIdx >= 3) {
                        frame.Position[infoIdx - 3] = num;
                    }
                    else {
                        if (infoIdx == 0) frame.Rotation[partIdx] *= Quaternion.Euler(num, 0, 0);
                        else if (infoIdx == 1) frame.Rotation[partIdx] *= Quaternion.Euler(0, num, 0);
                        else if (infoIdx == 2) frame.Rotation[partIdx] *= Quaternion.Euler(0, 0, num);
                    }
                }
                motion.motionData.Add(frame);
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
                        b[i, 0] += B[i] * motionData[t].Position.x;
                    if (zIdx >= 0)
                        b[i, 1] += B[i] * motionData[t].Position.z;
                }
            }
            var x = A.Solve(b);

            CurveGameObject = Curve.CreateCurve(x, n, "Path");
            Curve curve = CurveGameObject.GetComponent<Curve>();
            
            for(int t = 0; t < n; t++){
                Vector3 curPos = curve.GetPos((float)t / n);
                if (xIdx >= 0)
                    motionData[t].Position.x -= curPos.x;
                if (zIdx >= 0)
                    motionData[t].Position.z -= curPos.z;
            }
        }

        public void ApplyFrame(float frameIdx, BVHObject obj) {
            int previousFrameIdx = (int)frameIdx;
            int nextFrameIdx = (previousFrameIdx + 1) % frameCount;
            float alpha = frameIdx - previousFrameIdx;

            for(int i = 0; i < 18; i++){
                var partObj = obj.ChannelDatas[i].Item1;
                int posAndRotIdx = obj.ChannelDatas[i].Item2;
                
                Quaternion previousRotValue = motionData[previousFrameIdx].Rotation[i];
                Quaternion nextRotValue = motionData[nextFrameIdx].Rotation[i];
                Quaternion thisRotValue = Utility.GetQuaternionAvg(previousRotValue, nextRotValue, alpha);
                // Debug.Log(previousRotValue + ", " + nextRotValue + ", " + thisRotValue) ; 
                obj.Part[i].transform.localPosition = obj.Part[i].Offset;
                obj.Part[i].transform.localRotation = thisRotValue;
            }

            Vector3 previousPos = CurveGameObject.GetComponent<Curve>().GetPos((float)previousFrameIdx / frameCount);
            Vector3 nextPos = CurveGameObject.GetComponent<Curve>().GetPos((float)nextFrameIdx / frameCount);
            previousPos += motionData[previousFrameIdx].Position;
            nextPos += motionData[nextFrameIdx].Position;
            obj.Root.transform.position += previousPos * (1 - alpha) + nextPos * alpha;
            obj.UpdateLines();
        }
        public void ResetMotionInfo(int frameCount, float frameTime) {
            this.frameCount = frameCount;
            this.frameTime = frameTime;
            this.motionData.Clear();
        }
        public float getMotion(float frameIdx, int valueIdx, Tuple<BVHPartObject, int> chData) {
            // int previousFrameIdx = (int)frameIdx;
            // int nextFrameIdx = (previousFrameIdx + 1) % frameCount;
            // float previousMotionValue = motionData[previousFrameIdx, valueIdx];
            // float nextMotionValue = motionData[nextFrameIdx, valueIdx];
            // var partObj = chData.Item1;
            // int posAndRotIdx = chData.Item2;
            // if (partObj.Parent == null && posAndRotIdx >= 3) {
            //     Vector3 previousPos = CurveGameObject.GetComponent<Curve>().GetPos((float)previousFrameIdx / frameCount);
            //     Vector3 nextPos = CurveGameObject.GetComponent<Curve>().GetPos((float)nextFrameIdx / frameCount);
            //     previousMotionValue += previousPos[posAndRotIdx - 3];
            //     nextMotionValue += nextPos[posAndRotIdx - 3];
            // }
            // float alpha = frameIdx - previousFrameIdx;
            // return Utility.GetAngleAvg(previousMotionValue, nextMotionValue, alpha);
            return 0;
        }
    }

}