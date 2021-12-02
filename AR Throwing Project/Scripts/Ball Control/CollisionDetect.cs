using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.UI;

public class CollisionDetect : MonoBehaviour
{
    // Start is called before the first frame update
    public ParticleSystem particleObject;
    public GameObject partO;
    public bool isNext = false;
    private void Start()
    {
        particleObject.Play();
    }
    private void Update()
	{
    }
    private void OnCollisionEnter(Collision other)
    {
        if (other.transform.name.Contains("Lowpoly"))
        {
            partO = Instantiate(particleObject.transform.gameObject, other.gameObject.transform.position, other.gameObject.transform.rotation);
            Destroy(other.gameObject);
            partO.GetComponent<ParticleSystem>().Play();
            isNext = true;
        }
    }
}
