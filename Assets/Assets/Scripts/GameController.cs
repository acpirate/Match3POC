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
		List<MatchThreeOrFour> tempMatchList=GetThreeMatches();
		
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

	void RemoveSubmatchFrom4Match(List<MatchThreeOrFour> matches, MatchThreeOrFour parent4Match)
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

	void RemoveSubmatchFrom5Match(List<MatchThreeOrFour> matches, MatchThreeOrFour parent5Match)
	{
		int removeXoffset=0;
		int removeYoffset=0;
		
		if (parent5Match.matchDirection==MATCHDIRECTION.HORIZONTAL)
		{
			removeXoffset=1;
		}
		if (parent5Match.matchDirection==MATCHDIRECTION.VERTICAL)
		{
			removeYoffset=1;
		}
		
		for (int i=0;i<matches.Count;i++)
		{
			if (matches[i].matchStart.x==parent5Match.matchStart.x+removeXoffset &&
			    matches[i].matchStart.y==parent5Match.matchStart.y+removeYoffset)
			{
				matches[i]=null;
				break;
			}
		}
	}

	//sort matches into 3, 4, and 5 matches
	public MatchesContainer SortMatches(List<MatchThreeOrFour> reportedMatches)
	{
		MatchesContainer calculatedMatches=new MatchesContainer();

		//sorting matches into 3 and 4 matches
		foreach(MatchThreeOrFour match in reportedMatches)
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
		foreach (MatchThreeOrFour fourMatch in calculatedMatches.fourMatches)
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
				MatchThreeOrFour tempMatch=calculatedMatches.threeMatches[i];

				if (tempMatch.matchStart.x==testCoords.x && tempMatch.matchStart.y==testCoords.y && tempMatch.matchDirection==fourMatch.matchDirection)
				{
					calculatedMatches.threeMatches.RemoveAt(i);
				}
			}

		}
		//remove empty slots from threematch list
		calculatedMatches.threeMatches.TrimExcess();


		//sortFiveMathches

		//(outer loop) foreach match
		//	add outer loop match pieces coords to temporary 5 match
		//  (inner loop) iterate over all matches
		//  	if the inner matches' shape corresponds to outer match
		//			for every piece in the outer match
		//				check if any piece in the inner match is adjacent OR equal to the coords of the outer match piece
		//					
		calculatedMatches=Straight5MatchRevise(calculatedMatches);


		return calculatedMatches;
	}

	MatchesContainer Straight5MatchRevise(MatchesContainer inMatchesContainer)
	{	
		//return matchescontainer object
		MatchesContainer revisedSortedMatches=new MatchesContainer();
		//threematches transferred to new matchescontainer object
		revisedSortedMatches.threeMatches=inMatchesContainer.threeMatches;
		//temporary storage of four matches
		List<MatchThreeOrFour> fourMatchStorage=new List<MatchThreeOrFour>(inMatchesContainer.fourMatches);
		//temporary storage of five matches
		List<MatchThreeOrFour> straight5MatchStorage=new List<MatchThreeOrFour>();

		//sort the 4 matches into 4 and 5 matches
		for(int i=0;i<fourMatchStorage.Count;i++)
		{
			if (fourMatchStorage[i]!=null)
			{
				MatchThreeOrFour workingMatch=fourMatchStorage[i];
				if (IsStraightFiveMatch(workingMatch))
				{
					straight5MatchStorage.Add(workingMatch);
					RemoveSubmatchFrom5Match(fourMatchStorage,workingMatch);
				}
				else 
				{
					revisedSortedMatches.fourMatches.Add(workingMatch);
				}
			}
		}

		//iterate over 5 matches and add the coordinates to the revised matchcontainer
		foreach(MatchThreeOrFour fiveMatch in straight5MatchStorage)
		{
			revisedSortedMatches.fiveMatches.Add(new MatchFiveOrMore(TranslateMatchToCoords(fiveMatch)));
		}

		return revisedSortedMatches;
	}

	//returns list of coordinates for input 5 match
	List<Coords> TranslateMatchToCoords(MatchThreeOrFour in5Match)
	{
		List<Coords> translatedCoords=new List<Coords>();

		Coords matchOffSetCoords=new Coords(0,0);

		if (in5Match.matchDirection==MATCHDIRECTION.HORIZONTAL)
		{
			matchOffSetCoords.x=1;
		}
		if (in5Match.matchDirection==MATCHDIRECTION.VERTICAL)
		{
			matchOffSetCoords.y=1;
		}

		Coords startCoords=in5Match.matchStart;

		for(int i=0;i<5;i++)
		{
			translatedCoords.Add(new Coords(startCoords.x+i*matchOffSetCoords.x,startCoords.y+i*matchOffSetCoords.y));
		}

		return translatedCoords;
	}

	//create score popup effect
	void PopScore(MatchThreeOrFour matchToScore, int scoreValue, float scoreOffsetValue)
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

	void PopFiveMatchScore(MatchFiveOrMore matchToScore, int scoreValue)
	{
		Vector3 scoreLocation=Vector3.zero;
		Color scoreColor=Color.black;

		GameObject samplePiece=board[matchToScore.matchFivePieces[0].x,matchToScore.matchFivePieces[0].y];
		scoreColor=samplePiece.GetComponent<MeshRenderer>().material.color;

		scoreLocation=FindCenterPieceLocationOfFiveMatch(matchToScore);

		boardController.ShowScore(scoreValue,scoreLocation,scoreColor);
		
	}

	Vector3 FindCenterPieceLocationOfFiveMatch(MatchFiveOrMore in5Match)
	{
		float averageX=0;
		float averageY=0;

		foreach(Coords pieceCoords in in5Match.matchFivePieces)
		{
			averageX+=board[pieceCoords.x,pieceCoords.y].transform.position.x;
			averageY+=board[pieceCoords.x,pieceCoords.y].transform.position.y;
		}
		averageX=averageX/in5Match.matchFivePieces.Count;
		averageY=averageY/in5Match.matchFivePieces.Count;

		Vector3 centerPoint=new Vector3(averageX,averageY,0);
		float shortestDistance=10000f;


		GameObject tempPiece=board[in5Match.matchFivePieces[0].x,in5Match.matchFivePieces[0].y];

		foreach(Coords pieceCoords in in5Match.matchFivePieces)
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


	void ScoreMatches(List<MatchThreeOrFour> reportedMatches)
	{
		MatchesContainer allMatches=SortMatches(reportedMatches);


		foreach (MatchThreeOrFour threeMatch in allMatches.threeMatches)
		{
			PopScore(threeMatch,30,50f);
			AddScore(30);
		}

		foreach (MatchThreeOrFour fourMatch in allMatches.fourMatches)
		{
			PopScore(fourMatch,60,75f);
			AddScore(60);
		}

		foreach (MatchFiveOrMore fiveMatch in allMatches.fiveMatches)
		{
			PopFiveMatchScore(fiveMatch,100);
			AddScore(100);
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

	public List<MatchThreeOrFour> GetThreeMatches()
	{

		List<MatchThreeOrFour> threeMatches=new List<MatchThreeOrFour>();

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
					threeMatches.Add(new MatchThreeOrFour(new Coords(colCounter,rowCounter),MATCHDIRECTION.HORIZONTAL));
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
					threeMatches.Add(new MatchThreeOrFour(new Coords(colCounter,rowCounter),MATCHDIRECTION.VERTICAL));
				}
			}
		}


		return threeMatches;

	}

	public bool IsFourMatch(MatchThreeOrFour matchToCheck)
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

	//checks if a 4 match is a straight 5 match
	public bool IsStraightFiveMatch(MatchThreeOrFour matchToCheck)
	{
		//Debug.Log("checking for fourmatch "+matchToCheck.MatchDisplayString());
		
		bool fiveMatchTest=false;
		
		GameObject matchStartPiece=board[matchToCheck.matchStart.x,matchToCheck.matchStart.y];
		PieceController startPieceController=matchStartPiece.GetComponent<PieceController>();
		GameObject endPieceController=new GameObject();
		
		if (matchToCheck.matchDirection==MATCHDIRECTION.HORIZONTAL)
		{
			if (matchToCheck.matchStart.x<4)
			{
				endPieceController=board[matchToCheck.matchStart.x+4,matchToCheck.matchStart.y];
			}
		}
		if (matchToCheck.matchDirection==MATCHDIRECTION.VERTICAL)
		{
			if (matchToCheck.matchStart.y<4)
			{
				endPieceController=board[matchToCheck.matchStart.x,matchToCheck.matchStart.y+4];
			}
		}
		//Debug.Log("start "+startPieceController.GetType().ToString());
		//Debug.Log("end "+endPieceController.GetType().ToString());
		if (endPieceController.GetComponent<PieceController>()!=null)
			if (startPieceController.myShape==endPieceController.GetComponent<PieceController>().myShape)
			{
				fiveMatchTest=true;
			}
		
		return fiveMatchTest;
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
		List<MatchThreeOrFour> matches = GetThreeMatches();

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

//holds information about a match three or four
public class MatchThreeOrFour 
{

	public Coords matchStart;
	public MATCHDIRECTION matchDirection;

	public MatchThreeOrFour(Coords inCoords,MATCHDIRECTION inMatchDirection)
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

//a 5 match is a list of piece coordinates
public class MatchFiveOrMore
{
	public List<Coords> matchFivePieces;
	public SHAPE matchFiveShape;

	public MatchFiveOrMore(List<Coords> inStartCoords)
	{
		matchFivePieces=inStartCoords;
	}

}

//holds a set of matches for a boardstate
public class MatchesContainer
{
	public List<MatchThreeOrFour> threeMatches=new List<MatchThreeOrFour>();
	public List<MatchThreeOrFour> fourMatches=new List<MatchThreeOrFour>();
	public List<MatchFiveOrMore> fiveMatches=new List<MatchFiveOrMore>();
}
