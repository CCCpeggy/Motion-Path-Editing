using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using MathNet.Numerics.LinearAlgebra;
using MathNet.Numerics.LinearAlgebra.Double;

public class Curve : MonoBehaviour {
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
    
    
    static Matrix<double> bm = DenseMatrix.OfArray(new double[,] {
        {-1 , 3, -3 , 1},
        {3 , -6, 3 , 0},
        {-3 , 0, 3 , 0},
        {1 , 4, 1 , 0}}
    );
    public static Vector3 GetP(double u, Matrix<double> p)
    {
        Vector<double> v = new DenseVector(new double[] {u*u*u, u*u, u, 1});
        var points = v.ToRowMatrix().Multiply(bm).Multiply(p) * one_six;
        return new Vector3((float)points[0, 0], 0, (float)points[0, 1]);
    }

    private static GameObject CreateLine(Vector3 start, Vector3 end, string name="line") {
        var line = new GameObject();
        line.name = name;
        LineRenderer lr = line.AddComponent<LineRenderer>();
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
        lr.startWidth = 0.2f;
        lr.endWidth = 0.2f;
        return line;
    }
    private static void UpdateLine(GameObject line, Vector3 start, Vector3 end) {
        LineRenderer lr = line.GetComponent<LineRenderer>();
        lr.SetPosition(0, start);
        lr.SetPosition(1, end);
    }

    GameObject[] controlPointObjs = new GameObject[4];
    GameObject[] controlPointLineObjs = new GameObject[3];
    List<GameObject> curveLineObjs = new List<GameObject>();
    Matrix<double> controlPoints;
    Matrix<double> originControlPoints;
    float[] timeToT = new float[40];
    int n;

    public Curve Clone() {
        var newCurve = CreateCurve(controlPoints, n, name).GetComponent<Curve>();
        return newCurve;
    }

    public void UpdateTimeToT()
    {
        if (controlPoints == null) return;
        float amount = 0, sum = 0;
        for (float t = 1; t <= 100; t++)
        {
            sum += Vector3.Distance(GetPos((t - 1) / 100, false), GetPos(t / 100, false));
        }
        float divide = sum / 30;
        int count = 6;
        for (float t = 0; count < 40; t++)
        {
            float dis = Vector3.Distance(GetPos((t - 1) / 100, false), GetPos(t / 100, false));
            amount += dis;
            if (amount > divide)
            {
                timeToT[count++] = t / 100;
                amount -= divide;
            }
        }
        timeToT[5] = 0;
        count = 4;
        for (float t = -1; count >= 0; t--)
        {
            float dis = Vector3.Distance(GetPos((t - 1) / 100, false), GetPos(t / 100, false));
            amount += dis;
            if (amount > divide)
            {
                timeToT[count--] = t / 100;
                amount -= divide;
            }
        }
    }

    public float getTByTime(float t)
    {
        t = t * 30 + 5;
        int previousIdx = (int)t;
        int nextIdx = previousIdx + 1;
        float alpha = t - previousIdx;
        return timeToT[previousIdx] * (1 - alpha) + timeToT[nextIdx] * alpha;
    }
    public void SetLineVisible(bool visible) {
       gameObject.SetActive(visible);
    }

    public void SetMaterial(Material controlLineMat, Material curveLineMat) {
        for(int i=0; i<3;i++) {
            var line = controlPointLineObjs[i].GetComponent<LineRenderer>();
            if (line) {
                line.material = controlLineMat;
                line.startWidth = 5f;
                line.endWidth = 5f;
            }
        }
        for(int i=0; i<curveLineObjs.Count;i++) {
            var line = curveLineObjs[i].GetComponent<LineRenderer>();
            if (line) {
                line.material = curveLineMat;
                line.startWidth = 3f;
                line.endWidth = 3f;
            }
        }
        for(int i=0; i<4;i++) {
            controlPointObjs[i].transform.localScale = new Vector3(5, 5, 5);
        }
    }

    public void CreateCurvePointsAndLine() {
        originControlPoints = controlPoints.Clone();
        // 畫控制點
        var controlPointsGroup = new GameObject();
        controlPointsGroup.name = "ControlPoints";
        controlPointsGroup.transform.parent = transform;
        for (int i = 0; i < 4; i++) {
            GameObject sphere = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            sphere.transform.position = new Vector3((float)controlPoints[i, 0], 0, (float)controlPoints[i, 1]);
            sphere.transform.parent = controlPointsGroup.transform;
            controlPointObjs[i] = sphere;
        }

        // 將控制點之間連線
        var controlPointLines = new GameObject();
        controlPointLines.name = "ControlPointLines";
        controlPointLines.transform.parent = transform;
        for (int i = 0; i < 3; i++) {
            Vector3 last = new Vector3((float)controlPoints[i, 0], 0, (float)controlPoints[i, 1]);
            Vector3 p = new Vector3((float)controlPoints[i + 1, 0], 0, (float)controlPoints[i + 1, 1]);;
            GameObject line = CreateLine(last, p);
            line.transform.parent = controlPointLines.transform;
            controlPointLineObjs[i] = line;
        }

        // 畫曲線
        var curveLineGroup = new GameObject();
        curveLineGroup.name = "CurveLineGroup";
        curveLineGroup.transform.parent = transform;
        int count = 0;
        double step=1;
        for (double i = 0; i < n; i += step)
        {
            Vector3 last = GetP((i - step * 1.05) / n, controlPoints);
            Vector3 p = GetP(i / n, controlPoints);
            GameObject line = CreateLine(last, p, "line_" + count++);
            line.transform.parent = curveLineGroup.transform;
            curveLineObjs.Add(line);
        }
        UpdateTimeToT();
    }

    // public void ReCreateCurvePointsAndLine() {
    //     double step=1;
    //     for(double i=curveLineObjs.Count - 1; i >= n;i -= step){
    //     }

    //     // 曲線
    //     int count = 0;
    //     for(double i=curveLineObjs.Count - 1; i < n;i += step){
    //         GameObject line = CreateLine(Vector3.forward, Vector3.forward, "line_" + count++);
    //         line.transform.parent = transform.chi.transform;
    //         curveLineObjs.Add(line);
    //     }
    // }

    public static GameObject CreateCurve(Matrix<double> controlPoints, int n, string name="Curve") {
        var curve = new GameObject();
        curve.name = name;

        Curve curveObj = curve.AddComponent<Curve>();
        curveObj.controlPoints = controlPoints;
        curveObj.originControlPoints = controlPoints.Clone();
        curveObj.n = n;
        curveObj.CreateCurvePointsAndLine();

        return curve;
    }

    public void ReCreateCurve(Matrix<double> controlPoints, int n) {
        GameObject.Destroy(this.GetComponent<Curve>());
        Curve curveObj = gameObject.AddComponent<Curve>();
        curveObj.controlPoints = controlPoints;
        curveObj.originControlPoints = controlPoints;
        curveObj.n = n;
        curveObj.CreateCurvePointsAndLine();
    }

    void Update() {
        // Upate Control Points
        for (int i = 0; i < 4; i++) {
            controlPoints[i, 0] = controlPointObjs[i].transform.position.x;
            controlPoints[i, 1] = controlPointObjs[i].transform.position.z;
        }

        // 將控制點之間連線
        for (int i = 0; i < 3; i++) {
            Vector3 last = new Vector3((float)controlPoints[i, 0], 0, (float)controlPoints[i, 1]);
            Vector3 p = new Vector3((float)controlPoints[i + 1, 0], 0, (float)controlPoints[i + 1, 1]);
            UpdateLine(controlPointLineObjs[i], last, p);
        }

        // 畫曲線
        int step = 1;
        int count = 0;
        for(double i=0; i < n;i += step){
            Vector3 last = GetP((i-step*1.05)/n, controlPoints);
            Vector3 p = GetP(i/n, controlPoints);
            UpdateLine(curveLineObjs[count++], last, p);
        }
        UpdateTimeToT();
    }
    
    public Vector3 GetPos(float t, bool isTime=true)
    {
        return GetP(isTime?getTByTime(t):t, controlPoints);
    }
    public Vector3 GetOriPos(float t, bool isTime=true)
    {
        return GetP(isTime ? getTByTime(t) : t, originControlPoints);
    }

    public Quaternion GetRot(float t)
    {
        Vector3 oriPrevious = GetOriPos((float)(t - 0.05));
        Vector3 oriNext = GetOriPos((float)(t + 0.05));
        Vector3 oriDir = oriNext - oriPrevious;
        Vector3 nowPrevious = GetPos((float)(t - 0.05));
        Vector3 nowNext = GetPos((float)(t + 0.05));
        Vector3 nowDir = nowNext - nowPrevious;
        float oriAngle = BVH.Utility.ConvertVecToAngle(new Vector2(oriDir.x, oriDir.z));
        float nowAngle = BVH.Utility.ConvertVecToAngle(new Vector2(nowDir.x, nowDir.z));
        return Quaternion.Euler(0, oriAngle - nowAngle, 0);
    }
}

