using UnityEngine;
using System.Collections;

public class PieceController : MonoBehaviour {

	private static float rotateSpeed=180f;
	private Quaternion myStartRotation;
	private bool selected=false;

	private Rigidbody myBody; 
	private ParticleSystem myParticles;

	void Awake()
	{
		myStartRotation=transform.rotation;
		myBody=GetComponent<Rigidbody>();
		myParticles=GetComponent<ParticleSystem>();
	}

	// Use this for initialization
	void Start () 
	{

	}
	
	// Update is called once per frame
	void Update () 
	{
	
	}

	void OnMouseEnter() 
	{
		myBody.AddTorque(new Vector3(0,rotateSpeed,0));
	}

	void OnMouseExit() 
	{
		myBody.angularVelocity=new Vector3(0,0,0);
		transform.rotation=myStartRotation;
	}

	void OnMouseDown()
	{
		selected=!selected;

		myParticles.Stop();
		myParticles.Clear();
		if (selected) myParticles.Play();
	}

}
