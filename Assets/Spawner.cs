using UnityEngine;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject prefab;
    [SerializeField] private float delay;
    [SerializeField] private float lifetime;

    private float timer;

    private void OnEnable()
    {
        prefab.SetActive(false);
    }

    private void Update()
    {
        if (timer > delay)
        {
            timer -= delay;
            var instance = Instantiate(prefab);
            instance.SetActive(true);
            Destroy(instance, lifetime);
        }
        timer += Time.deltaTime;
    }
}
