using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class ConsoleController : MonoBehaviour {
	[SerializeField]
	private InputField consoleInput;
	[SerializeField]
	private Text consoleOutput;
	[SerializeField]
	private int consoleLines;

	public GameController gameController;
	public BoardController boardController;

	private GameObject[,] board;


	void Awake()
	{
		board=boardController.GetBoard();
	}

	void OnEnable()
	{	
		//Debug.Log("consoleenabled");
		//consoleInput.ActivateInputField();
		consoleInput.ActivateInputField();
		consoleInput.text="";
		//EventSystem.current.SetSelectedGameObject(consoleInput.gameObject, null);
		//consoleInput.OnPointerClick(new PointerEventData(EventSystem.current));

	}
	
	// Use this for initialization
	void Start () {
		InputField.SubmitEvent submitEvent = new InputField.SubmitEvent();
		submitEvent.AddListener(ConsoleCommand);
		consoleInput.onEndEdit = submitEvent;
		consoleInput.ActivateInputField();
		MakeInputActive();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void MakeInputActive()
	{
		consoleInput.ActivateInputField();
	}

	void ConsoleCommand(string commandString)
	{
		consoleInput.text="";
		ConsoleOutputAdd(">" + commandString);
		ConsoleCommandParse(commandString);
		consoleInput.ActivateInputField();
	}

	void ConsoleOutputAdd(string outputToAdd)
	{
		if (consoleOutput.text!="" && consoleOutput.text.Split('\n').Length<consoleLines+1) consoleOutput.text+="\n";
		consoleOutput.text+=outputToAdd;

		string[] consoleText=consoleOutput.text.Split('\n');

		if (consoleText.Length>consoleLines)
		{
			consoleOutput.text="";
			for(int i=1;i<consoleText.Length;i++)
			{
				consoleOutput.text+=consoleText[i];
				if (i<consoleLines) consoleOutput.text+="\n";
			}
		}

	}

	void ConsoleCommandParse(string commandString)
	{
		string[] commandTokens=commandString.ToUpper().Split(' ');

		switch (commandTokens[0]) 
		{
			case "HELP" :
			{
				HelpCommand(commandTokens);
				break;
			}
			case "?":
			{
				HelpCommand(commandTokens);
				break;
			}
			case "CHANGEPIECE":
			{
				ChangePieceCommand(commandTokens);
				break;
			}
			case "SHOWPOSSIBLELINEMATCHES":
			{
				ShowPossibleLineMatchesCommand();
				break;
			}
			case "SHOWCURRENTLINEMATCHES":
			{
				ShowCurrentLineMatchesCommand();
				break;
			}
			case "RESETBOARD":
			{
				ResetBoardCommand(commandTokens);
				break;
			}
			case "ENDGAME":
			{
				EndGameCommand();
				break;
			}
			case "SETHIGHSCORE":
			{
				SetHighScoreCommandValidate(commandTokens);
				break;
			}
			case "SHOWBOARDSTRING":
			{
				ShowBoardStringCommand();
				break;
			}
			case "SAVEBOARDSTATE":
			{
				SaveBoardStateCommand();
				break;
			}
			case "LOADBOARDSTATE":
			{
				LoadBoardStateCommand();
				break;
			}
			case "SHOWCURRENTBLOBMATCHES":
			{
				ShowCurrentBlobMatchesCommnad();
				break;
			}
		default:
			{
				ConsoleOutputAdd("- Invalid Command: "+commandTokens[0]);
				break;
			}

		}
	}

	void ShowCurrentBlobMatchesCommnad()
	{
		List<Match> currentMatches=gameController.DesignateBlobMatches(gameController.GetBaseMatches());
		
		if (currentMatches.Count==0)
		{
			ConsoleOutputAdd("- no current matches");
			return;
		}
		
		ConsoleOutputAdd("- current matches:");
		foreach(Match match in currentMatches)
		{
			ConsoleOutputAdd(match.ToString());
		}
	}

	void LoadBoardStateCommand()
	{

		string boardString=PlayerPrefs.GetString("BoardState");

		if (boardString=="")
		{
			ConsoleOutputAdd("- No saved board state");
			return;
		}

		ConsoleOutputAdd("- Loading Board State");

		foreach(string pieceString in boardString.Split('|'))
		{
			if (pieceString.Length==3)
			{
				string xString=pieceString.Substring(0,1);
				string yString=pieceString.Substring(1,1);
				string shapeString=pieceString.Substring(2,1);

				gameController.ChangePieceAction(int.Parse(xString), int.Parse(yString), (SHAPE) int.Parse(shapeString));

				//Debug.Log("xcoord: "+xString+" ycoord: "+yString+" shape: "+(SHAPE) int.Parse(shapeString));
			}
		}
	


	}

	void SaveBoardStateCommand()
	{
		ConsoleOutputAdd("- Saving board state");
		PlayerPrefs.SetString("BoardState",boardController.BoardString());
	}

	void ShowBoardStringCommand()
	{
		ConsoleOutputAdd("- Board State String");
		ConsoleOutputAdd(boardController.BoardString());
	}


	void SetHighScoreCommand(int highScoreToSet)
	{
		GameController.highScore=highScoreToSet;
		ConsoleOutputAdd("Setting high score to "+highScoreToSet.ToString());
	}

	void SetHighScoreCommandValidate(string[] commandTokens)
	{
		string usageInfo="- Usage: SETHIGHSCORE <Score>";

		//validate number of arguments
		if (commandTokens.Length!=2) {
			ConsoleOutputAdd("- Invalid number of arguments to sethighscore\n"+
			                 usageInfo);
			return;

		}

		//validate int
		int tempHighScore=0;
		if (!(int.TryParse(commandTokens[1],out tempHighScore)))
		{
			ConsoleOutputAdd("- Invalid highscore value: '"+commandTokens[1]+"'\n"+usageInfo);
			return;
		}

		SetHighScoreCommand(tempHighScore);
	}

	void EndGameCommand()
	{
		gameController.EndGame();
	}

	void ResetBoardCommand(string[] commandTokens)
	{
		gameController.ResetBoard();
	}

	void ShowCurrentLineMatchesCommand()
	{
		List<Match> currentMatches=gameController.GetBaseMatches();

		if (currentMatches.Count==0)
		{
			ConsoleOutputAdd("- no current line matches");
			return;
		}

		ConsoleOutputAdd("- current line matches:");
		foreach(Match match in currentMatches)
		{
			ConsoleOutputAdd(match.ToString());
		}

	}

	void ShowPossibleLineMatchesCommand()
	{
		List<Swap> possibleMatchList=gameController.PossibleMatches();

		if (possibleMatchList.Count==0)
		{
			ConsoleOutputAdd("- no possible matches");
			return;
		}

		foreach (Swap matchSwap in possibleMatchList)
		{
			ConsoleOutputAdd("- possible match: "+matchSwap.ToString());
		}
	}

	void ChangePieceCommand(string[] splitCommand)
	{
		if (ChangePieceCommandValidate(splitCommand)) {
			ConsoleOutputAdd("- Changing piece at "+splitCommand[1]+","+splitCommand[2]+" to "+splitCommand[3]);
			gameController.ChangePieceAction(int.Parse(splitCommand[1]), int.Parse(splitCommand[2]), splitCommand[3]);
		}
	}

	bool ChangePieceCommandValidate(string[] splitCommand)
	{
		string usageInfo="- Usage: CHANGEPIECE [0-7] [0-7] [piecetype]\n"+
						 "- Valid PieceTypes: CONE, CROSS, HEART, CUBE, SPHERE, STAR, TORUS";
		//invalid number of parameters
		if (splitCommand.Length!=4) {
			ConsoleOutputAdd("- Invalid number of arguments to changepiece\n"+
			                 usageInfo);
			return false;
		}
		//invalid x coord
		int tempCoordX=0;
		if (!(int.TryParse(splitCommand[1],out tempCoordX)))
		{
			ConsoleOutputAdd("- Invalid X coordinate: '"+splitCommand[1]+"'\n"+usageInfo);
			return false;
		}
		//invalid y coord
		int tempCoordY=0;
		if (!(int.TryParse(splitCommand[2],out tempCoordY)))
		{
			ConsoleOutputAdd("- Invalid Y coordinate: '"+splitCommand[2]+"'\n"+usageInfo);
			return false;
		}
		//out of range x coord
		if (tempCoordX>7 || tempCoordX<0)
		{
			ConsoleOutputAdd("- X coordinate must be between 0 and 7\n" + usageInfo);
			return false;
		}
		//out of range y coord
		if (tempCoordY>7 || tempCoordY<0)
		{
			ConsoleOutputAdd("- Y coordinate must be between 0 and 7\n" + usageInfo);
			return false;
		}
		//invalid shape
		if (splitCommand[3]!="CONE" &&
		    splitCommand[3]!="CROSS" &&
		    splitCommand[3]!="HEART" &&
		    splitCommand[3]!="CUBE" &&
		    splitCommand[3]!="SPHERE" &&
		    splitCommand[3]!="STAR" &&
		    splitCommand[3]!="TORUS")
		{
			ConsoleOutputAdd("- Invalid shape: '"+splitCommand[3]+"'\n"+usageInfo);
			return false;
		}
		return true;
	}

	void HelpCommand(string[] splitCommand)
	{
		if (splitCommand.Length==1) {
			TextAsset helpFile=Resources.Load("HelpCommand") as TextAsset;
			ConsoleOutputAdd(helpFile.text);
		}
		else 
		{
			switch (splitCommand[1])
			{
			case "HELP":
				ConsoleOutputAdd("- Type \"Help\" or \"?\" to see a list of commands");
				break;
			case "CHANGEPIECE":
				ConsoleOutputAdd("- Usage: CHANGEPIECE [0-7] [0-7] [piecetype]\n"+
				                 "- Valid PieceTypes: CONE, CROSS, HEART, CUBE, SPHERE, STAR, TORUS");
				break;
			case "SHOWPOSSIBLELINEMATCHES":
				ConsoleOutputAdd("- shows possible matches display the coordinates of the pieces needed to be swapped");
				break;
			case "SHOWCURRENTLINEMATCHES":
				ConsoleOutputAdd("- shows the current line match coordinates");
				break;
			case "RESETBOARD":
				ConsoleOutputAdd("- destroys the board and re-rolls the pieces\n" +
								 "- new board will have no current matches and at least 1 possible match");
				break;
			case "ENDGAME":
				ConsoleOutputAdd("- ends the game");
				break;
			case "SETHIGHSCORE":
				ConsoleOutputAdd("- sets the high score");
				break;
			case "SHOWBOARDSTRING":
				ConsoleOutputAdd("- show board state as string");
				break;
			case "SAVEBOARDSTATE":
				ConsoleOutputAdd("- saves the board state");
				break;
			case "LOADBOARDSTATE":
				ConsoleOutputAdd("- loads the saved board state");
				break;
			case "SHOWCURRENTBLOBMATCHES":
				ConsoleOutputAdd("- shows the current matches after calculating blob matches");
				break;
			default:
				ConsoleOutputAdd("- Could not find help for '"+splitCommand[1]+"'");
				break;
			}
		}
	}
}
