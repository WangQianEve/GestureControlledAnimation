using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;

public class CubeScript : MonoBehaviour {

	public AnimationCurve anim;
	public AnimationCurve animX, animY, animZ;

	public KeyCode record = KeyCode.R;
	public KeyCode finishAndSave = KeyCode.S;
	public KeyCode resetRecording = KeyCode.Space;
	public KeyCode play = KeyCode.P;
	public KeyCode MakeCurve = KeyCode.M;
	
	private string contextPath;
	private string path;
	private FileStream readfs;
	private StreamReader read;

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
		path = contextPath + "\\Assets\\UserFiles\\Positions\\test.txt";
		Debug.Log (path);
		originX = transform.position.x;
		originY = transform.position.y;
		originZ = transform.position.z;

		finishedReading = false;
		isReplaying = false;

		isRecording = false;
		finishedRecording = true;

		//makeCurve ();
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
			isRecording = true;
			finishedRecording = false;
		}

		if (Input.GetKeyDown (finishAndSave)) 
		{
			Debug.Log ("Recording Finished! ");
			isRecording = false;
			finishedRecording = true;
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
			//finishedReading = false;
			currentTime = Time.time;
		}


		if (isRecording) {
			WritePos (transform.position);
		}
		
		//if (!finishedReading && isReplaying) 
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

		float frameTime = 0.017f;
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
			if (cnt % 3 == 0) 	//增大抽样间隔
			{
				string[] rec = strReadline.Split (' ');
				float recX = float.Parse (rec [0]);
				float recY = float.Parse (rec [1]);
				float recZ = float.Parse (rec [2]);

				ksX [cnt] = new Keyframe (sumTime, recX);
				ksY [cnt] = new Keyframe (sumTime, recY);
				ksZ [cnt] = new Keyframe (sumTime, recZ);

			}
			sumTime += frameTime;
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

	public void WritePos(Vector3 pos)
	{
		FileStream fs = null;
		Encoding encoder = Encoding.UTF8;  //将待写的入数据从字符串转换为字节数组  
		byte[] bytes = encoder.GetBytes(pos.x.ToString() + " " + pos.y.ToString() + " " + pos.z.ToString() + " \r\n");  
		try  
		{
			if(!File.Exists(path)){
				Debug.Log("Making new file " + path);
				File.Create(path);
			}
			fs = File.OpenWrite(path);
			fs.Position = fs.Length;  //设定书写的開始位置为文件的末尾
			fs.Write(bytes, 0, bytes.Length);  //将待写入内容追加到文件末尾
		}  
		catch (Exception ex)  
		{  
			Console.WriteLine("Open File Fails {0}", ex.ToString());  
		}  
		finally  
		{  
			if(fs != null)
			fs.Close();  
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
}



