using UnityEngine;

public class Lane : MonoBehaviour
{
    public int laneIndex;

    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform visualTransform;
    [SerializeField] private Sprite laneSprite;

    [SerializeField] private float laneWidth = 20f;
    [SerializeField] private float laneHeight = 1.5f;

    private void Awake()
    {
        if (spriteRenderer == null)
        {
            Debug.LogError("Lane SpriteRenderer is not assigned.");
            return;
        }

        if (visualTransform == null)
        {
            Debug.LogError("Lane Visual Transform is not assigned.");
            return;
        }

        if (laneSprite == null)
        {
            Debug.LogError("Lane Sprite is not assigned for: " + name);
            return;
        }

        spriteRenderer.sprite = laneSprite;

        Vector2 spriteSize = laneSprite.bounds.size;

        visualTransform.localScale = new Vector3(
            laneWidth / spriteSize.x,
            laneHeight / spriteSize.y,
            1f
        );
    }
}