using System;
using System.Collections;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BVH
{
    class TimeWarping
    {
        public class Data
        {
            public int ContinuesI = 0;
            public int ContinuesJ = 0;
            public int PreviousI = -1;
            public int PreviousJ = -1;
            public double SumDistance = -1;
            public float Length = 0;
            public float Distance = -1;
            public float Theta = 0;
            public float X0 = 0;
            public float Z0 = 0;
        }
        Data[,] twTable;
        BVHObject basicObj; // 時間以這個為基礎
        BVHObject refObj;   // 對應的
        BVHObject newBasicObj, newRefObj;
        public float[] Warping;
        int startFrameIdx = -1;
        float lastRefFrameIdx = -1;
        // public List<float[]> Alinement;
        public TimeWarping(BVHObject basicObj, BVHObject refObj)
        {
            Assert.IsNotNull(basicObj);
            Assert.IsNotNull(refObj);
            this.basicObj = basicObj;
            this.refObj = refObj;
        }
        public BVHObject Concat() {
            Do();
            BlendPart blendPart = new BlendPart(newBasicObj, newRefObj);
            var resultObj = newBasicObj.Clone();
            resultObj.Motion.MotionData.Clear();
            Vector3 lastPos = new Vector3();
            // for (int i=0; i < basicObj.Motion.FrameCount; i++) {
            //     var frame = basicObj.getFrame(i);
            //     resultObj.Motion.MotionData.Add(frame);
            //     lastPos = frame.Position;
            // }
            for (int i=0; i < startFrameIdx; i++) {
                var frame = basicObj.getFrame(i);
                frame.Position += lastPos;
                resultObj.Motion.MotionData.Add(frame);
            }
            Vector3 lastPos2 = new Vector3();
            for (int i=0; i < newBasicObj.Motion.FrameCount; i++) {
                float alpha = (float)i / (newBasicObj.Motion.FrameCount - 1);
                var frame = blendPart.getFrame(i, alpha);
                frame.Position += lastPos2;
                resultObj.Motion.MotionData.Add(frame);
                lastPos2 = frame.Position;
            }
            for (int i=(int)lastRefFrameIdx+1; i < refObj.Motion.FrameCount; i++) {
                var frame = refObj.getFrame(i);
                frame.Position += lastPos;
                resultObj.Motion.MotionData.Add(frame);
                // lastPos2 = frame.Position;
            }
            // for (int i=0; i < refObj.Motion.FrameCount; i++) {
            //     var frame = refObj.getFrame(i);
            //     frame.Position += lastPos2;
            //     resultObj.Motion.MotionData.Add(frame);
            // }
            resultObj.Motion.FrameCount = resultObj.Motion.MotionData.Count();
            Debug.Log(resultObj.Motion.FrameCount);
            resultObj.Motion.FrameTime = (newBasicObj.Motion.FrameTime + newRefObj.Motion.FrameTime) / 2;
            ResetPath(resultObj);
            return resultObj;
        }
        public BVHObject Blend() {
            Do();
            BlendPart blendPart = new BlendPart(newBasicObj, newRefObj);
            var resultObj = blendPart.Get();
            ResetPath(resultObj);
            return resultObj;
        }
        public void Do() {
            newBasicObj = basicObj.Clone();
            newBasicObj.name += "_after_timewarping";
            newBasicObj.gameObject.SetActive(false);
            newRefObj = refObj.Clone();
            newRefObj.name += "_after_timewarping";
            newRefObj.gameObject.SetActive(false);

            var endFrame = createTimeWarpingTable();
            doTimeWarping(endFrame.Item1, endFrame.Item2);
            applyTimeWarping();
            
            newBasicObj.gameObject.SetActive(false);
            newRefObj.gameObject.SetActive(false);
        }
        private static bool isLess(double num1, double num2) {
            // -1 代表無限大
            if (num1 <= 0) return false;
            if (num2 <= 0) return true;
            return num1 < num2;
        }
        private Tuple<int, int> createTimeWarpingTable()
        {
            int o1Count = basicObj.Motion.FrameCount;
            int o2Count = refObj.Motion.FrameCount;
            twTable = new Data[o1Count, o2Count];
            int maxLenI = -1, maxLenJ = -1;
            for (int i = 0; i < o1Count; i++)
            {
                int jStart = Utility.Clip(i - 50, 0, o2Count - 1);
                int jEnd = Utility.Clip(i + 50, 0, o2Count - 1);
                for (int j = jStart; j <= jEnd; j++)
                {
                    var twData = Distance(newBasicObj, newRefObj, i, j);
                    if (maxLenI >= 0) {
                        var maxLenTwData = twTable[maxLenI, maxLenJ];
                        double maxLenAvgDis = maxLenTwData.SumDistance / maxLenTwData.Length;
                        if (!isLess(twData.Distance, maxLenAvgDis * 2)) continue;
                    }
                    twTable[i, j] = twData;
                    Data left = j > 0 ? twTable[i, j - 1] : null;
                    Data up = i > 0 ? twTable[i - 1, j] : null;
                    Data leftup = i > 0 && j > 0 ? twTable[i - 1, j - 1] : null;
                    double leftAvgDis = left != null ? (left.SumDistance / left.Length) : -1;
                    double upAvgDis = up != null ? (up.SumDistance / up.Length) : -1;
                    double leftupAvgDis = leftup != null ? (leftup.SumDistance / leftup.Length) : -1;

                    // 限制斜率
                    if (left != null && left.ContinuesJ >= 2) leftAvgDis = -1;
                    if (up != null && up.ContinuesI >= 2) upAvgDis = -1;

                    if (isLess(leftupAvgDis, upAvgDis) && isLess(leftupAvgDis, leftAvgDis))
                    {
                        twData.PreviousI = i - 1;
                        twData.PreviousJ = j - 1;
                        twData.ContinuesI = 0;
                        twData.ContinuesJ = 0;
                        twData.SumDistance = leftup.SumDistance + twData.Distance;
                        twData.Length = leftup.Length + 1;
                    }
                    else if (isLess(leftAvgDis, upAvgDis))
                    {
                        twData.PreviousI = i;
                        twData.PreviousJ = j - 1;
                        twData.ContinuesI = 0;
                        twData.ContinuesJ = left.ContinuesJ + 1;
                        twData.SumDistance = left.SumDistance + twData.Distance;
                        twData.Length = left.Length + 1;
                    }
                    else if (isLess(upAvgDis, -1))
                    {
                        twData.PreviousI = i - 1;
                        twData.PreviousJ = j;
                        twData.ContinuesI = up.ContinuesI + 1;
                        twData.ContinuesJ = 0;
                        twData.SumDistance = up.SumDistance + twData.Distance;
                        twData.Length = up.Length + 1;
                    }
                    // SumDistance 
                    if (twData.Length <= 0)
                    {
                        twData.ContinuesI = 0;
                        twData.ContinuesJ = 0;
                        twData.Length = 1;
                        twData.SumDistance = twData.Distance;
                    }
                    
                    if (maxLenI <= 0) {
                        maxLenI = i;
                        maxLenJ = j;
                    }
                    else {
                        var maxLenTwData = twTable[maxLenI, maxLenJ];
                        var avgDis = twData.SumDistance / twData.Length;
                        var maxLenAvgDis = maxLenTwData.SumDistance / maxLenTwData.Length;
                        if (twData.Length > maxLenTwData.Length ||
                            (twData.Length == maxLenTwData.Length && avgDis > maxLenAvgDis))
                        {
                            maxLenI = i;
                            maxLenJ = j;
                        }
                    }
                }
            }
            return new Tuple<int, int>(maxLenI, maxLenJ);
        }
        public static Data Distance(BVHObject o1, BVHObject o2, int o1Idx, int o2Idx)
        {
            int partLen = o1.Part.Length;
            float tan11 = 0, tan12 = 0, tan21 = 0, tan22 = 0;
            float x1 = 0, x2 = 0, z1 = 0, z2 = 0;
            Vector3[] vi1 = new Vector3[partLen], vi2 = new Vector3[partLen];
            float w = 1f / partLen;
            for (int i = 0; i < 5; i++)
            {
                float offsetTime = (i - 2) * 0.1f;
                float o1IdxPlusI = Utility.Clip(o1Idx * o1.Motion.FrameTime + offsetTime, 0, o1.Motion.FrameCount - 1);
                float o2IdxPlusI = Utility.Clip(o2Idx * o2.Motion.FrameTime + offsetTime, 0, o2.Motion.FrameCount - 1);
                o1.ApplyFrame(o1IdxPlusI);
                o2.ApplyFrame(o2IdxPlusI);
                for (int j = 0; j < partLen; j++)
                {
                    Vector3 v1 = o1.Part[i].transform.position;
                    Vector3 v2 = o2.Part[i].transform.position;
                    if (i == 2)
                    {
                        vi1[j] = v1;
                        vi2[j] = v2;
                    }
                    tan11 += w * (v1.x * v2.z - v2.x * v1.z);
                    tan21 += w * (v1.x * v2.x + v1.z * v2.z);
                    x1 += w * v1.x;
                    x2 += w * v2.x;
                    z1 += w * v1.z;
                    z2 += w * v2.z;
                }
            }
            tan12 = x1 * z2 - x2 * z1;
            tan22 = x1 * x2 + z1 * z2;
            Data data = new Data();
            data.Theta = Mathf.Atan((tan11 - tan12) / (tan21 - tan22));
            // data.Theta = Utility.ConvertVecToAngle(new Vector2(tan21 - tan22, tan11 - tan12)) * Mathf.Deg2Rad;
            data.X0 = x1 - x2 * Mathf.Cos(data.Theta) - z2 * Mathf.Sin(data.Theta);
            data.Z0 = z1 + x2 * Mathf.Sin(data.Theta) - z2 * Mathf.Cos(data.Theta);
            for (int i = 0; i < partLen; i++)
            {
                Vector3 v2 = Quaternion.Euler(0, data.Theta, 0) * (vi2[i] + new Vector3(data.X0, 0, data.Z0));
                data.Distance += Vector3.Distance(vi1[i], v2);
            }
            return data;
        }
        private void doTimeWarping(int endIFrame, int endJFrame)
        {
            List<Tuple<int, int>> tmpWarping = new List<Tuple<int, int>>();
            int i = endIFrame, j = endJFrame;
            while (i >= 0 && j >= 0)
            {
                tmpWarping.Add(new Tuple<int, int>(i, j));
                var tmp_timewarp = twTable[i, j];
                i = tmp_timewarp.PreviousI;
                j = tmp_timewarp.PreviousJ;
            }
            tmpWarping.Reverse();
            Warping = new float[basicObj.Motion.FrameCount];
            int lastHasValIdx = -1;
            for (i = 0; i < basicObj.Motion.FrameCount; i++)
            {
                var idics = tmpWarping.Where(x => x.Item1 == i).Select(x => x.Item2).ToArray();
                if (idics.Length > 0) {
                    Warping[i] = (float) idics.Average();
                    if (lastHasValIdx >= 0) {
                        for (j = lastHasValIdx + 1; j < i; j++) {
                            Warping[j] = Warping[lastHasValIdx] + (Warping[i] - Warping[lastHasValIdx]) / (i - lastHasValIdx);
                        }
                    }
                    lastHasValIdx = i;
                }
                else {
                    Warping[i] = -1;
                }
            }
            // for (i = 0; i < basicObj.Motion.FrameCount; i++) Debug.Log(i + ": " + Warping[i]);
        }
        private void applyTimeWarping() {
            newRefObj.Motion.MotionData.Clear();
            startFrameIdx = -1;
            int lastFrameIdx = -1;
            lastRefFrameIdx = -1;
            for (int i = 0; i < basicObj.Motion.FrameCount; i++) {
                if (Warping[i] >= 0 && Warping[i] <= refObj.Motion.FrameCount - 1) {
                    lastFrameIdx = i;
                    if (startFrameIdx < 0) startFrameIdx = i + 1;
                    else {
                        newRefObj.Motion.MotionData.Add(refObj.getFrame(Warping[i]));
                        lastRefFrameIdx = Warping[i];
                    }
                }
            }
            newBasicObj.Motion.MotionData.Clear();
            for (int i = startFrameIdx; i <= lastFrameIdx; i++) {
                newBasicObj.Motion.MotionData.Add(basicObj.getFrame(i));
            }
            int count = lastFrameIdx - startFrameIdx + 1;
            newBasicObj.Motion.FrameCount = newRefObj.Motion.FrameCount = count;
            newRefObj.Motion.FrameTime = basicObj.Motion.FrameTime;
            
            ResetPath(newRefObj);
            ResetPath(newBasicObj);
        }

        private void ResetPath(BVHObject obj) {
            if (obj.Motion.CurveGameObject)
                GameObject.Destroy(obj.Motion.CurveGameObject);
            obj.Motion.FitPathCurve(obj);
            obj.Motion.CurveGameObject.transform.parent = obj.transform;
        }
    }
}