using UnityEngine;

namespace FR8.Interactions
{
    public interface IDriver : IBehaviour
    {
        float Output { get; }
        bool CanInteract { get; }
        bool Limited { get; }
        string DisplayName { get; }
        string DisplayValue { get; }
        
        void Nudge(int direction);
        void Press();

        void BeginDrag(Ray ray);
        void ContinueDrag(Ray ray);
    }
}