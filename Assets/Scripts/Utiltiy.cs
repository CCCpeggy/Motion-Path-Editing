using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

namespace BVH {

    class Utility {
        public class Curve {
            const float one_six = (float)1/6;
            public static double GetB0(double t) {
                return one_six * (1-t) * (1-t) * (1-t);
            }

            public static double GetB1(double t) {
                return one_six * (3*t*t*t - 6*t*t + 4);
            }

            public static double GetB2(double t) {
                return one_six * (-3*t*t*t + 3*t*t + 3*t + 1);
            }

            public static double GetB3(double t) {
                return one_six * t * t * t;
            }

            public static Vector3 GetP(double u, Matrix<double> p) {
                Matrix<double> m = DenseMatrix.OfArray(new double[,] {
                    {-1 , 3, -3 , 1},
                    {3 , -6, 3 , 0},
                    {-3 , 0, 3 , 0},
                    {1 , 4, 1 , 0}}
                );
                Vector<double> v = new DenseVector(new double[] {u*u*u, u*u, u, 1});
                var points = v.ToRowMatrix().Multiply(m).Multiply(p) * one_six;
                return new Vector3((float)points[0, 0], 0, (float)points[0, 1]);
            }

        }

        public static IEnumerable<string> SplitString(string data)
        {
            var components = data.Split((char[])null, System.StringSplitOptions.RemoveEmptyEntries);
            foreach(var c in components) {
                yield return c;
            }
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