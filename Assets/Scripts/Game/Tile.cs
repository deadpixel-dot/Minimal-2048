using System.Collections;

using UnityEngine;
using UnityEngine.UI;

public class Tile : MonoBehaviour
{
    public TileImage imageData { get; private set; } // Use TileImage instead of TileState
    public TileCell cell { get; private set; }
    public bool locked { get; set; }

    private Image background;

    private void Awake()
    {
        background = GetComponent<Image>();
    }

    // Update the method to set the image instead of the state
    public void SetImage(TileImage imageData)
    {
        this.imageData = imageData;

        // Set the background image directly
        background.sprite = imageData.image; // Use the image from TileImage
        background.color = Color.white; // Set color to white to ensure the image is displayed correctly
    }

    public void Spawn(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        transform.position = cell.transform.position;
    }

    public void MoveTo(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = cell;
        this.cell.tile = this;

        StartCoroutine(Animate(cell.transform.position, false));
    }

    public void Merge(TileCell cell)
    {
        if (this.cell != null)
        {
            this.cell.tile = null;
        }

        this.cell = null;
        cell.tile.locked = true;

        StartCoroutine(Animate(cell.transform.position, true));
    }

    private IEnumerator Animate(Vector3 to, bool merging)
    {
        float elapsed = 0f;
        float duration = 0.1f;

        Vector3 from = transform.position;

        while (elapsed < duration)
        {
            transform.position = Vector3.Lerp(from, to, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.position = to;

        if (merging)
        {
            Destroy(gameObject);
        }
    }
}
