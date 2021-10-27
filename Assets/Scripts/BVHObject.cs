using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


namespace BVH {
    public class BVHObject: MonoBehaviour{
        public BVHPartObject Root;
        BVHMotion Motion;
        float time = 0;
        public List<Tuple<BVHPartObject, int>> ChannelDatas = new List<Tuple<BVHPartObject, int>>();

        public static GameObject CreateBVHObject(string filename) {
            GameObject gameObject = new GameObject();
            var filenameArr = filename.Split('\\');
            gameObject.name = filenameArr[filenameArr.Length - 1].Split('.')[0];
            var bvhObject = gameObject.AddComponent<BVHObject>();
            bvhObject.LoadFile(filename);
            return gameObject;
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
            Root.transform.parent = transform;
            RenamePart();
            Utility.IterData.CompareAndNext(ref bvhDataIter, "MOTION");
            Motion = BVHMotion.readMotion(ref bvhDataIter, this);
            Motion.CurveGameObject.transform.parent = transform;
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

        public void RenamePart() {
            Root.name = "Hips";
            bool chest = false, leftLeg = false, rightLeg = false;
            Assert.IsTrue(Root.Child.Count == 3);
            foreach(var part in Root.Child) {
                if (!chest && part.Offset.y > 0 && part.Offset.x <= 0){
                    chest = true;
                    part.name = "Chest";
                    Assert.IsTrue(part.Child.Count == 3);
                    bool leftCollar = false, rightCollar = false, neck = false;
                    foreach(var part2 in part.Child) {
                        if (!neck && part2.Offset.x == 0 ) {
                            neck = true;
                            part2.name = "Neck";
                            Assert.IsTrue(part2.Child.Count == 1);
                            part2.Child[0].name = "Head";
                            Assert.IsTrue(part2.Child[0].Child.Count == 1);
                            Assert.IsTrue(part2.Child[0].Child[0].name == "End");
                        }
                        else if (!leftCollar && part2.Offset.x > 0){
                            leftCollar = true;
                            part2.name = "LeftCollar";
                            Assert.IsTrue(part2.Child.Count == 1);
                            part2.Child[0].name = "LeftUpArm";
                            Assert.IsTrue(part2.Child[0].Child.Count == 1);
                            part2.Child[0].Child[0].name = "LeftLowArm";
                            Assert.IsTrue(part2.Child[0].Child[0].Child.Count == 1);
                            part2.Child[0].Child[0].Child[0].name = "LeftHand";
                            Assert.IsTrue(part2.Child[0].Child[0].Child[0].Child.Count == 1);
                            Assert.IsTrue(part2.Child[0].Child[0].Child[0].Child[0].name == "End");
                        }
                        else if (!rightCollar && part2.Offset.x < 0){
                            rightCollar = true;
                            part2.name = "RightCollar";
                            Assert.IsTrue(part2.Child.Count == 1);
                            part2.Child[0].name = "RightUpArm";
                            Assert.IsTrue(part2.Child[0].Child.Count == 1);
                            part2.Child[0].Child[0].name = "RightLowArm";
                            Assert.IsTrue(part2.Child[0].Child[0].Child.Count == 1);
                            part2.Child[0].Child[0].Child[0].name = "RightHand";
                            Assert.IsTrue(part2.Child[0].Child[0].Child[0].Child.Count == 1);
                            Assert.IsTrue(part2.Child[0].Child[0].Child[0].Child[0].name == "End");
                        }
                        else{
                            Assert.IsTrue(false);
                        }
                    }
                }
                else if (!leftLeg && part.Offset.x > 0){
                    leftLeg = true;
                    part.name = "LeftUpLeg";
                    Assert.IsTrue(part.Child.Count == 1);
                    part.Child[0].name = "LeftLowLeg";
                    Assert.IsTrue(part.Child[0].Child.Count == 1);
                    part.Child[0].Child[0].name = "LeftFoot";
                    Assert.IsTrue(part.Child[0].Child[0].Child.Count == 1);
                    Assert.IsTrue(part.Child[0].Child[0].Child[0].name == "End");
                }
                else if (!rightLeg && part.Offset.x < 0){
                    rightLeg = true;
                    part.name = "RightUpLeg";
                    Assert.IsTrue(part.Child.Count == 1);
                    part.Child[0].name = "RightLowLeg";
                    Assert.IsTrue(part.Child[0].Child.Count == 1);
                    part.Child[0].Child[0].name = "RightFoot";
                    Assert.IsTrue(part.Child[0].Child[0].Child.Count == 1);
                    Assert.IsTrue(part.Child[0].Child[0].Child[0].name == "End");
                }
                else{
                    Assert.IsTrue(false);
                }
            }
        }
        void Update()
        {
            if(Root){
                ApplyFrame(Time.deltaTime);
            }
        }

    }

}