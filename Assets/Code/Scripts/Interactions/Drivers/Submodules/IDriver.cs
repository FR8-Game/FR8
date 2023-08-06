using FR8.Interactions.Drivables;
using UnityEngine;

namespace FR8.Interactions.Drivers.Submodules
{
    public interface IDriver : IInteractable, IDrivable
    {
        void Nudge(int direction);

        void BeginDrag(Ray ray);
        void ContinueDrag(Ray ray);
    }
}