using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Assertions;


namespace BVH
{
    public class BVHObject : MonoBehaviour
    {
        public BVHPartObject Root;
        public BVHMotion Motion;
        public List<Tuple<BVHPartObject, int>> ChannelDatas = new List<Tuple<BVHPartObject, int>>();
        public BVHPartObject[] Part;
        public static GameObject CreateBVHObject(string filename)
        {
            GameObject gameObject = new GameObject();
            var filenameArr = filename.Split('\\');
            gameObject.name = filenameArr[filenameArr.Length - 1].Split('.')[0];
            var bvhObject = gameObject.AddComponent<BVHObject>();
            bvhObject.LoadFile(filename);
            return gameObject;
        }
        public void LoadFile(string filename)
        {
            string bvhStrData = System.IO.File.ReadAllText(filename);
            Read(bvhStrData);
        }
        public BVHObject Clone(bool isMotion = true)
        {
            GameObject newObj = new GameObject();
            newObj.name = name;
            var newBVH = newObj.AddComponent<BVHObject>();
            newBVH.Root = Root.Clone();
            newBVH.Root.transform.parent = newObj.transform;
            newBVH.RenamePart();
            if (isMotion)
            {
                newBVH.Motion = Motion.Clone();
                newBVH.Motion.CurveGameObject.transform.parent = newObj.transform;
            }
            // newBVH.time = time;
            foreach (var data in ChannelDatas)
            {
                var part = newBVH.Part[Utility.GetPartIdxByName(data.Item1.name)];
                newBVH.ChannelDatas.Add(new Tuple<BVHPartObject, int>(part, data.Item2));
            }
            return newBVH;
        }

        public void ResetChannel()
        {
            ChannelDatas = new List<Tuple<BVHPartObject, int>>();
            ChannelDatas.Add(new Tuple<BVHPartObject, int>(Root, 3));
            ChannelDatas.Add(new Tuple<BVHPartObject, int>(Root, 4));
            ChannelDatas.Add(new Tuple<BVHPartObject, int>(Root, 5));
            for (int i = 0; i < 18; i++)
            {
                ChannelDatas.Add(new Tuple<BVHPartObject, int>(Part[i], 2));
                ChannelDatas.Add(new Tuple<BVHPartObject, int>(Part[i], 0));
                ChannelDatas.Add(new Tuple<BVHPartObject, int>(Part[i], 1));
            }
        }
        public void Read(string bvhStrData)
        {
            var bvhDataIter = BVH.Utility.SplitString(bvhStrData).GetEnumerator();

            bvhDataIter.MoveNext();
            Utility.IterData.CheckAndNext(ref bvhDataIter, "HIERARCHY");
            Utility.IterData.CompareAndNext(ref bvhDataIter, "ROOT");
            Root = BVHPartObject.ReadPart(ref bvhDataIter, this);
            Root.transform.parent = transform;
            RenamePart();
            Utility.IterData.CompareAndNext(ref bvhDataIter, "MOTION");
            Motion = BVHMotion.readMotion(ref bvhDataIter, this);
            Motion.FitPathCurve(this);
            Motion.CurveGameObject.transform.parent = transform;
        }
        public void UpdateLines(BVHPartObject partObject = null)
        {
            if (partObject == null) partObject = Root;
            foreach (var childObj in partObject.Child)
            {
                UpdateLines(childObj);
            }
            partObject.UpdateSingleLine();
        }
        public void ApplyFrame(BVHMotion.Frame frame)
        {
            Motion.ApplyFrame(frame, this);
        }
        public void ApplyFrameByIdx(float frameIdx)
        {
            Motion.ApplyFrame(frameIdx, this);
        }
        public void ApplyFrame(float time)
        {
            //time += deltaTime;
            float frameIdx = time / Motion.FrameTime;
            float frame = frameIdx - ((int)(frameIdx / Motion.FrameCount)) * Motion.FrameCount;
            Motion.ApplyFrame(frame, this);
        }

        public void RenamePart()
        {
            Part = new BVHPartObject[] {
                null, null, null, null, null, null, null, null, null,
                null, null, null, null, null, null, null, null, null
            };
            Root.name = "Hips";
            Part[0] = Root;
            bool chest = false, leftLeg = false, rightLeg = false;
            Assert.IsTrue(Root.Child.Count == 3);
            foreach (var part in Root.Child)
            {
                if (!chest && part.Offset.y > 0 && part.Offset.x <= 0)
                {
                    chest = true;
                    part.name = "Chest";
                    Part[1] = part;
                    Assert.IsTrue(part.Child.Count == 3);
                    bool leftCollar = false, rightCollar = false, neck = false;
                    foreach (var part2 in part.Child)
                    {
                        if (!neck && part2.Offset.x == 0)
                        {
                            neck = true;
                            part2.name = "Neck";
                            Part[2] = part2;
                            Assert.IsTrue(part2.Child.Count == 1);
                            part2.Child[0].name = "Head";
                            Part[3] = part2.Child[0];
                            Assert.IsTrue(part2.Child[0].Child.Count == 1);
                            Assert.IsTrue(part2.Child[0].Child[0].name == "End");
                        }
                        else if (!leftCollar && part2.Offset.x > 0)
                        {
                            leftCollar = true;
                            part2.name = "LeftCollar";
                            Part[4] = part2;
                            Assert.IsTrue(part2.Child.Count == 1);
                            part2.Child[0].name = "LeftUpArm";
                            Part[5] = part2.Child[0];
                            Assert.IsTrue(part2.Child[0].Child.Count == 1);
                            part2.Child[0].Child[0].name = "LeftLowArm";
                            Part[6] = part2.Child[0].Child[0];
                            Assert.IsTrue(part2.Child[0].Child[0].Child.Count == 1);
                            part2.Child[0].Child[0].Child[0].name = "LeftHand";
                            Part[7] = part2.Child[0].Child[0].Child[0];
                            Assert.IsTrue(part2.Child[0].Child[0].Child[0].Child.Count == 1);
                            Assert.IsTrue(part2.Child[0].Child[0].Child[0].Child[0].name == "End");
                        }
                        else if (!rightCollar && part2.Offset.x < 0)
                        {
                            rightCollar = true;
                            part2.name = "RightCollar";
                            Part[8] = part2;
                            Assert.IsTrue(part2.Child.Count == 1);
                            part2.Child[0].name = "RightUpArm";
                            Part[9] = part2.Child[0];
                            Assert.IsTrue(part2.Child[0].Child.Count == 1);
                            part2.Child[0].Child[0].name = "RightLowArm";
                            Part[10] = part2.Child[0].Child[0];
                            Assert.IsTrue(part2.Child[0].Child[0].Child.Count == 1);
                            part2.Child[0].Child[0].Child[0].name = "RightHand";
                            Part[11] = part2.Child[0].Child[0].Child[0];
                            Assert.IsTrue(part2.Child[0].Child[0].Child[0].Child.Count == 1);
                            Assert.IsTrue(part2.Child[0].Child[0].Child[0].Child[0].name == "End");
                        }
                        else
                        {
                            Assert.IsTrue(false);
                        }
                    }
                }
                else if (!leftLeg && part.Offset.x > 0)
                {
                    leftLeg = true;
                    part.name = "LeftUpLeg";
                    Part[12] = part;
                    Assert.IsTrue(part.Child.Count == 1);
                    part.Child[0].name = "LeftLowLeg";
                    Part[13] = part.Child[0];
                    Assert.IsTrue(part.Child[0].Child.Count == 1);
                    part.Child[0].Child[0].name = "LeftFoot";
                    Part[14] = part.Child[0].Child[0];
                    Assert.IsTrue(part.Child[0].Child[0].Child.Count == 1);
                    Assert.IsTrue(part.Child[0].Child[0].Child[0].name == "End");
                }
                else if (!rightLeg && part.Offset.x < 0)
                {
                    rightLeg = true;
                    part.name = "RightUpLeg";
                    Part[15] = part;
                    Assert.IsTrue(part.Child.Count == 1);
                    part.Child[0].name = "RightLowLeg";
                    Part[16] = part.Child[0];
                    Assert.IsTrue(part.Child[0].Child.Count == 1);
                    part.Child[0].Child[0].name = "RightFoot";
                    Part[17] = part.Child[0].Child[0];
                    Assert.IsTrue(part.Child[0].Child[0].Child.Count == 1);
                    Assert.IsTrue(part.Child[0].Child[0].Child[0].name == "End");
                }
                else
                {
                    Assert.IsTrue(false);
                }
            }
            for (int i = 0; i < 18; i++)
            {
                Part[i].PartIdx = i;
            }
        }

        public void RenamePartCMU()
        {
            Part = new BVHPartObject[31];
            List<BVHPartObject> bfs = new List<BVHPartObject>();
            bfs.Add(Root);
            int queueIdx = 0;
            while (queueIdx < bfs.Count)
            {
                var cur = bfs[queueIdx++];
                int idx = BVH.Utility.CMUMotion.GetPartIdxByNameCMU(cur.name);
                if (idx >= 0)
                {
                    Part[idx] = cur;
                }
                else
                {
                    Debug.LogError(cur.name + "找不到對應部位");
                    Assert.IsTrue(false);
                }
            }
        }
        public void SetLineMaterial(Material material)
        {
            for (int i = 0; i < Part.Length; i++) {
                var line = Part[i].gameObject.GetComponent<LineRenderer>();
                if (line) {
                    line.material = material;
                    line.startWidth = 1f;
                    line.endWidth = 0.5f;
                }
            }
        }
        public BVHMotion.Frame getFrame(float frameIdx)
        {
            BVHMotion.Frame frame = new BVHMotion.Frame(Part.Length);
            frame.Position = Motion.getFramePosition(frameIdx);
            for (int i = 0; i < Part.Length; i++)
            {
                frame.Rotation[i] = Motion.getFrameQuaternion(frameIdx, i);
            }
            return frame;
        }
        void Update()
        {
            if (Root && gameObject.activeSelf)
            {
                ApplyFrame(Time.time);
            }
        }

    }

}