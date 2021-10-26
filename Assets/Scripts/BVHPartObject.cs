using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


namespace BVH {
    class BVHPartObject: MonoBehaviour{
        static Vector3 empty = new Vector3(0, 0, 0);
        public Vector3 Offset = new Vector3();
        public List<BVHPartObject> Child = new List<BVHPartObject>();
        public BVHPartObject Parent = null;
        GameObject myLine;

        public void setPosOrRot(int idx, float num) {
            if (idx == 0) transform.rotation *= Quaternion.Euler(num, 0, 0);
            else if (idx == 1) transform.rotation *= Quaternion.Euler(0, num, 0);
            else if (idx == 2) transform.rotation *= Quaternion.Euler(0, 0, num);

            else if (idx == 3) transform.position += new Vector3(num, 0, 0);
            else if (idx == 4) transform.position += new Vector3(0, num, 0);
            else if (idx == 5) transform.position += new Vector3(0, 0, num);
        }


        public static BVHPartObject CreateGameObject(string name, BVHPartObject parentObject){
            GameObject gobj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            gobj.name = name;
            var obj = gobj.AddComponent<BVHPartObject>();
            if (parentObject){
                parentObject.AddChild(obj);
                obj.myLine = new GameObject();
                obj.myLine.transform.position = parentObject.transform.position;
                LineRenderer lr = obj.myLine.AddComponent<LineRenderer>();
                lr.startWidth = 0.5f;
                lr.endWidth = 0.5f;
                lr.SetPosition(0, obj.gameObject.transform.position);
                lr.SetPosition(1, parentObject.gameObject.transform.position);
            }
            return obj;
        }
        public void AddChild(BVHPartObject childObj){
            Child.Add(childObj);
            childObj.Parent = this;
            childObj.transform.parent = transform;
        }
    
        public void UpdateSingleLine(){
            if(myLine){
                LineRenderer lr = myLine.GetComponent<LineRenderer>();
                lr.SetPosition(0, gameObject.transform.position);
                lr.SetPosition(1, Parent.gameObject.transform.position);
            }
        }
        public void UpdateMutiLines(){
            UpdateSingleLine();
            foreach(var c in Child){
                c.UpdateSingleLine();
            }
        }
    }

    


}