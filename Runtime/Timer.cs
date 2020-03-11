using UnityEngine;

namespace CerealDevelopment.TimeManagement
{
    public class Timer : MonoBehaviour, IUpdatable
    {
        public enum TimerStatus
        {
            None,
            InProgress,
            Stopped
        }

        public TimerStatus Status { get; private set; }

        public event System.Action<Timer> Started;
        public event System.Action<Timer> Done;
        public event System.Action<Timer> Stopped;

        private float startTime = -1f;
        public float StartTime
        {
            get
            {
                return startTime;
            }
            set
            {
                startTime = value;
                duration = endTime - startTime;
                OnUpdate();
            }
        }

        private float duration;
        public float Duration
        {
            get
            {
                return duration;
            }
            set
            {
                if(value < 0f)
                {
                    throw new System.ArgumentException();
                }
                duration = value;
                endTime = startTime + duration;
                OnUpdate();
            }
        }

        private float endTime;
        public float EndTime
        {
            get
            {
                return endTime;
            }
            set
            {
                endTime = value;
                duration = endTime - startTime;
                OnUpdate();
            }
        }

        private float progress;
        public float Progress { get { return progress; } }

        private float timeLeft;
        public float TimeLeft { get { return timeLeft; } }

        private bool realtime;

        public Timer(float duration, bool realtime = true, bool startAutomatically = false)
        {
            Duration = duration;
            this.realtime = realtime;
            if (startAutomatically)
            {
                Start();
            }
        }

        public void Start()
        {
            startTime = realtime ? Time.unscaledTime : Time.time;
            endTime = startTime + duration;
            TimeManager.AddUpdatable(this);

            Status = TimerStatus.InProgress;

            Started?.Invoke(this);
        }
        
        public void Stop()
        {
            TimeManager.RemoveUpdatable(this);

            Status = TimerStatus.Stopped;

            Stopped?.Invoke(this);
        }

        private void Complete()
        {
            Done?.Invoke(this);

            Status = TimerStatus.Stopped;

            TimeManager.RemoveUpdatable(this);
        }
        
        private void OnUpdate()
        {
            ((IUpdatable)this).OnUpdate();
        }
        void IUpdatable.OnUpdate()
        {
            RecalculateTimings();
            if (progress >= 1f)
            {
                Stop();
            }
        }

        private void RecalculateTimings()
        {
            if (startTime >= 0f)
            {
                timeLeft = Mathf.Clamp(endTime - (realtime ? Time.unscaledTime : Time.time), 0f, duration);
                progress = 1f - timeLeft / duration;
            }
        }
    }
}
