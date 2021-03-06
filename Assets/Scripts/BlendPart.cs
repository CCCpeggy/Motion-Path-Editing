using UnityEngine;

class BlendPart
{

    BVH.BVHObject basicObj; // 錄影的
    BVH.BVHObject refObj; // 參考的

    public BlendPart(BVH.BVHObject basicObj, BVH.BVHObject refObj)
    {
        this.basicObj = basicObj;
        this.refObj = refObj;
        if (basicObj.Motion.MotionData.Count != refObj.Motion.MotionData.Count)
        {
            Debug.LogError("MotionData 筆數不一致");
        }
    }

    public BVH.BVHObject Get()
    {
        BVH.BVHObject newObj = basicObj.Clone();
        Vector3 preRefPos = new Vector3(), preBasicPos = new Vector3();
        for (int j = 0; j < newObj.Motion.MotionData.Count; j++)
        {
            refObj.ApplyFrameByIdx(j);
            newObj.ApplyFrameByIdx(j);
            Vector3 nowRefPos = refObj.Root.transform.position;
            Vector3 nowBasicPos = newObj.Root.transform.position;
            for (int i = 0; i < refObj.Part.Length; i++)
            {
                var refRotation = refObj.Motion.MotionData[j].Rotation[i];
                var basicRotation = newObj.Motion.MotionData[j].Rotation[i];
                newObj.Motion.MotionData[j].Rotation[i] = Quaternion.Lerp(refRotation, basicRotation, 0.5f);
            }
            if (j > 0) {
                Vector3 refMovVec = nowRefPos - preRefPos;
                Vector3 basicMovVec = nowBasicPos - preBasicPos;
                Quaternion angle = Quaternion.FromToRotation(basicMovVec, refMovVec);
                Vector3 move = Quaternion.Lerp(angle, new Quaternion(), 0.5f) * basicMovVec;
                move.Normalize();
                move *= (refMovVec.magnitude + basicMovVec.magnitude) / 2;
                newObj.Motion.MotionData[j].Position = newObj.Motion.MotionData[j - 1].Position + move;
            }
            preRefPos = nowRefPos;
            preBasicPos = nowBasicPos;
        }
        return newObj;
    }

    public BVH.BVHMotion.Frame getFrame(int frameIdx, float alpha=0.5f)
    {
        refObj.ApplyFrameByIdx(frameIdx);
        basicObj.ApplyFrameByIdx(frameIdx);

        BVH.BVHMotion.Frame frame = new BVH.BVHMotion.Frame();
        for (int i = 0; i < refObj.Part.Length; i++)
        {
            var refRotation = refObj.Motion.MotionData[frameIdx].Rotation[i];
            var basicRotation = basicObj.Motion.MotionData[frameIdx].Rotation[i];
            frame.Rotation[i] = Quaternion.Lerp(basicRotation, refRotation, alpha);
        }
        Vector3 nowRefPos = refObj.Root.transform.position;
        Vector3 nowBasicPos = basicObj.Root.transform.position;
        if (frameIdx >= 1) {

            refObj.ApplyFrameByIdx(frameIdx-1);
            basicObj.ApplyFrameByIdx(frameIdx-1);
            Vector3 preRefPos = refObj.Root.transform.position;
            Vector3 preBasicPos = basicObj.Root.transform.position;

            Vector3 refMovVec = nowRefPos - preRefPos;
            Vector3 basicMovVec = nowBasicPos - preBasicPos;
            Quaternion angle = Quaternion.FromToRotation(basicMovVec, refMovVec);
            Vector3 move = Quaternion.Lerp(angle, new Quaternion(), 1-alpha) * basicMovVec;
            move.Normalize();
            move *= (refMovVec.magnitude + basicMovVec.magnitude) / 2;
            frame.Position = move;
        }
        else{
            frame.Position = Vector3.Lerp(nowBasicPos, nowRefPos, alpha);
        }
            
        return frame;
    }
}