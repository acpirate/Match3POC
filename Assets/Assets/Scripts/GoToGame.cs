using UnityEngine;
using System.Collections;

public class GoToGame : MonoBehaviour {

	public GameSelectController gameSelectController;


	// Use this for initialization
	void Start () {
	
	}

	public void GameStart() 
	{
		Application.LoadLevel("Main");
	}

	public void BackToTitle() 
	{
		Application.LoadLevel("Title");
	}

	public void ResetHighScore()
	{
		GameController.SetHighScore(0);
		gameSelectController.UpdateHighScoreDisplay();
	}

	
	// Update is called once per frame
	void Update () {
	
	}
}
