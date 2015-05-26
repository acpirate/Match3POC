﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class BoardController : MonoBehaviour {

	public static readonly int boardSize=8;

	public GameObject piece;
	public GameObject removeVFX;
	public GameObject matchFailFX;
	public GameObject scoreFX;

	public float boardPieceOffest = 30f;
	public float boardPieceSpacing= 60f;

	private float boardCorner;
	private GameObject[,] board;
	private GameController gameController;

	//unity default methods

	void Awake() 
	{
		gameController=GameObject.FindGameObjectWithTag("GameController").GetComponent<GameController>();

		boardCorner=boardPieceSpacing*(boardSize-2f)*-.5f-boardPieceOffest;

		board = new GameObject[boardSize,boardSize];
	}

	// Use this for initialization
	void Start () 
	{
		CreateBoard();
	}


	//end unity default methods

	public void ShowScore(int scoreToShow, Vector3 scoreLocation, Color scoreColor)
	{
		Vector3 scorePostion=new Vector3(scoreLocation.x, scoreLocation.y, -100f);
		
		GameObject scoreContainer=(GameObject) Instantiate(scoreFX,scorePostion,Quaternion.identity);
		
		ScorePopupController scoreController=scoreContainer.GetComponent<ScorePopupController>();
		scoreController.SetScore(scoreToShow);
		scoreController.SetColor(scoreColor);
	}


	public void StopAllSpin()
	{
			foreach (GameObject piece in board)
			{
				piece.GetComponent<PieceController>().StopSpin();
			}
	}
	
	public void CreateBoard()
	{
		int possibleMatches=0;
		while (possibleMatches<1)
		{
			NewBoard();
			AvoidCurrentMatches();
			possibleMatches=gameController.PossibleMatches().Count;
		}
	}

	void NewBoard()
	{
		for(int colCounter=0;colCounter<boardSize;colCounter++)
		{
			for (int rowCounter=0;rowCounter<boardSize;rowCounter++)
			{
				if ((board[colCounter,rowCounter])!=null) DestroyImmediate(board[colCounter,rowCounter]);
				board[colCounter,rowCounter] = Instantiate(piece);
				SnapToWorldPosition(colCounter,rowCounter);
			}
		}
	}

	void AvoidCurrentMatches()
	{
		// adjust board until there are no matches
		List<Match> matchList=gameController.GetBaseMatches();
		
		//Debug.Log(matchList.Count.ToString());
		int matchResetCounter=0;
		
		while (matchList.Count>0)
		{
			matchResetCounter++;
			foreach (Match match in matchList)
			{
				foreach (Coords coords in match.matchCoords)
				{
					board[coords.x,coords.y].GetComponent<PieceController>().SetRandomShape();
				}
			}
			matchList=gameController.GetBaseMatches();
		}
		Debug.Log(matchResetCounter);
	}

	public GameObject GetPieceAt(Coords pieceCoords)
	{
		return board[pieceCoords.x,pieceCoords.y];
	}

	void SnapToWorldPosition(int col, int row)
	{

		board[col,row].transform.position=CalculateWorldPosition(col, row);

	}

	public Vector3 CalculateWorldPosition(int col, int row)
	{
		float xPosition = boardCorner + col * boardPieceSpacing;
		float yPosition = boardCorner + row * boardPieceSpacing;

		Vector3 piecePosition = new Vector3(xPosition, yPosition,0f);

		return piecePosition;
	}

	public void ActivatePiece(GameObject pieceToActivate)
	{
		//break out if we aren't in the selection state
		if (!(GameController.gameState==GAMESTATE.SELECTION)) return;
		
		PieceController pieceController = pieceToActivate.GetComponent<PieceController>();
		
		//check to see if each neighbor is selected, if so break out and try to swap them
		foreach (GameObject neighbor in GetNeighbors(pieceToActivate))
		{
			if (neighbor.gameObject.GetComponent<PieceController>().GetSelected())
			{
				AnimateMovePairPieces(pieceToActivate,neighbor, GAMESTATE.TRYMATCHMOVE);
				UnselectAll();
				//gameController.MoveActivePices(pieceToActivate,neighbor);
				return;
			}
		}
		
		//finally if none of the other conditions are true unselect all of the pieces and select the input piece
		UnselectAll();
		pieceController.SetSelected(true);
	}

	public void AnimateMovePairPieces(GameObject piece1, GameObject piece2, GAMESTATE stateToSet)
	{
		Coords piece1index=GetIndexOf(piece1);
		Coords piece2index=GetIndexOf(piece2);
		PieceController piece1Controller=piece1.GetComponent<PieceController>();
		PieceController piece2Controller=piece2.GetComponent<PieceController>();

		GameController.gameState=stateToSet;

		piece1Controller.animateMove=true;
		piece2Controller.animateMove=true;

		piece1Controller.SetMoveTargetPosition(CalculateWorldPosition(piece2index.x,piece2index.y));
		piece2Controller.SetMoveTargetPosition(CalculateWorldPosition(piece1index.x,piece1index.y));

		gameController.SetTriedPieces(piece1,piece2);

		MakeSwap(piece1,piece2);

		if (stateToSet==GAMESTATE.FAILMATCHMOVE)
		{
			Vector3 failLocation=(piece1.transform.position+(piece2.transform.position-piece1.transform.position)*.5f);
			failLocation.z=-100f;
			Instantiate(matchFailFX,failLocation,Quaternion.identity);
		}

	}

	public List<GameObject> GetNeighbors(GameObject queriedPiece)
	{
		List<GameObject> returnNeighbors = new List<GameObject>();

		Coords pieceIndex=GetIndexOf(queriedPiece);

		//south neighbor
		if (pieceIndex.y>0) returnNeighbors.Add(board[pieceIndex.x,pieceIndex.y-1]);
		//north neighbor
		if (pieceIndex.y<boardSize-1) returnNeighbors.Add(board[pieceIndex.x,pieceIndex.y+1]);
		//east neighbor
		if (pieceIndex.x>0) returnNeighbors.Add(board[pieceIndex.x-1,pieceIndex.y]);
		//west neighbor
		if (pieceIndex.x<boardSize-1) returnNeighbors.Add(board[pieceIndex.x+1,pieceIndex.y]);


		//Debug.Log(returnNeighbors.Count);

		return returnNeighbors;
	}

	public Coords GetIndexOf(GameObject queriedPiece)
	{
		Coords returnCoords= new Coords(-1,-1);


		for(int colCounter=0;colCounter<boardSize;colCounter++)
		{
			for (int rowCounter=0;rowCounter<boardSize;rowCounter++)
			{
				if (board[colCounter,rowCounter]==queriedPiece)
				{
					returnCoords.x=colCounter;
					returnCoords.y=rowCounter;
					break;
				}
				if (returnCoords.x!=-1 && returnCoords.y!=-1) break;
			}
		}
		//error checking
		if (returnCoords.x==-1 && returnCoords.y==-1)
		{
			Debug.LogError("tried to get the index of a piece that isn't on the board");
			Application.Quit();
		}

		return returnCoords;
	}

	public void MakeSwap(GameObject piece1, GameObject piece2)
	{
		Coords piece1Coords = GetIndexOf (piece1);
		Coords piece2Coords = GetIndexOf (piece2);

		GameObject tempPiece = piece1;
		board[piece1Coords.x,piece1Coords.y]=piece2;
		board[piece2Coords.x,piece2Coords.y]=tempPiece;

		//SnapToWorldPosition(piece1Coords.x,piece1Coords.y);
		//SnapToWorldPosition(piece2Coords.x,piece2Coords.y);

	}
	
	public void RemoveMatches(List<Match> matchesToRemove)
	{
		foreach (Match match in matchesToRemove)
		{
			foreach(Coords pieceCoords in match.matchCoords)
			{
				RemovePiece(pieceCoords);
			}
		}

		MoveDownAndReplacePieces();
		AnimateMove();

	}

	public GameObject GetPieceAtCoords(Coords inCoords)
	{
		GameObject tempPiece=RetrievePiece(inCoords);

		return  tempPiece;
	}

	public GameObject GetPieceAtCoords(Coords inCoords, Coords inOffset)
	{
		GameObject tempPiece=RetrievePiece(inCoords+inOffset);

		return tempPiece;

	}

	GameObject RetrievePiece(Coords inCoords)
	{
		return board[inCoords.x,inCoords.y];
	}

	void RemovePiece(Coords pieceToProcess)
	{
		GameObject pieceStorage=board[pieceToProcess.x,pieceToProcess.y];

		if (pieceStorage!=null)
		{
			DestroyImmediate(pieceStorage);
		}
	}

	void AnimateMove()
	{
		foreach(GameObject piece in board)
		{
			piece.GetComponent<PieceController>().animateMove=true;
		}
	}

	void MoveDownAndReplacePieces()
	{
		for(int colCounter=0;colCounter<boardSize;colCounter++)
		{
			int missingCounter=0;
			for (int rowCounter=0;rowCounter<boardSize;rowCounter++)
			{
				if (board[colCounter, rowCounter]==null)
				{
					missingCounter++;
				}
				else 
				{
					if (missingCounter>0) 
					{
						GameObject tempPiece=board[colCounter,rowCounter];
						board[colCounter,rowCounter]=null;
						board[colCounter,rowCounter-missingCounter]=tempPiece;
						tempPiece.GetComponent<PieceController>().SetMoveTargetPosition(CalculateWorldPosition(colCounter,rowCounter-missingCounter));
						//MoveToWorldPosition(colCounter,rowCounter-missingCounter);
					}
				}
			}
			ReplaceMatches(colCounter,missingCounter);

		}

	}

	void ReplaceMatches(int col, int missingCounter)
	{
		for(int i = 1;i<missingCounter+1;i++)
		{

			board[col,boardSize-i] = (GameObject) Instantiate(piece,CalculateMissingPosition(missingCounter-i+1,col),Quaternion.identity);
			board[col,boardSize-i].GetComponent<PieceController>().SetMoveTargetPosition(CalculateWorldPosition(
				col,boardSize-i));
			//Debug.Log(col.ToString()+" "+(boardSize-i).ToString());
		}
	}

	Vector3 CalculateMissingPosition(int missingCounter, int col)
	{
		float xPosition = boardCorner + col * boardPieceSpacing;
		float yPosition = -boardCorner + missingCounter * boardPieceSpacing;
		
		Vector3 piecePosition = new Vector3(xPosition, yPosition,0f);
		
		return piecePosition;

	}

	//end private methods

	//public methods

	public void UnselectAll()
	{
		gameController.hintsShowing=false;
		gameController.hintCountdown=gameController.hintTime;

		foreach (GameObject piece in board)
		{
			PieceController tempController=piece.GetComponent<PieceController>();
			tempController.SetSelected(false);
			tempController.ShowHint(false);
		}
	}

	public void HintsOn(GameObject piece1, GameObject piece2)
	{
		PieceController piece1Controller=piece1.GetComponent<PieceController>();
		PieceController piece2Controller=piece2.GetComponent<PieceController>();

		piece1Controller.ShowHint(true);
		piece2Controller.ShowHint(true);
	}



	public GameObject[,] GetBoard()
	{
		return board;
	}

	//end public methods

}

public struct Coords {
	public int x;
	public int y;

	//plus operator allows adding two coordinates together
	public static Coords operator +(Coords c1, Coords c2) 
	{
		return new Coords(c1.x+c2.x, c1.y+c2.y);
	}
	//== operator returns true if the coordinates have the same value
	public static bool operator ==(Coords c1, Coords c2) 
	{
		bool returnValue=false;
		if (c1.x==c2.x && c1.y==c2.y)
		{
			returnValue=true;
		}
		return returnValue;
	}
	//!= operator returns true if the coordinates do not have the same value
	public static bool operator !=(Coords c1, Coords c2) 
	{
		bool returnValue=true;
		if (c1.x==c2.x && c1.y==c2.y)
		{
			returnValue=false;
		}
		return returnValue;
	}
	//overriding == and != operators require this
	public override bool Equals(object obj)
	{
		if (obj == null || GetType() != obj.GetType())
		{
			return false;
		}
		
		Coords coordsToCompareWith = (Coords) obj;
		
		return (coordsToCompareWith.x==x && coordsToCompareWith.y==y);	
	}

	//overriding gethash code, required for overriding equals
	public override int GetHashCode()
	{
		return x ^ y;
	}


	// Override the ToString
	public override string ToString()
	{
		return(x.ToString() + "," + y.ToString());
	}


	public Coords(int inX, int inY)
	{
		x=inX;
		y=inY;
	}

	public string CoordString() {
		return x.ToString() + "," + y.ToString();
	}
}
