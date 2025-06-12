using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using DG.Tweening;
using System.Runtime.ConstrainedExecution;
using Unity.VisualScripting;
public class SystemManager : MonoBehaviour
{
    public static SystemManager instance;
    public Transform TipsPanel;
    public Transform NPC;

    public Transform canvas;
    public Transform cam;
    public Transform Player;
    public GameObject TutorialPanel;
    public GameObject CPRPanel;
    bool isAroundNpc;

    public float SimulationTime = 10f;
    private bool isStartTest;
    public bool IsStartTest
    {
        get 
        {
            return isStartTest;
        }
    }
    public HandsDepthController handsDepthController;
    private void Awake()
    {
        instance = this;
    }

    void Start()

    {
        TypeWriter.instance.Run("In this scenario, a student in the classroom will be simulated fainting, so please follow the instructions.\r\nAre you ready?", TipsPanel.GetChild(0).GetComponent<Text>(), delegate {
            TipsPanel.GetChild(1).gameObject.SetActive(true);
            TipsPanel.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate
            {
                StartCoroutine(StartSimulation());
            });
        });
        CPRPanel.transform.GetChild(1).GetChild(4).GetChild(0).GetComponent<Button>().onClick.AddListener(delegate {
            CPRPanel.transform.GetChild(1).gameObject.SetActive(false);
            CPRPanel.transform.GetChild(0).gameObject.SetActive(true);
            StartCoroutine(StartTest());

        });
        CPRPanel.transform.GetChild(1).GetChild(4).GetChild(1).GetComponent<Button>().onClick.AddListener(delegate {
            StartCoroutine(WakeUp());
        });
    }

    
    void Update()
    {
        
    }
    public void DetectPlayerPosition()
    {
        if(Vector3.Distance(OVRManager.instance.transform.position,NPC.position)<=2)
            isAroundNpc = true;
        else isAroundNpc = false;
    }
    IEnumerator StartSimulation()
    {
        NPC.GetComponent<Animator>().SetBool("fall",true);
        yield return new WaitForSeconds(3f);
        yield return Step1MoveToPosition();
        yield return Step2Observe();
        //yield return Step4FollowTutorial();
    }
    IEnumerator Step1MoveToPosition()
    {
        ShowInfo("First, you need to get close to the injured person. @Please move the joystick of the controller to position yourself around the injured person@.", delegate {
            TipsPanel.GetChild(1).gameObject.SetActive(true);
            TipsPanel.GetChild(1).GetChild(0).GetComponent<Text>().text = "OK";
        });
        while(!isAroundNpc)
        {
            yield return null;
        }
        TipsPanel.GetChild(1).gameObject.SetActive(false);
        TipsPanel.gameObject.SetActive(false);
    }
    IEnumerator Step2Observe()
    {
        ShowInfo("The second step is to observe whether he is awake and whether he is breathing.", delegate {
            TipsPanel.GetChild(1).gameObject.SetActive(true); 
            TipsPanel.GetChild(1).GetComponent<Button>().onClick.AddListener(delegate
            {
                StartCoroutine(Step3PreparePress());
            });
        },true);
        yield return null ;
    }
    IEnumerator Step3PreparePress()
    {
        //TutorialPanel.SetActive(true);
        NPC.GetComponentInChildren<HighlightController>().StartHighlight();
        ShowInfo("Move your hands near the highlighted area of the casualty's chest.", null, false);
        while(!HandsPositionTrigger.instance.IsReady)
        {
            yield return null ;
        }
        yield return Step4FollowTutorial();
    }
    IEnumerator Step4FollowTutorial()
    {
        TutorialPanel.SetActive(true);
        
        ShowInfo("Place your hands on the casualty's chest, press and hold the trigger on the middle finger of both hands, and perform CPR on the casualty according to the tutorial prompts.", null, true);
        yield return null;
    }

    IEnumerator WakeUp()
    {
        TutorialPanel.gameObject.SetActive(false);
        NPC.GetComponent<Animator>().SetBool("wake", true);
        yield return new WaitForSeconds(2);
        ShowInfo("Congratulations on completing the simulation and successfully treating the injured!",null,true);
    }

    public IEnumerator StartTest()
    {
        handsDepthController.ResetAll();
        isStartTest = true;
        float timer=SimulationTime;
        while(timer>0)
        {
            timer-=Time.deltaTime;
            CPRPanel.transform.GetChild(0).GetChild(1).GetComponent<Text>().text=timer.ToString("F1");
            yield return null ;
        }
        isStartTest=false;
        CPRPanel.transform.GetChild(0).gameObject.SetActive(false);
        CPRPanel.transform.GetChild(1).gameObject.SetActive(true);
        CPRPanel.transform.GetChild(1).GetChild(1).GetComponent<Text>().text= "AverageFrequency: "+ handsDepthController.CalculateAverageFrequency().ToString("F0");
        CPRPanel.transform.GetChild(1).GetChild(2).GetComponent<Text>().text= "DepthAccuracy:" + handsDepthController.CalculateDepthAccuracy().ToString("P2");
        CPRPanel.transform.GetChild(1).GetChild(3).GetComponent<Text>().text = "Score:" + handsDepthController.CalculateScore().ToString("F2");
        if(handsDepthController.CalculateScore()>=70)
        {
            CPRPanel.transform.GetChild(1).GetChild(4).GetChild(1).gameObject.SetActive(true);
        }
        else
        {
            CPRPanel.transform.GetChild(1).GetChild(4).GetChild(1).gameObject.SetActive(false);
        }

    }
    private void ShowInfo(string words,UnityAction EndEvent=null,bool needResetCanvasPos=false)
    {
        if (needResetCanvasPos)
            ChangePos();
        TipsPanel.gameObject.SetActive(true);
        TipsPanel.GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
        TypeWriter.instance.Run(words, TipsPanel.GetChild(0).GetComponent<Text>(),EndEvent);
    }
    public void ShowInfoSimulationPanel(string words, UnityAction EndEvent = null)
    {
        CPRPanel.gameObject.SetActive(true);
        CPRPanel.transform.GetChild(2).gameObject.SetActive(true);
        CPRPanel.transform.GetChild(2).GetChild(1).GetComponent<Button>().onClick.RemoveAllListeners();
        CPRPanel.transform.GetChild(2).GetChild(0).GetComponent<Text>().text = words;
        CPRPanel.transform.GetChild(2).GetChild(1).GetComponent<Button>().onClick.AddListener(EndEvent);
    }
    public void ChangePos()
    {
        Vector3 cameraForward = cam.transform.forward;

        cameraForward.y = 0;
        cameraForward.Normalize();
        //canvas.position = cameraForward + cam.position;
        

        //canvas.rotation = Quaternion.Euler(0, rotation.eulerAngles.y, 0);
        canvas.DOMove(cameraForward + cam.position, 0.3f).OnComplete(() => {
            Vector3 direction = cam.position - canvas.position;
            Quaternion rotation = Quaternion.LookRotation(-direction);
            canvas.DORotateQuaternion(Quaternion.Euler(0, rotation.eulerAngles.y, 0), 0.3f); });
        ;
    }
}
