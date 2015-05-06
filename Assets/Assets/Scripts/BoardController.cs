using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum GAMESTATE {SELECTION, MOVING};

public class BoardController : MonoBehaviour {

	const int boardSize=8;

	public GameObject[] jewels;
	public float boardPieceOffest = 30f;
	public float boardPieceSpacing= 60f;

	private float boardCorner;
	private GameObject[,] board;
	private GAMESTATE gameState=GAMESTATE.SELECTION;

	//unity default methods

	void Awake() {
		boardCorner=boardPieceSpacing*(boardSize-2f)*-.5f-boardPieceOffest;

		board = new GameObject[boardSize,boardSize];


	}

	// Use this for initialization
	void Start () {
		RandomizeBoard();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	//end unity default methods

	//private methods


	GameObject GetRandomJewel()
	{
		int jewelIndex=Random.Range(0,jewels.Length);

		return jewels[jewelIndex];
	}

	void RandomizeBoard()
	{
		for(int colCounter=0;colCounter<boardSize;colCounter++)
		{
			for (int rowCounter=0;rowCounter<boardSize;rowCounter++)
			{
				board[colCounter,rowCounter] = Instantiate(GetRandomJewel());
				SetBoardPosition(colCounter,rowCounter);
			}
		}
	}

	void SetBoardPosition(int col, int row)
	{
		float xPosition = boardCorner + col * boardPieceSpacing;
		float yPosition = boardCorner + row * boardPieceSpacing;

		Vector3 piecePosition = new Vector3(xPosition, yPosition,0f);

		board[col,row].transform.position=piecePosition;

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

	Coords GetIndexOf(GameObject queriedPiece)
	{
		Coords returnCoords= new Coords();
		returnCoords.x=-1;
		returnCoords.y=-1;

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

	void SwapPieces(GameObject piece1, GameObject piece2)
	{
		gameState=GAMESTATE.MOVING;
		UnselectAll();

		Coords piece1Coords = GetIndexOf (piece1);
		Coords piece2Coords = GetIndexOf (piece2);

		//Debug.Log("piece1coords="+piece1Coords.CoordString()+" piece2coords="+piece2Coords.CoordString());

		GameObject tempPiece = piece1;
		board[piece1Coords.x,piece1Coords.y]=piece2;
		board[piece2Coords.x,piece2Coords.y]=tempPiece;

		SetBoardPosition(piece1Coords.x,piece1Coords.y);
		SetBoardPosition(piece2Coords.x,piece2Coords.y);
		gameState=GAMESTATE.SELECTION;
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
		if (!(gameState==GAMESTATE.SELECTION)) return;

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
				SwapPieces(pieceToTryToSelect,neighbor);
				return;
			}
		}

		//finally if none of the other conditions are true unselect all of the pieces and select the input piece
		UnselectAll();
		pieceController.SetSelected(true);

	}

	//end public methods

}

public class Coords {
	public int x;
	public int y;

	public string CoordString() {
		return x.ToString() + "," + y.ToString();
	}
}
