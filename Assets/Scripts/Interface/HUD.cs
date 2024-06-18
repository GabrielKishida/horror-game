using System.Collections;
using TMPro;
using UnityEngine;

public class HUD : MonoBehaviour

{
    [SerializeField] private TextMeshProUGUI uiCounters;
    [SerializeField] private TextMeshProUGUI centerText;
    [SerializeField] private CanvasGroup centerCanvas;
    [SerializeField] private float timeToDisappear = 5.0f;
    [SerializeField] private float gradientTimeToDisappear = 2.0f;

    private bool coroutineIsRunning = false;
    private Coroutine displayCoroutine;

    public int keyCounter = 0;
    public int keyMax = 0;
    public int noteCounter = 0;
    public int noteMax = 0;

    private string GetUiCounters ()
    {
        return $"Chaves: {keyCounter}/{keyMax}\nNotas: {noteCounter}/{noteMax}";
    }

    private IEnumerator DisplayText(string text)
    {
        coroutineIsRunning = true;
        float startAlpha = 1;
        centerCanvas.alpha = startAlpha;
        centerText.text = text;
        yield return new WaitForSeconds(timeToDisappear);

        float rate = 1.0f / gradientTimeToDisappear;
        float progress = 0.0f;
        while (progress < 1.0f)
        {
            centerCanvas.alpha = Mathf.Lerp(startAlpha, 0, progress);
            progress += rate * Time.deltaTime;

            yield return null;
        }

        centerCanvas.alpha = 0;
        coroutineIsRunning = false;
    }

    private void ShowCenterText(string text)
    {
        if (coroutineIsRunning)
        {
            StopCoroutine(displayCoroutine);
        }
        displayCoroutine = StartCoroutine(DisplayText(text));
    }


    void Start()
    {
        centerCanvas.alpha = 0f;
        keyMax = GameObject.FindGameObjectsWithTag("Key").Length;
        noteMax = GameObject.FindGameObjectsWithTag("Note").Length;
        uiCounters.text = GetUiCounters();
    }

    public void AddKey()
    {
        keyCounter++;
        ShowCenterText($"Você coletou uma chave.\nChaves: {keyCounter}/{keyMax}");
        uiCounters.text = GetUiCounters();
    }

    public void AddNote()
    {
        noteCounter++;
        ShowCenterText($"Você coletou uma nota.\nNotas: {noteCounter}/{noteMax}");
        uiCounters.text = GetUiCounters();
    }

    public void OpenDoor()
    {
        ShowCenterText($"Você abriu a porta.\nFuja.");
    }

    public void LockedDoor()
    {
        ShowCenterText($"Você precisa de mais chaves.\nChaves: {keyCounter}/{keyMax}");
    }
}
