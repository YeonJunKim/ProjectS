using UnityEngine;
using System.Collections;

enum DigState
{
	TO_COMMAND,
	TO_MINERAL,
	DIGGING,
}


public class CsCommandProcessor : MonoBehaviour 
{
	// States
	public bool isAbleToAttack;
	public bool isAbleToMove;
	public bool isInvisible;
	bool isMoving;
	bool isAttackAwake;
	bool isAttacking;
	bool isSelected;
	bool isSelecting;
	bool onDesignationAttack;
	bool onDesignationMove;

	// Resources
	public AudioClip[] voice_affirmation;
	public AudioClip[] voice_acknowledge;
	public AudioClip[] attackEffectSound;
	public GameObject fireEffect;	// the effect that flashes when this object attacks
	public GameObject bulletEffect;	// the effect that flashes on the attack target
	public GameObject deathEffect;	// the death effect when this Object dies (if it uses the death effect in EffectManager, this is null) 
	public AudioClip birthQuote;
	public AudioClip[] deathQuote;
	public Transform spawnPoint; // Only for Buildings, for summoning Units

	public float fireEffectTime;	// the flashing time of attack fire effect
    
	// References
    CsProperties csProperties;
	CsAttackRange csAttackRange;	// Has the list of Objects that are in Attack Range
	CsFieldOfVision csFieldOfVision;	// Has the list of Enemy Objects that are in sight
	CsObjectManager csObjectManager;
	PlayerCommand playerCommand;	// For interaction between PlayerCommand and this Object 
	PlayerProperties playerProperties;
	ObjectManager playerObjectManager;	// For Free List of player's Objects
	ObjectManager neutralObjectManager;	// Only for Scv, for Mineral finding
	EffectManager effectManager;	// the manager that manages free list of death effects
	SpriteRenderer selectingCircle_Mine;
	SpriteRenderer selectingCircle_Neutral;
	SpriteRenderer selectingCircle_Enemy;
	SpriteRenderer selectedCircle_Mine;
	SpriteRenderer selectedCircle_Enemy;
	SpriteRenderer selectedCircle_Neutral;
	Animator anim;
	NavMeshAgent navMeshAgent;
	MeshRenderer meshRenderer;

	// Hashes
	int borednessHash = Animator.StringToHash("boredness");
	int attackPoseHash = Animator.StringToHash("attackPose");
	int movingHash = Animator.StringToHash("moving");
	int attackHash = Animator.StringToHash ("attack");

	float collapsedTimeSinceBorned = 0;
    int voiceNumCount_Affirmation;
	int voiceNumCount_Acknowledge;
    float voiceCoolTimeCount_Affirmation;
	float voiceCoolTimeCount_Acknowledge;
    float attackCoolTimeCount;
	GameObject designationTarget;
	float targetRadius;
	Vector3 destinationPoint;
	Team team;
	Type type;

	// for scv
	bool onDiggingRoutine;	// is it on DiggingRoutine?
	int resourceAmount;	// current resource amount that Scvs carrying
	GameObject resourceTarget;	// the target Mineral or Gas for digging
	GameObject commandCenterTarget;	// the target CommandCenter for returning Mineral
	DigState digState;	// the state when Scv is on dig routine
	public GameObject visualMineral;	// the Mineral that Scv carrys on his hand
    
	// Use this for initialization
	void Start () 
    {
		anim = GetComponentInChildren<Animator>();
		csAttackRange = GetComponentInChildren<CsAttackRange>();
		csFieldOfVision = GetComponentInChildren<CsFieldOfVision> ();
		csProperties = GetComponent<CsProperties>();
		csObjectManager = GetComponent<CsObjectManager> ();
		selectedCircle_Mine = transform.Find ("SelectedCircle_Mine").GetComponent<SpriteRenderer> ();
		selectedCircle_Enemy = transform.Find ("SelectedCircle_Enemy").GetComponent<SpriteRenderer> ();
		selectedCircle_Neutral = transform.Find ("SelectedCircle_Neutral").GetComponent<SpriteRenderer> ();
		selectingCircle_Mine = transform.Find ("SelectingCircle_Mine").GetComponent<SpriteRenderer> ();
		selectingCircle_Enemy = transform.Find ("SelectingCircle_Enemy").GetComponent<SpriteRenderer> ();
		selectingCircle_Neutral = transform.Find ("SelectingCircle_Neutral").GetComponent<SpriteRenderer> ();
		meshRenderer = transform.GetComponentInChildren<MeshRenderer> ();
		effectManager = GameObject.Find ("EffectManager").GetComponent<EffectManager> ();
		navMeshAgent = GetComponent<NavMeshAgent> ();

		playerCommand = GameObject.Find ("Player1").GetComponent<PlayerCommand> ();	// I don't like this part where which team you are, you look at the same playerCommand

		if (csProperties.team == Team.MINE)
		{
			GameObject manager = GameObject.Find ("Player1");

			playerObjectManager = manager.GetComponent<ObjectManager> ();
			playerProperties = manager.GetComponent<PlayerProperties> ();
		}
		else if (csProperties.team == Team.ENEMY)
		{
			GameObject manager = GameObject.Find ("Player2");

			playerObjectManager = manager.GetComponent<ObjectManager> ();
			playerProperties = manager.GetComponent<PlayerProperties> ();
		}
		else
		{
			GameObject manager = GameObject.Find ("Neutral");

			playerObjectManager = manager.GetComponent<ObjectManager> ();
			playerProperties = manager.GetComponent<PlayerProperties> ();
		}

		neutralObjectManager = GameObject.Find ("Neutral").GetComponent<ObjectManager> ();

		csObjectManager.SetPlayerObjectManager (playerObjectManager);

		if(!csProperties.notFreeListObject)
			csObjectManager.ReturnObject();
		else
			Invoke("Initialize", 1);	// this needs to be Invoked because it takes time to find the 'manager' with GameObject.Find at the start
	}

	// Update is called once per frame
	void Update () 
    {
		if(Time.fixedTime < 1)	// wait for all the Initializing
			return;

		if(type != Type.MINERAL && type != Type.GAS)
		{
	        ControlVoiceFrequency();
	        SetAnimatorParam();
	        CountAttackCoolTime();
			GiveBoredness();
			UpdateCollapsedTimeSinceBorned();
			Move ();
			IsFocusTargetInRange ();
			CheckEnemyAround ();
		}

		CheckSelection ();
		OnOffSelectingCircle ();
		OnOffSeletedCircle ();
		SpinSelectingCircle ();

		// only for scv
		DigResourceRoutine ();
		FreeMineral ();
	}


	public void Initialize()
	{
		isInvisible = false;
		isMoving = false;
		isAttacking = false;
		isSelected = false;
		isSelecting = false;
		isAttackAwake = true;
		onDesignationAttack = false;
		collapsedTimeSinceBorned = 0;
		voiceNumCount_Affirmation = 0;
		voiceCoolTimeCount_Affirmation = 0;
		voiceNumCount_Acknowledge = 0;
		voiceCoolTimeCount_Acknowledge = 0;
		attackCoolTimeCount = 0;
		team = csProperties.team;
		type = csProperties.type;
		playerObjectManager.AddToActiveList (gameObject);
		playerProperties.IncreasePopulation(csProperties.costPopulation);

		// don't do this at the start of the game
		if(Time.fixedTime > 1)
		{
			playerProperties.DecreaseMin(csProperties.costMin);
			playerProperties.DecreaseGas(csProperties.costGas);

			if(type == Type.UNIT)
				AudioSource.PlayClipAtPoint(birthQuote, transform.position);
		}
    }
    
	// for debugging
	void DrawPath()
	{
		for (int i = 0; i < navMeshAgent.path.corners.Length - 1; i++)
		{
			Debug.DrawLine (navMeshAgent.path.corners [i], navMeshAgent.path.corners [i + 1]);
        }
    }


    public void StartPointMoving(Vector3 destination)
    {
		if(!isAbleToMove)
			return;

		destinationPoint = destination;
		onDiggingRoutine = false;
		onDesignationAttack = false;
		onDesignationMove = false;
		isMoving = true;
		isAttacking = false;
		isAttackAwake = false;

		navMeshAgent.SetDestination(destinationPoint);
	}

	public void StartTargetMoving(GameObject target)
	{
		if(!isAbleToMove)
			return;

		if(csProperties.objectName.Equals("Scv") && target.GetComponent<CsProperties>().objectName.Equals("CommandCenter"))
		{
			onDiggingRoutine = true;
			digState = DigState.TO_COMMAND;
			commandCenterTarget = target;
			destinationPoint = target.transform.position;
			isMoving = true;
			navMeshAgent.SetDestination(destinationPoint);
			return;
		}


		destinationPoint = target.transform.position;
		designationTarget = target;
		targetRadius = designationTarget.GetComponent<NavMeshAgent>().radius;
		onDesignationMove = true;
		onDiggingRoutine = false;
		onDesignationAttack = false;

		isMoving = true;
		isAttacking = false;
		isAttackAwake = false;

		navMeshAgent.SetDestination(destinationPoint);
	}


	public void StartAttackMoving(Vector3 destination)
	{
		if(!isAbleToMove)
			return;

		destinationPoint = destination;
		isMoving = true;
		onDesignationMove = false;
		onDesignationAttack = false;
		isAttacking = false;
		isAttackAwake = true;

		navMeshAgent.SetDestination(destinationPoint);
    }
    

	void Move()
	{
		if(!isMoving || onDesignationAttack)
			return;

		// for debugging
		DrawPath();

		// we don't want Object's Y axis to rotate
		destinationPoint.y = transform.position.y;

		if(!onDesignationMove)
		{
			// yes, you can use 'NavMeshPath.status' to check if NavAgent reaches the destinationPoint, but it's better this way
			// because you can set 'destinationPoint.y = transform.position.y', and by doing this, the NavAgent doesn't dig into the ground :)
			if(Vector3.SqrMagnitude(destinationPoint - transform.position) < 0.1)
			{
				Stop();
			}
		}

		else // if it's onDesignationMove
		{
			if(Vector3.SqrMagnitude(designationTarget.transform.position - transform.position) < Mathf.Pow(targetRadius + navMeshAgent.radius, 2))
			{
				Stop();
			}
			else
			{
				navMeshAgent.SetDestination(designationTarget.transform.position);
			}
		}
	}
	

	public void AttackObject(GameObject target)
	{
		if(!isAbleToAttack)
			return;

        if(!target || !target.activeInHierarchy || target == gameObject)
		{
			// this is a bug of Unity, the OnTriggerExit doesn't occur when Unit or Building dies
			if(!target.activeInHierarchy)
			{
				csAttackRange.RemoveObjectFromList(target);
				csFieldOfVision.RemoveObjectFromList(target);
			}

			Stop();
			return;
		}

		designationTarget = target;
		onDesignationMove = false;

		if(csAttackRange.Contains (designationTarget))
		{
			isMoving = false;
			isAttacking = true;
			navMeshAgent.SetDestination(transform.position);
			transform.LookAt(new Vector3(designationTarget.transform.position.x, transform.position.y, designationTarget.transform.position.z));	// don't change y axis

			if(attackCoolTimeCount <= 0)
			{
				anim.SetTrigger(attackHash);
				PlayAttackEffect(target);
				designationTarget.SendMessage("GiveDamage", gameObject);
				attackCoolTimeCount = csProperties.attackCoolTime;
			}
		}
		else
		{
			if(!isAbleToMove)
				return;

            isAttackAwake = true;
			isMoving = true;
			isAttacking = false;
			destinationPoint = designationTarget.transform.position;
			navMeshAgent.SetDestination(destinationPoint);
			DrawPath();	// for debug
		}
    }

	// not much different from 'AttackObject', the only difference is making 'onDesignationAttack' to true and 'isAttackAwake' to false
	public void DesignationAttack(GameObject target)
	{
		if(!isAbleToAttack)
			return;
		
		if(!target || !target.activeInHierarchy || target == gameObject)
		{
			// this is a bug of Unity, the OnTriggerExit doesn't occur when Unit or Building dies
			if(!target.activeInHierarchy)
			{
				csAttackRange.RemoveObjectFromList(target);
				csFieldOfVision.RemoveObjectFromList(target);
			}
			
			Stop();
			return;
		}

		onDesignationAttack = true;
		onDesignationMove = false;
		onDiggingRoutine = false;
		isAttackAwake = false;
		designationTarget = target;
		
		if(csAttackRange.Contains (designationTarget))
		{
			isMoving = false;
			isAttacking = true;
			navMeshAgent.SetDestination(transform.position);
			transform.LookAt(new Vector3(designationTarget.transform.position.x, transform.position.y, designationTarget.transform.position.z));	// don't change y axis
			
			if(attackCoolTimeCount <= 0)
			{
				anim.SetTrigger(attackHash);
				PlayAttackEffect(target);
				designationTarget.SendMessage("GiveDamage", gameObject);
				attackCoolTimeCount = csProperties.attackCoolTime;
			}
		}
		else
		{
			if(!isAbleToMove)
				return;
			
			isMoving = true;
			isAttacking = false;
			destinationPoint = designationTarget.transform.position;
			navMeshAgent.SetDestination(designationTarget.transform.position);
			DrawPath();	// for debug
		}
	}


	void IsFocusTargetInRange()
	{
		if(!onDesignationAttack)
			return;

		// if target is dead, or lost the target
		if(!designationTarget.activeInHierarchy)
		{
			// this is a bug of Unity, the OnTriggerExit doesn't occur when Unit or Building dies
			csAttackRange.RemoveObjectFromList(designationTarget);
			csFieldOfVision.RemoveObjectFromList(designationTarget);

			Stop();
		}

		else if(csAttackRange.Contains (designationTarget))
		{
			DesignationAttack(designationTarget);
		}
		else
		{
			isMoving = true;
			isAttacking = false;
			destinationPoint = designationTarget.transform.position;
			navMeshAgent.SetDestination(designationTarget.transform.position);
			DrawPath();
		}

	}


	public void GiveDamage(GameObject attacker)
	{
		int damage;

		if(gameObject.CompareTag("Mineral"))
		   damage = 2;
	   	else
			damage = attacker.GetComponent<CsProperties>().damage - csProperties.armor;

		if(damage > 0)
		{
			csProperties.hp -= damage;

			// revenge ! (if.. I can)
			if(csProperties.team != attacker.GetComponent<CsProperties>().team && isAttackAwake)
			{
				if(isAbleToAttack)
				{
					if(!csAttackRange.IsEnemyInAttackRange())
						StartAttackMoving(attacker.transform.position);
				}
				else
				{
					if(isAbleToMove)
						StartPointMoving(transform.position + transform.position - attacker.transform.position);	// avoid enemy (go to opposite direction)
				}
			}
			if(csProperties.hp <= 0)
				ObjectDeath();
		}
	}


	void CheckEnemyAround()
	{
		if(type != Type.UNIT || !isAttackAwake)	// only Units check around (just for now)
			return;

		if(csAttackRange.IsEnemyInAttackRange())
		{
			GameObject target = csAttackRange.GetPriorityTarget();

			AttackObject(target);
		}
		else if(csFieldOfVision.IsEnemyInSight())
		{
			GameObject target = csFieldOfVision.GetPriorityTarget();

			AttackObject(target);
		}
	}


	void VoiceRespondSelection()
	{
        if (voiceCoolTimeCount_Acknowledge == 0)
		{
			AudioSource.PlayClipAtPoint(voice_acknowledge[voiceNumCount_Acknowledge], transform.position);
			voiceCoolTimeCount_Acknowledge = voice_acknowledge[voiceNumCount_Acknowledge].length;
			voiceNumCount_Acknowledge += Random.Range(1, 3);
			if (voiceNumCount_Acknowledge >= voice_acknowledge.Length)
				voiceNumCount_Acknowledge = 0;
		}
	}

	void VoiceRespondCommand()
	{
		if(type != Type.UNIT)
			return;

		if (voiceCoolTimeCount_Affirmation == 0)
		{
			AudioSource.PlayClipAtPoint(voice_affirmation[voiceNumCount_Affirmation], transform.position);
			voiceCoolTimeCount_Affirmation = voice_affirmation[voiceNumCount_Affirmation].length + 1;
			voiceNumCount_Affirmation += Random.Range(1, 3);
			if (voiceNumCount_Affirmation >= voice_affirmation.Length)
				voiceNumCount_Affirmation = Random.Range(0, 2);
		}
	}


    void SetAnimatorParam()
    {
		if(type != Type.UNIT)
			return;

        if (isMoving)
            anim.SetBool(movingHash, true);
        else
            anim.SetBool(movingHash, false);

		if(csProperties.hasAttackingPose)
		{
			if(isAttacking)
				anim.SetBool (attackPoseHash, true);
			else
				anim.SetBool (attackPoseHash, false);
		}
    }


    void ControlVoiceFrequency()
    {
        if (voiceCoolTimeCount_Affirmation != 0)
        {
			voiceCoolTimeCount_Affirmation -= Time.smoothDeltaTime;
			if (voiceCoolTimeCount_Affirmation < 0)
				voiceCoolTimeCount_Affirmation = 0;
        }
		if (voiceCoolTimeCount_Acknowledge != 0)
		{
			voiceCoolTimeCount_Acknowledge -= Time.smoothDeltaTime;
			if (voiceCoolTimeCount_Acknowledge < 0)
				voiceCoolTimeCount_Acknowledge = 0;
		}
    }

    void CountAttackCoolTime()
    {
        attackCoolTimeCount -= Time.smoothDeltaTime;
        if (attackCoolTimeCount < 0)
            attackCoolTimeCount = 0;
    }

	
	void GiveBoredness()
	{
		if(type != Type.UNIT)
			return;

		anim.SetFloat(borednessHash, collapsedTimeSinceBorned % 60);
	}
	
	void UpdateCollapsedTimeSinceBorned()
	{
		collapsedTimeSinceBorned += Time.deltaTime;
	}

	
	void CheckSelection()
	{
		if(!meshRenderer.isVisible)
			return;

		// when Player selects with dragging
		if(playerCommand.isDraggingMouse)
		{
			Vector3 screenPosOfThis = Camera.main.WorldToScreenPoint(transform.position);
			// WorldToScreenPoint.y is upside down, because in Camera's coordinates system, (0,0) is leftBottom
			screenPosOfThis.y = Screen.height - screenPosOfThis.y;
			
			isSelecting = playerCommand.dragRect.Contains(screenPosOfThis);
		}

		if(Input.GetMouseButtonUp (0))
		{
			if(isSelecting && !isSelected)
				playerCommand.AddSelectedUnitToList(gameObject);

			isSelecting = false;
		}

		// when Player selects with double clicks(or shift+click)
		if(playerCommand.isDoubleClicked)
		{
			if(!playerCommand.GetObjectOnMouse())
				return;

			// Do I need to be selected too?
			if(playerCommand.GetObjectOnMouse().GetComponent<CsProperties>().objectName.Equals(csProperties.objectName))
			{
			    playerCommand.AddSelectedUnitToList(gameObject);
			}
		}
	}

	public void SetIsSelecting(bool activate)
	{
		isSelecting = activate;
	}

	public void SetIsSelected(bool activate)
	{
		isSelected = activate;
	}
	
	void OnOffSelectingCircle()
	{
		if (isSelecting) 
		{
			if (team == Team.MINE)
				selectingCircle_Mine.enabled = true;
			else if(team == Team.ENEMY)
				selectingCircle_Enemy.enabled = true;
			else
				selectingCircle_Neutral.enabled = true;
		}
		else 
		{
			selectingCircle_Mine.enabled = false;
			selectingCircle_Enemy.enabled = false;
			selectingCircle_Neutral.enabled = false;
		}
	}
	
	void OnOffSeletedCircle()
	{
		if(isSelected)
		{
			if (team == Team.MINE)
				selectedCircle_Mine.enabled = true;
			else if(team == Team.ENEMY)
				selectedCircle_Enemy.enabled = true;
			else
				selectedCircle_Neutral.enabled = true;
		}
		else 
		{
			selectedCircle_Mine.enabled = false;
			selectedCircle_Enemy.enabled = false;
			selectedCircle_Neutral.enabled = false;
		}
	}
	
	void SpinSelectingCircle()
	{
		if (isSelecting) 
		{
			selectingCircle_Mine.transform.Rotate (new Vector3 (0, 0, 1));
			selectingCircle_Enemy.transform.Rotate (new Vector3 (0, 0, 1));
			selectingCircle_Neutral.transform.Rotate (new Vector3 (0, 0, 1));
		}
	}

	public void PlayAttackEffect(GameObject target)
	{
		if(!target.activeInHierarchy)
			return;

		AudioSource.PlayClipAtPoint (attackEffectSound[Random.Range(0, attackEffectSound.Length -1)], transform.position);

		if(bulletEffect != null)
		{
			Vector3 bulletEffectPos = target.transform.position;
			Vector3 targetBoundSize = target.GetComponent<CsCommandProcessor> ().GetBoundSize ();
			
			bulletEffectPos = Vector3.MoveTowards (bulletEffectPos, Camera.main.transform.position, Mathf.Sqrt (Mathf.Pow (targetBoundSize.x / 2, 2) + 
			                                                                                                    Mathf.Pow (targetBoundSize.y / 2, 2)));
			Instantiate (bulletEffect, bulletEffectPos, Quaternion.identity);
		}

		if (fireEffect != null) 
		{
			fireEffect.SetActive (true);
			Invoke ("TurnOffFireEffect", fireEffectTime);
		}
	}

	// the real GameObject which has mesh and rigidBody
	public GameObject GetChildWithRigidBody()
	{
		return GetComponentInChildren<Rigidbody> ().gameObject;
	}

	public Vector3 GetBoundSize()
	{
		return GetChildWithRigidBody().collider.bounds.size;
	}


	public void RemoveThisFromSelecteds()
	{
		playerCommand.RemoveObjectFromSelecteds (gameObject);
	}

	public bool IsOnDesignationAttack()
	{
		if(onDesignationAttack)
			return true;

		return false;
	}

	void Stop()
	{
		navMeshAgent.SetDestination(transform.position);
		onDesignationMove = false;
		onDesignationAttack = false;
		isMoving = false;
		isAttacking = false;
		isAttackAwake = true;
		onDiggingRoutine = false;
	}

	void ObjectDeath()
	{
		playerCommand.RemoveObjectFromSelecteds(gameObject);

		if(type == Type.UNIT)
		{
			if(csProperties.hasDeathAnimation)
			{
				effectManager.GetGameObject(csProperties.objectName + "Death", csProperties.color, transform.position, transform.rotation);
				AudioSource.PlayClipAtPoint(deathQuote[Random.Range(0, deathQuote.Length -1)], transform.position);
			}
			else
			{
				Instantiate(deathEffect, new Vector3(transform.position.x, transform.position.y + GetBoundSize().y, transform.position.z), Quaternion.identity);
				AudioSource.PlayClipAtPoint(deathQuote[Random.Range(0, deathQuote.Length -1)], transform.position);
			}
			playerProperties.DecreasePopulation(csProperties.costPopulation);
		}
		else if(type == Type.BUILDING)
		{
			Instantiate(deathEffect, new Vector3(transform.position.x, transform.position.y + GetBoundSize().y, transform.position.z), Quaternion.identity);
			AudioSource.PlayClipAtPoint(deathQuote[Random.Range(0, deathQuote.Length -1)], transform.position);
		}

		csObjectManager.ReturnObject();
    }
    
    void TurnOffFireEffect()
	{
		fireEffect.SetActive (false);
	}


	
	///////////////////////////////////////////////// From here, it's only for Scv.., why it's here? Had no time to seperate /////////////////////////////////

	void StartDiggingResource(GameObject resource)
	{
		if(csProperties.objectName.Equals("Scv"))
		{
			resourceTarget = resource;
			onDiggingRoutine = true;
			isMoving = true;
			digState = DigState.TO_MINERAL;
			destinationPoint = resourceTarget.transform.position;
			navMeshAgent.SetDestination(destinationPoint);
		}
		else
			StartTargetMoving(resource);
	}


	void DigResourceRoutine()
	{
		if(!onDiggingRoutine)
			return;

		isAttackAwake = false;
		onDesignationMove = false;
		onDesignationAttack = false;

		if(digState == DigState.TO_MINERAL || digState == DigState.DIGGING)
		{
			// if mineral disappeared
			if(!resourceTarget.activeInHierarchy)
			{
				FindClosestFreeMineral();
				return;
			}
			
			if(csAttackRange.Contains (resourceTarget))
			{
				if(resourceAmount >= 8)
				{
					FindClosestCommandCenter();
					return;
				}

				// is somebody digging the target Mineral?
				if(resourceTarget.GetComponent<CsMineral>().diggingObject != null)
				{
					if(resourceTarget.GetComponent<CsMineral>().diggingObject != gameObject)
					{
						Stop ();
						onDiggingRoutine = true;
						FindClosestFreeMineral();
						return;
					}
				}

           		isMoving = false;
				navMeshAgent.SetDestination(transform.position);
				transform.LookAt(new Vector3(resourceTarget.transform.position.x, transform.position.y, resourceTarget.transform.position.z));	// don't change y axis
				digState = DigState.DIGGING;

				if(attackCoolTimeCount <= 0)
				{
					resourceTarget.GetComponent<CsMineral>().diggingObject = gameObject;
                    anim.SetTrigger(attackHash);
					PlayAttackEffect(resourceTarget);
					resourceTarget.SendMessage("GiveDamage", gameObject);
					resourceAmount += 2;
					visualMineral.SetActive(true);
					attackCoolTimeCount = csProperties.attackCoolTime;
				}
			}
			else
			{
				isMoving = true;
				destinationPoint = resourceTarget.transform.position;
				navMeshAgent.SetDestination(destinationPoint);
			}
		}

		else if(digState == DigState.TO_COMMAND)
		{
			// if target is dead, or lost the target
			if(!commandCenterTarget.activeInHierarchy)
			{
				FindClosestCommandCenter();
				return;
			}
			
			if(csAttackRange.Contains (commandCenterTarget))
			{
				playerProperties.currentMin += resourceAmount;
				resourceAmount = 0;
				visualMineral.SetActive(false);

				if(resourceTarget != null)
				{
					destinationPoint = resourceTarget.transform.position;
					navMeshAgent.SetDestination(destinationPoint);
					digState = DigState.TO_MINERAL;
				}
				else
					FindClosestFreeMineral();
			}
		}
	}


	void FindClosestCommandCenter()
	{
		digState = DigState.TO_COMMAND;
		ArrayList commandCenters = playerObjectManager.GetActiveGameObjects("CommandCenter");

		if(commandCenters.Count <= 0)
		{
			Stop();
			return;
		}
		// find closest commandCenter
		if (commandCenters.Count >= 2)
		{
			float targetValue = 0;
			GameObject targetObject;
			int roopNum = commandCenters.Count;
			
			for(int j = 0; j < commandCenters.Count - 1; j++)
			{
				targetObject = commandCenters[0] as GameObject;
				targetValue = Vector3.SqrMagnitude(targetObject.transform.position - transform.position);
				
				for(int i = 0; i < roopNum - 1; i++)
				{
					GameObject currentObject = commandCenters[i+1] as GameObject;
					float currentValue = Vector3.SqrMagnitude(currentObject.transform.position - transform.position);
					
					if(targetValue > currentValue)
					{
						object temp = commandCenters[i];
						commandCenters[i] = commandCenters[i+1];
						commandCenters[i+1] = temp;
					}
					else
					{
						targetValue = currentValue;
					}
				}
				roopNum--;
			}
		}

		commandCenterTarget = commandCenters[0] as GameObject;
		isMoving = true;
		destinationPoint = commandCenterTarget.transform.position;
		navMeshAgent.SetDestination(destinationPoint);
	}

	void FindClosestFreeMineral()
	{
		digState = DigState.TO_MINERAL;
		resourceTarget = null;

		ArrayList mineralList = neutralObjectManager.GetActiveGameObjects("Mineral");

		// if there is no Mineral in the world, Stop
		if(mineralList.Count <= 0)
		{
			Stop();
			return;
		}

		// first, remove those which are currently digged
		for(int i = 0; i < mineralList.Count; i++)
		{
			GameObject target = mineralList[i] as GameObject;

			if(target.GetComponent<CsMineral>().diggingObject != null)
				mineralList.RemoveAt(i);
		}

		// if there is no Mineral that is not being digged, find it again a moment later 
		if(mineralList.Count <= 0)
		{
			Invoke("FindClosestFreeMineral", 0.1f);
			return;
		}

		// sort out the closest Mineral
		if (mineralList.Count >= 2)
		{
			float targetValue = 0;
			GameObject targetObject;
			int roopNum = mineralList.Count;
			
			for(int j = 0; j < mineralList.Count - 1; j++)
			{
				targetObject = mineralList[0] as GameObject;;
				targetValue = Vector3.SqrMagnitude(targetObject.transform.position - transform.position);
				
				for(int i = 0; i < roopNum - 1; i++)
				{
					GameObject currentObject = mineralList[i+1] as GameObject;;
					float currentValue = Vector3.SqrMagnitude(currentObject.transform.position - transform.position);
					
					if(targetValue > currentValue)
					{
						object temp = mineralList[i];
						mineralList[i] = mineralList[i+1];
						mineralList[i+1] = temp;
					}
					else
					{
						targetValue = currentValue;
					}
				}
				roopNum--;
			}
		}

		GameObject closestMineral = mineralList [0] as GameObject;

		resourceTarget = closestMineral;
		isMoving = true;
		destinationPoint = resourceTarget.transform.position;
		navMeshAgent.SetDestination(destinationPoint);
	}

	void FreeMineral()
	{
		if(onDiggingRoutine)
		{
			if(digState == DigState.TO_COMMAND || digState == DigState.TO_MINERAL )
			{
				if(resourceTarget != null)
				{
					// if I was digging the resourceTarget, free it when leaving (if somebody else was digging, don't make it null)
					if(resourceTarget.GetComponent<CsMineral>().diggingObject == gameObject)
						resourceTarget.GetComponent<CsMineral>().diggingObject = null;
				}
			}
		}
		else
		{
			if(resourceTarget != null)
			{
				// if I was digging the resourceTarget, free it when leaving (if somebody else was digging, don't make it null)
				if(resourceTarget.GetComponent<CsMineral>().diggingObject == gameObject)
					resourceTarget.GetComponent<CsMineral>().diggingObject = null;

				resourceTarget = null;
			}
		}

	}

	// only for buildings
	public Transform GetSpawnPoint()
	{
		return spawnPoint;
	}


	// only for Marine
	public void Stimpack()
	{
		if(csProperties.hp > 10)
		{
			csProperties.hp -= 10;
			csProperties.attackCoolTime = 0.7f;
			navMeshAgent.speed = 5;
			fireEffectTime = 0.5f;
			Invoke ("StimpackOver", 10);
		}
	}

	void StimpackOver()
	{
		csProperties.attackCoolTime = csProperties.static_attackCoolTime;
		fireEffectTime = 0.7f;
		navMeshAgent.speed = csProperties.static_speed;
	}
}







