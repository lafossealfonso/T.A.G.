using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class TimerEvent : MonoBehaviour
{
    [SerializeField] public float _timerRemaining;
    public bool _runTimer = false;
    public UnityEvent onTimerEnd;
    public CustomTimeEvent[] customLogic;
    bool _initialSet = false;
    float _maxTime;

    private void Start()
    {
        //Debug.LogError("TimerEvent");
        _maxTime = _timerRemaining;
    }

    public void SetTimer(float durationSeconds, bool startTimerNow)
    {
        _timerRemaining = durationSeconds;
        _maxTime = durationSeconds;
        _runTimer = startTimerNow;

        if (!_initialSet)
            _initialSet = true;
    }

    public void PauseTimer()
    {
        _runTimer = false;
    }

    public void ResumeTimer()
    {
        if (_timerRemaining > 0f)
            _runTimer = true;
        else
            Debug.LogError(this.name + " Timer has not been set");
    }

    public void RestartTimer()
    {
        if (!_initialSet)
            return;

        _timerRemaining = _maxTime;
    }

    private void Update()
    {
        //Timer counts down
        if (_runTimer && _timerRemaining > 0f)
        {
            _timerRemaining -= Time.deltaTime;

            //Timer checks for custom logic
            if (customLogic.Length > 0)
            {
                for (int i = 0; i < customLogic.Length; i++)
                {
                    var tick = customLogic[i];

                    if (tick.atSecondsLeft > _maxTime)
                        return;

                    if (_timerRemaining <= tick.atSecondsLeft && !tick.triggered)
                    {
                        tick.OnTick?.Invoke();
                        customLogic[i].triggered = true;
                    }
                }
            }
            return;
        }

        //Timer checks for custom logic
        //if(customLogic.Length > 0)
        //{
        //    for(int i = 0; i < customLogic.Length; i++)
        //    {
        //        var tick = customLogic[i];

        //        if (tick.atTime > _maxTime)
        //            return;

        //        if(_timerRemaining <= tick.atTime && !tick.triggered)
        //        {
        //            tick.OnTick?.Invoke();
        //            tick.triggered = true;
        //        }
        //    }
        //}

        //Timer ends
        if (_timerRemaining <= 0f && _runTimer)
        {
            _timerRemaining = 0f;
            _runTimer = false;
            onTimerEnd?.Invoke();
        }


    }
}

[System.Serializable]
public struct CustomTimeEvent
{
    public int atSecondsLeft;
    [HideInInspector] public bool triggered;
    public UnityEvent OnTick;

    public CustomTimeEvent(int time)
    {
        atSecondsLeft = time;
        triggered = false;
        OnTick = new();
    }


}
