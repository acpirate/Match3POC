using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public enum MATCHDIRECTION {HORIZONTAL, VERTICAL};
public enum GAMESTATE {SELECTION, MOVING};


public class GameController : MonoBehaviour {

	public static GAMESTATE gameState;

	BoardController boardController;
	GameObject[,] board;

	//helper methods

	//get a random item of the given type
	public static T GetRandomEnum<T>()
	{
		System.Array A = System.Enum.GetValues(typeof(T));
		T V = (T)A.GetValue(UnityEngine.Random.Range(0,A.Length));
		return V;
	}
	//end helper methods

	//unity builtin methods
	
	void Awake() 
	{
		gameState=GAMESTATE.SELECTION;
		boardController=GameObject.FindGameObjectWithTag("Board").GetComponent<BoardController>();

	}

	void Start()
	{
		board=boardController.GetBoard();
	}

	//end unity builtin methods

	//public methods

	public List<ThreeMatch> GetThreeMatches()
	{

		List<ThreeMatch> threeMatches=new List<ThreeMatch>();

		//horizontal matches
		for(int colCounter=0;colCounter<BoardController.boardSize-2;colCounter++)
		{
			for (int rowCounter=0;rowCounter<BoardController.boardSize;rowCounter++)
			{
				//Debug.Log(colCounter.ToString()+","+rowCounter.ToString());
				//Debug.Log(board[colCounter,rowCounter].GetComponent<PieceController>().myShape);
				if (board[colCounter,rowCounter].GetComponent<PieceController>().myShape ==
				    board[colCounter+1,rowCounter].GetComponent<PieceController>().myShape &&
				    board[colCounter,rowCounter].GetComponent<PieceController>().myShape ==
				    board[colCounter+2,rowCounter].GetComponent<PieceController>().myShape)
				{
					threeMatches.Add(new ThreeMatch(new Coords(colCounter,rowCounter),MATCHDIRECTION.HORIZONTAL));
				}
			}
		}

		//vertical matches
		for(int colCounter=0;colCounter<BoardController.boardSize;colCounter++)
		{
			for (int rowCounter=0;rowCounter<BoardController.boardSize-2;rowCounter++)
			{
				if (board[colCounter,rowCounter].GetComponent<PieceController>().myShape ==
				    board[colCounter,rowCounter+1].GetComponent<PieceController>().myShape &&
				    board[colCounter,rowCounter].GetComponent<PieceController>().myShape ==
				    board[colCounter,rowCounter+2].GetComponent<PieceController>().myShape)
				{
					threeMatches.Add(new ThreeMatch(new Coords(colCounter,rowCounter),MATCHDIRECTION.VERTICAL));
				}
			}
		}


		return threeMatches;

	}

	public bool CheckMatches()
	{
		bool isMatches=false;

		if (GetThreeMatches().Count>0) isMatches=true;
		return isMatches;
	}

	//end public methods

}

public class ThreeMatch {

	public Coords matchStart;
	public MATCHDIRECTION matchDirection;

	public ThreeMatch(Coords inCoords,MATCHDIRECTION inMatchDirection)
	{
		matchStart=inCoords;
		matchDirection=inMatchDirection;
	}
}