using UnityEngine;

public class Fly : MonoBehaviour
{
    public float speed;
    public float rotationSpeed;
    private bool crashed;
    private Rigidbody body;
    private float boost;

	// Use this for initialization
	private void Start ()
    {
        body = GetComponent<Rigidbody>();
        boost = 1;
    }
	
	// Update is called once per frame
	private void Update ()
    {
        CheckBoost();

        CheckCollision();
	}

    private void CheckCollision()
    {
        if (crashed)
        {
            body.useGravity = true;
        }
        else
        {
            Vector3 controller = microbit.getRotations();
            Vector3 newRotation = new Vector3(controller.z, 0, -controller.x);
            transform.Rotate(newRotation * rotationSpeed);
            transform.Translate(Vector3.left * Time.deltaTime * speed * boost);
        }
    }

    private void CheckBoost()
    {
        if (microbit.getAPressed())
        {
            boost = 5;
        }
    }

    private void OnCollisionEnter(Collision collision)
    {
        crashed = true;
    }
}
