using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Ellipsoids : MonoBehaviour
{
    static private float semiMajor, semiMinor, semiAxes;

    private Vector3 parentPosition;
    private Vector3 selfPosition;

    static private float h, k,m;

    public int resolution = 1000;
    public float theta = 0.0f;

    public GameObject parent, self;

    static private Vector3[] positions;

    static public Vector3 rotate;

    LineRenderer lr;

    static Vector3 targetDir;

    private void Start()
    {
        Vector3 parentPosition = parent.transform.position;
        Vector3 selfPosition = self.transform.position;

        lr = GetComponent<LineRenderer>();

        targetDir = parentPosition - selfPosition;

        lr.transform.position = new Vector3(selfPosition.x + targetDir.x / 2, selfPosition.y + targetDir.y / 2, selfPosition.z + targetDir.z / 2);

        drawEllipsoids(parentPosition, selfPosition, lr);
    }

    public static void drawEllipsoids(Vector3 parentPosition, Vector3 selfPosition, LineRenderer lr)
    {
        h = (float)(parentPosition.x + selfPosition.x) / 2;
        k = (float)(parentPosition.y + selfPosition.y) / 2;
        m = (float)(parentPosition.z + selfPosition.z) / 2;

        semiMajor = Vector3.Distance(parentPosition, selfPosition) / 2;
        semiMinor = semiMajor / 5;
        semiAxes = semiMinor;

        targetDir = parentPosition - selfPosition;

        float rotate = Vector3.Angle(targetDir, Vector3.up);

        positions = createEllipsoids(h, k, m);
        lr.positionCount = positions.Length;
        for (int i = 0; i < positions.Length; ++i)
            lr.SetPosition(i, positions[i]);
    }

    public static Vector3[] createEllipsoids(float h, float k, float m)
    {
        positions = new Vector3[361 * 181];

        Vector3 center = new Vector3(h, k, m);

        Quaternion qx = Quaternion.AngleAxis(rotate.x, Vector3.right);
        Quaternion qy = Quaternion.AngleAxis(rotate.y, Vector3.up);
        Quaternion qz = Quaternion.AngleAxis(rotate.z, Vector3.forward);

        for (int i = 0; i <= 360; ++i)
            for (int j = 0; j <= 180; ++j)
            {
                float beta = (float)j / 180 * 2.0f * Mathf.PI;
                float lamda = (float)i / 360 * 2.0f * Mathf.PI;

                positions[i * 180 + j] = new Vector3(semiMajor * Mathf.Cos(beta) * Mathf.Cos(lamda), semiMinor * Mathf.Cos(beta) * Mathf.Sin(lamda), semiAxes * Mathf.Sin(beta));

                positions[i * 180 + j] = qx * qy * qz * positions[i * 180 + j] + center;
            }

        Debug.Log(positions[361 * 181 - 1]);

        return positions;
    }
}