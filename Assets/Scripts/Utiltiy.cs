using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BVH {

    class Utility {
        public static int GetPartIdxByName(string name){
            
            switch(name){
                case "Hips":
                    return 0;
                case "Chest":
                    return 1;
                case "Neck":
                    return 2;
                case "Head":
                    return 3;
                case "LeftCollar":
                    return 4;
                case "LeftUpArm":
                    return 5;
                case "LeftLowArm":
                    return 6;
                case "LeftHand":
                    return 7;
                case "RightCollar":
                    return 8;
                case "RightUpArm":
                    return 9;
                case "RightLowArm":
                    return 10;
                case "RightHand":
                    return 11;
                case "LeftUpLeg":
                    return 12;
                case "LeftLowLeg":
                    return 13;
                case "LeftFoot":
                    return 14;
                case "RightUpLeg":
                    return 15;
                case "RightLowLeg":
                    return 16;
                case "RightFoot":
                    return 17;
                default:
                    return -1;
            }
        }
        public static IEnumerable<string> SplitString(string data)
        {
            var components = data.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
            foreach(var c in components) {
                yield return c;
            }
        }

        public static Vector2 ConvertAngleToVec(float a) {
            return new Vector2(Mathf.Cos(a * Mathf.Deg2Rad), Mathf.Sin(a * Mathf.Deg2Rad));
        }
        public static float ConvertVecToAngle(Vector2 v) {
            float angle = Vector2.Angle(v, new Vector2(1, 0));
            if (v.y < 0) angle = -angle;
            return angle;
        }

        public static float GetAngleAvg(float a1, float a2, float alpha) {
            Vector2 v1 = new Vector2(Mathf.Cos(a1 * Mathf.Deg2Rad), Mathf.Sin(a1 * Mathf.Deg2Rad));
            Vector2 v2 = new Vector2(Mathf.Cos(a2 * Mathf.Deg2Rad), Mathf.Sin(a2 * Mathf.Deg2Rad));
            Vector2 v3 = v1 * (1-alpha) + v2 * alpha;
            float angle = Vector2.Angle(v3, new Vector2(1, 0));
            if (v3.y < 0) angle = -angle;
            return angle;
        }

        public class IterData {

            public static string GetAndNext(ref IEnumerator<string> iter) {
                string tmp = iter.Current;
                iter.MoveNext();
                return tmp;
            }

            public static void CheckAndNext(ref IEnumerator<string> iter, string str) {
                if (iter.Current != str) {
                    Debug.LogError("預期是 " + str + "，但解析到的是 " + iter.Current);
                }
                // Debug.Assert(iter.Current == str);
                iter.MoveNext();
            }
            public static bool CompareAndNext(ref IEnumerator<string> iter, string str) {
                return GetAndNext(ref iter) == str;
            }

            public static Vector3 GetVec3AndNext(ref IEnumerator<string> iter) {
                float x = float.Parse(Utility.IterData.GetAndNext(ref iter));
                float y = float.Parse(Utility.IterData.GetAndNext(ref iter));
                float z = float.Parse(Utility.IterData.GetAndNext(ref iter));
                return new Vector3(x, y, z);
            }
        }
    }
}