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
					ReplacementPiecesStoppedMoving();
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
		List<Match> tempMatchList=GetThreeMatches();
		
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

	void RemoveSubmatchFrom4Match(List<Match> matches, Match parent4Match)
	{
		int removeXoffset=0;
		int removeYoffset=0;

		if (parent4Match.matchDirection==MATCHDIRECTION.HORIZONTAL)
		{
			removeXoffset=1;
		}
		if (parent4Match.matchDirection==MATCHDIRECTION.VERTICAL)
		{
			removeYoffset=1;
		}

		for (int i=0;i<matches.Count;i++)
		{
			if (matches[i].matchStart.x==parent4Match.matchStart.x+removeXoffset &&
			    matches[i].matchStart.y==parent4Match.matchStart.y+removeYoffset)
			{
				matches[i]=null;
				break;
			}
		}
	}

	public MatchesContainer SortMatches(List<Match> reportedMatches)
	{
		MatchesContainer calculatedMatches=new MatchesContainer();

		//sorting matches into 3 and 4 matches
		foreach(Match match in reportedMatches)
		{
			if (IsFourMatch(match))
			{
				calculatedMatches.fourMatches.Add(match);
			}
			else
			{
				calculatedMatches.threeMatches.Add(match);
			}
		}

		//remove overlap 3 matches form 3 match list
		if (calculatedMatches.fourMatches.Count>0)
		foreach (Match fourMatch in calculatedMatches.fourMatches)
		{
			Coords offsetCoords=new Coords(0,0);

			if (fourMatch.matchDirection==MATCHDIRECTION.HORIZONTAL)
			{
				offsetCoords.x=1;
			}
			if (fourMatch.matchDirection==MATCHDIRECTION.VERTICAL)
			{
				offsetCoords.y=1;
			}

			Coords testCoords=new Coords(fourMatch.matchStart.x+offsetCoords.x,
			                             fourMatch.matchStart.y+offsetCoords.y);

			for(int i=0;i<calculatedMatches.threeMatches.Count;i++)
			{
				Match tempMatch=calculatedMatches.threeMatches[i];

				if (tempMatch.matchStart.x==testCoords.x && tempMatch.matchStart.y==testCoords.y && tempMatch.matchDirection==fourMatch.matchDirection)
				{
					calculatedMatches.threeMatches.RemoveAt(i);
				}
			}

		}

		calculatedMatches.threeMatches.TrimExcess();

		return calculatedMatches;
	}


	void PopScore(Match matchToScore, int scoreValue, float scoreOffsetValue)
	{
		Vector3 scoreLocation=Vector3.zero;
		Vector3 scoreOffset=Vector3.zero;
		Color scoreColor=Color.black;

		if (matchToScore.matchDirection==MATCHDIRECTION.HORIZONTAL)
		{
			scoreOffset=new Vector3(scoreOffsetValue,0,0);
		}
		if (matchToScore.matchDirection==MATCHDIRECTION.VERTICAL)
		{
			scoreOffset=new Vector3(0,scoreOffsetValue,0);
		}

		GameObject tempPiece=board[matchToScore.matchStart.x,matchToScore.matchStart.y];

		scoreLocation=tempPiece.transform.position+scoreOffset;
		scoreColor=tempPiece.GetComponent<MeshRenderer>().material.color;

		boardController.ShowScore(scoreValue,scoreLocation,scoreColor);

	}

	void ScoreMatches(List<Match> reportedMatches)
	{
		MatchesContainer allMatches=SortMatches(reportedMatches);


		foreach (Match threeMatch in allMatches.threeMatches)
		{
			PopScore(threeMatch,30,50f);
			AddScore(30);
		}

		foreach (Match fourMatch in allMatches.fourMatches)
		{
			PopScore(fourMatch,60,75f);
			AddScore(60);
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

	public List<Match> GetThreeMatches()
	{

		List<Match> threeMatches=new List<Match>();

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
					threeMatches.Add(new Match(new Coords(colCounter,rowCounter),MATCHDIRECTION.HORIZONTAL));
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
					threeMatches.Add(new Match(new Coords(colCounter,rowCounter),MATCHDIRECTION.VERTICAL));
				}
			}
		}


		return threeMatches;

	}

	public bool IsFourMatch(Match matchToCheck)
	{
		//Debug.Log("checking for fourmatch "+matchToCheck.MatchDisplayString());

		bool fourMatchTest=false;

		GameObject matchStartPiece=board[matchToCheck.matchStart.x,matchToCheck.matchStart.y];
		PieceController startPieceController=matchStartPiece.GetComponent<PieceController>();
		GameObject endPieceController=new GameObject();

		if (matchToCheck.matchDirection==MATCHDIRECTION.HORIZONTAL)
		{
			if (matchToCheck.matchStart.x<5)
			{
				endPieceController=board[matchToCheck.matchStart.x+3,matchToCheck.matchStart.y];
			}
		}
		if (matchToCheck.matchDirection==MATCHDIRECTION.VERTICAL)
		{
			if (matchToCheck.matchStart.y<5)
			{
				endPieceController=board[matchToCheck.matchStart.x,matchToCheck.matchStart.y+3];
			}
		}
		//Debug.Log("start "+startPieceController.GetType().ToString());
		//Debug.Log("end "+endPieceController.GetType().ToString());
		if (endPieceController.GetComponent<PieceController>()!=null)
		if (startPieceController.myShape==endPieceController.GetComponent<PieceController>().myShape) fourMatchTest=true;

		return fourMatchTest;
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

	void ReplacementPiecesStoppedMoving()
	{
		List<Match> matches = GetThreeMatches();

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

//holds a set of coordinates for pieces to swap
public class Swap 
{
	public Coords piece1Coords;
	public Coords piece2Coords;

	public string DisplayString()
	{
		return "piece1: "+piece1Coords.CoordString()+" piece2: "+piece2Coords.CoordString();
	}
}

//holds information about a match
public class Match 
{

	public Coords matchStart;
	public MATCHDIRECTION matchDirection;

	public Match(Coords inCoords,MATCHDIRECTION inMatchDirection)
	{
		matchStart=inCoords;
		matchDirection=inMatchDirection;
	}

	public string MatchDisplayString()
	{
		string directionString="right";
		if (matchDirection==MATCHDIRECTION.VERTICAL) directionString="up";
		return matchStart.CoordString()+" "+directionString;
	}
}

//holds a set of matches for a boardstate
public class MatchesContainer
{
	public List<Match> threeMatches=new List<Match>();
	public List<Match> fourMatches=new List<Match>();
}
