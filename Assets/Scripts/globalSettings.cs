using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class globalSettings : MonoBehaviour
{
    [SerializeField]
    private int targetFrameRate;

    internal AudioSource audio;
    public static globalSettings instance;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
        Application.targetFrameRate = targetFrameRate;
        audio = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
