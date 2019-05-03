using UnityEngine;
using System.Collections;

public class CsFieldOfVision : MonoBehaviour {
	
	ArrayList inRangeObjects = new ArrayList ();
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
	}

	void OnTriggerEnter(Collider other)
	{
		GameObject targetObject = other.transform.parent.gameObject;

		// if the target is itself
		if(targetObject == transform.parent.gameObject)
			return;

		if(targetObject.CompareTag("Unit") || targetObject.CompareTag("Building"))
		{
			// if it's Ally or Neutral or if it's already in list, just ignore it
			if(inRangeObjects.Contains(targetObject))
				return;
			if(csProperties.team != targetObject.GetComponent<CsProperties>().team)
			{
				if(targetObject.GetComponent<CsProperties>().team != Team.NEUTRAL)
					inRangeObjects.Add (targetObject);
			}
		}
	}
	
	void OnTriggerExit(Collider other)
	{
		inRangeObjects.Remove (other.transform.parent.gameObject);
	}
	
	public bool Contains(GameObject gameObject)
	{
		return inRangeObjects.Contains (gameObject);
	}

	public bool IsEnemyInSight()
	{
		if(inRangeObjects.Count <= 0)
			return false;

		return true;
	}

	public GameObject GetPriorityTarget()
	{
		if(inRangeObjects.Count >= 1)
		{
			SortObjectsByDistance();
			return inRangeObjects [0] as GameObject;
		}
		return null;
	}


	void SortObjectsByDistance()
	{
		if (inRangeObjects.Count < 2)
			return;
		
		float targetDistance;
		GameObject targetObject;
		int roopNum = inRangeObjects.Count;
		
		for(int j = 0; j < inRangeObjects.Count - 1; j++)
		{
			targetObject = inRangeObjects[0] as GameObject;
			targetDistance = Vector3.SqrMagnitude(gameObject.transform.position - targetObject.transform.position);
			
			for(int i = 0; i < roopNum - 1; i++)
			{
				GameObject currentObject = inRangeObjects[i+1] as GameObject;
				float currentDistance = Vector3.SqrMagnitude(gameObject.transform.position - currentObject.transform.position);
				
				if(targetDistance > currentDistance)
				{
					object temp = inRangeObjects[i];
					inRangeObjects[i] = inRangeObjects[i+1];
					inRangeObjects[i+1] = temp;
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
		SortObjectsByDistance ();
	}

	public void SetColliderSize(float radius)
	{
		sphereCollider = GetComponent<SphereCollider> ();
		sphereCollider.radius = radius;
	}
}





