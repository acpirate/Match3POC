using UnityEngine;
using System.Collections;

public class PieceController : MonoBehaviour {

	public GameObject selectedIndicator;

	private static float rotateSpeed=180f;
	private Quaternion myStartRotation;
	private bool selected=false;

	private Rigidbody myBody; 
	private GameObject mySelectedIndicator;
	private BoardController boardController;


	//built in unity methods

	void Awake()
	{
		boardController=GameObject.FindGameObjectWithTag("Board").GetComponent<BoardController>();
		mySelectedIndicator=null;
		myStartRotation=transform.rotation;
		myBody=GetComponent<Rigidbody>();
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
		/*boardController.UnselectAll();
		boardController.GetNeighbors(gameObject);
		SetSelected(!selected);*/
		boardController.TrySelect(gameObject);
	}

	//end built in unity methods

	//public methods

	public void SetSelected(bool inSelected)
	{
		selected=inSelected;

		if (mySelectedIndicator!=null) Destroy(mySelectedIndicator);
		
		if (selected)
		{
			mySelectedIndicator=Instantiate(selectedIndicator,transform.position,Quaternion.identity) as GameObject;
			selected=true;
		}
	}

	public bool GetSelected()
	{
		return selected;
	}

	//end public methods

	//private methods


	//end private methods

}
