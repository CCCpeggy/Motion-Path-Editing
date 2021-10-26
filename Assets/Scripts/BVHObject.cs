using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BVH {
    class BVHObject{
        BVHPartObject Root;
        BVHMotion Motion;
        float time = 0;
        
        public List<Tuple<BVHPartObject, int>> ChannelDatas = new List<Tuple<BVHPartObject, int>>();

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
            Root = BVHPartObject.ReadPart(ref bvhDataIter, this);
            Utility.IterData.CompareAndNext(ref bvhDataIter, "MOTION");
            Motion = BVHMotion.readMotion(ref bvhDataIter, this);
            Motion.FitPathCurve();
        }
        
        public void UpdateLines(BVHPartObject partObject=null){
            if (partObject == null) partObject = Root;
            foreach(var childObj in partObject.Child){
                UpdateLines(childObj);
            }
            partObject.UpdateSingleLine();
        }

        public void ApplyFrame(float deltaTime) {
            time += deltaTime;
            float frameTime = time / Motion.FrameTime;
            float frame = frameTime - ((int)(frameTime / Motion.FrameCount)) * Motion.FrameCount;
            Motion.ApplyFrame(frame, this);
        }
    }

}