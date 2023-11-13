using TMPro;
using UnityEngine;

namespace FR8.Runtime.Train
{
    public class Nameplate : MonoBehaviour
    {
        private void Start()
        {
            GetComponentInChildren<TMP_Text>().text = GetComponentInParent<INameplateProvider>().Name.ToUpper();
        }
    }
}
