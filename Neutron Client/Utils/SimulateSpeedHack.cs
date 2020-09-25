using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SimulateSpeedHack : MonoBehaviour
{
    [SerializeField] private float _timeScale = 1;
    void Start()
    {
        Time.timeScale = _timeScale;
    }
}
