using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BVH {

    class Utility {
        public static IEnumerable<string> SplitString(string data)
        {
            var components = data.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
            foreach(var c in components) {
                yield return c;
            }
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