using UnityEngine;
using System.Collections;

public enum Team
{
	MINE,
	ENEMY,
	NEUTRAL,
};

public enum Type
{
	UNIT,
	BUILDING,
	MINERAL,
	GAS,
};

public class CsProperties : MonoBehaviour {
	
	public string objectName;
	public Team team;
	public Type type;
	public ObjectColor color;
	public bool hasAttackingPose;
	public bool hasDeathAnimation;
	public bool notFreeListObject;

	public int static_hp;
	public int static_mp;
	public int static_damage;
	public int static_armor;
	public float static_speed;
	public float static_attackRange;
	public float static_fieldOfVision;
	public float static_attackCoolTime;
	public int static_expTime;
	public int costMin;
	public int costGas;
	public int costPopulation;

	[HideInInspector] public int hp;
	[HideInInspector] public int hpChanges;
	int startHp;
	
	[HideInInspector] public int mp;
	[HideInInspector] public int mpChanges;
	int startMp;
	
	[HideInInspector] public int damage;
	[HideInInspector] public int damageChanges;
	
	[HideInInspector] public int armor;
	[HideInInspector] public int armorChanges;
	
	[HideInInspector] public float speed;
	[HideInInspector] public float speedChanges;
	
	[HideInInspector] public float attackRange;
	[HideInInspector] public float attackRangeChanges;
	
	[HideInInspector] public float fieldOfVision;
	[HideInInspector] public float fieldOfVisionChanges;

	[HideInInspector] public float attackCoolTime;
	[HideInInspector] public float attackCoolTimeChanges;
	
	[HideInInspector] public int expTime;
	[HideInInspector] public int expTimeChanges;

	NavMeshAgent navMeshAgent;
	CsFieldOfVision csFieldOfVision;
	CsAttackRange csAttackRange;

	// Use this for initialization
	void Start () {
		Initialize ();
	}
	
	// Update is called once per frame
	void Update () {
	
	}

	public void Initialize()
	{

		csFieldOfVision = GetComponentInChildren<CsFieldOfVision> ();
		csAttackRange = GetComponentInChildren<CsAttackRange> ();
		if(type == Type.UNIT)
			navMeshAgent = GetComponent<NavMeshAgent> ();

		hp = static_hp + hpChanges;
		startHp = static_hp + hpChanges;
		mp = static_mp + mpChanges;
		damage = static_damage + damageChanges;
		armor = static_armor + armorChanges;
		speed = static_speed + speedChanges;
		attackRange = static_attackRange + attackRangeChanges;
		fieldOfVision = static_fieldOfVision + fieldOfVisionChanges;
		attackCoolTime = static_attackCoolTime + attackCoolTimeChanges;
		expTime = static_expTime + expTimeChanges;

		if (type == Type.UNIT)
		{
			navMeshAgent.speed = speed;
			csFieldOfVision.SetColliderSize (fieldOfVision);
			csAttackRange.SetColliderSize (attackRange);
		}
	}


	public int GetValue()
	{
		return costMin + costGas;
	}

	public float GetHpPercentage()
	{
		return (float)hp / startHp;
	}


}















