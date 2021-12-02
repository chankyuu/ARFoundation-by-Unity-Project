using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using Unity.Collections;
using UnityEngine.XR.ARCore;

public class ARCoreFaceRegionManager : MonoBehaviour
{
    ARFaceManager arFaceManager;
    ARSessionOrigin sessionOrigin;

    public GameObject nosePrefab;
    public GameObject leftHeadPrefab;
    public GameObject rightHeadPrefab;

    GameObject noseObject;
    GameObject leftHeadObject;
    GameObject rightHeadObject;

    NativeArray<ARCoreFaceRegionData> faceRegions;
    // Start is called before the first frame update
    void Start()
    {
        arFaceManager = GetComponent<ARFaceManager>();
        sessionOrigin = GetComponent<ARSessionOrigin>();
    }

    // Update is called once per frame
    void Update()
    {
        ARCoreFaceSubsystem subsystem = (ARCoreFaceSubsystem) arFaceManager.subsystem;

        foreach (ARFace face in arFaceManager.trackables)
        {
            subsystem.GetRegionPoses(face.trackableId, Unity.Collections.Allocator.Persistent, ref faceRegions);
            foreach (ARCoreFaceRegionData faceRegion in faceRegions)
            {
                ARCoreFaceRegion regionType = faceRegion.region;

                if (regionType == ARCoreFaceRegion.NoseTip)
                {
                    if (!noseObject)
                    {
                        noseObject = Instantiate(nosePrefab, sessionOrigin.trackablesParent);
                    }
                    noseObject.transform.localPosition = faceRegion.pose.position;
                    noseObject.transform.localRotation = faceRegion.pose.rotation;
                }

                else if (regionType == ARCoreFaceRegion.ForeheadLeft)
                {
                    if (!leftHeadObject)
                    {
                        leftHeadObject = Instantiate(leftHeadPrefab, sessionOrigin.trackablesParent);
                    }
                    leftHeadObject.transform.localPosition = faceRegion.pose.position;
                    leftHeadObject.transform.localRotation = faceRegion.pose.rotation;
                }

                else if (regionType == ARCoreFaceRegion.ForeheadRight)
                {
                    if (!rightHeadObject)
                    {
                        rightHeadObject = Instantiate(rightHeadPrefab, sessionOrigin.trackablesParent);
                    }
                    rightHeadObject.transform.localPosition = faceRegion.pose.position;
                    rightHeadObject.transform.localRotation = faceRegion.pose.rotation;
                }
            }
        }
    }
}
