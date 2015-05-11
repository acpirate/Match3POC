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
		ConsoleOutputAdd(commandString);
		consoleInput.ActivateInputField();
	}

	void ConsoleOutputAdd(string outputToAdd)
	{


		if (consoleOutput.text!="" && consoleOutput.text.Split('\n').Length<consoleLines+1) consoleOutput.text+="\n";
		consoleOutput.text+=">"+outputToAdd;

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
}
