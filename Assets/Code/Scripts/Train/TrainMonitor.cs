using System;
using FMODUnity;
using FR8.Runtime.Interactions.Drivers;
using UnityEngine;

namespace FR8.Runtime.Train
{
    [SelectionBase, DisallowMultipleComponent]
    public class TrainMonitor : MonoBehaviour
    {
        public EventReference warningBuzz;
        public StatTracker loadTracker;

        private DriverNetwork driverNetwork;
        private Canvas canvas;
        private GameObject[] panels;
        
        private event Action UpdateEvent;

        private void Awake()
        {
            canvas = GetComponent<Canvas>();
            driverNetwork = GetComponentInParent<DriverNetwork>();

            var parent = transform.Find("Mask");
            panels = new GameObject[parent.childCount];
            for (var i = 0; i < panels.Length; i++)
            {
                panels[i] = parent.GetChild(i).gameObject;
                panels[i].SetActive(i == 0);
            }
            
            loadTracker.Init(() => driverNetwork.GetValue("load"), ref UpdateEvent, ShowPanel(1));
        }

        private void OnEnable()
        {
            driverNetwork.ValueChangedEvent += OnValueChanged;
            canvas.enabled = driverNetwork.GetValue("mainfuse") > 0.5f;
        }

        private void OnDisable()
        {
            driverNetwork.ValueChangedEvent -= OnValueChanged;
        }

        private void OnValueChanged(string key, float value)
        {
            if (key != "mainfuse") return;
            canvas.enabled = value > 0.5f;
        }

        private Action<bool> ShowPanel(int index)
        {
            return state =>
            {
                for (var i = 0; i < panels.Length; i++)
                {
                    var panel = panels[i];
                    panel.SetActive(i == (state ? index : 0));
                }
            };
        }

        private void Update()
        {
            UpdateEvent?.Invoke();
        }

        [Serializable] 
        public class StatTracker
        {
            [SerializeField] private float threshold;
            [SerializeField] private float delay;
            
            private Func<float> value;
            private float counter;
            private bool state;

            private Action<bool> changeStateCallback;
            
            public bool State => state;
            
            public void Init(Func<float> getValue, ref Action update, Action<bool> changeStateCallback)
            {
                update += Update;
                value = getValue;
                this.changeStateCallback = changeStateCallback;
            }

            private void Update()
            {
                counter += (value() > threshold ? 1.0f : -1.0f) * Time.deltaTime / delay;
                if (!state && counter >= 1.0f) ChangeState(true);
                if (state && counter <= 0.0f) ChangeState(false);
                counter = Mathf.Clamp01(counter);
            }

            private void ChangeState(bool state)
            {
                if (this.state == state) return;
                this.state = state;
                changeStateCallback?.Invoke(state);
            }
        }
    }
}