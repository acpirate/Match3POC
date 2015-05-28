﻿using UnityEngine;
using System.Collections;

public enum SHAPE {CONE, CROSS, HEART,
	HOLLOWCUBE, ICOSPHERE,
	STAR, TORUS, NONE}

public class PieceController : MonoBehaviour {

	public GameObject selectedIndicator;
	public GameObject removeVFX;

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

	private static float rotateSpeed=360f;
	private Vector3 myStartRotation;
	private bool selected=false;

	private Vector3 moveTargetPosition;


	private Rigidbody myBody; 
	private GameObject mySelectedIndicator;
	private BoardController boardController;
	private MeshRenderer myRenderer;
	private MeshFilter myFilter;
	private ParticleSystem myParticles;

	//built in unity methods

	void Awake()
	{
		boardController=GameObject.FindGameObjectWithTag("Board").GetComponent<BoardController>();
		mySelectedIndicator=null;
		myBody=GetComponent<Rigidbody>();
		myRenderer=GetComponent<MeshRenderer>();
		myFilter=GetComponent<MeshFilter>();
		myParticles=GetComponent<ParticleSystem>();

		moveTargetPosition=Vector3.zero;
		SetRandomShape();
	}

	// Use this for initialization
	void Start () 
	{
		//SetRandomShape();
	}
	
	// Update is called once per frame
	void Update () 
	{
		if (animateMove) AnimateMove();
	}

	void OnMouseEnter() 
	{
		StartSpin();
		//Debug.Log(boardController.GetIndexOf(gameObject).CoordString());
	}

	void OnMouseExit() 
	{
		StopSpin();
	}

	void OnMouseDown()
	{
		/*boardController.UnselectAll();
		boardController.GetNeighbors(gameObject);
		SetSelected(!selected);*/
		if (selected)
		{
			SetSelected(false);
			return;
		}
		boardController.ActivatePiece(gameObject);
	}

	void OnDestroy()
	{
		if (!(GameController.quitting || GameController.resetting || GameController.gameState==GAMESTATE.ENDGAME))
		Instantiate(removeVFX,transform.position,Quaternion.identity);
	}
	//end built in unity methods

	//public methods

	public void ShowHint(bool show)
	{
		myParticles.Stop();
		myParticles.Clear();

		if (show) myParticles.Play();

	}

	public void StartSpin()
	{
		boardController.StopAllSpin();
		if (GameController.gameState==GAMESTATE.SELECTION)
			myBody.AddTorque(new Vector3(0,rotateSpeed,0));
	}

	public void StopSpin()
	{
		myBody.angularVelocity=new Vector3(0,0,0);
		transform.localEulerAngles=myStartRotation;
	}


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

	public void SetRandomShape()
	{
		//none designation needed for piece finding
		SHAPE randomShape=SHAPE.NONE;
		//repeat randomize until a non-NONE shape appears
		while(randomShape==SHAPE.NONE)
		{
			randomShape=GameController.GetRandomEnum<SHAPE>();
		}

		SetShape(randomShape);
		myStartRotation=transform.localEulerAngles;
	}

	public void SetShapeFromString(string shapeString)
	{
		//CONE, CROSS, HEART, CUBE, SPHERE, STAR, TORUS
		switch (shapeString)
		{
		case "CONE":
			SetShape(SHAPE.CONE);
			break;
		case "CROSS":
			SetShape(SHAPE.CROSS);
			break;
		case "HEART":
			SetShape(SHAPE.HEART);
			break;
		case "CUBE":
			SetShape(SHAPE.HOLLOWCUBE);
			break;
		case "SPHERE":
			SetShape(SHAPE.ICOSPHERE);
			break;
		case "STAR":
			SetShape(SHAPE.STAR);
			break;
		case "TORUS":
			SetShape(SHAPE.TORUS);
			break;
		default:
			Debug.Log("ERROR: ATTEMPTING TO SET PIECE TO INVALID SHAPE '"+shapeString+"'");
			Application.Quit();
			break;
		}
	}

	//end public methods
	
	//private methods
	public void SetShape(SHAPE shape)
	{
		transform.localEulerAngles=Vector3.zero;
		
		switch (shape) 
		{
		case SHAPE.CONE:
			myFilter.mesh=coneMesh;
			myRenderer.material.color=coneColor;
			transform.localEulerAngles=new Vector3(0,0,90f);
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
			transform.localEulerAngles=new Vector3(90f,0,0);
			myShape=SHAPE.TORUS;
			break;
		}
		
		myStartRotation=transform.localEulerAngles;

	}


	//end private methods

}
