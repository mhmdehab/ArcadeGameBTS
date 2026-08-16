using UnityEngine;

public class Pickup : MonoBehaviour
{
    bool isHolding = false;

    [SerializeField]
    float throwForce = 600f;
    [SerializeField]
    float maxDistance = 3f;
    [SerializeField] float followSpeed = 15f;
    float distance;

    TempParent tempParent;
    Rigidbody rb;

    Vector3 objectPos;

    void Start()
    {
        rb=GetComponent<Rigidbody>();
        tempParent = TempParent.Instance;
    }

    void Update()
    {
        if (isHolding)
        {
            Hold();
        }
    }
    private void OnMouseDown()
    {
        if (tempParent == null) return;
        distance = Vector3.Distance(transform.position, tempParent.transform.position);
        if (distance <= maxDistance)
        {
            isHolding = true;
            rb.useGravity = false;
            rb.detectCollisions = true;
            Ball ball = GetComponent<Ball>();
            if (ball != null && BallHud.Instance != null)
                BallHud.Instance.ShowNumber(ball.ballNumber);
        }
    }
    private void OnMouseUp()
    {
        Drop();
    }

    private void Hold()
    {
        Vector3 target = tempParent.transform.position;
        rb.linearVelocity = (target - transform.position) * followSpeed;
        rb.angularVelocity = Vector3.zero;

        distance = Vector3.Distance(transform.position, target);
        if (distance >= maxDistance)
        {
            Drop();
            return;
        }

        if (Input.GetMouseButtonDown(1))
        {
            rb.linearVelocity = Vector3.zero;
            rb.AddForce(tempParent.transform.forward * throwForce);
            Drop();
        }
    }
    private void Drop()
    {
        if (isHolding)
        {
            isHolding = false;
            transform.SetParent(null);
            rb.useGravity=true;
            
            if (BallHud.Instance != null)
                BallHud.Instance.HideNumber();
        }
    }
}
