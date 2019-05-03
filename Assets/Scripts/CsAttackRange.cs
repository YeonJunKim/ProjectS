using UnityEngine;
using System.Collections;

public class CsAttackRange : MonoBehaviour {

	ArrayList inRangeObjects = new ArrayList ();
	ArrayList inRangeEnemyObjects = new ArrayList ();

	CsProperties csProperties;
	SphereCollider sphereCollider;
    
	// Use this for initialization
	void Start () 
    {
		csProperties = GetComponentInParent<CsProperties>();
	}
	
	// Update is called once per frame
	void Update () 
    {
	}

	public void Initialize()
	{
		inRangeObjects.Clear ();
		inRangeEnemyObjects.Clear ();
	}

	void OnTriggerEnter(Collider other)
	{
		GameObject targetObject = other.transform.parent.gameObject;

		// if the target is itself
		if(targetObject == transform.parent.gameObject)
			return;

		if(!targetObject.CompareTag("Ground"))
		{
			if(inRangeObjects.Contains(targetObject))
				return;

			inRangeObjects.Add (targetObject);

			if(csProperties.team != targetObject.GetComponent<CsProperties>().team)
			{
				if(targetObject.GetComponent<CsProperties>().team != Team.NEUTRAL)
				{
					inRangeEnemyObjects.Add(targetObject);
				}
			}
		}
	}

	void OnTriggerExit(Collider other)
	{
		GameObject targetObject = other.transform.parent.gameObject;

		inRangeObjects.Remove (targetObject);

		if(csProperties.team != targetObject.GetComponent<CsProperties>().team)
		{
			if(targetObject.GetComponent<CsProperties>().team != Team.NEUTRAL)
			{
				inRangeEnemyObjects.Remove(targetObject);
			}
		}
	}

	public bool Contains(GameObject target)
	{
		return inRangeObjects.Contains (target);
	}

	public bool IsEnemyInAttackRange()
	{
		if(inRangeEnemyObjects.Count <= 0)
			return false;

		return true;
	}

	public GameObject GetPriorityTarget()
	{
		if(inRangeEnemyObjects.Count >= 1)
		{
			SortObjectsByDistance();
			return inRangeEnemyObjects [0] as GameObject;
		}

		return null;
	}

	// done by bubble sort
	void SortObjectsByDistance()
	{
		if (inRangeEnemyObjects.Count < 2)
			return;
		
		float targetDistance;
		GameObject targetObject;
		int roopNum = inRangeEnemyObjects.Count;
		
		for(int j = 0; j < inRangeEnemyObjects.Count - 1; j++)
		{
			targetObject = inRangeEnemyObjects[0] as GameObject;
			targetDistance = Vector3.SqrMagnitude(gameObject.transform.position - targetObject.transform.position);
			
			for(int i = 0; i < roopNum - 1; i++)
			{
				GameObject currentObject = inRangeEnemyObjects[i+1] as GameObject;
				float currentDistance = Vector3.SqrMagnitude(gameObject.transform.position - currentObject.transform.position);
				
				if(targetDistance > currentDistance)
				{
					object temp = inRangeEnemyObjects[i];
					inRangeEnemyObjects[i] = inRangeEnemyObjects[i+1];
					inRangeEnemyObjects[i+1] = temp;
				}
				else
				{
					targetDistance = currentDistance;
				}
			}
			roopNum--;
		}
	}

	public void RemoveObjectFromList(GameObject target)
	{
		inRangeObjects.Remove (target);
		inRangeEnemyObjects.Remove (target);
		SortObjectsByDistance ();
	}

	public void SetColliderSize(float radius)
	{
		sphereCollider = GetComponent<SphereCollider> ();
		sphereCollider.radius = radius;
	}
	
}







