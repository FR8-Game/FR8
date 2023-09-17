using TMPro;
using UnityEngine;

namespace FR8Runtime.Train
{
    public class Nameplate : MonoBehaviour
    {
        private void Start()
        {
            GetComponentInChildren<TMP_Text>().text = GetComponentInParent<INameplateProvider>().Name;
        }
    }
}
