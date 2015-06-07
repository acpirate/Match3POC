using UnityEngine;
using System.Collections;

public class ExitGameScript : MonoBehaviour {

	void Start()
	{
		Screen.orientation=ScreenOrientation.LandscapeLeft;


		#if UNITY_EDITOR
		gameObject.SetActive(false);
		#endif
		#if UNITY_WEBGL
		gameObject.SetActive(false);
		#endif
	}


	public void QuitGame()
	{

		Application.Quit();
	}


}
