using System.IO;
using System.Xml.Serialization;
using FR8Runtime.Contracts.Predicates;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR

#endif

namespace FR8Runtime.Contracts
{
    [CreateAssetMenu(menuName = "Scriptable Objects/Contract")]
    [XmlInclude(typeof(DeliveryPredicate))]
    public class Contract : ScriptableObject
    {
        public ContractPredicate[] predicates;

        [ContextMenu("Debug Serialize")]
        public void DebugSerialize()
        {
            var temp = CreateInstance<Contract>();

            temp.name = "Testing Contract";
            temp.predicates = new ContractPredicate[]
            {
                new DeliveryPredicate()
                {
                    carriageNames = new[]
                    {
                        "Carriage.1",
                        "Carriage.2",
                        "Carriage.3"
                    },
                    deliveryLocationName = "The Delivery Location"
                },
            };

            Debug.Log(temp.Serialize());
            DestroyImmediate(temp);
        }

        public string Serialize()
        {
            string text;
            using (var stream = new MemoryStream())
            {
                Serialize(stream);
                stream.Position = 0;
                using (var reader = new StreamReader(stream))
                {
                    text = reader.ReadToEnd();
                }
            }

            return text;
        }

        public void Serialize(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(Contract));
            serializer.Serialize(stream, this);
        }

        public static Contract Deserialize(Stream stream)
        {
            var serializer = new XmlSerializer(typeof(Contract));
            return (Contract)serializer.Deserialize(stream);
        }

#if UNITY_EDITOR
        [MenuItem("Tools/Contracts/Create Contract Template")]
        public static void CreateContractTemplate()
        {
            var contract = CreateInstance<Contract>();
            contract.name = "Contract Template";

            contract.predicates = new ContractPredicate[]
            {
                new DeliveryPredicate
                {
                    carriageNames = new[]
                    {
                        "Train Carriage Scene Name 0",
                        "Train Carriage Scene Name 1",
                        "Train Carriage Scene Name 2"
                    },
                    deliveryLocationName = "Track Section Scene Name 1"
                },
                new DeliveryPredicate
                {
                    carriageNames = new[]
                    {
                        "Train Carriage Scene Name 3",
                        "Train Carriage Scene Name 4"
                    },
                    deliveryLocationName = "Track Section Scene Name 2"
                },
            };

            var path = Application.dataPath;
            var filename = Path.Combine(path, $"{contract.name}.xml");

            Directory.CreateDirectory(path);
            using (var fs = new FileStream(filename, FileMode.OpenOrCreate))
            {
                contract.Serialize(fs);
            }
            AssetDatabase.Refresh();

            DestroyImmediate(contract);
        }

#endif
    }
}