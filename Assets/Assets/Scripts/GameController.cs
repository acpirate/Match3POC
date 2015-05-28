using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public enum MATCHDIRECTION {HORIZONTAL, VERTICAL};
public enum GAMESTATE {SELECTION, TRYMATCHMOVE, MOVING, FAILMATCHMOVE, CONSOLE, ENDGAME};
public enum MATCHTYPE {THREEMATCH, FOURMATCH};

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
				{	
					TryMatchMoveStop();
				}
			break;
			case GAMESTATE.FAILMATCHMOVE:
				if (!ArePiecesMoving())
				{
					FailMatchMoveStop();
				}
			break;
			case GAMESTATE.SELECTION:
				if (Input.GetKeyDown(KeyCode.Tab)) 
				{
					ToggleConsole();
				}
				if (!hintsShowing)
				{
					hintCountdown-=Time.deltaTime;
					//if (hintCountdown<=0) HintDisplay();
				}	
				break;
			case GAMESTATE.CONSOLE:
				if (Input.GetKeyDown(KeyCode.Tab)) 
				{
					ToggleConsole();
				}
			break;
			case GAMESTATE.ENDGAME:
				if (Input.GetMouseButtonDown(0))
				{	
					Application.LoadLevel("GameSelect");
				}
			break;
		}
	} //End Update

	void TryMatchMoveStop()
	{
		List<Match> tempMatchList=GetBaseMatches();
		
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
	} //End TryMatchMoveStop

	void HintDisplay()
	{
		hintsShowing=true;
		hintCountdown=hintTime;
		List<Swap> possibleMatches = PossibleMatches();
		Swap swapHint=possibleMatches[Random.Range(0,possibleMatches.Count)];

		GameObject hintPiece1=board[swapHint.piece1Coords.x,swapHint.piece1Coords.y];
		GameObject hintpiece2=board[swapHint.piece2Coords.x,swapHint.piece2Coords.y];

		boardController.HintsOn(hintPiece1,hintpiece2);
	} //End HintDisplay

	void FailMatchMoveStop()
	{
		gameState=GAMESTATE.SELECTION;
	} //End FailMatchMoveStop

	void OnApplicationQuit()
	{
		quitting=true;
	} //End OnApplicationQuit

	//end unity builtin methods
	

	public void SetTriedPieces(GameObject piece1, GameObject piece2)
	//sets reference to the activated pieces so they can be put back if there is no match
	{
		piece1Tried=piece1;
		piece2Tried=piece2;
	} //End SetTriedPieces
	

	//sort matches into 3, 4, and 5 matches
	public List<Match> DesignateBlobMatches(List<Match> reportedMatches)
	{
		List<Match> calculatedMatches=new List<Match>();



		return calculatedMatches;
	}

	//returns the coords of the match and any adjacent for 5 matches
	List<Coords> MatchAndNeighborCoords(Match inMatch)
	{
		List<Coords> coordsStorage=new List<Coords>();

		//iterate over each piece 
		foreach(Coords pieceCoords in inMatch.matchCoords)
		{
			bool mainListCheck=false;
			foreach(Coords storageCoords in coordsStorage)
			{
				if (storageCoords==pieceCoords)
					mainListCheck=true;
			}
			if (!(mainListCheck))
			{
				coordsStorage.Add(pieceCoords);
			}

			//check each neighbor to see if they are in the main list
			foreach(Coords pieceAndNeighborCoords in NeighborCoods(pieceCoords))
			{
				bool neighborCheck=false;
				foreach(Coords storageCoords in coordsStorage)
				{
					if (pieceAndNeighborCoords==storageCoords)
						neighborCheck=true;
				}
				if (!(neighborCheck))
				{
					coordsStorage.Add(pieceAndNeighborCoords);
				}
			}

		}

		return coordsStorage;
	}

	//returns the neighboring coords to the input coords
	List<Coords> NeighborCoods(Coords inCoords)
	{
		List<Coords> neighborCoords=new List<Coords>();
		//north neighbor
		if (inCoords.y<BoardController.boardSize-1)
		{
			neighborCoords.Add(new Coords(inCoords.x, inCoords.y+1));
		}
		//south neighbor
		if (inCoords.y>0)
		{
			neighborCoords.Add(new Coords(inCoords.x, inCoords.y-1));
		}
		//east neighbor
		if (inCoords.x<BoardController.boardSize-1)
		{
			neighborCoords.Add(new Coords(inCoords.x+1, inCoords.y));
		}
		//west neighbor
		if (inCoords.x>0)
		{
			neighborCoords.Add(new Coords(inCoords.x-1, inCoords.y));
		}

		return neighborCoords;
	}

	//returns true if the there are any equalities in the two input coordinate lists
	bool CorrespondingCoords(List<Coords> testMatchCoords1, List<Coords> testMatchCoords2)
	{
		bool coordsMatch=false;

		foreach (Coords outerCoords in testMatchCoords1)
		{
			foreach (Coords innerCoords in testMatchCoords2)
			{
				if (outerCoords==innerCoords)
				{
					coordsMatch=true;
					break;
				}
			}
			if (coordsMatch) break;
		}

		return coordsMatch;
	}

	//create score popup effect
	void PopScore(Match matchToScore, int scoreValue)
	{
		Vector3 scoreLocation=Vector3.zero;
		Color scoreColor=Color.black;

		GameObject samplePiece=board[matchToScore.matchCoords[0].x,matchToScore.matchCoords[0].y];
		scoreColor=samplePiece.GetComponent<MeshRenderer>().material.color;

		scoreLocation=FindCenterPieceLocationOfMatch(matchToScore);

		boardController.ShowScore(scoreValue,scoreLocation,scoreColor);
		
	}

	Vector3 FindCenterPieceLocationOfMatch(Match inMatch)
	{
		float averageX=0;
		float averageY=0;

		foreach(Coords pieceCoords in inMatch.matchCoords)
		{
			averageX+=board[pieceCoords.x,pieceCoords.y].transform.position.x;
			averageY+=board[pieceCoords.x,pieceCoords.y].transform.position.y;
		}
		averageX=averageX/inMatch.matchCoords.Count;
		averageY=averageY/inMatch.matchCoords.Count;

		Vector3 centerPoint=new Vector3(averageX,averageY,0);
		float shortestDistance=10000f;


		GameObject tempPiece=board[inMatch.matchCoords[0].x,inMatch.matchCoords[0].y];

		foreach(Coords pieceCoords in inMatch.matchCoords)
		{
			Vector3 offset = centerPoint - board[pieceCoords.x,pieceCoords.y].transform.position;
			float centerDistance = offset.sqrMagnitude;
			if (centerDistance<shortestDistance)
			{
				tempPiece=board[pieceCoords.x,pieceCoords.y];
				shortestDistance=centerDistance;
			}
		}

		return tempPiece.transform.position;

	}


	void ScoreMatches(List<Match> reportedMatches)
	{
		foreach(Match match in reportedMatches)
		{
			switch (match.matchCoords.Count)
			{
				case 3:
					PopScore(match,30);
				break;

				case 4:
					PopScore(match,60);
				break;
				default:
					PopScore(match,100);
				break;
			}
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


	//return a list of all of the shape matches currently showing
	public List<Match> GetBaseMatches()
	{

		List<Match> baseMatches=new List<Match>();

		SHAPE baseShape=SHAPE.NONE;
		int runCount=1;

		//vertical matches
		for(int colCounter=0;colCounter<BoardController.boardSize;colCounter++)
		{
			baseShape=SHAPE.NONE;
			runCount=1;
			Match tempMatch=new Match();
			for (int rowCounter=0;rowCounter<BoardController.boardSize;rowCounter++)
			{
				PieceController tempPieceController=boardController.GetPieceAtCoords(new Coords(colCounter,rowCounter)).GetComponent<PieceController>();
				if (tempPieceController.myShape==baseShape)
				{
					runCount++;
					if (runCount==3)
					{
						tempMatch.matchCoords.Add(new Coords(colCounter,rowCounter));
						tempMatch.matchCoords.Add(new Coords(colCounter,rowCounter-1));
						tempMatch.matchCoords.Add(new Coords(colCounter,rowCounter-2));
						baseMatches.Add(tempMatch);
					}
					if (runCount>3)
					{
						tempMatch.matchCoords.Add(new Coords(colCounter,rowCounter));
					}
				}
				//the piece doesnt match the previous match
				else 
				{
					//add the previous match to the matchlist if the runcount > 2
					runCount=1;
					baseShape=tempPieceController.myShape;
					tempMatch=new Match(baseShape);

				}

			}
		}

		//horizontal matches
		for(int rowCounter=0;rowCounter<BoardController.boardSize;rowCounter++)
		{
			baseShape=SHAPE.NONE;
			runCount=1;
			Match tempMatch=new Match();
			for (int colCounter=0;colCounter<BoardController.boardSize;colCounter++)
			{
				PieceController tempPieceController=boardController.GetPieceAtCoords(new Coords(colCounter,rowCounter)).GetComponent<PieceController>();
				if (tempPieceController.myShape==baseShape)
				{
					runCount++;
					if (runCount==3)
					{
						tempMatch.matchCoords.Add(new Coords(colCounter,rowCounter));
						tempMatch.matchCoords.Add(new Coords(colCounter-1,rowCounter));
						tempMatch.matchCoords.Add(new Coords(colCounter-2,rowCounter));
					}
					if (runCount>3)
					{
						tempMatch.matchCoords.Add(new Coords(colCounter,rowCounter));
					}
				}
				//the piece doesnt match the previous match
				else 
				{
					//add the previous match to the matchlist if the runcount > 2
					if (runCount>2)
					{
						baseMatches.Add(tempMatch);
					}
					runCount=1;
					baseShape=tempPieceController.myShape;
					tempMatch=new Match(baseShape);
					
				}
				
			}
		}


		return baseMatches;

	}
	//called by changepiece console command
	public void ChangePieceAction(int x, int y, string shape)
	{
		board[x,y].GetComponent<PieceController>().SetShapeFromString(shape);
	}	

	//called by loadboardstate console command	
	public void ChangePieceAction(int x, int y, SHAPE inShape)
	{
		board[x,y].GetComponent<PieceController>().SetShape(inShape);
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
					if (GetBaseMatches().Count>0)
					{
						Swap tempSwap;
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
					if (GetBaseMatches().Count>0)
					{
						Swap tempSwap;
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
		List<Match> matches = GetBaseMatches();

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




//public classes


//holds a set of coordinates for pieces to swap
public struct Swap 
{
	public Coords piece1Coords;
	public Coords piece2Coords;


	// Override the ToString
	public override string ToString()
	{
		return "piece1: "+piece1Coords.ToString()+" piece2: "+piece2Coords.ToString();
	}
}

public struct Match
{
	public List<Coords> matchCoords;
	public SHAPE matchShape;


	public Match(SHAPE inShape)
	{
		matchCoords=new List<Coords>();
		matchShape=inShape;
	}

	// Override the ToString
	public override string ToString()
	{
		string matchString="";

		matchString+="- "+matchCoords.Count.ToString()+" piece match of shape "+matchShape.ToString()+"\n";
		matchString+="- Coords: ";

		foreach(Coords coords in matchCoords)
		{
			matchString+=coords.ToString()+"|";
		}

		return matchString;
	}
}


