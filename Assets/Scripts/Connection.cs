using System.Collections;
using UnityEngine;

[RequireComponent(typeof(DistanceJoint2D))]
public class Connection : MonoBehaviour
{
    [SerializeField] private KeyCode _detach = KeyCode.Mouse0;
    [SerializeField] private Transform _mainCamera;
    [SerializeField] private Collider2D _ropeCollider;

    private DistanceJoint2D _distanceJoint2D;

    private void Start()
    {
        _distanceJoint2D = GetComponent<DistanceJoint2D>();
    }

    private void Update()
    {
        if (Input.GetKey(_detach) && _distanceJoint2D.isActiveAndEnabled)
        {
            _distanceJoint2D.enabled = false;
            _ropeCollider.enabled = false;
            StartCoroutine(Waiting(_ropeCollider));
        }
        _mainCamera.position = new Vector3(transform.position.x, transform.position.y, -10f);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (_distanceJoint2D.isActiveAndEnabled == false && collision.gameObject.TryGetComponent(out EndOfRope endOfRope))
        {
            _distanceJoint2D.connectedBody = collision.GetComponent<Rigidbody2D>();
            _distanceJoint2D.enabled = true;
            _ropeCollider = collision;

            //_mainCamera.position = new Vector3(collision.GetComponentInParent<Transform>().position.x, 0f, -10f);
        }
    }

    private IEnumerator Waiting(Collider2D collider)
    {
        yield return new WaitForSeconds(1f);
        EnableCollider(collider);
    }

    private void EnableCollider(Collider2D collider)
    {
        collider.enabled = true;
    }
}