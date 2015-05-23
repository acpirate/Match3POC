using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum MATCHDIRECTION {HORIZONTAL, VERTICAL};
public enum GAMESTATE {SELECTION, TRYMATCHMOVE, MOVING, FAILMATCHMOVE, CONSOLE, ENDGAME};


public class GameController : MonoBehaviour {

	public static GAMESTATE gameState;
	public static bool quitting=false;
	public static bool resetting=false;
	public static int highScore=0;
	public static int score=0;


	public GameObject console;
	public Text scoreDisplay;
	public Text endScoreDisplay;
	public GameObject gameEndDisplay;
	public Text highScoreDisplay;
	public float hintTime=3f;

	[HideInInspector]
	public bool hintsShowing=false;

	BoardController boardController;
	GameObject[,] board;
	
	private GameObject piece1Tried;
	private GameObject piece2Tried;
	[HideInInspector]
	public float hintCountdown;
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

		hintCountdown=hintTime;
	}

	void Start()
	{
		score=0;
		board=boardController.GetBoard();
	}


	void Update()
	{
		switch (GameController.gameState) 
		{
			case GAMESTATE.MOVING:
				if (!ArePiecesMoving())
				{
					PiecesStoppedMoving();
				}
			break;
			case GAMESTATE.TRYMATCHMOVE:
				if (!ArePiecesMoving())
					TryMatchMoveStop();
			break;
			case GAMESTATE.FAILMATCHMOVE:
				if (!ArePiecesMoving())
					FailMatchMoveStop();
			break;
			case GAMESTATE.SELECTION:
				
				if (Input.GetKeyDown(KeyCode.Tab)) 
				{
					ToggleConsole();
				}
				if (!hintsShowing)
				{
					hintCountdown-=Time.deltaTime;
					if (hintCountdown<=0) HintDisplay();
				}	
				break;
			case GAMESTATE.CONSOLE:
				
				if (Input.GetKeyDown(KeyCode.Tab)) 
				{
					ToggleConsole();
				}
			break;
			case GAMESTATE.ENDGAME:
				if (Input.GetMouseButtonDown(0)) Application.LoadLevel("GameSelect");
			break;
		}

		//if (GameController.gameState==GAMESTATE.MOVING && !(ArePiecesMoving())) GameController.gameState=GAMESTATE.SELECTION;
	}

	void TryMatchMoveStop()
	{
		List<ThreeMatch> tempMatchList=GetThreeMatches();
		
		if (tempMatchList.Count>0) 
		{
			ScoreMatches(tempMatchList);
			boardController.RemoveMatches(tempMatchList);
			GameController.gameState=GAMESTATE.MOVING;
		}
		else 
		{
			boardController.AnimateMovePairPieces(piece1Tried,piece2Tried,GAMESTATE.FAILMATCHMOVE);
		}
	}

	void HintDisplay()
	{
		hintsShowing=true;
		hintCountdown=hintTime;
		List<Swap> possibleMatches = PossibleMatches();
		Swap swapHint=possibleMatches[Random.Range(0,possibleMatches.Count)];

		GameObject hintPiece1=board[swapHint.piece1Coords.x,swapHint.piece1Coords.y];
		GameObject hintpiece2=board[swapHint.piece2Coords.x,swapHint.piece2Coords.y];

		boardController.HintsOn(hintPiece1,hintpiece2);
	}

	void FailMatchMoveStop()
	{
		gameState=GAMESTATE.SELECTION;
	}

	void OnApplicationQuit()
	{
		quitting=true;
	}

	//end unity builtin methods

	//public methods

	public void SetTriedPieces(GameObject piece1, GameObject piece2)
	{
		piece1Tried=piece1;
		piece2Tried=piece2;

	}

	void ScoreMatches(List<ThreeMatch> matches)
	{
		foreach(ThreeMatch match in matches)
		{
			int tempScore=30;
			if (match.matchDirection==MATCHDIRECTION.HORIZONTAL)
				board[match.matchStart.x+1,match.matchStart.y].GetComponent<PieceController>().ShowScore(tempScore);
			else 
				board[match.matchStart.x,match.matchStart.y+1].GetComponent<PieceController>().ShowScore(tempScore);

			AddScore(tempScore);
		}
	}


	public void AddScore(int scoreToAdd)
	{
		score+=scoreToAdd;
		SetScoreDisplay();
	}

	void SetScoreDisplay() 
	{
		scoreDisplay.text="Score: "+score.ToString();
	}

	public void ResetBoard()
	{
		resetting=true;
		boardController.CreateBoard();
		resetting=false;
	}

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

	public void ChangePieceAction(int x, int y, string shape)
	{
		board[x,y].GetComponent<PieceController>().SetShapeFromString(shape);
	}	

	//end public methods

	//private methods
	public List<Swap> PossibleMatches()
	{
		List<Swap> foundMatches=new List<Swap>();

		for(int colCounter=0;colCounter<BoardController.boardSize;colCounter++)
		{
			for (int rowCounter=0;rowCounter<BoardController.boardSize;rowCounter++)
			{
				//east swap check
				if (colCounter<BoardController.boardSize-1) 
				{
					boardController.MakeSwap(board[colCounter,rowCounter],board[colCounter+1,rowCounter]);
					if (GetThreeMatches().Count>0)
					{
						Swap tempSwap=new Swap();
						tempSwap.piece1Coords=new Coords(colCounter,rowCounter);
						tempSwap.piece2Coords=new Coords(colCounter+1,rowCounter);
						foundMatches.Add(tempSwap);
					}
					boardController.MakeSwap(board[colCounter,rowCounter],board[colCounter+1,rowCounter]);
				}
				//south swap
				if (rowCounter<BoardController.boardSize-1) 
				{
					boardController.MakeSwap(board[colCounter,rowCounter],board[colCounter,rowCounter+1]);
					if (GetThreeMatches().Count>0)
					{
						Swap tempSwap=new Swap();
						tempSwap.piece1Coords=new Coords(colCounter,rowCounter);
						tempSwap.piece2Coords=new Coords(colCounter,rowCounter+1);
						foundMatches.Add(tempSwap);
					}
					boardController.MakeSwap(board[colCounter,rowCounter],board[colCounter,rowCounter+1]);
				}
			}
		}


		return foundMatches;
	}


	void ToggleConsole()
	{
		if (console.activeSelf) 
		{
			console.SetActive(false);
			gameState=GAMESTATE.SELECTION;
		}
		else 
		{
			console.SetActive(true);
			console.GetComponent<ConsoleController>().MakeInputActive();
			gameState=GAMESTATE.CONSOLE;
		}
	}

	void PiecesStoppedMoving()
	{
		List<ThreeMatch> matches = GetThreeMatches();

		if (matches.Count>0) 
		{
			ScoreMatches(matches);
			boardController.RemoveMatches(matches);
		}
		else if (PossibleMatches().Count==0)
		{
			EndGame();
		}
		else
		{
			gameState=GAMESTATE.SELECTION;
		}
	}

	public void EndGame()
	{
		if (gameState==GAMESTATE.CONSOLE)
			ToggleConsole();
		string highScoreString="Highscore: "+highScore.ToString();
		if (score>highScore) { 
			PlayerPrefs.SetInt("Highscore", score);
			PlayerPrefs.Save();
			highScoreString="New High Score!";
		}

		highScoreDisplay.text=highScoreString;
		gameState=GAMESTATE.ENDGAME;
		scoreDisplay.enabled=false;
		gameEndDisplay.SetActive(true);
		endScoreDisplay.text="Score: "+score;
		//Application.LoadLevel("GameSelect");
	}

	bool ArePiecesMoving()
	{
		bool piecesMoving=false;
		for(int colCounter=0;colCounter<BoardController.boardSize;colCounter++)
		{
			for (int rowCounter=0;rowCounter<BoardController.boardSize;rowCounter++)
			{
				if (board[colCounter,rowCounter].GetComponent<PieceController>().animateMove)
				{
					piecesMoving=true;
					break;
				}
			}
			if (piecesMoving) break;
		}

		return piecesMoving;
	}

	// end private methods

}

public class Swap {
	public Coords piece1Coords;
	public Coords piece2Coords;

	public string DisplayString()
	{
		return "piece1: "+piece1Coords.CoordString()+" piece2: "+piece2Coords.CoordString();
	}
}


public class ThreeMatch {

	public Coords matchStart;
	public MATCHDIRECTION matchDirection;

	public ThreeMatch(Coords inCoords,MATCHDIRECTION inMatchDirection)
	{
		matchStart=inCoords;
		matchDirection=inMatchDirection;
	}

	public string ThreeMatchString()
	{
		string directionString="right";
		if (matchDirection==MATCHDIRECTION.VERTICAL) directionString="up";
		return matchStart.CoordString()+" "+directionString;
	}
}