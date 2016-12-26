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
	public KeyCode endRecord = KeyCode.E;
	public KeyCode resetRecording = KeyCode.Space;
	public KeyCode play = KeyCode.P;
	public KeyCode MakeCurve = KeyCode.M;
	public KeyCode changeSpeed = KeyCode.Q;
	public KeyCode saveEdit = KeyCode.S;
	public KeyCode shift = KeyCode.LeftShift;
	public KeyCode optimize = KeyCode.O;


	private const float recordPrecision = 0.02f;
	private const float pi = 3.1416f;

	private Keyframe[] ksX = new Keyframe[10000];
	private Keyframe[] ksY = new Keyframe[10000];
	private Keyframe[] ksZ = new Keyframe[10000];

	private string contextPath;
	private string path;
	private FileStream readfs;
	private StreamReader read;

	private float frameTime = 0.017f;
	private float currentTime = 0;
	private float originX = 0, originY = 0, originZ = 0;
	private float totalTime = 0;

	private Vector3 initPos = new Vector3(0, 0, 0);
	public float initPhi;

	private bool finishedReading;
	private bool isReplaying;

	private bool isRecording;
	private bool finishedRecording;

	public void Start()
	{
		contextPath = System.IO.Directory.GetCurrentDirectory();
		path = contextPath + "\\Assets\\UserFiles\\Positions\\test.txt";

		originX = transform.position.x;
		originY = transform.position.y;
		originZ = transform.position.z;

		initPos = new Vector3 (originX, originY, originZ);

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
//			//transform.position = new Vector3 (anim.Evaluate (Time.time), anim.Evaluate (Time.time), 0);
//			transform.position = new Vector3(transform.position.x + 0.015f, 0, 0);
//		}

		if (Input.GetKeyDown (record)) 
		{
			Debug.Log ("Start Recording...");
			isRecording = true;
			finishedRecording = false;
		}

		if (Input.GetKeyDown (endRecord)) 
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

		if (Input.GetKeyDown (changeSpeed)) //for "change" testing
		{
//			Debug.Log ("Change Speed!");
//			changeCurveSpeed (2, 4, 2);		//这里的三个参数将来应来自用户的“输入”
//			changeCurveSpeed (7, 9, 0.5f);

//			Debug.Log("translate Curve!");
//			translateCurve(2, 5, new Vector3(0, 1, 0), 1);

//			Debug.Log("extend Curve!");
//			extendCurve (2, 5, new Vector3 (2, 2, 2));

			Debug.Log ("rotate Curve!");
			rotateInXOZ (0.1f, 9.97f, pi / 4);
		}

		if (Input.GetKeyDown (saveEdit)) 
		{
			Debug.Log ("Save New Curve!");
			saveNewCurveToFile ();
		}

		if (Input.GetKeyDown (optimize)) 
		{
			Debug.Log ("Optimize curve!");
			OptimizeCurve (118, 352);
			OptimizeCurve (165, 165);
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






	public void OptimizeCurve(int startNum, int endNum)
	{

		if (startNum > endNum) {
			Debug.Log ("error Input in OptimizeCurve()!");
			return;
		}

		int totalFrameNum = 0;

		for (int i = endNum + 1; i <= ksX.Length; i++) {
			if (ksX [i].value == 0 && ksY [i].value == 0 && ksZ [i].value == 0 && ksX [i].time == 0) {
				totalTime = ksX [i - 1].time;
				totalFrameNum = i - 1;
				break;
			}
		}

		Vector3 startPos = new Vector3 (ksX [startNum].value, ksY [startNum].value, ksZ [startNum].value);
		Vector3 preStartPos = new Vector3 (ksX [startNum - 1].value, ksY [startNum - 1].value, ksZ [startNum - 1].value);
		Vector3 endPos = new Vector3 (ksX [endNum].value, ksY [endNum].value, ksZ [endNum].value);
		Vector3 postEndPos = new Vector3 (ksX [endNum + 1].value, ksY [endNum + 1].value, ksZ [endNum + 1].value);

		int segmentOfdT = 20;	//陡变处的延时倍数
		float dT = ksX [startNum].time - ksX[startNum - 1].time;
		float incT = (float)(segmentOfdT) * dT;
		if (Vector3.Distance (startPos, preStartPos) < recordPrecision)
			incT = dT;
		float dT2 = ksX [endNum + 1].time - ksX [endNum].time;
		float incT2 = (float)(segmentOfdT) * dT2;
		if (Vector3.Distance (endPos, postEndPos) < recordPrecision)
			incT2 = dT2;
		float shrinkTimeRatio = (totalTime - incT - incT2) / totalTime;	//缩时比例

		float[] oldDT = new float[10000];
		for (int i = 0; i <= ksX.Length; i++) {
			if (ksX [i+1].value == 0 && ksY [i+1].value == 0 && ksZ [i+1].value == 0 && ksX [i+1].time == 0) {
				break;
			}
			oldDT [i] = ksX [i + 1].time - ksX [i].time;
		}

		for (int i = 0; i <= startNum - 2; i++) {
			float newTime = ksX [i].time + oldDT[i] * shrinkTimeRatio;
			ksX [i + 1].time = newTime;
			ksY [i + 1].time = newTime;
			ksZ [i + 1].time = newTime;
		}

		float newStartTime = ksX[startNum].time + (float)(segmentOfdT) * oldDT[startNum - 1];
		ksX [startNum].time = newStartTime;
		ksY [startNum].time = newStartTime;
		ksZ [startNum].time = newStartTime;

		for (int i = startNum; i <= endNum - 1; i++) {
			float newTime = ksX [i].time + oldDT[i] * shrinkTimeRatio;
			ksX [i + 1].time = newTime;
			ksY [i + 1].time = newTime;
			ksZ [i + 1].time = newTime;
		}

		float newEndTime = ksX [endNum].time + (float)(segmentOfdT) * oldDT [endNum];
		ksX [endNum + 1].time = newEndTime;
		ksY [endNum + 1].time = newEndTime;
		ksZ [endNum + 1].time = newEndTime;

		for (int i = endNum + 1; i <= ksX.Length; i++) {
			if (ksX [i+1].value == 0 && ksY [i+1].value == 0 && ksZ [i+1].value == 0 && ksX [i+1].time == 0) {
				totalTime = ksX [i].time;
				break;
			}
			float newTime = ksX [i].time + oldDT[i] * shrinkTimeRatio;
			ksX [i + 1].time = newTime;
			ksY [i + 1].time = newTime;
			ksZ [i + 1].time = newTime;
		}

		animX = new AnimationCurve(ksX);
		animY = new AnimationCurve(ksY);
		animZ = new AnimationCurve(ksZ);

		Debug.Log ("After Optimize, total time = " + totalTime);
		
	}

	public void changeCurveSpeed(float startTime, float endTime, float speedRatio)	//效果：speedRatio倍速播放（一段时间区间内）
	{
		float sumTime = 0;
		float reducedTime = (endTime - startTime) - (endTime - startTime) / speedRatio;
		for (int i = 0; i < ksX.Length; i++) 
		{
			if (ksX [i].value == 0 && ksY [i].value == 0 && ksZ [i].value == 0)
				break;
			sumTime = ksX [i].time;
			if (sumTime > startTime && sumTime < endTime) {	//需要改速部分
				ksX [i].time = startTime + (ksX [i].time - startTime) / speedRatio;
				ksY [i].time = startTime + (ksY [i].time - startTime) / speedRatio;
				ksZ [i].time = startTime + (ksZ [i].time - startTime) / speedRatio;
			} 
			else if (sumTime > endTime) {
				ksX [i].time = ksX[i].time - reducedTime;
				ksY [i].time = ksY[i].time - reducedTime;
				ksZ [i].time = ksZ[i].time - reducedTime;
			}
			totalTime = ksX [i].time;
		}
		animX = new AnimationCurve(ksX);
		animY = new AnimationCurve(ksY);
		animZ = new AnimationCurve(ksZ);

		Debug.Log ("After speed change, new total time = " + totalTime + "s");
	}

	public void translateCurve(float startTime, float endTime, Vector3 dir, float dist)	//效果：轨迹整体往dir方向平移dist单位（一段时间区间内）
	{
		float sumTime = 0;
		float deltaX = dir.x * dist;
		float deltaY = dir.y * dist;
		float deltaZ = dir.z * dist;
		for (int i = 0; i < ksX.Length; i++) 
		{
			if (ksX [i].value == 0 && ksY [i].value == 0 && ksZ [i].value == 0)
				break;
			sumTime = ksX [i].time;
			if (sumTime > startTime && sumTime < endTime) {	//需要平移轨迹部分
				ksX [i].value += deltaX;
				ksY [i].value += deltaY;
				ksZ [i].value += deltaZ;
			} 
			totalTime = ksX [i].time;
		}
		animX = new AnimationCurve(ksX);
		animY = new AnimationCurve(ksY);
		animZ = new AnimationCurve(ksZ);

		Debug.Log ("After translation, new total time = " + totalTime + "s");
	}

	public void extendCurve(float startTime, float endTime, Vector3 extendRatio)	//效果：轨迹在三个维度伸缩比例为extendRatio（一段时间区间内）
	{
		float sumTime = 0;
		Vector3 startPos = new Vector3(0, 0, 0);
		for (int i = 0; i < ksX.Length; i++) 	//find startPos
		{
			if (ksX [i].value == 0 && ksY [i].value == 0 && ksZ [i].value == 0)
				break;
			if (ksX [i].time > startTime) {
				if (i > 0)
					startPos = new Vector3 (ksX [i-1].value, ksY [i-1].value, ksZ [i-1].value);
				else 
					startPos = new Vector3 (ksX [i].value, ksY [i].value, ksZ [i].value);
				break;
			} 
		}
		for (int i = 0; i < ksX.Length; i++) 
		{
			if (ksX [i].value == 0 && ksY [i].value == 0 && ksZ [i].value == 0)
				break;
			sumTime = ksX [i].time;
			if (sumTime > startTime && sumTime < endTime) {	//需要轨迹伸缩部分
				ksX[i].value = startPos.x + (ksX[i].value - startPos.x) * extendRatio.x;
				ksY[i].value = startPos.y + (ksY[i].value - startPos.y) * extendRatio.y;
				ksZ[i].value = startPos.z + (ksZ[i].value - startPos.z) * extendRatio.z;
			} 
			totalTime = ksX [i].time;
		}
		animX = new AnimationCurve(ksX);
		animY = new AnimationCurve(ksY);
		animZ = new AnimationCurve(ksZ);

		Debug.Log ("After extension, new total time = " + totalTime + "s");
	}

	public void rotateInXOY(float startTime, float endTime, float phi)
	{
		float sumTime = 0;
		float midTime = (startTime + endTime) / 2.0f;
		Vector3 midPos = new Vector3(0, 0, 0);

		for (int i = 0; i < ksX.Length; i++) 	//find midPos
		{
			if (ksX [i].value == 0 && ksY [i].value == 0 && ksZ [i].value == 0)
				break;
			if (ksX [i].time > midTime) {
				midPos = new Vector3 (ksX [i-1].value, ksY [i-1].value, ksZ [i-1].value);
				break;
			} 
		}

		float x0 = midPos.x;
		float y0 = midPos.y;

		for (int i = 0; i < ksX.Length; i++) 
		{
			if (ksX [i].value == 0 && ksY [i].value == 0 && ksZ [i].value == 0)
				break;
			sumTime = ksX [i].time;
			if (sumTime > startTime && sumTime < endTime) {	//需要旋转部分
				float x_ = ksX [i].value - x0;	//x' = x - x0
				float y_ = ksY [i].value - y0;	//y' = y - y0
				float rho = Mathf.Sqrt (x_*x_ + y_*y_);	//旋转半径
				float theta = Mathf.Atan(y_ / x_);	//原始角度
				if (x_ < 0) theta += pi;	//由arctan的特性，需对theta处理
				float xd_ = rho * Mathf.Cos (theta + phi);
				float yd_ = rho * Mathf.Sin (theta + phi);

				ksX [i].value = xd_ + x0;
				ksY [i].value = yd_ + y0;
			} 
			totalTime = ksX [i].time;
		}
		animX = new AnimationCurve(ksX);
		animY = new AnimationCurve(ksY);
		animZ = new AnimationCurve(ksZ);

		Debug.Log ("After rotation, new total time = " + totalTime + "s");
	}

//	public void rotateInYOZ(float startTime, float endTime, float phi)
//	{
//		float sumTime = 0;
//		Vector3 startPos = new Vector3(0, 0, 0);
//
//		for (int i = 0; i < ksX.Length; i++) 	//find midPos
//		{
//			if (ksX [i].value == 0 && ksY [i].value == 0 && ksZ [i].value == 0)
//				break;
//			if (ksX [i].time > startTime) {
//				startPos = new Vector3 (ksX [i-1].value, ksY [i-1].value, ksZ [i-1].value);
//				break;
//			} 
//		}
//
//		float y0 = startPos.y;
//		float z0 = startPos.z;
//		for (int i = 0; i < ksX.Length; i++) 
//		{
//			if (ksX [i].value == 0 && ksY [i].value == 0 && ksZ [i].value == 0)
//				break;
//			sumTime = ksX [i].time;
//			if (sumTime > startTime && sumTime < endTime) {	//需要旋转部分
//				float y_ = ksY [i].value - y0;	
//				float z_ = ksZ [i].value - z0;	
//				float rho = Mathf.Sqrt (y_*y_ + z_*z_);	//旋转半径
//				float theta = Mathf.Atan(z_ / y_);	//原始角度
//				if (y_ < 0) theta += pi;	//由arctan的特性，需对theta处理
//				float yd_ = rho * Mathf.Cos (theta + phi);
//				float zd_ = rho * Mathf.Sin (theta + phi);
//
//				ksY [i].value = yd_ + y0;
//				ksZ [i].value = zd_ + z0;
//			} 
//			totalTime = ksX [i].time;
//		}
//		animX = new AnimationCurve(ksX);
//		animY = new AnimationCurve(ksY);
//		animZ = new AnimationCurve(ksZ);
//		Debug.Log ("After rotation, new total time = " + totalTime + "s");
//	}

	public void rotateInXOZ(float startTime, float endTime, float phi)
	{
		float sumTime = 0;
		//float midTime = (startTime + endTime) / 2.0f;
		Vector3 midPos = new Vector3(0, 0, 0);

		for (int i = 0; i < ksX.Length; i++) 	//find midPos
		{
			if (ksX [i].value == 0 && ksY [i].value == 0 && ksZ [i].value == 0)
				break;
			if (ksX [i].time > startTime) {
				midPos = new Vector3 (ksX [i-1].value, ksY [i-1].value, ksZ [i-1].value);
				break;
			} 
		}

		float x0 = midPos.x;
		float z0 = midPos.z;
		for (int i = 0; i < ksX.Length; i++) 
		{
			if (ksX [i].value == 0 && ksY [i].value == 0 && ksZ [i].value == 0)
				break;
			sumTime = ksX [i].time;
			if (sumTime > startTime && sumTime < endTime) {	//需要旋转部分
				float x_ = ksX [i].value - x0;	
				float z_ = ksZ [i].value - z0;	
				float rho = Mathf.Sqrt (x_*x_ + z_*z_);	//旋转半径
				float theta = Mathf.Atan(z_ / x_);	//原始角度
				if (x_ < 0) theta += pi;	//由arctan的特性，需对theta处理
				float xd_ = rho * Mathf.Cos (theta + phi);
				float zd_ = rho * Mathf.Sin (theta + phi);

				ksX [i].value = xd_ + x0;
				ksZ [i].value = zd_ + z0;
			} 
			totalTime = ksX [i].time;
		}
		animX = new AnimationCurve(ksX);
		animY = new AnimationCurve(ksY);
		animZ = new AnimationCurve(ksZ);
		Debug.Log ("After rotation, new total time = " + totalTime + "s");
	}

	private float getStartTime()
	{
		float startTime = 0;
		for (int i = 0; i < ksX.Length; i++) {
			if (ksX [i].value == 0 && ksY [i].value == 0 && ksZ [i].value == 0) {
				break;
			}
			startTime = ksX [i].time;
			break;
		}
		return startTime;
	}

	private float getEndTime()
	{
		float endTime = 0;
		for (int i = 0; i < ksX.Length; i++) {
			if (ksX [i].value == 0 && ksY [i].value == 0 && ksZ [i].value == 0) {
				endTime = ksX [i - 1].time;
				break;
			}
		}
		return endTime;
	}

	public void makeCurve()
	{
		FileStream fs = new FileStream(path, FileMode.Open, FileAccess.Read);
		StreamReader read = new StreamReader(fs, Encoding.Default);     
		string strReadline;

		float sumTime = 0;

		int cnt = 0;
		Vector3 offset = new Vector3 (0, 0, 0);

		while ((strReadline = read.ReadLine()) != null) // strReadline即为按照行读取的字符串
		{
			//if (cnt % 3 == 2) 	//增大抽样间隔
			//{
			string[] rec = strReadline.Split (' ');
			float recX = float.Parse (rec [0]);
			float recY = float.Parse (rec [1]);
			float recZ = float.Parse (rec [2]);
			float recTime = float.Parse (rec [3]);

			if (cnt == 0) {
				offset.x = initPos.x - recX;
				offset.y = initPos.y - recY;
				offset.z = initPos.z - recZ;
			}

		//	ksX [cnt / 3] = new Keyframe (sumTime, recX);
		//	ksY [cnt / 3] = new Keyframe (sumTime, recY);
		//	ksZ [cnt / 3] = new Keyframe (sumTime, recZ);
//			ksX [cnt] = new Keyframe (sumTime, recX);
//			ksY [cnt] = new Keyframe (sumTime, recY);
//			ksZ [cnt] = new Keyframe (sumTime, recZ);
			ksX [cnt] = new Keyframe (recTime, recX + offset.x);
			ksY [cnt] = new Keyframe (recTime, recY + offset.y);
			ksZ [cnt] = new Keyframe (recTime, recZ + offset.z);

			//}
			//sumTime += frameTime;
			totalTime = recTime;

			cnt++;
		}

		Debug.Log ("total time = " + totalTime + "s");

		rotateInXOZ (getStartTime (), getEndTime (), initPhi);

		animX = new AnimationCurve(ksX);
		animY = new AnimationCurve(ksY);
		animZ = new AnimationCurve(ksZ);

		fs.Close();
		read.Close();
	}

	public void saveNewCurveToFile () 
	{
		FileStream fs = new FileStream (path, FileMode.Create, FileAccess.Write);
		StreamWriter sw = new StreamWriter (fs, Encoding.Default);
		string writeString = "";
		try
		{
			for(int i = 0; i < ksX.Length; i++)
			{
				sw.Flush();
				if (ksX [i].value == 0 && ksY [i].value == 0 && ksZ [i].value == 0 && ksX[i].time == 0) {
					Debug.Log ("save frames num = " + i);
					break;
				}
				writeString = ksX[i].value.ToString() + " " + ksY[i].value.ToString() + " " + ksZ[i].value.ToString() + " " + ksX[i].time.ToString();
				sw.WriteLine(writeString);
				//Debug.Log("[line" + i + "]" + writeString);
			}
		}
		catch (Exception ex)  
		{  
			Console.WriteLine("文件打开失败{0}", ex.ToString());  
		}
		finally  
		{  
			if(fs != null)
				fs.Close();  
		} 
	}

	public void WritePos(Vector3 pos)
	{
		FileStream fs = null;  
		Encoding encoder = Encoding.UTF8;  //将待写的入数据从字符串转换为字节数组  
		byte[] bytes = encoder.GetBytes(pos.x.ToString() + " " + pos.y.ToString() + " " + pos.z.ToString() + " \r\n");  
		try  
		{  
			fs = File.OpenWrite(path);  
			fs.Position = fs.Length;  //设定书写的開始位置为文件的末尾
			fs.Write(bytes, 0, bytes.Length);  //将待写入内容追加到文件末尾
		}  
		catch (Exception ex)  
		{  
			Console.WriteLine("文件打开失败{0}", ex.ToString());  
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



