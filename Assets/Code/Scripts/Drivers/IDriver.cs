using UnityEngine;

namespace FR8.Drivers
{
    public interface IDriver : IInteractable
    {
        void Nudge(int direction);
        void Press();

        void BeginDrag(Ray ray);
        void ContinueDrag(Ray ray);
        
        void ValueUpdated();
        void SetDriverGroup(DriverGroup group);
    }
}