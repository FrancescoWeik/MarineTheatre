using UnityEngine;
using UnityEngine.Events;

public class Curtains : MonoBehaviour
{
    [SerializeField] private Animator animator;
    public UnityEvent onFinishClosing;
    public static Curtains Instance { get; private set; }

    private void Awake()
    {
        // If an instance already exists and it's not this one, destroy this one
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        // Set the instance to this object
        Instance = this;

        // Optional: Keep this object alive when switching scenes
        // DontDestroyOnLoad(gameObject);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        animator = GetComponent<Animator>();
    }

    public void OpenCurtains()
    {
        animator.SetTrigger("Open");
    }

    public void CloseCurtains()
    {
        animator.SetTrigger("Close");
    }

    public void FinishedCLosing()
    {
        onFinishClosing?.Invoke();
    }
}
