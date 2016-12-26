using System.Collections.Generic;
using UnityEngine;
using System.Collections;
using System.IO;
using System.Text;
using System;

public class CubeScript : MonoBehaviour
{
    //modes
    public bool isSelected = true;
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
    public int endLine = 0;
    //
    private string contextPath;
    private string path;
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

    public void Start()
    {
        contextPath = System.IO.Directory.GetCurrentDirectory();
        path = contextPath + "\\Assets\\UserFiles\\Positions";
        Debug.Log("Test path : " + path);
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

        if (Input.GetKeyDown(record))
        {
            try
            {
                if (!File.Exists(path + "\\read.txt"))
                {
                    Debug.Log("Making new file " + path + "\\read.txt");
                    File.Create(path + "\\read.txt");
                }
                if (!File.Exists(path + "\\write.txt"))
                {
                    Debug.Log("Making new file " + path + "\\write.txt");
                    File.Create(path + "\\write.txt");
                }
                readfs = new FileStream(path + "\\read.txt", FileMode.Open, FileAccess.Read);
                writefs = new FileStream(path + "\\write.txt", FileMode.Open, FileAccess.Write);
                reader = new StreamReader(readfs, Encoding.Default);
                writer = new StreamWriter(writefs, Encoding.Default);
                preRead();
                Debug.Log("Start Recording...");
                recordFlagTime = Time.time;
                recordFlagPos = transform.position;
                isRecording = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine("Open File Fails {0}", ex.ToString());
            }
        }

        if (Input.GetKeyDown(finishRecording))
        {
            Debug.Log("Recording Finished! ");
            isRecording = false;
            postRead();
        }

        if (Input.GetKeyDown(saveRecording) && Input.GetKeyDown(KeyCode.LeftShift))
        {
            Debug.Log("Saving Files... ");
            reader.Close();
            writer.Close();
            readfs.Close();
            writefs.Close();
            File.Replace(path + "\\write.txt", path + "\\read.txt", path + "\\readBackup.txt");
            Debug.Log("Saving finished!");
        }

        if (Input.GetKeyDown(MakeCurve))
        {
            makeCurve();
            Debug.Log("finished making curve!");
        }

        if (Input.GetKeyDown(play))
        {
            Debug.Log("Replay!");
            isReplaying = true;
            currentTime = Time.time;
        }


        if (isRecording)
        {
            Vector3 temp = transform.position;
            if (Vector3.Distance(temp, recordFlagPos) > recordPrecision)
            {
                writer.WriteLine((Time.time - recordFlagTime).ToString() + "," + temp.x.ToString() + "," + temp.y.ToString() + "," + temp.z.ToString());
                recordFlagTime = Time.time;
                recordFlagPos = temp;
            }
        }

        if (isReplaying)
        {
            //ReadPos ();
            if (Time.time - currentTime < totalTime)
            {
                transform.position = new Vector3(animX.Evaluate(Time.time - currentTime),
                    animY.Evaluate(Time.time - currentTime), animZ.Evaluate(Time.time - currentTime));
            }
        }

    }

    public void makeCurve()
    {
        reader = new StreamReader(path + "\\read.txt");
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
            string[] rec = strReadline.Split(',');
            float deltaTime = float.Parse(rec[0]);
            float recX = float.Parse(rec[1]);
            float recY = float.Parse(rec[2]);
            float recZ = float.Parse(rec[3]);

            ksX[cnt] = new Keyframe(sumTime, recX);
            ksY[cnt] = new Keyframe(sumTime, recY);
            ksZ[cnt] = new Keyframe(sumTime, recZ);
            sumTime += deltaTime;
            cnt++;
        }

        totalTime = sumTime;

        Debug.Log("cnt = " + cnt);

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

    private void preRead()
    {
        int i = startLine;
        string temp = "";
        while (i-- > 0)
        {
            temp = reader.ReadLine();
            writer.WriteLine(temp);
        }
    }

    private void postRead()
    {
        if (endLine == -1)
            return;
        int i = endLine > startLine ? endLine - startLine : startLine;
        string temp = "";
        while (i-- > 0)
        {
            reader.ReadLine();
        }
        while ((temp = reader.ReadLine()) != null)
        {
            writer.WriteLine(temp);
        }
    }
}