using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class TypeWriter : MonoBehaviour
{
    public AudioClip audioClip;
    public static TypeWriter instance;
    public float Speed = 15;
    [HideInInspector] public bool iswritting;
    private void Awake()
    {
        instance = this;
    }
    public void Run(string textToType, Text textLabel, UnityAction EndEvent = null)
    {
        StopAllCoroutines();
        StartCoroutine(TypeText(textToType, textLabel, EndEvent));
        iswritting = true;
    }
    IEnumerator TypeText(string textToType, Text textLabel, UnityAction EndEvent)
    {
        float t = 0;
        int charIndex = 0;
        string processedText = "";
        int i=AudioManager.instance.PlayLoop(audioClip, 0.5f,0);
        
        while (charIndex < textToType.Length)
        {
            t += Time.deltaTime * Speed;
            charIndex = Mathf.FloorToInt(t);
            charIndex = Mathf.Clamp(charIndex, 0, textToType.Length);


            processedText = ProcessText(textToType.Substring(0, charIndex));
            textLabel.text = processedText;

            yield return null;
        }


        processedText = ProcessText(textToType);
        textLabel.text = processedText;
        EndEvent?.Invoke();

        iswritting = false;
        AudioManager.instance.Stop(i);
    }
    private string ProcessText(string inputText)
    {
        string result = "";
        bool inRedSection = false;
        int startIndex = 0;

        for (int i = 0; i < inputText.Length; i++)
        {
            if (inputText[i] == '@')
            {
                if (inRedSection)
                {

                    result += "<color=yellow>" + inputText.Substring(startIndex, i - startIndex) + "</color>";
                    startIndex = i + 1;
                    inRedSection = false;
                }
                else
                {

                    result += inputText.Substring(startIndex, i - startIndex);
                    startIndex = i + 1;
                    inRedSection = true;
                }
            }
        }


        if (inRedSection)
        {
            result += "<color=yellow>" + inputText.Substring(startIndex) + "</color>";
        }
        else
        {
            result += inputText.Substring(startIndex);
        }

        return result;
    }
}