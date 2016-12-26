using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;
using System;
public class PersonCtrlRecord : MonoBehaviour {
	//modes
	public CubeControl cubeCtrl;
	//
	public AnimationCurve anim;
	public AnimationCurve animX, animY, animZ;
	//keys
	public KeyCode recordKey = KeyCode.R;
	public KeyCode finishKey = KeyCode.F;
	public KeyCode selectKey = KeyCode.S;
	public KeyCode playKey = KeyCode.Space;
	//parameters
	public float recordPrecision = 0.02f;//距离
	//
	private MeshRenderer meshRenderer;

	private bool recordSelected;
	private string contextPath;
	private string path;
	private string readFileName;
	private string writeFileName;
	private string readFileBackupName;
	private FileStream readfs;
	private FileStream writefs;
	private StreamReader reader;
	private StreamWriter writer;
	private Encoding encoder = Encoding.UTF8;
	private float recordFlagTime = 0;
	private Vector3 recordFlagPos;

	private float currentTime = 0;
	private float originX = 0, originY = 0, originZ = 0;
	private float totalTime = 0;
	private float startTime = 0;
	private float endTime = -1;
	private float curveStartTime = 0;
	private float breathTime = 0.5f;

	private float sumTime = 0f;

	private int editEndLine = 0;
	private int editStartLine = 0;
	private int recordStartLine = -1;
	private int recordEndLine = -2;

	private bool finishedReading;
	private bool isReplaying;
	private bool isRecording;
	private bool finishedRecording;
	private bool isFirstFrame = true;
	public void Start()
	{
		meshRenderer = GetComponent<MeshRenderer> ();
		cubeCtrl = GetComponent (typeof(CubeControl)) as CubeControl;
		path = System.IO.Directory.GetCurrentDirectory();
		path = path + "\\Assets\\UserFiles\\Positions\\";
		readFileName = path + name + "_r.txt";
		writeFileName = path + name + "_w.txt";
		readFileBackupName = path + name + "_b.txt";
		originX = transform.position.x;
		originY = transform.position.y;
		originZ = transform.position.z;

		totalTime = 0f;

		finishedReading = false;
		isReplaying = false;
		isRecording = false;
		finishedRecording = true;
	}

	public void Update()
	{

		if (Input.GetKeyDown (recordKey)) 
		{
			recordSelected = cubeCtrl.selected;
			curveStartTime = Time.time - startTime + breathTime;
			Debug.Log (curveStartTime.ToString ());
			if (!recordSelected) {
				isRecording = false;
				isReplaying = true;
				meshRenderer.enabled = false;
				currentTime = Time.time;
			} else {
				try {
					if (!File.Exists (readFileName)) {
						Debug.Log ("Making new file " + readFileName);
						File.Create (readFileName);
					}
					if (!File.Exists (writeFileName)) {
						Debug.Log ("Making new file " + writeFileName);
						File.Create (writeFileName);
					}
					Debug.Log ("Start Recording...1");
					readfs = new FileStream (readFileName, FileMode.Open, FileAccess.Read);
					if(readfs == null)Debug.Log("readfs null");
					writefs = new FileStream (writeFileName, FileMode.Open, FileAccess.Write);
					if(writefs == null)Debug.Log("writefs null");
					reader = new StreamReader (readfs, Encoding.Default);
					if(reader == null)Debug.Log("reader null");
					writer = new StreamWriter (writefs, Encoding.Default);
					if(writer == null)Debug.Log("writer null");
					Debug.Log ("Start Recording..."+startTime.ToString());
					recordFlagTime = Time.time - startTime + breathTime;
					Debug.Log(recordFlagTime.ToString());
					Debug.Log(recordFlagTime.CompareTo(curveStartTime).ToString());
					preRead ();
					editStartLine = recordStartLine + 1;
					editEndLine = recordStartLine ;
					isRecording = true;
					isReplaying = false;
				} catch (Exception ex) {  
					Console.WriteLine ("Open File Fails {0}", ex.ToString ());  
				}  
			}
		}

		if (Input.GetKeyDown (finishKey)) 
		{
			isRecording = false;
			isReplaying = false;
			if (cubeCtrl.selected) {
				if (recordSelected) {
					Debug.Log (name+ " Recording Finished! ");
					postRead ();
					Debug.Log (name+" Saving Files ");
					reader.Close ();
					writer.Close ();
					readfs.Close ();
					writefs.Close ();
					File.Replace (writeFileName, readFileName, readFileBackupName);
					Debug.Log ("Saving finished!");
				}
				makeCurve ();
				Debug.Log (name+ " finished making curve");
			}
		}
			
		if (isRecording) {
			Vector3 temp = transform.position;
			float ttime = Time.time - curveStartTime;
			if (ttime <= startTime) {
				Debug.Log ("ttime "+ttime.ToString());
				if (startTime == 0 && ttime==0) {
					writer.WriteLine ("0" + "," + temp.x.ToString () + "," + temp.y.ToString () + "," + temp.z.ToString () );
					recordFlagTime = Time.time;
					recordFlagPos = temp;
					editEndLine++;
				}
				return;
			}
			if (Time.time - curveStartTime >= endTime && endTime >= 0) {
				isRecording = false;
			}
			if (Vector3.Distance (temp, recordFlagPos) > recordPrecision || (!isRecording) ) {
				writer.WriteLine ((Time.time - recordFlagTime).ToString () + "," + temp.x.ToString () + "," + temp.y.ToString () + "," + temp.z.ToString () );
				recordFlagTime = Time.time;
				recordFlagPos = temp;
				editEndLine++;
			}
		}

		if (isReplaying)
		{
			float delta = Time.time - curveStartTime;
			if (delta < 0)
				delta = 0f;
			if (delta  < totalTime) {
				transform.position = new Vector3 (animX.Evaluate (delta), animY.Evaluate (delta), animZ.Evaluate (delta));
			} else {
				meshRenderer.enabled = true;
				isReplaying = false;
			}
		}
	}

	public void makeCurve()
	{
		reader = new StreamReader(readFileName);
		string strReadline;
		sumTime = 0f;
		Keyframe[] ksX = new Keyframe[10000];
		Keyframe[] ksY = new Keyframe[10000];
		Keyframe[] ksZ = new Keyframe[10000];
		int cnt = 0;
		float lastX = originX;
		float lastY = originY;
		float lastZ = originZ;

		while ((strReadline = reader.ReadLine()) != null)
		{
			string[] rec = strReadline.Split (',');
			float deltaTime = float.Parse (rec [0]);
			float recX = float.Parse (rec [1]);
			float recY = float.Parse (rec [2]);
			float recZ = float.Parse (rec [3]);
			sumTime += deltaTime;
			ksX [cnt] = new Keyframe (sumTime, recX);
			ksY [cnt] = new Keyframe (sumTime, recY);
			ksZ [cnt] = new Keyframe (sumTime, recZ);
			cnt++;
		}

		totalTime = sumTime;

		Debug.Log ("cnt = " + cnt);

		animX = new AnimationCurve(ksX);
		animX.preWrapMode = WrapMode.Loop;
		animX.postWrapMode = WrapMode.Loop;

		animY = new AnimationCurve(ksY);
		animY.preWrapMode = WrapMode.Loop;
		animY.postWrapMode = WrapMode.Loop;

		animZ = new AnimationCurve(ksZ);
		animZ.preWrapMode = WrapMode.Loop;
		animZ.postWrapMode = WrapMode.Loop;

		reader.Close();
	}

	private void preRead(){
		int i = recordStartLine;
		sumTime = 0f;
		while (i-->=0) {
			string temp = reader.ReadLine ();
			string[] rec = temp.Split (',');
			float deltaTime = float.Parse (rec [0]);
			if(i==0)
				recordFlagPos = new Vector3 (float.Parse (rec [1]), float.Parse(rec[2]), float.Parse(rec[3]) );
			sumTime = sumTime + deltaTime;
			writer.WriteLine (temp);
		}
		recordFlagTime = recordFlagTime + sumTime;
	}

	private void postRead(){
		if (recordEndLine == -2)
			return;
		int i = recordEndLine > recordStartLine ? recordEndLine - recordStartLine : recordStartLine;
		while (i-->0) {
			string temp = reader.ReadLine ();
			string[] rec = temp.Split (',');
			sumTime = sumTime + float.Parse (rec [0]);
		}
		string ttemp = reader.ReadLine ();
		if (ttemp != null) {//the modified line
			string[] rrec = ttemp.Split (',');
			sumTime = sumTime + float.Parse (rrec [0]);
			float time = sumTime - recordFlagTime + curveStartTime;
			writer.WriteLine (time.ToString()+","+float.Parse(rrec[1]).ToString()+","+float.Parse(rrec[2]).ToString()+","+float.Parse(rrec[3]).ToString());
		}
		ttemp = "";
		while ((ttemp = reader.ReadLine ())!=null) {
			writer.WriteLine (ttemp);
		}
	}
}
