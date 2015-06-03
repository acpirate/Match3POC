using UnityEngine;
using System.Collections;

public class QuitScript : MonoBehaviour {

	public GameController gameController;
	

	public void EndGameClicked()
	{
		gameController.EndGame("End Game Button Clicked");
	}
}
