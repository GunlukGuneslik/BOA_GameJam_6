using UnityEngine;

public class GameManager : MonoBehaviour
{
    public int roundNum;
    public static GameManager Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
        }
        else
        {
            Instance = this;
        }
    }

    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void HandleHunterWin()
    {
        
    }

    public void HandleWizardWin()
    {
        
    }
}
