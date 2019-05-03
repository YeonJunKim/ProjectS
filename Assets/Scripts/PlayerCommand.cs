using UnityEngine;
using System.Collections;

public class PlayerCommand : MonoBehaviour {


	public GameObject mousePoint_Move;
	public GameObject mousePoint_Attack;
	public Texture2D minImage;
	public Texture2D gasImage;
	public Texture2D supplyImage;
	public Texture2D dragHighlight;
	public Texture2D docImage;
	public Texture2D[] orderButtons;
	public Texture2D[] orderButtonsDown;
	public Texture2D[] unitButtons;
	public Texture2D[] skillButtons;
	public Texture2D[] scvImages;
	public Texture2D[] marineImages;
	public Texture2D[] tankImages;
	public AudioClip buttonDownSound;
	public AudioClip errorSound;
	public AudioClip notEnoughMineral;
	public AudioClip stimpackSound;

	[HideInInspector] public Rect dragRect;
	[HideInInspector] public bool isDraggingMouse;
	[HideInInspector] public bool isDoubleClicked;
	GameObject gameObjectOnCursor;

	ArrayList selectedObjects = new ArrayList ();
	Vector2 mouseDownPos;
	PlayerProperties playerProperties;
	ObjectManager playerObjectManager;
	CsMouseCursor csMouseCursor;
	RaycastHit mouseHit;
	float timeSinceLastClick;
	bool onHotKey_Move;
	bool onHotKey_Attack;
	bool stopKeyPressed;
	int GameObectLayer = 1 << 8;
	Rect minImageRect;
	Rect gasImageRect;
	Rect supplyImageRect;
	Rect minInfoRect;
	Rect gasInfoRect;
	Rect populationInfoRect;
	Rect docImageRect;
	Rect[] orderButtonRect = new Rect[3];
	Rect skillRect;
	Rect[] selectedsImageRect = new Rect[12];
	bool preventMultipleOrder;	// because OnGUI, Input is called multiple times

	int screenWidth;
	int screenHeight;



	// Use this for initialization
	void Start () {
		playerProperties = GetComponent<PlayerProperties> ();
		csMouseCursor = GetComponent<CsMouseCursor> ();
		playerObjectManager = GetComponent<ObjectManager> ();
		Initialize ();
	}


	// Update is called once per frame
	void Update () {
		timeSinceLastClick += Time.smoothDeltaTime;
		RaycastFromMouse ();
		UpdateObjectOnMouse ();
		CheckDoubleClick ();
    }


	// ( I'm putting 'ProcessMouseCommands' in OnGUI because of timing
	// 	 for example, I'm doing something with GetMouseButtonUp here and all the other Units
	// 	 but I don't exactly know witch GetMouseButtonUp will be processed first if I put both of them in 'Update'
	//	 so I put this one in OnGUI, this will make it sure that 'ProcessMouseCommands's GetMouseButtonUp is processed last.
	// 	 O.M.G...... I found out that you get Input.GetButtonDown(or Up) twice if it's in OnGUI !!! )
	void OnGUI()
	{
		ProcessMouseCommands ();
		DrawDragRect ();
		DrawUI ();
	}


	void Initialize()
	{
		screenWidth = Screen.width;
		screenHeight = Screen.height;
		int infoImageWidth = minImage.width;
		int infoImageHeight = minImage.height;
		int infoImageRectAdjustX = -150;
		int infoImageRectAdjustY = 5;
		int infoTextRectAdjustY = 10;
		int distBetweenInfos = 150;
		int infoTextWidth = 100;
		int infoTextHeight = 20;
		int distBetweenInfoImageAndText = 35;

		// just.. setting UI positions
		minImageRect = new Rect (screenWidth + infoImageRectAdjustX - distBetweenInfos * 2, infoImageRectAdjustY, infoImageWidth, infoImageHeight);
		gasImageRect = new Rect (screenWidth + infoImageRectAdjustX - distBetweenInfos, infoImageRectAdjustY, infoImageWidth, infoImageHeight);
		supplyImageRect = new Rect (screenWidth + infoImageRectAdjustX, infoImageRectAdjustY, infoImageWidth, infoImageHeight);
		minInfoRect = new Rect (screenWidth + infoImageRectAdjustX - distBetweenInfos * 2 + distBetweenInfoImageAndText, infoTextRectAdjustY, infoTextWidth, infoTextHeight);
		gasInfoRect = new Rect (screenWidth + infoImageRectAdjustX - distBetweenInfos + distBetweenInfoImageAndText, infoTextRectAdjustY, infoTextWidth, infoTextHeight);
		populationInfoRect = new Rect (screenWidth + infoImageRectAdjustX + distBetweenInfoImageAndText, infoTextRectAdjustY, infoTextWidth, infoTextHeight);
		docImageRect = new Rect (0, screenHeight * 0.7f, screenWidth, screenHeight * 0.3f);
		orderButtonRect [0] = new Rect (screenWidth * 0.81f, screenHeight * 0.79f, orderButtons [0].width, orderButtons [0].height); 
		orderButtonRect [1] = new Rect (screenWidth * 0.81f + orderButtons [0].width, screenHeight * 0.79f, orderButtons [0].width, orderButtons [0].height); 
		orderButtonRect [2] = new Rect (screenWidth * 0.81f + orderButtons [0].width * 2, screenHeight * 0.79f, orderButtons [0].width, orderButtons [0].height); 
		skillRect = new Rect (screenWidth * 0.81f, screenHeight * 0.89f, orderButtons [0].width, orderButtons [0].height);

		for(int i = 0; i < selectedsImageRect.Length; i++)
		{
			if(i <= 5)
				selectedsImageRect[i] = new Rect (screenWidth * 0.3f + 68*i, screenHeight * 0.83f, 68, 68); 
			else
				selectedsImageRect[i] = new Rect (screenWidth * 0.3f + 68 * (i-6), screenHeight * 0.83f + 68 + 20, 
				                                  68, 68); 
		}

		preventMultipleOrder = true;
	}


	void CheckDoubleClick()
	{
		if (!gameObjectOnCursor)
			return;
		if (Input.GetKeyUp (KeyCode.LeftShift))
			isDoubleClicked = false;
		// so much 'if's.. I wonder if this will effect the game
		if (Input.GetMouseButtonUp (0)) 
		{
            if (timeSinceLastClick < 0.5f || Input.GetKey (KeyCode.LeftShift))
			{
				if(gameObjectOnCursor.CompareTag("Unit") || gameObjectOnCursor.CompareTag("Building"))
				{
					if(gameObjectOnCursor.GetComponent<CsProperties>().team == Team.MINE)
					{
						isDoubleClicked = true;
						Invoke("IsDoubleClickToFalse", 0.1f);
					}
				}
			}
			else
				isDoubleClicked = false;

			timeSinceLastClick = 0;
		}
	}

	void IsDoubleClickToFalse()
	{
		isDoubleClicked = false;
	}

	void ProcessMouseCommands()
	{
		if (!gameObjectOnCursor)
			return;

		// Event.current.mousePosition's coordinates system is same with GUI's, so using it is better than Input.MousePosition when you're dealing with GUI
		Event current = Event.current;

		if(current.isMouse)
		{
			// this is a trick for doubleClick selection (Your hands need to be steady for doubleClick selection^^)
			if (current.delta.SqrMagnitude () > 8)
				timeSinceLastClick += 1;

			switch(current.button)
			{
			case 0:	// mouse left
				if(current.type == EventType.mouseDown)
				{
					mouseDownPos = current.mousePosition; // save mouse drag started position
					
					// deselection
					if (!OnHotkeyCommand() && !Input.GetKey(KeyCode.LeftControl))
					{
						// preventing miss clicks (you don't want deselection when you mistakenly click the Ground or something)
						if (!gameObjectOnCursor.CompareTag("Ground")) 
                        	DeselectAll();
					}
				}
				else if(current.type == EventType.mouseDrag)
				{
					if(OnHotkeyCommand())
						return;

					dragRect = Rect.MinMaxRect(Mathf.Min(mouseDownPos.x, current.mousePosition.x), Mathf.Min(mouseDownPos.y, current.mousePosition.y), 
					                           Mathf.Max(mouseDownPos.x, current.mousePosition.x), Mathf.Max(mouseDownPos.y, current.mousePosition.y));

					// prevent Player's miss drag click
					if(dragRect.width + dragRect.height > 10)
					{
						if(! Input.GetKey(KeyCode.LeftControl))
               	 			DeselectAll();

						isDraggingMouse = true;
					}
				}
				else if(current.type == EventType.mouseUp)
				{
					RaycastHit currentMouseHit = mouseHit;
					
					if(onHotKey_Move)
					{
						if(gameObjectOnCursor.CompareTag("Ground"))
							GiveOrderToSelecteds("StartPointMoving", currentMouseHit.point, true);
						else
							GiveOrderToSelecteds("StartTargetMoving", gameObjectOnCursor, true);

						Instantiate(mousePoint_Move, currentMouseHit.point, Quaternion.identity);
						onHotKey_Move = false;
						csMouseCursor.SetMouseCondition(MouseCondition.BASIC);
					}

					else if(onHotKey_Attack)
					{
						if(gameObjectOnCursor.CompareTag("Ground"))
						{
							GiveOrderToSelecteds("StartAttackMoving", currentMouseHit.point, true);
							Instantiate(mousePoint_Attack, currentMouseHit.point, Quaternion.identity);	// point_Move effect
						}
						else if(gameObjectOnCursor.CompareTag("Unit") || gameObjectOnCursor.CompareTag("Building"))
						{
							// if Player attacks something that is not already in the list, don't select it (it gets selected becuase of 'Input.MouseUp')
							if(!selectedObjects.Contains(gameObjectOnCursor))
								gameObjectOnCursor.GetComponent<CsCommandProcessor>().Invoke("RemoveThisFromSelecteds", 0.005f);	// this is sort of like a trick

							GiveOrderToSelecteds("DesignationAttack", gameObjectOnCursor, true);
						}
						else
                       		Debug.Log ("Cannot Attack The Target");

						onHotKey_Attack = false;
						csMouseCursor.SetMouseCondition(MouseCondition.BASIC);
                	}
	                // make voice only when player selects by dragging or clicking a Unit or a Building
					else if(isDraggingMouse || gameObjectOnCursor.CompareTag("Unit") || gameObjectOnCursor.CompareTag("Building"))
					{
						// honestly, I really don't like this part , where I use 'Invoke', but I can't think of a better algorithm ㅠㅠ
						// probably 0.05 seconds later, the 'adding' of selectedObjects would be finished
	                    Invoke("SelectedsSortAndRespond", 0.05f);
	                }
	                // reset drag rect
	                dragRect = Rect.MinMaxRect(0, 0, 0, 0);
					isDraggingMouse = false;
                }
				break;

			case 1:	// mouse right
	            if(current.type == EventType.mouseDown)
	            {
					if(selectedObjects.Count <= 0 || ListContainsEnemy(selectedObjects) || ListContainsNeutral(selectedObjects))
						return;

					if(OnHotkeyCommand())
					{
						onHotKey_Move = false;
						onHotKey_Attack = false;
						csMouseCursor.SetMouseCondition(MouseCondition.BASIC);
						return;
	                }

	                RaycastHit currentMouseHit = mouseHit;
					// when Player clicks Ground, make selectedGameObjects move to where Player clicked
					if(gameObjectOnCursor.CompareTag("Ground"))
					{
						GiveOrderToSelecteds("StartPointMoving", currentMouseHit.point, true);
						Instantiate(mousePoint_Move, currentMouseHit.point, Quaternion.identity);	// point_Move effect
					}
					// when Player clicks Unit or Building
					else if(gameObjectOnCursor.CompareTag("Unit") || gameObjectOnCursor.CompareTag("Building"))
					{
						Team team = gameObjectOnCursor.GetComponent<CsProperties>().team;
						
						if(team == Team.MINE || team == Team.NEUTRAL)
							GiveOrderToSelecteds("StartTargetMoving", gameObjectOnCursor, true);
                        else if(team == Team.ENEMY)
							GiveOrderToSelecteds("DesignationAttack", gameObjectOnCursor, true);
	            	}
					// when Player clicks Mineral or Gas
                    else
                    {
						// if selectedObject is not Scv, it will just go there
						GiveOrderToSelecteds("StartDiggingResource", gameObjectOnCursor, true);
                    }
	            }
	            break;
            }
		}
	}

	void RaycastFromMouse()
	{
		// Raycasting only 100m forward from Camera
		Physics.Raycast (Camera.main.ScreenPointToRay (Input.mousePosition), out mouseHit, 100f, GameObectLayer);
	}

	void UpdateObjectOnMouse()
	{
		// compare previous GameObject on Cursor with current GameObject on Cursor
		// if they're different, change 'gameObjectOnCursor' to current GameObject on Cursor
		if (!mouseHit.collider)
			return;

		// because the collider component is in it's child
		if(gameObjectOnCursor != mouseHit.collider.transform.parent.gameObject)
		{
			// make previous pointed gameObject's 'isSelecting' to false
			if(gameObjectOnCursor != null && gameObjectOnCursor.activeInHierarchy)
			{
				if(!gameObjectOnCursor.CompareTag("Ground"))
				{
						gameObjectOnCursor.SendMessage("SetIsSelecting", false);
				}
			}

			// save current GameObject on Cursor
				// 'mouseHit.collider.transform.parent.gameObject'
				//  -> this line looks pretty bad, but due to Empty GameObject(which is the highest parent) dont't collide with 'Ray', so this had to be done ㅠㅠ 
			gameObjectOnCursor = mouseHit.collider.transform.parent.gameObject;

			// cursor reaction
			if(!OnHotkeyCommand())
			{
				if(!gameObjectOnCursor.CompareTag("Ground"))
				{
					Team team = gameObjectOnCursor.GetComponent<CsProperties>().team;
					if(team == Team.MINE)
						csMouseCursor.SetMouseCondition(MouseCondition.ON_MINE);
					else if(team == Team.ENEMY)
						csMouseCursor.SetMouseCondition(MouseCondition.ON_ENEMY);
					else
						csMouseCursor.SetMouseCondition(MouseCondition.ON_NEUTRAL);
	            }
				else
					csMouseCursor.SetMouseCondition(MouseCondition.BASIC);
			}
		}
		// make gameObjectOnCursor's 'isSelecting' to true
		// It's better if this is in 'if' right above, but that way, there is a slight Error, which if Player clicks a Object, IsSelecting Circle disapears
		// yes, this is a problem of the selecting system, but I can just solve the problem by doing this (but this will make some tasks)
		if(!gameObjectOnCursor.CompareTag("Ground"))
		{
			gameObjectOnCursor.SendMessage("SetIsSelecting", true);
		}
	}

	public void AddSelectedUnitToList(GameObject gameObjectToAdd)
	{	
		if(selectedObjects.Contains(gameObjectToAdd))
			return;

		// if nothing is currently selected, you can just simply add the Object to the list
		// but if something is currently selected, it needs to meet some 'if's before adding
		if(selectedObjects.Count > 0)
		{
			Team team = gameObjectToAdd.GetComponent<CsProperties> ().team;

			if (team == Team.MINE) 
			{
				// Mine selection is prior than Others
				if(ListContainsEnemy(selectedObjects) || ListContainsNeutral(selectedObjects))
					DeselectAll();

				// Unit selection is prior than Buildings
				if (gameObjectToAdd.CompareTag("Unit") && ListContainsBuilding(selectedObjects))
					DeselectAll();

				// Buildings can only be selected with Buildings
				if(gameObjectToAdd.CompareTag("Building"))
				{
					if(ListContainsUnit(selectedObjects) == true)
						return;
				}

				// if selectedObjects is full(maximum is 12), kick out the last one of the list and replace new one there
				if (selectedObjects.Count >= 12) 
				{
					GameObject targetObject = selectedObjects[11] as GameObject;
					targetObject.SendMessage ("SetIsSelected", false);
					selectedObjects.RemoveAt(11);
				}

				selectedObjects.Add (gameObjectToAdd);
				gameObjectToAdd.SendMessage("SetIsSelected", true);
			}
			else
			{
				// if 'Mine' is already in the list, no room for other team ^^
				if(ListContainsMine(selectedObjects))
					return;

				GameObject targetObject = selectedObjects[0] as GameObject;
				targetObject.SendMessage ("SetIsSelected", false);
				selectedObjects.RemoveAt(0);

				selectedObjects.Add(gameObjectToAdd);
				gameObjectToAdd.SendMessage("SetIsSelected", true);
			}
		}
		else
		{
			selectedObjects.Add (gameObjectToAdd);
			gameObjectToAdd.SendMessage("SetIsSelected", true);
		}
	}

	bool ListContainsUnit(ArrayList list)
	{
		GameObject targetObect;
		for(int i = 0; i < list.Count; i++)
		{
			targetObect = list[i] as GameObject;
			if(targetObect.CompareTag("Unit"))
			{
				return true;
			}
		}
		return false;
	}

	bool ListContainsBuilding(ArrayList list)
	{
		GameObject targetObect;
		for(int i = 0; i < list.Count; i++)
		{
			targetObect = list[i] as GameObject;
			if(targetObect.CompareTag("Building"))
			{
				return true;
			}
		}
		return false;
	}


	bool ListContainsMine(ArrayList list)
	{
		// you just need to check the first index of 'selectedObjects', because only one type is selected together
		GameObject targetObect = list [0] as GameObject;
		if(targetObect.GetComponent<CsProperties>().team == Team.MINE)
			return true;
		
		return false;
	}

	bool ListContainsEnemy(ArrayList list)
	{
		GameObject targetObect = list [0] as GameObject;
		if(targetObect.GetComponent<CsProperties>().team == Team.ENEMY)
			return true;

		return false;
	}

	bool ListContainsNeutral(ArrayList list)
	{
		GameObject targetObect = list [0] as GameObject;
		if(targetObect.GetComponent<CsProperties>().team == Team.NEUTRAL)
			return true;
		
		return false;
	}


	// done by bubble sort
	// sorts by value (costMin + costGas)
	void SortSelectedObjects()
	{
		if (selectedObjects.Count < 2)
			return;

		int targetValue = 0;
		GameObject targetObject;
		int roopNum = selectedObjects.Count;

		for(int j = 0; j < selectedObjects.Count - 1; j++)
		{
			targetObject = selectedObjects[0] as GameObject;
			targetValue = targetObject.GetComponent<CsProperties>().GetValue();

			for(int i = 0; i < roopNum - 1; i++)
			{
				GameObject currentObject = selectedObjects[i+1] as GameObject;
				int currentValue = currentObject.GetComponent<CsProperties>().GetValue();

				if(targetValue < currentValue)
				{
					object temp = selectedObjects[i];
					selectedObjects[i] = selectedObjects[i+1];
					selectedObjects[i+1] = temp;
				}
				else
				{
					targetValue = currentValue;
				}
			}
			roopNum--;
		}
	}

	void DeselectAll()
	{
		for(int i = 0; i < selectedObjects.Count; i++)
		{
			GameObject targetObject = selectedObjects[i] as GameObject;
			targetObject.SendMessage("SetIsSelected", false);
		}
		selectedObjects.Clear();
	}


	void GiveOrderToSelecteds(string order, object value, bool voiceRespond)
	{
		if (selectedObjects.Count <= 0)
			return;

		GameObject leaderObject = selectedObjects [0] as GameObject;

		// check if it's MINE that Player is selecting
		if(leaderObject.GetComponent<CsProperties>().team == Team.MINE)
		{
			// the leader(#1) of selectedObejects make voice 
			if(voiceRespond)
				leaderObject.SendMessage ("VoiceRespondCommand");

			for(int i = 0; i < selectedObjects.Count; i++)
			{
				GameObject targetObject = selectedObjects[i] as GameObject;
				targetObject.SendMessage(order, value);
			}
		}
	}

	void GiveOrderToParticularObjectsInSelecteds(string objectsName, string order)
	{
		if (selectedObjects.Count <= 0)
			return;
		
		GameObject leaderObject = selectedObjects [0] as GameObject;

		// check if it's MINE that Player is selecting
		if(leaderObject.GetComponent<CsProperties>().team == Team.MINE)
		{
			for(int i = 0; i < selectedObjects.Count; i++)
			{
				GameObject targetObject = selectedObjects[i] as GameObject;

				if(targetObject.GetComponent<CsProperties>().objectName.Equals(objectsName))
					targetObject.SendMessage(order);
			}
		}
	}

	public GameObject GetObjectOnMouse()
	{
		if (gameObjectOnCursor && !gameObjectOnCursor.CompareTag("Ground"))
			return gameObjectOnCursor;
	
		return null;
	}

	void SelectedsSortAndRespond()
	{
		// prevent double voices coming out when double clciking
		if (selectedObjects.Count <= 0 || isDoubleClicked)
			return;

		GameObject tagetObject = selectedObjects [0] as GameObject;
		// check if it's MINE that Player is selecting
		if(tagetObject.GetComponent<CsProperties>().team == Team.MINE)
		{
			SortSelectedObjects ();

			GameObject leaderObject = selectedObjects[0] as GameObject;
			leaderObject.SendMessage("VoiceRespondSelection");
		}
	}


	void DrawDragRect()
	{
		if (isDraggingMouse)
		{
			// draw mouse dragged rect on GUI
			GUI.color = new Color(1, 1, 1, 0.3f);
			GUI.DrawTexture(dragRect, dragHighlight);
		}
	}

	public bool OnHotkeyCommand()
	{
		if (onHotKey_Move || onHotKey_Attack)
			return true;

		return false;
	}
	
	public void RemoveObjectFromSelecteds(GameObject gameObject)
	{
		if (!selectedObjects.Contains(gameObject))
			return;

		GameObject target = selectedObjects[selectedObjects.IndexOf (gameObject)] as GameObject;
		target.GetComponent<CsCommandProcessor> ().SetIsSelected (false);
		selectedObjects.Remove (target);
	}

	void DrawUI()
	{
		// min, gas, population
		GUI.DrawTexture (minImageRect, minImage);
		GUI.DrawTexture (gasImageRect, gasImage);
		GUI.DrawTexture (supplyImageRect, supplyImage);
		GUI.Box (minInfoRect, playerProperties.currentMin.ToString ());
		GUI.Box (gasInfoRect, playerProperties.currentGas.ToString ());
		GUI.Box (populationInfoRect, playerProperties.currentPopulation.ToString ());
	
		// doc
		GUI.DrawTexture (docImageRect, docImage);

		// buttons
		if(selectedObjects.Count > 0)
		{
			GameObject leaderObject = selectedObjects [0] as GameObject;

			if(leaderObject.GetComponent<CsProperties>().team == Team.MINE)
			{
				if(leaderObject.CompareTag("Unit"))
				{
					// basic command buttons
					if(onHotKey_Attack == false)
					{
						if(GUI.Button (orderButtonRect[0], orderButtons[0])  || Input.GetKeyDown(KeyCode.A))
						{
							onHotKey_Attack = true;
							csMouseCursor.SetMouseCondition(MouseCondition.TARGET_ATTACK);
							AudioSource.PlayClipAtPoint(buttonDownSound, transform.position);
						}
					}
					else
						GUI.Button (orderButtonRect[0], orderButtonsDown[0]);

					if(onHotKey_Move == false)
					{
						if(GUI.Button (orderButtonRect[1], orderButtons[1])  || Input.GetKeyDown(KeyCode.M))
						{
							onHotKey_Move = true;
							csMouseCursor.SetMouseCondition(MouseCondition.TARGET_MOVE);
							AudioSource.PlayClipAtPoint(buttonDownSound, transform.position);
						}
	                }
					else
						GUI.Button (orderButtonRect[1], orderButtonsDown[1]);

					if(stopKeyPressed == false)
					{
						if(GUI.Button (orderButtonRect[2], orderButtons[2]) || Input.GetKeyDown(KeyCode.S))
						{
							stopKeyPressed = true;
							Invoke("ReActivateStopButton", 0.1f);
							GiveOrderToSelecteds("Stop", null, false);
							AudioSource.PlayClipAtPoint(buttonDownSound, transform.position);
						}
	                }
					else
						GUI.Button (orderButtonRect[2], orderButtonsDown[2]);

					// skill buttons
					switch(leaderObject.GetComponent<CsProperties>().objectName)
					{
					case "Scv":
						if(GUI.Button (skillRect, skillButtons[0]))
					  	{
							AudioSource.PlayClipAtPoint(errorSound, transform.position);
						}
						break;
					case "Marine":
						if(GUI.Button (skillRect, skillButtons[1]) || Input.GetKeyDown(KeyCode.T))
						{
							if(preventMultipleOrder == false)
								break;

							GiveOrderToParticularObjectsInSelecteds("Marine", "Stimpack");
							AudioSource.PlayClipAtPoint(stimpackSound, transform.position);
							preventMultipleOrder = false;
							Invoke("ReActivateOrderRecives", 0.1f);
						}
						break;
					case "Tank":
						if(GUI.Button (skillRect, skillButtons[2]) || Input.GetKeyDown(KeyCode.I))
						{
							if(preventMultipleOrder == false)
								break;

							AudioSource.PlayClipAtPoint(errorSound, transform.position);
							preventMultipleOrder = false;
							Invoke("ReActivateOrderRecives", 0.1f);
						}
						break;
					}
	            }
				else if(leaderObject.CompareTag("Building"))
		        {
					// production button
					switch(leaderObject.GetComponent<CsProperties>().objectName)
					{
					case "CommandCenter":
						if(GUI.Button (orderButtonRect[0], unitButtons[0]) || Input.GetKeyDown(KeyCode.S))
						{
							if(preventMultipleOrder == false)
								break;

							if(playerProperties.currentMin >= 50)
							{
								playerObjectManager.GetGameObject("Scv", leaderObject.GetComponent<CsCommandProcessor>().GetSpawnPoint().position, Quaternion.identity);
							}
							else
								AudioSource.PlayClipAtPoint(notEnoughMineral, transform.position);

							preventMultipleOrder = false;
							Invoke("ReActivateOrderRecives", 0.1f);
						}
						break;
					case "Barrack":
						if(GUI.Button (orderButtonRect[0], unitButtons[1])  || Input.GetKeyDown(KeyCode.M))
						{
							if(preventMultipleOrder == false)
								break;

							if(playerProperties.currentMin >= 50)
							{
								playerObjectManager.GetGameObject("Marine", leaderObject.GetComponent<CsCommandProcessor>().GetSpawnPoint().position, Quaternion.identity);
							}
							else
								AudioSource.PlayClipAtPoint(notEnoughMineral, transform.position);

							preventMultipleOrder = false;
							Invoke("ReActivateOrderRecives", 0.1f);
						}
						break;
					case "Factory":
						if(GUI.Button (orderButtonRect[0], unitButtons[2]) || Input.GetKeyDown(KeyCode.T))
						{
							if(preventMultipleOrder == false)
								break;
							if(playerProperties.currentMin >= 200)
							{
								playerObjectManager.GetGameObject("Tank", leaderObject.GetComponent<CsCommandProcessor>().GetSpawnPoint().position, Quaternion.identity);
							}
							else
								AudioSource.PlayClipAtPoint(notEnoughMineral, transform.position);

							preventMultipleOrder = false;
							Invoke("ReActivateOrderRecives", 0.1f);
						}
						break;
					}
				}
			}

			// draw selectedObjects on Doc
			for(int i = 0; i < selectedObjects.Count; i++)
			{
				if(i > 11)
					break;
				GameObject targetObject = selectedObjects[i] as GameObject;
				switch(targetObject.GetComponent<CsProperties>().objectName)
				{
				case "Scv":
					GUI.DrawTexture(selectedsImageRect[i], scvImages[0]);
					break;
				case "Marine":
					GUI.DrawTexture(selectedsImageRect[i], marineImages[0]);
					break;
				case "Tank":
					GUI.DrawTexture(selectedsImageRect[i], tankImages[0]);
					break;
				}
			}
		}

    }

	void ReActivateStopButton()
	{
		stopKeyPressed = false;
	}

	void ReActivateOrderRecives()
	{
		preventMultipleOrder = true;
	}

}






