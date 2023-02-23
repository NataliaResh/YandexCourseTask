using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class Slide : MonoBehaviour
{
    [SerializeField] private float _minGroundNormalY = .65f;
    [SerializeField] private float _gravityModifier = 1f;
    [SerializeField] private Vector2 _velocity; //ускорение гравитационное
    [SerializeField] private LayerMask _layerMask; //фильтр для объектов
    [SerializeField] private float _speed; //модификтор скорости

    private Rigidbody2D _rb2d;
    private SpriteRenderer _sprite;

    private Vector2 _groundNormal; // нормаль к повехности
    private Vector2 _targetVelocity;    //итоговое ускорение вдоль поверхности
    private bool _grounded; //проверка что стоим на земле
    private ContactFilter2D _contactFilter;  //настройка фильтров
    private RaycastHit2D[] _hitBuffer = new RaycastHit2D[16]; //массив объектов, с коротыми можем столкнуться 
    private List<RaycastHit2D> _hitBufferList = new List<RaycastHit2D>(16); //для удобства массив переводим в лист

    private const float MinMoveDistance = 0.001f;
    private const float ShellRadius = 0.01f;

    private void OnEnable()
    {
        _rb2d = GetComponent<Rigidbody2D>();
        _sprite = GetComponent<SpriteRenderer>();
    }

    private void Start()
    {
        _contactFilter.useTriggers = false;
        _contactFilter.SetLayerMask(_layerMask);
        _contactFilter.useLayerMask = true;
    }

    private void Update()
    {
        Vector2 alongSurface = Vector2.Perpendicular(_groundNormal); //направление движения вдоль поверхности

        _targetVelocity = alongSurface * _speed;  //скорость движения вдоль поверхности
    }

    private void FixedUpdate()
    {
        _velocity += _gravityModifier * Physics2D.gravity * Time.deltaTime;
        _velocity.x = _targetVelocity.x;

        _grounded = false;

        Vector2 deltaPosition = _velocity * Time.deltaTime; //длина возможного шага за 1 кадр
        Vector2 moveAlongGround = new Vector2(_groundNormal.y, -_groundNormal.x); //направление вдоль поверхности
        Vector2 move;

        if (_groundNormal.x >= 0)
        {
            _sprite.flipX = false;
            move = moveAlongGround * (-deltaPosition.x); //длина возможного шага вдоль поверхности за 1 кадр только по Х
        }
        else
        {
            _sprite.flipX = true;
            move = moveAlongGround * (deltaPosition.x); //длина возможного шага вдоль поверхности за 1 кадр только по Х
            
        }

        Movement(move, false); //сначала двигаемся по Х

        move = Vector2.up * deltaPosition.y; // перезаписываем вектор, чтобы получить направление движения по Y
        Movement(move, true); // двигаемся по Y

        if (Input.GetKey(KeyCode.Space))
        {
            StartCoroutine(Jump(_groundNormal));
        }
    }

    private void Movement(Vector2 move, bool yMovement)
    {
        float distance = move.magnitude; //определеяем длину шага

        if (distance > MinMoveDistance) //игнорируем шаги незначительной длины
        {

            int count = //количество объектов с кем мы можем столкнуться
                _rb2d.Cast( //проецируем наш коллайдер по ходу движения
                move, _contactFilter,  //указываем объекты, с кем мы хотим столкнуться
                _hitBuffer, // получаем все объекты, с которыми нам предстоит столкнуться
                distance +
                ShellRadius);  //дополнительная зона вокруг персонажа, которая позволяет немного увеличить коллайдер персонажа

            _hitBufferList.Clear();

            for (int i = 0; i < count; i++)
            {
                _hitBufferList.Add(_hitBuffer[i]);
            }

            for (int i = 0; i < _hitBufferList.Count; i++) //перебираем все объекты, с которыми можем столкнуться
            {
                Vector2 currentNormal = _hitBufferList[i].normal;  //определяем нормаль поверхности, на которую собираемся встать
                if (currentNormal.y > _minGroundNormalY) //смотим что Y нормали больше минимально возможной нормали
                {
                    _grounded = true;
                    if (yMovement)
                    {
                        _groundNormal = currentNormal;
                        currentNormal.x = 0;
                    }
                }

                float projection = Vector2.Dot(_velocity, currentNormal); //скалярное произведение векторов
                if (projection < 0)
                {
                    _velocity = _velocity - projection * currentNormal;
                }

                float modifiedDistance = _hitBufferList[i].distance - ShellRadius;
                distance = modifiedDistance < distance ? modifiedDistance : distance; //высчитываем минимальное расстояние, на которое можем шагнуть
            }
        }

        _rb2d.position = _rb2d.position + move.normalized * distance; //итоговое изменение позиции
    }

    private IEnumerator Jump(Vector2 groundNormal)
    {
        if (_grounded)
        {
            float directionX = groundNormal.x == 0 ? 1 : groundNormal.x / Mathf.Abs(groundNormal.x);
            float y = 0.3f, x = 0.05f * directionX;
            int countPoints = 30;
            float deltaY = y / countPoints;
            

            for (int i = 0; i < countPoints; i++)
            {
                _rb2d.position = _rb2d.position + new Vector2(x, y);
                y -= deltaY;
                yield return new WaitForSeconds(0.01f);
            }

            for (int i = 0; i < countPoints; i++)
            {
                _rb2d.position = _rb2d.position + new Vector2(x, y);
                yield return new WaitForSeconds(0.01f);
            }

        }
    }
}