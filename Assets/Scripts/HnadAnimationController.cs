using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class HnadAnimationController : MonoBehaviour
{
    Animator animator;
    public OVRInput.RawButton handTypeGrab;
    public OVRInput.RawButton handTypePoint;

    void Start()
    {
        animator = GetComponent<Animator>();
    }

    // Update is called once per frame
    void Update()
    {
        if (OVRInput.GetDown(handTypeGrab))
            animator.SetBool("Grab", true);
        if (OVRInput.GetUp(handTypeGrab))
            animator.SetBool("Grab", false);

        if (OVRInput.GetDown(handTypePoint))
            animator.SetBool("Point", true);
        if (OVRInput.GetUp(handTypePoint))
            animator.SetBool("Point", false);
    }
}
