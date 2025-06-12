using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCManager : MonoBehaviour
{
    public List<Rigidbody> rigidbodies = new List<Rigidbody>();
    public void Fall()
    {
        GetComponent<Animator>().enabled = false;
        foreach (var rigidbody in rigidbodies)
        {
            rigidbody.drag = 1f;
            rigidbody.velocity = Vector3.zero;
        }
    }
}
