using LenixSO.Sequences;
using LenixSO.Sequences.Decorator;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class Node : MonoBehaviour
{
    [SerializeField] protected Image image;
    [SerializeField] protected TMP_Text text;
    [Header("Colors")]
    [SerializeField] protected Color idleColor;
    [SerializeField] protected Color runningColor;
    [SerializeField] protected Color finishedColor;

    private RectTransform _rectTransform;
    
    public RectTransform rectTransform
    {
        get
        {
            if (_rectTransform == null) 
                _rectTransform = GetComponent<RectTransform>();
            return _rectTransform;
        }
    }

    public ISequence nodeSequence { get; protected set; }

    public TMP_Text Text => text;

    private void Awake()
    {
        ResetNode();
    }

    public ISequence InjectSequence(ISequence sequence)
    {
        var observer = new ObserverSequence(sequence, SetRunningColor);
        observer.OnFinished += SetFinishedColor;
        nodeSequence = observer;
        return observer;
    }

    public void SetIdleColor() => image.color = idleColor;
    public void SetRunningColor() => image.color = runningColor;
    public void SetFinishedColor() => image.color = finishedColor;

    public void ResetNode()
    {
        SetIdleColor();
    }
}
