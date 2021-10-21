using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BVH {

    class BVHPartObject: MonoBehaviour{
        static Vector3 empty = new Vector3(0, 0, 0);
        Vector3 offset = new Vector3();
        public Vector3 Offset{
            set { 
                offset = value;
                transform.position = value + position;
                if(transform.parent) {
                    transform.position += transform.parent.position;
                }
                updateLines();
            }
        }
        Vector3 position = empty;
        public Vector3 Position{
            set {
                position = value;
            }
        }
        Vector3 rotation = empty;
        public Vector3 Rotation{
            set { 
                rotation = value;
            }
        }
        public List<BVHPartObject> Child = new List<BVHPartObject>();
        public BVHPartObject Parent = null;
        public BVHPartObject Root = null;
        public List<Tuple<BVHPartObject, int>> ChannelDatas;

        public void setPosOrRot(int idx, float num) {
            if (idx < 3) {
                Vector3 tmp = rotation;
                if (idx == 0) tmp.x=num;
                else if (idx == 1) tmp.y=num;
                else if (idx == 2) tmp.z=num;
                Rotation = tmp;
            }
            else {
                Vector3 tmp = position;
                if (idx == 3) tmp.x=num;
                else if (idx == 4) tmp.y=num;
                else if (idx == 5) tmp.z=num;
                Position = tmp;
            }
        }

        public void update(){
            
            if(position != empty) {
                // value += transform.parent.position;
                var value = position + offset;
                transform.position = value;
            }

            if(rotation != empty) {
                var value = rotation;
                if(gameObject.name=="RightLowLeg")
                Debug.Log(transform.eulerAngles+", "+value);
                if (transform.parent){
                    value += transform.parent.eulerAngles;
                }
                transform.eulerAngles = value;
                if(gameObject.name=="RightLowLeg")
                Debug.Log(transform.eulerAngles);
            }
            foreach(var c in Child){
                c.update();
            }
            updateLine();
        }

        public void applyFrame(int frameIdx) {
            for(int i = 0; i < ChannelDatas.Count; i++){
                var tmp = ChannelDatas[i];
                var partObj = tmp.Item1;
                float num = motionData[frameIdx, i];
                partObj.setPosOrRot(tmp.Item2, num);
                // Debug.Log(tmp.Item2+", "+partObj.rotation);
            }
            update();
        }

        // for motion
        public int frameCount;
        public float frameTime;
        public float[, ] motionData;
        GameObject myLine;
        public static BVHPartObject CreateGameObject(string name, BVHPartObject parentObject){
            GameObject gobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gobj.name = name;
            var obj = gobj.AddComponent<BVHPartObject>();
            if (parentObject){
                parentObject.AddChild(obj);
                obj.myLine = new GameObject();
                obj.myLine.transform.position = parentObject.position;
                LineRenderer lr = obj.myLine.AddComponent<LineRenderer>();
                lr.startWidth = 0.5f;
                lr.endWidth = 0.5f;
                lr.SetPosition(0, obj.gameObject.transform.position);
                lr.SetPosition(1, parentObject.gameObject.transform.position);
            }else{
                obj.Root = obj;
                obj.ChannelDatas = new List<Tuple<BVHPartObject, int>>();
            }
            return obj;
        }
        public void AddChild(BVHPartObject childObj){
            Child.Add(childObj);
            childObj.Parent = this;
            childObj.Root = this.Root;
            childObj.transform.parent = transform;
        }
        public bool IsRoot(){
            return Parent == null;
        }
    
        private void updateLine(){
            if(myLine){
                LineRenderer lr = myLine.GetComponent<LineRenderer>();
                lr.SetPosition(0, gameObject.transform.position);
                lr.SetPosition(1, Parent.gameObject.transform.position);
            }
        }
        private void updateLines(){
            updateLine();
            foreach(var c in Child){
                c.updateLine();
            }
        }
    }

    class Reader {
        
        public static BVHPartObject LoadFile(string filename) {
            string bvhStrData = System.IO.File.ReadAllText(filename);
            return Read(bvhStrData);
        }
        public static BVHPartObject Read(string bvhStrData){
            var bvhDataIter = BVH.Utility.SplitString(bvhStrData).GetEnumerator();
            
            bvhDataIter.MoveNext();
            Utility.IterData.CheckAndNext(ref bvhDataIter, "HIERARCHY");
            Utility.IterData.CompareAndNext(ref bvhDataIter, "ROOT");
            BVHPartObject root = ReadPart(ref bvhDataIter);
            Utility.IterData.CompareAndNext(ref bvhDataIter, "MOTION");
            ReadMotion(ref bvhDataIter, ref root);
            return root;
        }
        
        private static BVHPartObject ReadPart(ref IEnumerator<string> bvhDataIter, BVHPartObject parentObject=null) {
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
                            obj.Root.ChannelDatas.Add(channelData);
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

        private static void ReadMotion(ref IEnumerator<string> bvhDataIter, ref BVHPartObject root) {
            Utility.IterData.CheckAndNext(ref bvhDataIter, "Frames:");
            root.frameCount = int.Parse(Utility.IterData.GetAndNext(ref bvhDataIter));
            Utility.IterData.CheckAndNext(ref bvhDataIter, "Frame");
            Utility.IterData.CheckAndNext(ref bvhDataIter, "Time:");
            root.frameTime = float.Parse(Utility.IterData.GetAndNext(ref bvhDataIter));
            root.motionData = new float[root.frameCount, root.ChannelDatas.Count];
            for (int i = 0; i < root.frameCount; i++) {
                for (int j = 0; j < root.ChannelDatas.Count; j++) {
                    float num = float.Parse(Utility.IterData.GetAndNext(ref bvhDataIter));
                    root.motionData[i, j] = num;
                }
            }
        }
    }

}

