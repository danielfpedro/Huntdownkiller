using UnityEngine;
using UnityEngine.Pool;

public class HitIndicatorManager : MonoBehaviour
{
    public static HitIndicatorManager Instance { get; private set; }

    public GameObject indicatorPrefab;
    private ObjectPool<GameObject> indicatorPool;
    private int initialPoolSize = 10;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        indicatorPool = new ObjectPool<GameObject>(
            createFunc: () => Instantiate(indicatorPrefab),
            actionOnGet: (obj) => obj.SetActive(true),
            actionOnRelease: (obj) => obj.SetActive(false),
            actionOnDestroy: (obj) => DestroyImmediate(obj),
            collectionCheck: false,
            defaultCapacity: initialPoolSize,
            maxSize: 100
        );
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void ShowIndicator(Vector3 position, string text)
    {
        Debug.Log("Showing hit indicator at " + position + " with text: " + text);
        GameObject indicator = indicatorPool.Get();
        indicator.transform.position = position;
        HitIndicator hitIndicator = indicator.GetComponent<HitIndicator>();
        if (hitIndicator != null)
        {
            hitIndicator.Initialize(text);
        }
    }

    public void ReturnIndicator(GameObject indicator)
    {
        indicatorPool.Release(indicator);
    }
}
