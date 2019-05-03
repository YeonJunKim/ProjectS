using UnityEngine;
using System.Collections;

public class CsHpBar : MonoBehaviour {

	CsChildObject csChildObject;
	CsProperties csProperties;
	MeshRenderer meshRenderer;

	public Texture hp_Empty;
	public Texture hp_Green;
	public Texture hp_LightGreen;
	public Texture hp_Yellow;
	public Texture hp_LightYellow;
	public Texture hp_Orange;
	public Texture hp_LightOrange;
	public Texture hp_Red;
	public Texture hp_LightRed;
	public Texture hp_LastRoom;

	int numOfHpRooms;
	float widthOfHpRoom;
	float heightOfHpRoom;
	Vector3 sizeOfObject;

	// Use this for initialization
	void Start () {
		csChildObject = transform.GetComponentInChildren<CsChildObject> ();
		csProperties = GetComponent<CsProperties> ();
		meshRenderer = transform.GetComponentInChildren<MeshRenderer> ();
		widthOfHpRoom = 9;
		heightOfHpRoom = 9;
		sizeOfObject = csChildObject.GetColliderSize();

		Initialize ();
	}
	
	// Update is called once per frame
	void Update () {
	}

	void OnGUI()
	{
		MakeHpBar ();
	}

	void Initialize()
	{
		float objectWidth = sizeOfObject.x + sizeOfObject.z;

		if(objectWidth < 1)
			numOfHpRooms = 4;
		else
			numOfHpRooms = (int)objectWidth * 4;
	}


	void MakeHpBar()
	{
		if(!meshRenderer.isVisible)
			return;

		Vector3 hpPositionInWorld = new Vector3 (transform.position.x, transform.position.y + sizeOfObject.y * 1.8f, transform.position.z);
		Vector3 hpPositionOnScreen = Camera.main.WorldToScreenPoint (hpPositionInWorld);
		// WorldToScreenPoint.y is upside down, because in Camera's coordinates system, (0,0) is leftBottom
		hpPositionOnScreen.y = Screen.height - hpPositionOnScreen.y;

		GUI.DrawTexture (new Rect (hpPositionOnScreen.x + widthOfHpRoom * (numOfHpRooms / 2 - 1) + 2, hpPositionOnScreen.y, widthOfHpRoom, heightOfHpRoom), hp_LastRoom);
		float hpPercentage = csProperties.GetHpPercentage ();

		for(int i = 0; i < numOfHpRooms; i++)
		{
			Rect hpRoomPosition;

			if(i < numOfHpRooms / 2)
				hpRoomPosition = new Rect (hpPositionOnScreen.x - widthOfHpRoom * (numOfHpRooms / 2 - i), hpPositionOnScreen.y, widthOfHpRoom, heightOfHpRoom);
			else
				hpRoomPosition = new Rect (hpPositionOnScreen.x + widthOfHpRoom * (i - numOfHpRooms / 2), hpPositionOnScreen.y, widthOfHpRoom, heightOfHpRoom);


			if(hpPercentage >= (float)((i+1)-0.5)/numOfHpRooms)
			{
				if(hpPercentage >= 0.75f)
					GUI.DrawTexture (hpRoomPosition, hp_Green);
				else if(hpPercentage >= 0.5f)
					GUI.DrawTexture (hpRoomPosition, hp_Yellow);
				else if(hpPercentage >= 0.25f)
					GUI.DrawTexture (hpRoomPosition, hp_Orange);
				else
					GUI.DrawTexture (hpRoomPosition, hp_Red);
			}
			else if(hpPercentage < (float)((i+1)-0.5)/numOfHpRooms && hpPercentage >= (float)i/numOfHpRooms)
			{
				if(hpPercentage >= 0.75f)
					GUI.DrawTexture (hpRoomPosition, hp_LightGreen);
				else if(hpPercentage >= 0.5f)
					GUI.DrawTexture (hpRoomPosition, hp_LightYellow);
				else if(hpPercentage >= 0.25f)
					GUI.DrawTexture (hpRoomPosition, hp_LightOrange);
				else
					GUI.DrawTexture (hpRoomPosition, hp_LightRed);
			}
			else
				GUI.DrawTexture (hpRoomPosition, hp_Empty);
		}
	}
}









