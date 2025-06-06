using UnityEngine;
using System.Collections;

public class Bullet : MonoBehaviour
{
    public float speed = 50f; // Bullet speed
    public Rigidbody rb;
    public LineRenderer lineRenderer; // Reference to the Line Renderer
    private Vector3 startPos;

    void Start()
    {
        //rb.velocity = transform.forward * speed;
        startPos = transform.position;

        if (lineRenderer)
        {
            StartCoroutine(DrawTracer());
        }
    }

    IEnumerator DrawTracer()
    {
        lineRenderer.positionCount = 2;
        while (gameObject)
        {
            lineRenderer.SetPosition(0, startPos);
            lineRenderer.SetPosition(1, transform.position);
            yield return null; // Update every frame
        }
    }
    private void FixedUpdate()
    {
        transform.position = transform.position+ transform.forward*speed*Time.fixedDeltaTime;
    }

    void OnTriggerEnter(Collider other)
    {
        Debug.Log("Hello");
        if(other.gameObject.CompareTag("Target"))
    {
            ShootingManager shootingManager = FindObjectOfType<ShootingManager>();
            if (shootingManager != null)
                shootingManager.RegisterHit(transform.position);

            Destroy(gameObject);
        }
    }
}
