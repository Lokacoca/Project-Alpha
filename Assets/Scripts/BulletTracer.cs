using UnityEngine;

public class BulletTracer : MonoBehaviour
{
    public float speed = 100f;
    private Vector3 target;
    private LineRenderer lr;

    public void Init(Vector3 start, Vector3 end)
    {
        target = end;
        lr = GetComponent<LineRenderer>();
        lr.SetPosition(0, start);
        lr.SetPosition(1, start);
        StartCoroutine(MoveTracer(start, end));
    }

    private System.Collections.IEnumerator MoveTracer(Vector3 start, Vector3 end)
    {
        float distance = Vector3.Distance(start, end);
        float duration = distance / speed;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            Vector3 current = Vector3.Lerp(start, end, elapsed / duration);
            lr.SetPosition(1, current);
            yield return null;
        }

        lr.SetPosition(1, end);
        Destroy(gameObject, 0.05f); // short lifespan
    }
}
