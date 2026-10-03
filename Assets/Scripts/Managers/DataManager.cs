using UnityEngine;

public class DataManager : MonoBehaviour
{
    private PlayerData currentPlayerData;

    public static DataManager Instance;

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

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void UpdateLeaderChart(int player1Score, int player2Score)
    {
        
    }
}
