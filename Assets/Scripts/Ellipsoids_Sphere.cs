using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ellipsoids_Sphere : MonoBehaviour
{
    static private Vector3 parentPosition;
    static private Vector3 selfPosition;

    static private float semiMajor, semiMinor, semiAxes;

    static public Vector3 rotate;

    public GameObject parent, self;

    static GameObject Ellipsoid;

    // Start is called before the first frame update
    void Start()
    {
        Ellipsoid = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        parentPosition = parent.transform.position;
        selfPosition = self.transform.position;

        Vector3 targetDir = parentPosition - selfPosition;
        float angle = Vector3.Angle(targetDir, transform.right);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        parentPosition = parent.transform.position;
        selfPosition = self.transform.position;
        //setEllipsoid(parentPosition, selfPosition);

        Vector3 targetDir = parentPosition - selfPosition;

        //float angleX = Vector3.Angle(targetDir, Vector3.right);
        //float angleY = Vector3.Angle(targetDir, Vector3.up);
        float angleZ = Vector3.Angle(targetDir, Vector3.forward);

        semiMajor = Vector3.Distance(parentPosition, selfPosition);
        semiMinor = semiMajor / 5;
        semiAxes = semiMinor;

        Vector3 scale = new Vector3(semiMinor, semiMajor, semiAxes);

        Debug.Log(targetDir);
        Debug.Log(angleZ);

        Ellipsoid.transform.position = new Vector3(selfPosition.x + targetDir.x / 2, selfPosition.y + targetDir.y / 2, selfPosition.z + targetDir.z / 2);

        Ellipsoid.transform.localScale = scale;

        Ellipsoid.transform.rotation = Quaternion.Euler(0, 0, angleZ);

        //Debug.Log(Ellipsoid.transform.rotation);
    }

    public static void setEllipsoid(Vector3 parentPos, Vector3 selfPos)
    {
        //Ellipsoid = GameObject.CreatePrimitive(PrimitiveType.Sphere);

        Vector3 targetDir = parentPos - selfPos;

        float angleX = Vector3.Angle(targetDir, Vector3.right);
        float angleY = Vector3.Angle(targetDir, Vector3.up);
        float angleZ = Vector3.Angle(targetDir, Vector3.forward);

        //Debug.Log(angleX);
        //Debug.Log(angleZ);


        semiMajor = Vector3.Distance(parentPosition, selfPosition);
        semiMinor = semiMajor / 5;
        semiAxes = semiMinor;

        Vector3 scale = new Vector3(semiMinor, semiMajor, semiAxes);

        Ellipsoid.transform.position = new Vector3(selfPosition.x + targetDir.x / 2, selfPosition.y + targetDir.y / 2, selfPosition.z + targetDir.z / 2);

        Ellipsoid.transform.localScale = scale;
    }
}
