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
            
            // for(int i = 1; i < objs.Count; i++) {
            //     Assert.IsTrue(objs[0].ChannelDatas.Count == objs[i].ChannelDatas.Count);
            //     for(int j = 0; j < objs[0].ChannelDatas.Count; j++) {
            //         Assert.IsTrue(objs[0].ChannelDatas[j].Item1.name == objs[i].ChannelDatas[j].Item1.name);
            //         Assert.IsTrue(objs[0].ChannelDatas[j].Item2 == objs[i].ChannelDatas[j].Item2);
            //     }
            // }
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
            blend.ResetChannel();
            blend.Motion = new BVHMotion();
            blend.Motion.ResetMotionInfo(maxI - minI + 1, sumFrameTime / objs.Count);
            int[, ] pareChxIdx = new int[objs.Count, blend.ChannelDatas.Count];
            for (int i = 0; i < blend.ChannelDatas.Count; i++) {
                pareChxIdx[0, i] = i;
                for (int j = 1; j < objs.Count; j++) {
                    for (int k = 0; k < blend.ChannelDatas.Count; k++) {
                        if(blend.ChannelDatas[i].Item1.name == objs[j].ChannelDatas[k].Item1.name) {
                            if(blend.ChannelDatas[i].Item2 == objs[j].ChannelDatas[k].Item2) {
                                pareChxIdx[j, i] = k;
                                break;
                            }
                        }
                    }
                }
            }
            // for (int i = 0; i <= maxI - minI; i++) {
            //     float sumX=0, sumZ=0;
            //     int xIdx=-1, zIdx=-1;
            //     for (int j = 0; j < blend.ChannelDatas.Count; j++) {
            //         int dataType = blend.ChannelDatas[j].Item2;
            //         if (dataType < 3){
            //             Vector2 sum = new Vector2();
            //             for (int k = 0; k < objs.Count; k++) {
            //                 int x = k == 0? -1: i + minI - timewarps[k-1][0].Item1;
            //                 float ii = k == 0? i + minI: timewarps[k-1][x].Item2;
            //                 int idx = pareChxIdx[k, j];
            //                 var partObj = objs[k].ChannelDatas[idx].Item1;
            //                 float angle = objs[k].Motion.getMotion(ii, idx, objs[k].ChannelDatas[idx]);
            //                 sum += Utility.ConvertAngleToVec(angle);
            //             }
            //             blend.Motion.motionData[i, j] = Utility.ConvertVecToAngle(sum / objs.Count);
            //         }
            //         else{
            //             float sum = 0;
            //             for (int k = 0; k < objs.Count; k++) {
            //                 int x = k == 0? -1: i + minI - timewarps[k-1][0].Item1;
            //                 float ii = k == 0? i + minI: timewarps[k-1][x].Item2;
            //                 int idx = pareChxIdx[k, j];
            //                 var partObj = objs[k].ChannelDatas[idx].Item1;
            //                 float pos = objs[k].Motion.getMotion(ii, idx, objs[k].ChannelDatas[idx]);
            //                 if (k > 0 && dataType == 3) {
            //                     sumX += pos * Mathf.Cos(alinements[k-1][x][0]);
            //                     sumZ += pos * Mathf.Sin(alinements[k-1][x][0]);
            //                     sumX += alinements[k-1][x][1];
            //                     xIdx = j;
            //                 }
            //                 else if (k > 0 && dataType == 5) {
            //                     sumX += pos * Mathf.Sin(alinements[k-1][x][0]);
            //                     sumZ += pos * Mathf.Cos(alinements[k-1][x][0]);
            //                     sumZ += alinements[k-1][x][2];
            //                     zIdx = j;
            //                 }
            //                 else sum += pos;
            //             }
            //             blend.Motion.motionData[i, j] += sum / objs.Count;
            //         }
            //     }
            //     if(xIdx >= 0) blend.Motion.motionData[i, xIdx] += sumX / objs.Count;
            //     if(zIdx >= 0) blend.Motion.motionData[i, zIdx] += sumZ / objs.Count;
            //     Debug.Log(blend.Motion.motionData[i, xIdx]);
            //     float x0=0, z0=0;
            //     Vector2 thetaVec = new Vector2();
            //     for (int k = 0; k < objs.Count; k++) {
            //         if (k == 0) {
            //             thetaVec += new Vector2((float)1 / objs.Count, 0);
            //         }
            //         else{
            //             int x = i + minI - timewarps[k-1][0].Item1;
            //             thetaVec += Utility.ConvertAngleToVec(-alinements[k-1][x][0]) / objs.Count;
            //             x0 += -alinements[k-1][x][1] / objs.Count;
            //             z0 += -alinements[k-1][x][2] / objs.Count;
            //         }
            //     }
            //     Debug.Log(blend.Motion.motionData[i, xIdx]);
            //     if(zIdx >= 0) 
            //         blend.Motion.motionData[i, xIdx] += x0;
            //     if(zIdx >= 0) 
            //         blend.Motion.motionData[i, zIdx] += z0;
            //     var theta = Utility.ConvertVecToAngle(thetaVec);
            //     float tmpX = xIdx >= 0 ? blend.Motion.motionData[i, xIdx]: 0;
            //     float tmpZ = zIdx >= 0 ? blend.Motion.motionData[i, zIdx]: 0;
            //     Debug.Log(blend.Motion.motionData[i, xIdx]);
            //     if(zIdx >= 0) 
            //         blend.Motion.motionData[i, xIdx] += tmpX * Mathf.Cos(theta) + tmpZ * Mathf.Sin(theta);
            //     if(zIdx >= 0) 
            //         blend.Motion.motionData[i, zIdx] += tmpX * Mathf.Sin(theta) + tmpZ * Mathf.Cos(theta);
            //     Debug.Log(blend.Motion.motionData[i, xIdx]);
            // }
            blend.Motion.FitPathCurve(blend);
            blend.Motion.CurveGameObject.transform.parent = blend.transform;
            blend.gameObject.SetActive(true);
            for(int i = 0; i < objs.Count; i++) {
                GameObject.Destroy(objs[i].gameObject);
            }
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
                o1.ApplyFrameByIdx(i);
                for (j = 0; j < o2Count; j++) {
                    o2.ApplyFrameByIdx(j);
                    timewarp[i, j] = Distance(o1.Part, o2.Part);
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
            while (i >= 0 && j >= 0){
                alinement.Add(new float[]{timewarp[i, j].Theta, timewarp[i, j].X0, timewarp[i, j].Z0});
                toO2Frame.Add(new Tuple<int, int>(i, j));
                var tmp_timewarp = timewarp[i, j];
                i = tmp_timewarp.PreviousI;
                j = tmp_timewarp.PreviousJ;
            }
            toO2Frame.Reverse();
            List<Tuple<int, float>> newtoO2Frame = new List<Tuple<int, float>>();
            List<float[]> newAlinement = new List<float[]>();
            for(i=toO2Frame[0].Item1;i<=toO2Frame[toO2Frame.Count-1].Item1; i++) {
                var idx = toO2Frame.Select((x, i) => new {i, x}).Where(x => x.x.Item1 == x.i).Select(x=>x.i).ToArray();
                float theta=0, x0=0, z0=0;
                for(j=0;j < idx.Length;j++){
                    theta += alinement[j][0] / idx.Length;
                    x0 += alinement[j][1] / idx.Length;
                    z0 += alinement[j][2] / idx.Length;
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
        public static Data Distance(BVH.BVHPartObject[] r1, BVH.BVHPartObject[] r2) {
            float tan11 = 0, tan12 = 0, tan21 = 0, tan22 = 0;
            float x1 = 0, x2 = 0, z1 = 0, z2 = 0;
            float w = (float)1.0 / 18;
            for (int i = 0; i < 18; i++) {
                Vector3 v1 = r1[i].transform.position;
                Vector3 v2 = r2[i].transform.position;
                tan11 += w * (v1.x * v2.z - v2.x * v1.z);
                tan21 += w * (v1.x * v2.x - v1.z * v2.z);
                x1 += w * v1.x;
                x2 += w * v2.x;
                z1 += w * v1.z;
                z2 += w * v2.z;
            }
            tan12 = x1 * z2 - x2 * z1;
            tan22 = x1 * x2 - z1 * z2;
            Data data = new Data();
            data.Theta = Mathf.Atan((tan11 - tan12) / (tan21 - tan22));
            data.X0 = x1 - x2 * Mathf.Cos(data.Theta) - z2 * Mathf.Sin(data.Theta);
            data.Z0 = z1 - x2 * Mathf.Sin(data.Theta) - z2 * Mathf.Cos(data.Theta);
            for (int i = 0; i < 18; i++) {
                Vector3 v1 = r1[i].transform.position;
                Vector3 v2 = r2[i].transform.position;
                float tmpX = v2.x, tmpZ = v2.z;
                v2.x = tmpX * Mathf.Cos(data.Theta) + tmpZ * Mathf.Sin(data.Theta);
                v2.z = tmpX * Mathf.Sin(data.Theta) + tmpZ * Mathf.Cos(data.Theta);
                v2.x += data.X0;
                v2.z += data.Z0;
                data.Distance += Vector3.Distance(v1, v2);
            }
            return data;
        }

    }
}