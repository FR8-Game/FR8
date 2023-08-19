using System;
using FMOD.Studio;
using FMODUnity;
using FR8.Interactions.Drivers;
using FR8.Train.Electrics;
using UnityEngine;

namespace FR8.Train
{
    [SelectionBase, DisallowMultipleComponent]
    [RequireComponent(typeof(TrainGasTurbine))]
    public sealed class LocomotiveAudio : MonoBehaviour
    {
        public EventReference loopAudio;

        private TrainGasTurbine engine;
        private ExternalTrainDoor[] doors;
        
        private bool wasEngineActive;
        private float engineRunningTime;

        private EventInstance loopEvent;

        private void Awake()
        {
            engine = GetComponent<TrainGasTurbine>();
            doors = GetComponentsInChildren<ExternalTrainDoor>();
        }

        private void OnEnable()
        {
            loopEvent = RuntimeManager.CreateInstance(loopAudio);
        }

        private void OnDisable()
        {
            loopEvent.release();
        }

        private void Update()
        {
            var rpm = engine.CurrentRpm;
            loopEvent.setParameterByName("RPM", rpm);
            loopEvent.set3DAttributes(gameObject.To3DAttributes());

            var isEngineActive = rpm > engine.stallRpm;
            if (isEngineActive != wasEngineActive)
            {
                if (isEngineActive) loopEvent.start();
                else loopEvent.stop(STOP_MODE.ALLOWFADEOUT);
            }

            var inside = true;
            foreach (var door in doors)
            {
                if (!door.Open) continue;
                inside = false;
                break;
            }
            RuntimeManager.StudioSystem.setParameterByName("Inside", inside ? 1.0f : 0.0f);
            
            wasEngineActive = isEngineActive;
        }
    }
}
