using UnityEngine;
using System.Collections;

public enum SHAPE {CONE, CROSS, HEART,
	HOLLOWCUBE, ICOSPHERE,
	STAR, TORUS}

public class PieceController : MonoBehaviour {

	public GameObject selectedIndicator;

	public Mesh coneMesh;
	public Mesh crossMesh;
	public Mesh heartMesh;
	public Mesh hollowCubeMesh;
	public Mesh icoSphereMesh;
	public Mesh starMesh;
	public Mesh torusMesh;

	public Color coneColor;
	public Color crossColor;
	public Color heartColor;
	public Color hollowCubeColor;
	public Color icoSphereColor;
	public Color starColor;
	public Color torusColor;

	public float moveSpeed;

	[HideInInspector]
	public SHAPE myShape;
	[HideInInspector]
	public bool animateMove=false;

	private static float rotateSpeed=180f;
	private Quaternion myStartRotation;
	private bool selected=false;

	private Vector3 moveTargetPosition;


	private Rigidbody myBody; 
	private GameObject mySelectedIndicator;
	private BoardController boardController;
	private MeshRenderer myRenderer;
	private MeshFilter myFilter;


	//built in unity methods

	void Awake()
	{
		boardController=GameObject.FindGameObjectWithTag("Board").GetComponent<BoardController>();
		mySelectedIndicator=null;
		myBody=GetComponent<Rigidbody>();
		myRenderer=GetComponent<MeshRenderer>();
		myFilter=GetComponent<MeshFilter>();

		moveTargetPosition=Vector3.zero;
	}

	// Use this for initialization
	void Start () 
	{
		SetRandomShape();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (animateMove) AnimateMove();
	}

	void OnMouseEnter() 
	{
		myBody.AddTorque(new Vector3(0,rotateSpeed,0));
		//Debug.Log(boardController.GetIndexOf(gameObject).CoordString());
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

	public void SetMoveTargetPosition(Vector3 inMoveTarget)
	{
		moveTargetPosition=inMoveTarget;
	}

	public void AnimateMove()
	{
		if (moveTargetPosition==Vector3.zero) 
		{
			animateMove=false;
			return;
		}
		float step = moveSpeed * Time.deltaTime;
		transform.position = Vector3.MoveTowards(transform.position, moveTargetPosition, step);

		if (transform.position==moveTargetPosition) 
		{
			animateMove=false;
			moveTargetPosition=Vector3.zero;
		}

	}

	//end public methods

	//private methods

	void SetRandomShape()
	{
		switch (GameController.GetRandomEnum<SHAPE>()) 
		{
		case SHAPE.CONE:
			myFilter.mesh=coneMesh;
			myRenderer.material.color=coneColor;
			transform.rotation=Quaternion.Euler(new Vector3(0,0,90));
			myShape=SHAPE.CONE;
			break;
		case SHAPE.CROSS:
			myFilter.mesh=crossMesh;
			myRenderer.material.color=crossColor;
			myShape=SHAPE.CROSS;
			break;
		case SHAPE.HEART:
			myFilter.mesh=heartMesh;
			myRenderer.material.color=heartColor;
			myShape=SHAPE.HEART;
			break;
		case SHAPE.HOLLOWCUBE:
			myFilter.mesh=hollowCubeMesh;
			myRenderer.material.color=hollowCubeColor;
			myShape=SHAPE.HOLLOWCUBE;
			break;
		case SHAPE.ICOSPHERE:
			myFilter.mesh=icoSphereMesh;
			myRenderer.material.color=icoSphereColor;
			myShape=SHAPE.ICOSPHERE;
			break;
		case SHAPE.STAR:
			myFilter.mesh=starMesh;
			myRenderer.material.color=starColor;
			myShape=SHAPE.STAR;
			break;
		case SHAPE.TORUS:
			myFilter.mesh=torusMesh;
			myRenderer.material.color=torusColor;
			transform.rotation=Quaternion.Euler(new Vector3(90f,0,0));
			myShape=SHAPE.TORUS;
			break;
		}

		myStartRotation=transform.localRotation;
	}

	//end private methods

}
