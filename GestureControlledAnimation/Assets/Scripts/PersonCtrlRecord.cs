using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.IO;
using System.Text;
using System;
using UnityEngine.UI;

public class PersonCtrlRecord : MonoBehaviour
{
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
    public KeyCode sliding = KeyCode.T;
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
    private bool isSliding;
    private bool finishedRecording;

    private float[] frameTime = new float[10000];

    public Slider sliderBar;
    float slide_time;
    float time_offset = 0;

    float begin_time = 0;
    float end_time = 0;

    int begin_frame = 0;
    int end_frame = 0;

    MeshRenderer meshRenderer;

    void initSliderBar()
    {
        GameObject g = GameObject.FindWithTag("SliderBar");
        Debug.Log(g);
        sliderBar = g.GetComponent<Slider>();
        Debug.Log(sliderBar.name);
        //sliderBar.value = 0.5f;
    }

    public void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
        cubeCtrl = GetComponent(typeof(CubeControl)) as CubeControl;
        path = System.IO.Directory.GetCurrentDirectory();
        path = path + "\\Assets\\UserFiles\\Positions\\";
        readFileName = path + name + "_r.txt";
        writeFileName = path + name + "_w.txt";
        readFileBackupName = path + name + "_b.txt";
        Debug.Log(name + " Test path : " + readFileName);
        originX = transform.position.x;
        originY = transform.position.y;
        originZ = transform.position.z;

        finishedReading = false;
        isReplaying = false;
        isRecording = false;
        finishedRecording = true;

        initSliderBar();
    }

    public void Update()
    {

        if (Input.GetKeyDown(record))
        {
            recordSelected = cubeCtrl.selected;
            if (!recordSelected)
            {
                isRecording = false;
                isReplaying = true;
                //meshRenderer.enabled = false;
                currentTime = Time.time;
            }
            else {
                try
                {
                    if (!File.Exists(readFileName))
                    {
                        Debug.Log("Making new file " + readFileName);
                        File.Create(readFileName);
                    }
                    if (!File.Exists(writeFileName))
                    {
                        Debug.Log("Making new file " + writeFileName);
                        File.Create(writeFileName);
                    }
                    Debug.Log("Start Recording...1");
                    readfs = new FileStream(readFileName, FileMode.Open, FileAccess.Read);
                    if (readfs == null) Debug.Log("readfs null");
                    writefs = new FileStream(writeFileName, FileMode.Open, FileAccess.Write);
                    if (writefs == null) Debug.Log("writefs null");
                    reader = new StreamReader(readfs, Encoding.Default);
                    if (reader == null) Debug.Log("reader null");
                    writer = new StreamWriter(writefs, Encoding.Default);
                    if (writer == null) Debug.Log("writer null");
                    preRead();
                    Debug.Log("Start Recording...");
                    recordFlagTime = Time.time;//?
                    recordFlagPos = transform.position;
                    isRecording = true;
                    isReplaying = false;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Open File Fails {0}", ex.ToString());
                }
            }
        }

        if (Input.GetKeyDown(finishRecording))
        {
            if (recordSelected)
            {
                Debug.Log(name + " Recording Finished! ");
                postRead();
            }
            isRecording = false;
            isReplaying = false;
        }

        if (Input.GetKeyDown(saveRecording) && Input.GetKey(KeyCode.LeftShift))
        {
            if (recordSelected)
            {
                Debug.Log(name + " Saving Files ");
                reader.Close();
                writer.Close();
                readfs.Close();
                writefs.Close();
                File.Replace(writeFileName, readFileName, readFileBackupName);
                Debug.Log("Saving finished!");
            }
        }

        if (Input.GetKeyDown(MakeCurve))
        {
            if (cubeCtrl.selected || recordSelected)
            {
                makeCurve();
                Debug.Log(name + " finished making curve");
            }
        }

        if (Input.GetKeyDown(sliding))
        {
            isSliding = !isSliding;
            Debug.Log(isSliding ? "Sliding!" : "no sliding");
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
            if(!isSliding)
            {
                if (Time.time - currentTime < totalTime)
                {
                    transform.position = new Vector3(animX.Evaluate(Time.time - currentTime),
                    animY.Evaluate(Time.time - currentTime), animZ.Evaluate(Time.time - currentTime));
                    slide_time = Time.time - currentTime;
                    sliderBar.value = slide_time;
                    getTimeAndFrame(Time.time - currentTime);
                }
                else {
                    meshRenderer.enabled = true;
                    isReplaying = false;
                }
            }
            else
            {
                slide_time = sliderBar.value;
                getTimeAndFrame(slide_time);
                transform.position = new Vector3(animX.Evaluate(slide_time),
                animY.Evaluate(slide_time), animZ.Evaluate(slide_time));
            }
            
            
        }

    }

    int getFrameIndex(float time)
    {
        int cnt = 0;
        while (frameTime[cnt] < time)
            cnt++;
        return cnt-1;
    }

    void getTimeAndFrame(float value)
    {
        if (Input.GetKeyDown(KeyCode.Alpha1))
        {
            begin_time = value;
            begin_frame = getFrameIndex(value);
            Debug.Log("begin: " + "time: " + begin_time.ToString() + " frame: " + begin_frame.ToString());
        }
        else if (Input.GetKeyDown(KeyCode.Alpha2))
        {
            end_time = value;

            if (getFrameIndex(value) > begin_frame)
                end_frame = getFrameIndex(value);
            else
                Debug.Log("make sure end_frame > begin_frame");
            Debug.Log("end: " + "time: " + end_time.ToString() + " frame: " + end_frame.ToString());
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
            string[] rec = strReadline.Split(',');
            float deltaTime = float.Parse(rec[0]);
            float recX = float.Parse(rec[1]);
            float recY = float.Parse(rec[2]);
            float recZ = float.Parse(rec[3]);
            frameTime[cnt] = sumTime;
            sumTime += deltaTime;
      
            ksX[cnt] = new Keyframe(sumTime, recX);
            ksY[cnt] = new Keyframe(sumTime, recY);
            ksZ[cnt] = new Keyframe(sumTime, recZ);
            cnt++;
        }

        totalTime = sumTime;

        sliderBar.maxValue = totalTime;

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

