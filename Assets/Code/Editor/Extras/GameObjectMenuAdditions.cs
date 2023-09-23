using FR8Runtime.Contracts;
using FR8Runtime.Contracts.Predicates;
using UnityEditor;
using UnityEngine;

namespace FR8Editor.Extras
{
    public static class GameObjectMenuAdditions
    {
        public static void InstanceSimpleObject<T>() where T : Component
        {
            var instance = new GameObject($"Unnamed {typeof(T).Name}").AddComponent<T>();

            Selection.objects = null;
            Selection.activeObject = instance;
        }
        
        [MenuItem("GameObject/Contract/Contract")]
        public static void NewContract() => InstanceSimpleObject<Contract>();
        
        [MenuItem("GameObject/Track/Track Section")]
        public static void NewTrackSection() => InstanceSimpleObject<TrackSection>();
        
        public static void AddPredicate<T>() where T : ContractPredicate
        {
            var parent = Selection.activeGameObject ? Selection.activeGameObject.GetComponent<Contract>() : null;
            if (!parent)
            {
                parent = Object.FindObjectOfType<Contract>();
            }
            if (!parent)
            {
                parent = new GameObject("Unnamed Contract").AddComponent<Contract>();
            }
            
            var instance = new GameObject(typeof(T).Name).AddComponent<T>();
            instance.transform.parent = parent.transform;
            instance.transform.SetAsLastSibling();

            Selection.objects = null;
            Selection.activeObject = instance.gameObject;
        }
        
        [MenuItem("GameObject/Contract/Predicates/Delivery Predicate")]
        public static void AddDeliveryPredicate() => AddPredicate<DeliveryPredicate>();
    }
}