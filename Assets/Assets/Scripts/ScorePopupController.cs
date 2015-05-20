using UnityEngine;
using System.Collections;
using UnityEngine.UI;

public class ScorePopupController : MonoBehaviour {

	Text myText;

	void Awake()
	{
		myText=GetComponentInChildren<Text>();
	}

	// Use this for initialization
	void Start () {
		Destroy(gameObject,1.5f);
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void SetScore(int scoreToSet)
	{
		myText.text=scoreToSet.ToString();

	}

	public void SetColor(Color colorToSet)
	{
		myText.color=colorToSet;
	}
}
