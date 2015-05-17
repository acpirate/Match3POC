using UnityEngine;
using System.Collections;

public class GoToGame : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}

	public void GameStart() {
		Application.LoadLevel("Main");
	}

	public void BackToTitle() 
	{
		Application.LoadLevel("Title");
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
