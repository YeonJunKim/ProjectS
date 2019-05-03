using UnityEngine;
using System.Collections;

public class GameManager : MonoBehaviour {

	public GameObject theLastBuilding;

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
		if(theLastBuilding != null)
		{
			if(!theLastBuilding.activeInHierarchy)
			{
				Invoke("LoadVictoryScene", 3);
			}
		}

		if(Input.GetKeyDown(KeyCode.Escape))
		{
			Application.Quit();
		}
	}

	void LoadVictoryScene()
	{
		Application.LoadLevel ("VictoryScene");
	}
}
