using System;
using System.Collections;
using UnityEngine.Assertions;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BVH {
    class Blend{
        public static BVH.BVHObject Do(List<BVH.BVHObject> blendObjs) {
            if(blendObjs.Count == 0) return null;
            else if(blendObjs.Count == 1) return blendObjs[0].Clone();
            
            List<BVH.BVHObject> objs = new List<BVH.BVHObject>();
            for(int i = 0; i < blendObjs.Count; i++) {
                objs.Add(blendObjs[i].Clone());
                objs[i].name = blendObjs[i].name + "_tmp";
                objs[i].gameObject.SetActive(false);
            }
            BVH.BVHObject reference = objs[0];
            List<List<Tuple<int, float>>> timewarps = new List<List<Tuple<int, float>>>();
            List<List<float[]>> alinements = new List<List<float[]>>();
            int minI = reference.Motion.FrameCount, maxI = 0;
            float sumFrameTime = reference.Motion.FrameTime;
            for(int i = 1; i < objs.Count; i++) {
                var data = CreateTimeWarp(reference, objs[i]);
                var timewarp = data.Item1;
                timewarps.Add(timewarp);
                if (timewarp[0].Item1 < minI) minI = timewarp[0].Item1;
                if (timewarp[timewarp.Count-1].Item1 > maxI) maxI = timewarp[timewarp.Count-1].Item1;
                var alinement = data.Item2;
                alinements.Add(alinement);
                sumFrameTime += objs[i].Motion.FrameTime;
            }
            BVH.BVHObject blend = reference.Clone(false);
            blend.gameObject.name = "blend";
            blend.Motion = new BVHMotion();
            blend.Motion.ResetMotionInfo(maxI - minI + 1, sumFrameTime / objs.Count);
            
            for (int i = 0; i <= maxI - minI; i++) {
                BVHMotion.Frame frame = new BVHMotion.Frame();
                for (int j = 0; j < 18; j++) {
                    for (int k = 0; k < objs.Count; k++) {
                        int x = k == 0? -1: i + minI - timewarps[k-1][0].Item1;
                        float ii = k == 0? i + minI: timewarps[k-1][x].Item2;
                        var objRot = objs[k].Motion.getFrameQuaternion(ii, k);
                        frame.Rotation[j] = Utility.GetQuaternionAvg(objs[k].Motion.motionData[i].Rotation[j], objRot, 1/(k+1));
                    }
                }
                
                float x0=0, z0=0;
                Vector2 thetaVec = new Vector2();
                Vector3 centerPos = new Vector3();
                for (int k = 0; k < objs.Count; k++) {
                    int x = k == 0? -1: i + minI - timewarps[k-1][0].Item1;
                    float ii = k == 0? i + minI: timewarps[k-1][x].Item2;
                    Vector3 pos = objs[k].Motion.getFramePosition(ii);
                    if (k == 0) {
                        centerPos = pos;
                        thetaVec += new Vector2((float)1 / objs.Count, 0);
                    }
                    else {
                        float theta_k = alinements[k-1][x][0], x_k = alinements[k-1][x][1], z_k = alinements[k-1][x][2];
                        pos = Quaternion.Euler(0, theta_k, 0) * (pos + new Vector3(x_k, 0, z_k) - centerPos);
                        frame.Position += pos;

                        thetaVec += Utility.ConvertAngleToVec(-theta_k);
                        x0 += -x_k;
                        z0 += -z_k;
                    }
                }
                frame.Position /= objs.Count;
                var theta = Utility.ConvertVecToAngle(thetaVec / objs.Count);
                frame.Position = Quaternion.Euler(0, theta, 0) * frame.Position;
                frame.Position.x += x0 / objs.Count + centerPos.x;
                frame.Position.z += z0 / objs.Count + centerPos.z;
                blend.Motion.motionData.Add(frame);
            }
            blend.Motion.FitPathCurve(blend);
            blend.Motion.CurveGameObject.transform.parent = blend.transform;
            blend.gameObject.SetActive(true);
            for(int i = 0; i < objs.Count; i++) {
                GameObject.Destroy(objs[i].gameObject);
            }
            return blend;
        }
        public static BVH.BVHObject Do2(List<BVH.BVHObject> objs)
        {
            if (objs.Count == 0) return null;
            else if (objs.Count == 1) return objs[0].Clone();

            float frameTime = 0;
            int frameCount = 0;
            for (int i = 0; i < objs.Count; i++)
            {
                frameTime += objs[i].Motion.FrameTime;
                frameCount += objs[i].Motion.FrameCount;
            }
            frameTime /= objs.Count;
            frameCount /= objs.Count;

            BVH.BVHObject blend = objs[0].Clone(false);
            blend.gameObject.name = "blend";
            blend.Motion = new BVHMotion();
            blend.Motion.ResetMotionInfo(frameCount, frameTime);

            for (int i = 0; i < frameCount; i++)
            {
                BVHMotion.Frame frame = new BVHMotion.Frame();
                for (int j = 0; j < 18; j++)
                {
                    for (int k = 0; k < objs.Count; k++)
                    {
                        float frameIdx = (float)i / frameCount * objs[k].Motion.FrameCount;
                        var angle = objs[k].Motion.getFrameQuaternion(frameIdx, j);
                        frame.Rotation[j] = k == 0 ? angle : Quaternion.Lerp(angle, frame.Rotation[j], 1.0f/k+1);
                        Debug.Log(frame.Rotation[j]);
                    }
                }
                for (int k = 0; k < objs.Count; k++)
                {
                    float frameIdx = (float)i / frameCount * objs[k].Motion.FrameCount;
                    frame.Position += objs[k].Motion.getFramePosition(frameIdx);
                }
                frame.Position /= objs.Count;
                blend.Motion.motionData.Add(frame);
            }
            blend.Motion.FitPathCurve(blend);
            blend.Motion.CurveGameObject.transform.parent = blend.transform;
            blend.gameObject.SetActive(true);
            return blend;
        }

        public static float S(List<Tuple<int, int>> timewarp, int idx1) {
            return (float)timewarp.Where(x => x.Item1 == idx1).Select(x => x.Item2).Average();
        }

        public static Tuple<List<Tuple<int, float>>, List<float[]>> CreateTimeWarp(BVH.BVHObject o1, BVH.BVHObject o2) {
            List<Tuple<int, int>> toO2Frame = new List<Tuple<int, int>>();
            List<float[]> alinement = new List<float[]>();
            int o1Count = o1.Motion.FrameCount;
            int o2Count = o2.Motion.FrameCount;
            Data[,] timewarp = new Data[o1Count, o2Count];
            int i, j, lastI = 0, lastJ = 0;
            for (i = 0; i < o1Count; i++) {
                for (j = 0; j < o2Count; j++) {
                    timewarp[i, j] = Distance(o1, o2, i, j);
                    var left = j > 0 ? timewarp[i, j - 1].SumDistance : 0;
                    var up = i > 0 ? timewarp[i - 1, j].SumDistance : 0;
                    var leftup = i > 0 && j > 0 ? timewarp[i - 1, j - 1].SumDistance : 0;
                    if (i > 0 && j > 0 && leftup >= up && leftup >= left) {
                        timewarp[i, j].PreviousI = i - 1;
                        timewarp[i, j].PreviousJ = j - 1;
                        timewarp[i, j].ContinuesI = 0;
                        timewarp[i, j].ContinuesJ = 0;
                        timewarp[i, j].SumDistance += leftup;
                        if (i >= lastI && j >= lastJ) {
                            lastI = i;
                            lastJ = j;
                        }
                    }
                    else if (j > 0 && left > up && timewarp[i, j].ContinuesI < 3) {
                        timewarp[i, j].PreviousI = i;
                        timewarp[i, j].PreviousJ = j - 1;
                        timewarp[i, j].ContinuesI = 0;
                        timewarp[i, j].ContinuesJ++;
                        timewarp[i, j].SumDistance += left;
                        if (i >= lastI && j >= lastJ) {
                            lastI = i;
                            lastJ = j;
                        }
                    }
                    else if (i > 0 && up > left && timewarp[i, j].ContinuesJ < 3) {
                        timewarp[i, j].PreviousI = i - 1;
                        timewarp[i, j].PreviousJ = j;
                        timewarp[i, j].ContinuesI++;
                        timewarp[i, j].ContinuesJ = 0;
                        timewarp[i, j].SumDistance += up;
                        if (i >= lastI && j >= lastJ) {
                            lastI = i;
                            lastJ = j;
                        }
                    }
                    else{
                        timewarp[i, j].ContinuesI = 0;
                        timewarp[i, j].ContinuesJ = 0;
                        timewarp[i, j].SumDistance = timewarp[i, j].Distance;
                    }
                }
            }
            i = lastI;
            j = lastJ;
            while (i >= 0 && j >= 0)
            {
                alinement.Add(new float[]{timewarp[i, j].Theta, timewarp[i, j].X0, timewarp[i, j].Z0});
                toO2Frame.Add(new Tuple<int, int>(i, j));
                var tmp_timewarp = timewarp[i, j];
                i = tmp_timewarp.PreviousI;
                j = tmp_timewarp.PreviousJ;
            }
            alinement.Reverse();
            toO2Frame.Reverse();
            List<Tuple<int, float>> newtoO2Frame = new List<Tuple<int, float>>();
            List<float[]> newAlinement = new List<float[]>();
            for(i=toO2Frame[0].Item1;i<=toO2Frame[toO2Frame.Count-1].Item1; i++) {
                var idx = toO2Frame.Select((x, i) => new {i, x}).Where(x => x.x.Item1 == i).Select(x=>x.i).ToArray();
                float theta=0, x0=0, z0=0;
                for (j=0;j < idx.Length;j++)
                {
                    theta += alinement[idx[j]][0] / idx.Length;
                    x0 += alinement[idx[j]][1] / idx.Length;
                    z0 += alinement[idx[j]][2] / idx.Length;
                }
                newAlinement.Add(new float[]{theta, x0, z0});
                newtoO2Frame.Add(new Tuple<int, float>(i, S(toO2Frame, i)));
            }
            return new Tuple<List<Tuple<int, float>>, List<float[]>>(newtoO2Frame, newAlinement);
        }

        public class Data{
            public int ContinuesI = 0;
            public int ContinuesJ = 0;
            public int PreviousI = -1;
            public int PreviousJ = -1;
            public float SumDistance = 0;
            public float Distance = 0;
            public float Theta = 0;
            public float X0 = 0;
            public float Z0 = 0;
        }
        public static Data Distance(BVH.BVHObject o1, BVH.BVHObject o2, int o1Idx, int o2Idx) {
            float tan11 = 0, tan12 = 0, tan21 = 0, tan22 = 0;
            float x1 = 0, x2 = 0, z1 = 0, z2 = 0;
            float w = (float)1.0 / 18 / 5;
            Vector3[] vi1 = new Vector3[18], vi2 = new Vector3[18];
            for (int i = 0; i < 5; i++)
            {
                int o1IdxPlusI = Utility.Clip(o1Idx + i - 2, 0, o1.Motion.FrameCount - 1);
                int o2IdxPlusI = Utility.Clip(o2Idx + i - 2, 0, o2.Motion.FrameCount - 1);
                o1.ApplyFrameByIdx(o1IdxPlusI);
                o2.ApplyFrameByIdx(o2IdxPlusI);
                for (int j = 0; j < 18; j++)
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
            for (int i = 0; i < 18; i++) {
                Vector3 v2 = Quaternion.Euler(0, data.Theta, 0) * (vi2[i] + new Vector3(data.X0, 0, data.Z0));
                data.Distance += Vector3.Distance(vi1[i], v2);
            }
            return data;
        }

    }
}