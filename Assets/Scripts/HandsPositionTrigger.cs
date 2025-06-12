using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HandsPositionTrigger : MonoBehaviour
{
    public GameObject LeftHand;
    public GameObject RightHand;
    public GameObject LeftController;
    public GameObject RightController;
    public static HandsPositionTrigger instance;
    private bool isReady;
    public bool IsReady
    {
        get
        {
            return isReady;
        }
    }

    private bool leftHold;
    private bool rightHold;
    private Vector3 centerPos;
    private Vector3 startCenterPos;

    public Vector3 CenterPos
    {
        get
        {
            return centerPos;
        }
    }

    public Vector3 StartCenterPos
    {
        get
        {
            return startCenterPos;
        }
    }
    private void Awake()
    {
        instance = this;
    }
    // Start is called before the first frame update
    private void Update()
    {
        if (LeftHand.activeSelf&&RightHand.activeSelf)
            isReady = true;
        else
            isReady = false;
        if(OVRInput.GetDown(OVRInput.RawButton.RHandTrigger))
        {
            rightHold = true;
            InitStartCenterPos();
        }
        if (OVRInput.GetDown(OVRInput.RawButton.LHandTrigger))
        {
            leftHold = true;
            InitStartCenterPos();
        }

        if (OVRInput.GetUp(OVRInput.RawButton.RHandTrigger))
        {
            rightHold = false;
        }
        if (OVRInput.GetUp(OVRInput.RawButton.LHandTrigger))
        {
            leftHold = false;
        }

        if(leftHold&&rightHold&&isReady)
            centerPos = (RightController.transform.parent.parent.position+LeftController.transform.parent.parent.position)/2 ;
        else
        {
            startCenterPos = Vector3.zero;
                centerPos = Vector3.zero;
        }
            
    }
    void InitStartCenterPos()
    {
        if (IsReady && leftHold && rightHold)
            startCenterPos = (RightController.transform.parent.parent.position + LeftController.transform.parent.parent.position) / 2;
        else
            startCenterPos = Vector3.zero;

    }
    private void OnTriggerEnter(Collider other)
    {
        if(other.tag=="LeftHand")
        {
            LeftHand.SetActive(true);
            LeftController.SetActive(false);
            leftHold = false;
        }
        if(other.tag=="RightHand")
        {
            RightHand.SetActive(true);
            RightController.SetActive(false);
            rightHold = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "LeftHand")
        {
            LeftHand.SetActive(false);
            LeftController.SetActive(true);
        }
        if (other.tag == "RightHand")
        {
            RightHand.SetActive(false);
            RightController.SetActive(true);
        }
    }
}
