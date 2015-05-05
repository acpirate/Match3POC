using UnityEngine;
using System.Collections;

public class BoardController : MonoBehaviour {

	const int boardSize=8;

	public GameObject[] jewels;
	public float boardPieceOffest = 30f;
	public float boardPieceSpacing= 60f;

	private float boardCorner;
	private GameObject[,] board;

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
				board[rowCounter,colCounter] = Instantiate(GetRandomJewel());
				SetBoardPosition(colCounter,rowCounter);
			}
		}
	}

	void SetBoardPosition(int col, int row)
	{
		float xPosition = boardCorner + row * boardPieceSpacing;
		float yPosition = boardCorner + col * boardPieceSpacing;

		Vector3 piecePosition = new Vector3(xPosition, yPosition,0f);

		board[row,col].transform.position=piecePosition;

	}


}
