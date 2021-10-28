using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

namespace BVH {
    class Blend{
        public static BVH.BVHObject Do(List<BVH.BVHObject> objs) {
            if(objs.Count == 0) return null;
            else if(objs.Count == 1) return objs[0].Clone();
            for(int i = 0; i < objs.Count; i++) {
                string name = objs[i].name + "_tmp";
                objs[i] = objs[i].Clone();
                objs[i].name = name;
                objs[i].gameObject.SetActive(false);
            }
            BVH.BVHObject reference = objs[0];
            List<List<Tuple<int, int>>> timewarps = new List<List<Tuple<int, int>>>();
            int minI = reference.Motion.FrameCount, maxI = 0;
            float sumFrameTime = reference.Motion.FrameTime;
            for(int i = 1; i < objs.Count; i++) {
               var timewarp = CreateTimeWarp(reference, objs[i]);
               timewarps.Add(timewarp);
               if (timewarp[0].Item1 < minI) minI = timewarp[0].Item1;
               if (timewarp[timewarp.Count-1].Item1 > maxI) maxI = timewarp[timewarp.Count-1].Item1;
               sumFrameTime += objs[i].Motion.FrameTime;
            }
            BVH.BVHObject blend = reference.Clone();
            blend.gameObject.name = "blend";
            blend.ResetChannel();
            int[, ] pareChxIdx = new int[objs.Count, blend.ChannelDatas.Count];
            for (int i = 0; i < blend.ChannelDatas.Count; i++) {
                pareChxIdx[0, i] = i;
                for (int j = 1; j < objs.Count; j++) {
                    for (int k = 0; k < blend.ChannelDatas.Count; k++) {
                        if(blend.ChannelDatas[i].Item2 == objs[j].ChannelDatas[k].Item2) {
                            if(blend.ChannelDatas[i].Item1.name == objs[j].ChannelDatas[k].Item1.name) {
                                pareChxIdx[j, i] = k;
                                break;
                            }
                        }
                    }
                }
            }
            blend.Motion.ResetMotionInfo(maxI - minI + 1, sumFrameTime / objs.Count);
            for (int i = 0; i <= maxI - minI; i++) {
                for (int j = 0; j < blend.ChannelDatas.Count; j++) {
                    int dataType = blend.ChannelDatas[j].Item2;
                    if (dataType < 3){
                        Vector2 sum = new Vector2();
                        for (int k = 1; k < objs.Count; k++) {
                            float ii = S(timewarps[k-1], i+minI);
                            int idx = pareChxIdx[k, j];
                            var partObj = objs[k].ChannelDatas[idx].Item1;
                            float angle = objs[k].Motion.getMotion(ii, idx, objs[k].ChannelDatas[idx]);
                            sum += Utility.ConvertAngleToVec(angle);
                        }
                        blend.Motion.motionData[0, j] = Utility.ConvertVecToAngle(sum / objs.Count);
                    }
                    else{
                        float sum = 0;
                        for (int k = 1; k < objs.Count; k++) {
                            float ii = S(timewarps[k-1], i+minI);
                            int idx = pareChxIdx[k, j];
                            var partObj = objs[k].ChannelDatas[idx].Item1;
                            float angle = objs[k].Motion.getMotion(ii, idx, objs[k].ChannelDatas[idx]);
                            sum += objs[k].Motion.getMotion(ii, idx, objs[k].ChannelDatas[idx]);
                        }
                        blend.Motion.motionData[0, j] = sum / objs.Count;
                    }
                }
            }
            blend.ApplyFrameByIdx(0);
            blend.gameObject.SetActive(true);
            return blend;
        }

        public static float S(List<Tuple<int, int>> timewarp, int idx1) {
            return (float)timewarp.Where(x => x.Item1 == idx1).Select(x => x.Item2).Average();
        }
        public static List<Tuple<int, int>> CreateTimeWarp(BVH.BVHObject o1, BVH.BVHObject o2) {
            List<Tuple<int, int>> toO2Frame = new List<Tuple<int, int>>();
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
                toO2Frame.Add(new Tuple<int, int>(i, j));
                var tmp_timewarp = timewarp[i, j];
                i = tmp_timewarp.PreviousI;
                j = tmp_timewarp.PreviousJ;
            }
            toO2Frame.Reverse();
            return toO2Frame;
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