using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BVH {
    class BVHObject{
        BVHPartObject Root;
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
        private List<Tuple<BVHPartObject, int>> channelDatas = new List<Tuple<BVHPartObject, int>>();
        public List<Tuple<BVHPartObject, int>> ChannelDatas{
            get{
                return channelDatas;
            }
        }
        public BVHObject(string filename){
            LoadFile(filename);
        }
        public void LoadFile(string filename) {
            string bvhStrData = System.IO.File.ReadAllText(filename);
            Read(bvhStrData);
        }
        public void Read(string bvhStrData){
            var bvhDataIter = BVH.Utility.SplitString(bvhStrData).GetEnumerator();
            
            bvhDataIter.MoveNext();
            Utility.IterData.CheckAndNext(ref bvhDataIter, "HIERARCHY");
            Utility.IterData.CompareAndNext(ref bvhDataIter, "ROOT");
            Root = ReadPart(ref bvhDataIter);
            Utility.IterData.CompareAndNext(ref bvhDataIter, "MOTION");
            ReadMotion(ref bvhDataIter);
        }
        public BVHPartObject ReadPart(ref IEnumerator<string> bvhDataIter, BVHPartObject parentObject=null) {
            string partName = Utility.IterData.GetAndNext(ref bvhDataIter);
            BVHPartObject obj = BVHPartObject.CreateGameObject(partName, parentObject);
            Utility.IterData.CheckAndNext(ref bvhDataIter, "{");
            while (true){
                switch(Utility.IterData.GetAndNext(ref bvhDataIter)) {
                    case "OFFSET":
                        obj.Offset = Utility.IterData.GetVec3AndNext(ref bvhDataIter);
                        break;
                    case "CHANNELS":
                        int channelAmount = int.Parse(Utility.IterData.GetAndNext(ref bvhDataIter));
                        for (int i = 0; i < channelAmount; i++) {
                            string channelName = Utility.IterData.GetAndNext(ref bvhDataIter);
                            int idx = 0;
                            switch(channelName.Substring(1)) {
                                case "rotation":
                                    idx = 0;
                                    break;
                                case "position":
                                    idx = 3;
                                    break;
                                default:
                                    Debug.LogError("非預期輸入");
                                    break;
                            }
                            switch(channelName[0]) {
                                case 'X':
                                    idx += 0;
                                    break;
                                case 'Y':
                                    idx += 1;
                                    break;
                                case 'Z':
                                    idx += 2;
                                    break;
                                default:
                                    Debug.LogError("非預期輸入");
                                    break;
                            }
                            var channelData = new Tuple<BVHPartObject, int>(obj, idx);
                            channelDatas.Add(channelData);
                        }
                        break;
                    case "JOINT":
                        var childObj = ReadPart(ref bvhDataIter, obj);
                        break;
                    case "End":
                        var endObj = BVHPartObject.CreateGameObject("end", obj);
                        Utility.IterData.CheckAndNext(ref bvhDataIter, "Site");
                        Utility.IterData.CheckAndNext(ref bvhDataIter, "{");
                        Utility.IterData.CheckAndNext(ref bvhDataIter, "OFFSET");
                        endObj.Offset = Utility.IterData.GetVec3AndNext(ref bvhDataIter);
                        Utility.IterData.CheckAndNext(ref bvhDataIter, "}");
                        break;
                    case "}":
                        return obj;
                    default:
                        Debug.LogError("非預期輸入");
                        return null;
                }
            }
        }
        public void ReadMotion(ref IEnumerator<string> bvhDataIter) {
            Utility.IterData.CheckAndNext(ref bvhDataIter, "Frames:");
            frameCount = int.Parse(Utility.IterData.GetAndNext(ref bvhDataIter));
            Utility.IterData.CheckAndNext(ref bvhDataIter, "Frame");
            Utility.IterData.CheckAndNext(ref bvhDataIter, "Time:");
            frameTime = float.Parse(Utility.IterData.GetAndNext(ref bvhDataIter));
            motionData = new float[frameCount, channelDatas.Count];
            for (int i = 0; i < frameCount; i++) {
                for (int j = 0; j < channelDatas.Count; j++) {
                    float num = float.Parse(Utility.IterData.GetAndNext(ref bvhDataIter));
                    motionData[i, j] = num;
                }
            }
        }
        public void ApplyFrame(float frameIdx) {
            BVHPartObject lastObj = null;
            for(int i = 0; i < channelDatas.Count; i++){
                var partObj = channelDatas[i].Item1;
                int posAndRotIdx = channelDatas[i].Item2;
                if (lastObj != partObj) {
                    partObj.transform.localPosition = Vector3.zero;
                    partObj.transform.localRotation = Quaternion.Euler(0, 0, 0);
                    partObj.transform.localPosition += partObj.Offset;
                    lastObj = partObj;
                }
                float lastMotionValue = motionData[(int)frameIdx, i];
                float nextMotionValue = motionData[((int)frameIdx+1)%frameCount, i];
                float alpha = frameIdx - (int)frameIdx;
                float thisMotionValue = lastMotionValue * (1-alpha) + nextMotionValue * alpha;

                partObj.setPosOrRot(posAndRotIdx, thisMotionValue);
            }
            updateLines();
        }
        private void updateLines(BVHPartObject partObject=null){
            if (partObject == null) partObject = Root;
            foreach(var childObj in partObject.Child){
                updateLines(childObj);
            }
            partObject.UpdateSingleLine();
        }
    }

}