using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;

public class CubeScript : MonoBehaviour {
	//modes
	public bool isSelected = true ;
	//
	public AnimationCurve anim;
	public AnimationCurve animX, animY, animZ;
	//keys
	public KeyCode record = KeyCode.R;
	public KeyCode finishRecording = KeyCode.S;
	public KeyCode resetRecording = KeyCode.Space;
	public KeyCode play = KeyCode.Space;
	public KeyCode MakeCurve = KeyCode.M;
	//parameters
	public float recordPrecision = 0.02f;//距离
	public long startLine = 0;
	public long endLine = 0;
	//
	private string contextPath;
	private string path;
	private StreamReader reader;
	private StreamWriter writer;
	private Encoding encoder = Encoding.UTF8;
	private float recordFlagTime = 0;
	private Transform recordFlagPos;

	private float currentTime = 0;
	private float originX = 0, originY = 0, originZ = 0;
	private float totalTime = 0;

	private bool finishedReading;
	private bool isReplaying;
	private bool isRecording;
	private bool finishedRecording;

	public void Start()
	{
		contextPath = System.IO.Directory.GetCurrentDirectory();
		path = contextPath + "\\Assets\\UserFiles\\Positions";
		Debug.Log ("Test path : " + path);
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
//		if (!isReplaying) 
//		{
//			transform.position = new Vector3 (anim.Evaluate (Time.time), anim.Evaluate (Time.time), 0);
//		}

		if (Input.GetKeyDown (record)) 
		{
			Debug.Log ("Start Recording...");
			recordFlagTime = Time.time;
			recordFlagPos = transform;
			isRecording = true;
			try  
			{
				if(!File.Exists(path+"\\read.txt")){
					Debug.Log("Making new file " + path);
					File.Create(path);
				}
				if(!File.Exists(path+"\\write.txt")){
					Debug.Log("Making new file " + path);
					File.Create(path);
				}
				reader = new StreamReader(path + "\\read.txt");
				writer = new StreamWriter(path + "\\write.txt");
				setWritePos();
			}  
			catch (Exception ex)  
			{  
				Console.WriteLine("Open File Fails {0}", ex.ToString());  
			}  
		}

		if (Input.GetKeyDown (finishAndSave)) 
		{
			if(fs != null)
				fs.Close();  
			Debug.Log ("Recording Finished! ");
			isRecording = false;
		}

		if (Input.GetKeyDown (MakeCurve))
		{
			makeCurve ();
			Debug.Log ("finished making curve!");
		}

		if (Input.GetKeyDown (play)) 
		{
			Debug.Log ("Replay!");

			isReplaying = true;
			currentTime = Time.time;
		}


		if (isRecording) {
			Transform temp = transform;
			WritePos (Time.time-recordFlagTime, temp.position);
			recordFlagTime = Time.time;
			recordFlagPos = temp;
			if (Vector3.Distance (temp.position, recordFlagPos.position) > recordPrecision) {
				//
			}
		}
		
		if (isReplaying)
		{
			//ReadPos ();
			if (Time.time - currentTime < totalTime) {
				transform.position = new Vector3 (animX.Evaluate (Time.time - currentTime), 
					animY.Evaluate (Time.time - currentTime), animZ.Evaluate (Time.time - currentTime));
			}
		}

	}




	public void makeCurve()
	{
		FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
		StreamReader read = new StreamReader(fs, Encoding.Default);     
		string strReadline;
//		float frameTime = 0.017f;
		float sumTime = 0;

		Keyframe[] ksX = new Keyframe[10000];
		Keyframe[] ksY = new Keyframe[10000];
		Keyframe[] ksZ = new Keyframe[10000];
		int cnt = 0;
		float lastX = originX;
		float lastY = originY;
		float lastZ = originZ;

		while ((strReadline = read.ReadLine()) != null) // strReadline即为按照行读取的字符串
		{
			string[] rec = strReadline.Split (',');
			float deltaTime = float.Parse (rec [0]);
			float recX = float.Parse (rec [1]);
			float recY = float.Parse (rec [2]);
			float recZ = float.Parse (rec [3]);

			ksX [cnt] = new Keyframe (sumTime, recX);
			ksY [cnt] = new Keyframe (sumTime, recY);
			ksZ [cnt] = new Keyframe (sumTime, recZ);
			sumTime += deltaTime;
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

		fs.Close();
		read.Close();
	}

	public void WritePos(float deltaTime, Vector3 pos)
	{
		byte[] bytes = encoder.GetBytes("   &   "+ deltaTime.ToString() + "," + pos.x.ToString() + "," + pos.y.ToString() + "," + pos.z.ToString() + "\r\n");  
		try  
		{
			fs.Write(bytes, 0, bytes.Length);
		}  
		catch (Exception ex)  
		{  
			Console.WriteLine("Write Fails {0}", ex.ToString());  
		}  
	}

	public void ReadPos()
	{
		
		string strReadline = read.ReadLine ();

		if (strReadline == null) {
			read.Close ();
			finishedReading = true;
			return;
		}

		string[] rec = strReadline.Split(' ');
		float recX = float.Parse (rec [0]);
		float recY = float.Parse (rec [1]);
		float recZ = float.Parse (rec [2]);
		transform.position = new Vector3 (recX, recY, recZ);

	}

	private void setWritePos(){
		fs.Position = fs.Seek(line , SeekOrigin.Begin);
	}
}



