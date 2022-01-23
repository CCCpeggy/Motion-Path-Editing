using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;
using UnityEngine.Assertions;


namespace BVH {
    public class BVHMotion{
        public int FrameCount;
        public float FrameTime;
        public class Frame{
            public Vector3 Position = new Vector3();
            public Quaternion[] Rotation = new Quaternion[18];
            public Frame Clone() {
                Frame newFrame = new Frame();
                newFrame.Position = Position;
                newFrame.Rotation = Rotation.Clone() as Quaternion[];
                return newFrame;
            }
            public Frame(int count = 18)
            {
                Rotation = new Quaternion[count];
            }
        }
        public List<Frame> MotionData;
        public GameObject CurveGameObject;
        public BVHMotion Clone(){
            BVHMotion bVHMotion = new BVHMotion();
            bVHMotion.FrameCount = FrameCount;
            bVHMotion.FrameTime = FrameTime;
            bVHMotion.MotionData = new List<Frame>();
            for(int i = 0; i < MotionData.Count; i++)  
                bVHMotion.MotionData.Add(MotionData[i].Clone());
            bVHMotion.CurveGameObject = CurveGameObject.GetComponent<Curve>().Clone().gameObject;
            return bVHMotion;
        }
        public static BVHMotion readMotion(ref IEnumerator<string> bvhDataIter, BVHObject obj) {
            BVHMotion motion = new BVHMotion();
            Utility.IterData.CheckAndNext(ref bvhDataIter, "Frames:");
            motion.FrameCount = int.Parse(Utility.IterData.GetAndNext(ref bvhDataIter));
            Utility.IterData.CheckAndNext(ref bvhDataIter, "Frame");
            Utility.IterData.CheckAndNext(ref bvhDataIter, "Time:");
            motion.FrameTime = float.Parse(Utility.IterData.GetAndNext(ref bvhDataIter));
            motion.MotionData = new List<Frame>();
            for (int i = 0; i < motion.FrameCount; i++) {
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
                motion.MotionData.Add(frame);
            }
            return motion;
        }

        public void FitPathCurve(BVHObject obj) {
            int n = FrameCount;

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
                        b[i, 0] += B[i] * MotionData[t].Position.x;
                    if (zIdx >= 0)
                        b[i, 1] += B[i] * MotionData[t].Position.z;
                }
            }
            var x = A.Solve(b);

            CurveGameObject = Curve.CreateCurve(x, n, "Path");
            Curve curve = CurveGameObject.GetComponent<Curve>();
            
            for(int t = 0; t < n; t++){
                Vector3 curPos = curve.GetPos((float)t / n);
                if (xIdx >= 0)
                    MotionData[t].Position.x -= curPos.x;
                if (zIdx >= 0)
                    MotionData[t].Position.z -= curPos.z;
            }
        }
        public void ApplyFrame(Frame frame, BVHObject obj) {
            var Part = obj.Part;
            for (int i = 0; i < Part.Length; i++)
            {
                Part[i].transform.localPosition = Part[i].Offset;
                Part[i].transform.localRotation = frame.Rotation[i];
            }

            //Vector3 previousPos = MotionData[previousFrameIdx].Position;
            //Vector3 nextPos = MotionData[nextFrameIdx].Position;
            //PoseObj.Root.transform.position += previousPos * (1 - alpha) + nextPos * alpha;
            // PoseObj.UpdateLines();
        }

        public void ApplyFrame(float frameIdx, BVHObject obj) {
            int previousFrameIdx = (int)frameIdx;
            int nextFrameIdx = (previousFrameIdx + 1) % FrameCount;
            float alpha = frameIdx - previousFrameIdx;

            for(int i = 0; i < 18; i++){
                Quaternion previousRotValue = MotionData[previousFrameIdx].Rotation[i];
                Quaternion nextRotValue = MotionData[nextFrameIdx].Rotation[i];
                Quaternion thisRotValue = Utility.GetQuaternionAvg(previousRotValue, nextRotValue, alpha);
                // Debug.Log(previousRotValue + ", " + nextRotValue + ", " + thisRotValue) ; 
                obj.Part[i].transform.localPosition = obj.Part[i].Offset;
                obj.Part[i].transform.localRotation = thisRotValue;
            }

            Vector3 previousPos = MotionData[previousFrameIdx].Position;
            Vector3 nextPos = MotionData[nextFrameIdx].Position;
            obj.Root.transform.position += previousPos * (1 - alpha) + nextPos * alpha;
            obj.Root.transform.position += CurveGameObject.GetComponent<Curve>().GetPos(frameIdx / FrameCount);
            obj.Root.transform.localRotation *= CurveGameObject.GetComponent<Curve>().GetRot(frameIdx / FrameCount);
            obj.UpdateLines();
        }
        public void ResetMotionInfo(int frameCount, float frameTime) {
            this.FrameCount = frameCount;
            this.FrameTime = frameTime;
            if (this.MotionData == null)
                this.MotionData = new List<Frame>();
            else
                this.MotionData.Clear();
        }
        public Vector3 getFramePosition(float frameIdx) {
            int previousFrameIdx = (int)frameIdx;
            int nextFrameIdx = (previousFrameIdx + 1) % FrameCount;
            Vector3 previous = MotionData[previousFrameIdx].Position;
            Vector3 next = MotionData[nextFrameIdx].Position;
            float alpha = frameIdx - previousFrameIdx;
            Vector3 pos = previous * (1 - alpha) + next * alpha;
            pos += CurveGameObject.GetComponent<Curve>().GetPos((float)frameIdx / FrameCount);
            return pos;
        }
        public Quaternion getFrameQuaternion(float frameIdx, int partIdx) {
            int previousFrameIdx = (int)frameIdx;
            int nextFrameIdx = (previousFrameIdx + 1) % FrameCount;
            Quaternion previous = MotionData[previousFrameIdx].Rotation[partIdx];
            Quaternion next = MotionData[nextFrameIdx].Rotation[partIdx];
            float alpha = frameIdx - previousFrameIdx;
            var angle = Utility.GetQuaternionAvg(previous, next, alpha);
            return partIdx == 0 ? angle * CurveGameObject.GetComponent<Curve>().GetRot(frameIdx / FrameCount) : angle;
        }
    }
}