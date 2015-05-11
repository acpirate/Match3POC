using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ConsoleController : MonoBehaviour {
	[SerializeField]
	private InputField consoleInput;
	[SerializeField]
	private Text consoleOutput;
	[SerializeField]
	private int consoleLines;

	void Awake()
	{

	}

	// Use this for initialization
	void Start () {
		InputField.SubmitEvent submitEvent = new InputField.SubmitEvent();
		submitEvent.AddListener(ConsoleCommand);
		consoleInput.onEndEdit = submitEvent;
		consoleInput.ActivateInputField();
	}
	
	// Update is called once per frame
	void Update () {
	
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
			default:
			{
				ConsoleOutputAdd("- Invalid Command: "+commandTokens[0]);
				break;
			}

		}
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
			default:
				ConsoleOutputAdd("- Could not find help for '"+splitCommand[1]+"'");
				break;
			}
		}
	}
}
