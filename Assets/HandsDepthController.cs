using Oculus.Haptics;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class HandsDepthController : MonoBehaviour
{
    public HapticSource hapticSource;
    public AudioClip greatClip;
    public enum MovementDirection
    {
        None,
        Rising,
        Falling
    }

    Vector3 StartPos;
    private float previousRawDepth;
    private MovementDirection currentDirection = MovementDirection.None;
    private bool isFirstUpdate = true;
    private const float TRIGGER_THRESHOLD = 0.002f; // 触发阈值，避免微小波动误触发

    // 新增按压时间记录相关变量
    private List<float> compressionTimes = new List<float>();
    private const int MAX_RECORDS = 7; // 用于频率计算的最大记录数

    // 新增存储数据的列表
    private List<float> recordedFrequencies = new List<float>();
    private List<float> recordedDepths = new List<float>();

    // Start is called before the first frame update
    void Start()
    {
        StartPos = transform.localPosition;
    }
    public void ResetAll()
    {
        recordedFrequencies.Clear();
        recordedDepths.Clear();
        compressionTimes.Clear();
        SystemManager.instance.CPRPanel.transform.GetChild(0).GetChild(3).GetComponent<Text>().text = "Depth: " + 0 + " cm";
        SystemManager.instance.CPRPanel.transform.GetChild(0).GetChild(2).GetComponent<Text>().text = "Frequency: " + 0 + " PM";
        SystemManager.instance.CPRPanel.transform.GetChild(0).GetChild(4).GetComponent<Text>().text = " ";
    }
    // Update is called once per frame
    void Update()
    {
        Vector3 StartCenterPos = HandsPositionTrigger.instance.StartCenterPos;
        Vector3 CenterPos = HandsPositionTrigger.instance.CenterPos;

        float rawDepth = (CenterPos - StartCenterPos).y;

        if (isFirstUpdate)
        {
            previousRawDepth = rawDepth;
            isFirstUpdate = false;
            return;
        }

        MovementDirection newDirection = GetMovementDirection(rawDepth);

        if (newDirection != currentDirection)
        {
            HandleDirectionChange(currentDirection, newDirection, rawDepth);
            currentDirection = newDirection;
        }

        previousRawDepth = rawDepth;

        float clampedDepth = Mathf.Clamp(rawDepth, -0.1f, 0.1f);
        transform.localPosition = StartPos - Vector3.right * clampedDepth;
    }

    private MovementDirection GetMovementDirection(float currentDepth)
    {
        if (currentDepth - previousRawDepth > TRIGGER_THRESHOLD)
        {
            return MovementDirection.Rising;
        }
        else if (previousRawDepth - currentDepth > TRIGGER_THRESHOLD)
        {
            return MovementDirection.Falling;
        }
        return currentDirection;
    }

    private void HandleDirectionChange(MovementDirection oldDirection, MovementDirection newDirection, float depth)
    {
        float frequency = CalculateFrequency();

        if (newDirection == MovementDirection.Falling)
        {
            // 记录新的按压时间
            RecordCompressionTime();
        }

        if (oldDirection == MovementDirection.Rising && newDirection == MovementDirection.Falling)
        {
            OnStartFalling(depth, frequency);
        }
        else if (oldDirection == MovementDirection.Falling && newDirection == MovementDirection.Rising)
        {
            OnStartRising(depth, frequency);
        }
    }

    // 新增方法：记录按压时间
    private void RecordCompressionTime()
    {
        compressionTimes.Add(Time.time);

        // 保持记录数量不超过最大值
        while (compressionTimes.Count > MAX_RECORDS)
        {
            compressionTimes.RemoveAt(0);
        }
    }

    // 新增方法：计算按压频率
    private float CalculateFrequency()
    {
        if (compressionTimes.Count < 2)
        {
            return 0f; // 不足两次记录无法计算频率
        }

        // 计算总时间间隔
        float totalDuration = compressionTimes[compressionTimes.Count - 1] - compressionTimes[0];
        // 计算平均间隔时间
        float averageInterval = totalDuration / (compressionTimes.Count - 1);
        // 转换为每分钟次数
        return 60f / averageInterval;
    }

    // 修改方法签名添加频率参数
    private void OnStartRising(float depth, float frequency)
    {
        if (SystemManager.instance.IsStartTest)
        {
            float depthP = depth * -100;
            SystemManager.instance.CPRPanel.transform.GetChild(0).GetChild(3).GetComponent<Text>().text = "Depth: " + Mathf.Clamp(depthP,0,100).ToString("F1") + " cm";
            SystemManager.instance.CPRPanel.transform.GetChild(0).GetChild(2).GetComponent<Text>().text = "Frequency: " + frequency.ToString("F0") + " PM";
            if(Mathf.Abs(depthP)>=4&&depthP<=6)
            {
                SystemManager.instance.CPRPanel.transform.GetChild(0).GetChild(4).GetComponent<Text>().text = "Great!";
                hapticSource.Play();
                AudioManager.instance.PlayOneShot(greatClip,1);
            }
            else
            {
                SystemManager.instance.CPRPanel.transform.GetChild(0).GetChild(4).GetComponent<Text>().text = "Bad!";
            }

            // 存储数据
            recordedFrequencies.Add(frequency);
            recordedDepths.Add(depth * -100);
        }
    }

    // 修改方法签名添加频率参数
    private void OnStartFalling(float depth, float frequency)
    {
        if (SystemManager.instance.IsStartTest)
        {

        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "FirstTrigger")
        {
            other.gameObject.SetActive(false);
            SystemManager.instance.ShowInfoSimulationPanel("Nice job, then repeat the same process as before, trying to keep the frequency and depth of compressions the same as in the tutorial", delegate {
                StartCoroutine(SystemManager.instance.StartTest());
            });
        }
    }

    // 新增方法：计算 frequency 的平均值
    public float CalculateAverageFrequency()
    {
        if (recordedFrequencies.Count == 0)
        {
            return 0f;
        }

        float sum = 0f;
        foreach (float frequency in recordedFrequencies)
        {
            sum += frequency;
        }

        return sum / recordedFrequencies.Count;
    }

    // 新增方法：计算 depth 的正确率
    public float CalculateDepthAccuracy()
    {
        if (recordedDepths.Count == 0)
        {
            return 0f;
        }

        int correctCount = 0;
        foreach (float depth in recordedDepths)
        {
            if (depth >= 4 && depth <= 6)
            {
                correctCount++;
            }
        }

        return (float)correctCount / (float)recordedDepths.Count;
    }

    // 新增方法：计算得分
    public float CalculateScore()
    {
        float averageFrequency = CalculateAverageFrequency();
        float depthAccuracy = CalculateDepthAccuracy();

        float frequencyScore = 0f;
        if (averageFrequency >= 100 && averageFrequency <= 120)
        {
            frequencyScore = 50f;
        }
        else if (averageFrequency > 120)
        {
            frequencyScore = 50f * (120f / averageFrequency);
        }
        else
        {
            frequencyScore = 50f * (averageFrequency / 100f);
        }

        float depthScore = depthAccuracy * 50f;

        return frequencyScore + depthScore;
    }
}