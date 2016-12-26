using UnityEngine;
using System.Collections;
using Leap;
using UnityEngine.UI;

public class CubeControl : MonoBehaviour {

	public HandController hc;
	public KeyCode selectKey = KeyCode.S;
	public bool selected;
	private Frame currentFrame;
	private Vector3 offset;
	private GameObject ctrler;
	// Use this for initialization
	void OnTriggerEnter(Collider c)
	{
		if (!Input.GetKey (selectKey)) {
			selected = false;
			return;
		}
		if (c.gameObject.transform.parent == null)
			return;
		if( c.gameObject.transform.parent.parent == null)
			return;
		if (c.gameObject.transform.parent.parent.name.Equals ("RigidRoundHand(Clone)")) {
			selected = true;
			offset = transform.position - c.transform.position;
			ctrler = c.gameObject;
		} else {
			selected = false;
			ctrler = null;
		}
	}
	void Start()
	{
		selected = false;
	}

	void Update()
	{
		if(!selected || ctrler==null )return;
		if (Input.GetKeyUp (selectKey) && selected)
			selected = false;
		transform.position = ctrler.transform.position + offset;
	}
}
