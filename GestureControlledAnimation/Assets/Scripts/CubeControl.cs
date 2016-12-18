using UnityEngine;
using System.Collections;
using Leap;
using UnityEngine.UI;

public class CubeControl : MonoBehaviour {

  public GameObject cube;
  public HandController hc;
  public bool selected;
  // Use this for initialization
  void OnTriggerEnter(Collider c)
  {
   if (c.gameObject.transform.parent.name.Equals("index"))
   {
    selected = true;
   }
  }

  void Start()
  {
	  selected = false;
  }
  
  Frame currentFrame;
  void Update()
  {
		if(!selected)return;
    this.currentFrame = hc.GetFrame();  
		foreach (var h in hc.GetFrame().Hands)
		{
			foreach (var f in h.Fingers)
			{
				if (f.Type == Finger.FingerType.TYPE_INDEX)
			  {
					Leap.Vector position = f.TipPosition;
					Vector3 unityPosition = position.ToUnityScaled(false);
					Vector3 worldPosition = hc.transform.TransformPoint(unityPosition);
  				this.cube.transform.position = worldPosition;
			  }
			}
		}
  }
}
