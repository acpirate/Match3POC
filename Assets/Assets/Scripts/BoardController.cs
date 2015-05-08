using UnityEngine;
using System.Collections;
using System.Collections.Generic;



public class BoardController : MonoBehaviour {

	public static readonly int boardSize=8;

	public GameObject piece;

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

	//private methods

	void CreateBoard()
	{
		for(int colCounter=0;colCounter<boardSize;colCounter++)
		{
			for (int rowCounter=0;rowCounter<boardSize;rowCounter++)
			{
				board[colCounter,rowCounter] = Instantiate(piece);
				SnapToWorldPosition(colCounter,rowCounter);
			}
		}
	}

	void SnapToWorldPosition(int col, int row)
	{

		board[col,row].transform.position=CalculateWorldPosition(col, row);

	}

	Vector3 CalculateWorldPosition(int col, int row)
	{
		float xPosition = boardCorner + col * boardPieceSpacing;
		float yPosition = boardCorner + row * boardPieceSpacing;

		Vector3 piecePosition = new Vector3(xPosition, yPosition,0f);

		return piecePosition;
	}

	public List<GameObject> GetNeighbors(GameObject queriedPiece)
	{
		List<GameObject> returnNeighbors = new List<GameObject>();

		Coords pieceIndex=GetIndexOf(queriedPiece);

		//north neighbor
		if (pieceIndex.y>0) returnNeighbors.Add(board[pieceIndex.x,pieceIndex.y-1]);
		//south neighbor
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

	void AttemptSwapPieces(GameObject piece1, GameObject piece2)
	{

		UnselectAll();

		MakeSwap(piece1,piece2);

		List<ThreeMatch> tempMatchList=gameController.GetThreeMatches();

		if (tempMatchList.Count>0) 
		{
			RemoveMatches(tempMatchList);
		}
		else 
		{
			MakeSwap(piece1,piece2);
		}

		GameController.gameState=GAMESTATE.MOVING;

	}

	void MakeSwap(GameObject piece1, GameObject piece2)
	{
		Coords piece1Coords = GetIndexOf (piece1);
		Coords piece2Coords = GetIndexOf (piece2);

		GameObject tempPiece = piece1;
		board[piece1Coords.x,piece1Coords.y]=piece2;
		board[piece2Coords.x,piece2Coords.y]=tempPiece;

		SnapToWorldPosition(piece1Coords.x,piece1Coords.y);
		SnapToWorldPosition(piece2Coords.x,piece2Coords.y);

	}
	
	public void RemoveMatches(List<ThreeMatch> matchesToRemove)
	{
		foreach (ThreeMatch match in matchesToRemove)
		{
			if (match.matchDirection==MATCHDIRECTION.HORIZONTAL)
			{
				DestroyImmediate(board[match.matchStart.x,match.matchStart.y]);
				DestroyImmediate(board[match.matchStart.x+1,match.matchStart.y]);
				DestroyImmediate(board[match.matchStart.x+2,match.matchStart.y]);
			}
			if (match.matchDirection==MATCHDIRECTION.VERTICAL)
			{
				DestroyImmediate(board[match.matchStart.x,match.matchStart.y]);
				DestroyImmediate(board[match.matchStart.x,match.matchStart.y+1]);
				DestroyImmediate(board[match.matchStart.x,match.matchStart.y+2]);
			}
		}

		MoveDownAndReplacePieces();
		AnimateMove();

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
		foreach (GameObject piece in board)
		{
			piece.GetComponent<PieceController>().SetSelected(false);
		}
	}

	public void TrySelect(GameObject pieceToTryToSelect)
	{
		//break out if we aren't in the selection state
		if (!(GameController.gameState==GAMESTATE.SELECTION)) return;

		PieceController pieceController = pieceToTryToSelect.GetComponent<PieceController>();

		//unselect the piece if it is currently selected and break out
		if (pieceController.GetSelected())
		{
			pieceController.SetSelected(false);
			return;
		}

		//check to see if each neighbor is selected, if so break out and try to swap them
		foreach (GameObject neighbor in GetNeighbors(pieceToTryToSelect))
		{
			if (neighbor.gameObject.GetComponent<PieceController>().GetSelected())
			{
				AttemptSwapPieces(pieceToTryToSelect,neighbor);
				return;
			}
		}

		//finally if none of the other conditions are true unselect all of the pieces and select the input piece
		UnselectAll();
		pieceController.SetSelected(true);

	}

	public GameObject[,] GetBoard()
	{
		return board;
	}

	//end public methods

}

public class Coords {
	public int x;
	public int y;

	public Coords(int inX, int inY)
	{
		x=inX;
		y=inY;
	}

	public string CoordString() {
		return x.ToString() + "," + y.ToString();
	}
}
