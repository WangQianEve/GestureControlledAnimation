using UnityEngine;
using System.Collections;

public class CreateCurve : MonoBehaviour {
	private bool isRecording;
	private bool isReplaying;
    private AnimationCurve anim;
	private Vector3 currentPosition= new Vector3(0,0,0); 
    void Start() {
        Keyframe[] ks = new Keyframe[3];
        ks[0] = new Keyframe(0, 0);
        ks[0].inTangent = 0;
		ks[0].outTangent = 0;
        ks[1] = new Keyframe(4, 5);
        ks[1].inTangent = 45;
        ks[2] = new Keyframe(8, 0);
        ks[2].inTangent = 90;
        anim = new AnimationCurve(ks);
    }
	void Update(){
        if (Input.GetKeyDown(KeyCode.R))
        {
            Debug.Log("您按下了W键");
        }
    }
    void FixedUpdate() {
      transform.position = new Vector3(Time.time, anim.Evaluate(Time.time), 0);
    }
}
