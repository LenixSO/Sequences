using System.Collections;
using System.Collections.Generic;
using LenixSO.Sequences;
using LenixSO.Sequences.Decorator;
using UnityEngine;

public class SequenceTester : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public ISequence LogSequence(ISequence sequence)
    {
        return new ObserverSequence(sequence,
                () => Debug.Log($"{sequence.name} begin"),
                () => Debug.Log($"{sequence.name} end"))
            .AddFinishedCallback(() => Debug.Log($"{sequence.name} finished"));
    }
}
