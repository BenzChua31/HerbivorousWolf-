using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tween
{

    public Tween(Transform target, Vector3 startPos, Vector3 endPos, float startTime, float duration)
    {
        this.Target = target;
        this.StartPos = startPos;
        this.EndPos = endPos;
        this.StartTime = startTime;
        this.Duration = duration;
        this.TimeElapsed = 0;
    }

    public Transform Target { get; private set; }
    public Vector3 StartPos { get; private set; }
    public Vector3 EndPos { get; private set; }
    public float StartTime { get; set; }
    public float Duration { get; private set; }
    public float TimeElapsed { get; set; }
}
