using UnityEngine;
using System.Collections;

public class MatchRemoveFXController : MonoBehaviour {

	public float lifeTime=.5f;

	private float lifeCountdown;
	private SpriteRenderer myRenderer;

	void Awake()
	{
		myRenderer=GetComponent<SpriteRenderer>();
	}

	// Use this for initialization
	void Start () 
	{
		lifeCountdown=lifeTime;
		Destroy(gameObject,lifeTime);
	}
	
	// Update is called once per frame
	void Update () 
	{
		lifeCountdown-=Time.deltaTime;
		FadeOut();
	}

	void FadeOut()
	{
		Color tempColor=new Color(myRenderer.color.r, myRenderer.color.g, myRenderer.color.b,
		                          lifeCountdown/lifeTime);

		myRenderer.color=tempColor;
	}
}
