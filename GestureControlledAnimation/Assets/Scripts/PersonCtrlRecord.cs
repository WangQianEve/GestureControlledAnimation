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
	public KeyCode record = KeyCode.R;
	public KeyCode finishRecording = KeyCode.Space;
	public KeyCode saveRecording = KeyCode.S;
	public KeyCode play = KeyCode.Space;
	public KeyCode MakeCurve = KeyCode.M;
	//parameters
	public float recordPrecision = 0.02f;//距离
	public int startLine = 0;
	public int endLine = -1;
	//
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

	private bool finishedReading;
	private bool isReplaying;
	private bool isRecording;
	private bool finishedRecording;

	MeshRenderer meshRenderer;
	public void Start()
	{
		meshRenderer = GetComponent<MeshRenderer> ();
		cubeCtrl = GetComponent (typeof(CubeControl)) as CubeControl;
		path = System.IO.Directory.GetCurrentDirectory();
		path = path + "\\Assets\\UserFiles\\Positions\\";
		readFileName = path + name + "_r.txt";
		writeFileName = path + name + "_w.txt";
		readFileBackupName = path + name + "_b.txt";
		Debug.Log (name + " Test path : " + readFileName);
		originX = transform.position.x;
		originY = transform.position.y;
		originZ = transform.position.z;

		finishedReading = false;
		isReplaying = false;
		isRecording = false;
		finishedRecording = true;
	}

	public void Update()
	{

		if (Input.GetKeyDown (record)) 
		{
			recordSelected = cubeCtrl.selected;
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
					preRead ();
					Debug.Log ("Start Recording...");
					recordFlagTime = Time.time;//?
					recordFlagPos = transform.position;
					isRecording = true;
					isReplaying = false;
				} catch (Exception ex) {  
					Console.WriteLine ("Open File Fails {0}", ex.ToString ());  
				}  
			}
		}

		if (Input.GetKeyDown (finishRecording)) 
		{
			if (recordSelected) {
				Debug.Log (name+ " Recording Finished! ");
				postRead ();
			}
			isRecording = false;
			isReplaying = false;
		}

		if (Input.GetKeyDown (saveRecording) && Input.GetKey(KeyCode.LeftShift)) 
		{
			if (recordSelected) {
				Debug.Log (name+" Saving Files ");
				reader.Close ();
				writer.Close ();
				readfs.Close ();
				writefs.Close ();
				File.Replace (writeFileName, readFileName, readFileBackupName);
				Debug.Log ("Saving finished!");
			}
		}

		if (Input.GetKeyDown (MakeCurve))
		{
			if (cubeCtrl.selected || recordSelected) {
				makeCurve ();
				Debug.Log (name+ " finished making curve");
			}
		}
			
		if (isRecording) {
			Vector3 temp = transform.position;
			if (Vector3.Distance (temp, recordFlagPos) > recordPrecision) {
				writer.WriteLine ((Time.time - recordFlagTime).ToString () + "," + temp.x.ToString () + "," + temp.y.ToString () + "," + temp.z.ToString () );
				recordFlagTime = Time.time;
				recordFlagPos = temp;
			}
		}

		if (isReplaying)
		{
			if (Time.time - currentTime < totalTime) {
				transform.position = new Vector3 (animX.Evaluate (Time.time - currentTime), 
					animY.Evaluate (Time.time - currentTime), animZ.Evaluate (Time.time - currentTime));
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
		float sumTime = 0;

		Keyframe[] ksX = new Keyframe[10000];
		Keyframe[] ksY = new Keyframe[10000];
		Keyframe[] ksZ = new Keyframe[10000];
		int cnt = 0;
		float lastX = originX;
		float lastY = originY;
		float lastZ = originZ;

		while ((strReadline = reader.ReadLine()) != null) // strReadline即为按照行读取的字符串
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
		int i = startLine;
		string temp = "";
		while (i-->0) {
			temp = reader.ReadLine ();
			writer.WriteLine (temp);
		}
	}

	private void postRead(){
		if (endLine == -1)
			return;
		int i = endLine > startLine ? endLine - startLine : startLine;
		string temp = "";
		while (i-->0) {
			reader.ReadLine ();
		}
		while ((temp = reader.ReadLine ())!=null) {
			writer.WriteLine (temp);
		}
	}
}
