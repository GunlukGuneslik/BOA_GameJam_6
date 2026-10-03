using System;
using UnityEngine;

public class Border : MonoBehaviour
{
    public static Action<Obstacle> OnObstacleHitBoundary;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.TryGetComponent(out Obstacle obs))
        {
            OnObstacleHitBoundary?.Invoke(obs);
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.collider.TryGetComponent(out Obstacle obs))
        {
            OnObstacleHitBoundary?.Invoke(obs);
        }
    }
}
