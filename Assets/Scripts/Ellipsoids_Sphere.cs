using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ellipsoids_Sphere : MonoBehaviour
{
    public GameObject obj1, obj2;

    static GameObject Ellipsoid;

    // Start is called before the first frame update
    void Start()
    {
        Ellipsoid = GameObject.CreatePrimitive(PrimitiveType.Sphere);
    }

    // Update is called once per frame
    void FixedUpdate()
    {
        if (obj1 != null && obj2 != null) {
            setEllipsoid(obj1.transform.position, obj2.transform.position);
            Ellipsoid.SetActive(true);
        }
        else {
            Ellipsoid.SetActive(false);
        }
    }

    public void setEllipsoid(Vector3 pos1, Vector3 pos2)
    {
        Vector3 targetDir = pos1 - pos2;
        Vector3 targetPos = targetDir / 2 + pos2;

        float semiMajor = Vector3.Distance(pos1, pos2);
        float semiMinor = semiMajor / 5;

        Vector3 scale = new Vector3(semiMinor, semiMajor, semiMinor);

        Ellipsoid.transform.position = targetPos;

        Ellipsoid.transform.localScale = scale;

        targetDir.Normalize();
        var rotation = Quaternion.FromToRotation(Vector3.up, targetDir);
        Ellipsoid.transform.rotation = rotation;
    }

    public void SetObject(GameObject obj1, GameObject obj2) {
        this.obj1 = obj1;
        this.obj2 = obj2;
    }
}
