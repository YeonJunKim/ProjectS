using UnityEngine;
using System.Collections;

public enum MouseCondition
{
	BASIC,
	ON_MINE,
	ON_ENEMY,
	ON_NEUTRAL,
	TARGET_MOVE,
	TARGET_ATTACK,
};

public class CsMouseCursor : MonoBehaviour {

	public Texture2D[] cursor_basic;
	public Texture2D[] cursor_onObject;
	public Texture2D cursor_scrollMove;
	int basicAnimIndex;
	float cursor_basic_Xspot;
	float cursor_basic_Yspot;
	float cursor_onObject_Xspot;
	float cursor_onObject_Yspot;
	float cursor_scrollMove_Xspot;
	float cursor_scrollMove_Yspot;

	MouseCondition mouseCondition;

	// Use this for initialization
	void Start () {
		cursor_basic_Xspot = cursor_basic [0].width / 6;
		cursor_basic_Yspot = cursor_basic [0].height / 4.5f;

		cursor_onObject_Xspot = cursor_onObject [0].width / 2;
		cursor_onObject_Yspot = cursor_onObject [0].height / 2 + 2; // +2 is just perfect

		cursor_scrollMove_Xspot = cursor_scrollMove.width / 2;
		cursor_scrollMove_Yspot = cursor_scrollMove.height / 2;

		// because 5th texture is the original one 
		Cursor.SetCursor (cursor_basic[5], new Vector2 (cursor_basic_Xspot, cursor_basic_Yspot), CursorMode.ForceSoftware);
	}
	
	// Update is called once per frame
	void Update () {
		switch(mouseCondition)
		{
		case MouseCondition.BASIC:
			basicAnimIndex = (int)(Mathf.PingPong (Time.time, 1) * 10);
			Cursor.SetCursor (cursor_basic[basicAnimIndex], new Vector2 (cursor_basic_Xspot, cursor_basic_Yspot), CursorMode.ForceSoftware);
			break;
		case MouseCondition.ON_MINE:
			Cursor.SetCursor (cursor_onObject[0], new Vector2 (cursor_onObject_Xspot, cursor_onObject_Yspot), CursorMode.ForceSoftware);
			break;
		case MouseCondition.ON_ENEMY:
			Cursor.SetCursor (cursor_onObject[1], new Vector2 (cursor_onObject_Xspot, cursor_onObject_Yspot), CursorMode.ForceSoftware);
			break;
		case MouseCondition.ON_NEUTRAL:
			Cursor.SetCursor (cursor_onObject[2], new Vector2 (cursor_onObject_Xspot, cursor_onObject_Yspot), CursorMode.ForceSoftware);
			break;
		case MouseCondition.TARGET_MOVE:
			Cursor.SetCursor (cursor_onObject[0], new Vector2 (cursor_onObject_Xspot, cursor_onObject_Yspot), CursorMode.ForceSoftware); // ON_MINE and TARGET_MOVE both uses the same cursor
			break;
		case MouseCondition.TARGET_ATTACK:
			Cursor.SetCursor (cursor_onObject[1], new Vector2 (cursor_onObject_Xspot, cursor_onObject_Yspot), CursorMode.ForceSoftware); // ON_ENEMY and TARGET_ATTACK both uses the same cursor
			break;
		}
	}

	public void SetMouseCondition(MouseCondition condition)
	{
		mouseCondition = condition;
	}

	public void CameraScrolling()
	{
		Cursor.SetCursor (cursor_scrollMove, new Vector2 (cursor_scrollMove_Xspot, cursor_scrollMove_Yspot), CursorMode.ForceSoftware);
	}
	
}





