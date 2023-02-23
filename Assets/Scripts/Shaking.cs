using UnityEngine;

[RequireComponent(typeof(DistanceJoint2D))]
[RequireComponent(typeof(Rigidbody2D))]
public class Shaking : MonoBehaviour
{
    [SerializeField] private KeyCode _shakeRight = KeyCode.D;
    [SerializeField] private KeyCode _shakeLeft = KeyCode.A;
    [SerializeField] private float _speed;

    private DistanceJoint2D _distanceJoint2D;
    private Rigidbody2D _playerRb;

    private void Start()
    {
        _distanceJoint2D = GetComponent<DistanceJoint2D>();
        _playerRb = GetComponent<Rigidbody2D>();
    }

    private void Update()
    {
        if (_distanceJoint2D.isActiveAndEnabled)
        {
            if (Input.GetKey(_shakeRight))
            {
                _playerRb.AddForce(Vector2.right * _speed);
            }

            if (Input.GetKey(_shakeLeft))
            {
                _playerRb.AddForce(Vector2.left * _speed);
            }
        }
    }
}
