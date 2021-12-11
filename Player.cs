using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Player : MonoBehaviour
{
    public bool IsDead { get; private set; }
    public HealthBar HealthBar;

    [SerializeField] private GameObject _gameOverPanel;
    [SerializeField] private float _forwardSpeed;
    [SerializeField] private float _backwardSpeed;
    [SerializeField] private int _health = 100;
    [SerializeField] private float _jumpHeight;
    [SerializeField] private Text _textScore;
    [SerializeField] private Portal _portal;
    [SerializeField] private GameObject _teleportationPoint;
    [SerializeField] private Weapon _weapon;
    [SerializeField] private float _gravity = 9.81f;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _distanceToGround = 0.1f;

    private List<Key> _keys;
    private Vector3 _direction;
    private Vector3 _moveDirection;
    private bool _hasPortalKey = false;
    private int _currentHealth;
    private int _score = 0;
    private Animator _animator;
    private bool _isGrounded = true;
    private bool _canMove = true;
    private Camera _mainCamera;
    private CharacterController _characterController;
    
    public void OnAttackFinished()
    {       
        _canMove = true;
    }

    public void ApplyDamage(int value)
    {
        _canMove = true;
        _currentHealth -= value;
        float f = _currentHealth / (float)_health;
        HealthBar.SetValue(f);
        
        if (_currentHealth <= 0)
        {
            Die();
        }
    }

    private void Start()
    {
        _keys = new List<Key>();
        _characterController = GetComponent<CharacterController>();
        _animator = GetComponent<Animator>();
        _mainCamera = Camera.main;
        _currentHealth = _health;
    }

    private void FixedUpdate()
    {
        _isGrounded = Physics.CheckSphere(transform.position, _distanceToGround, _groundLayer);
        
        if (!IsDead)
        {
            SetState();
            SetRotation();

            if(transform.position.y < -20)
            {
                Die();
            }
        }
    }


    private void SetState()
    {
        float axisX = Input.GetAxis("Vertical");
        float axisZ = Input.GetAxis("Horizontal");

        _moveDirection = axisX * -transform.forward + transform.right * axisZ;
        float jumpAxis = Input.GetAxis("Jump");
        
        if (axisX < 0 && _canMove)
        {
            RunForward();
        }
        else if (axisX > 0 && _canMove)
        {
            RunBackward();
        }
        else if (axisZ > 0 && _canMove)
        {
            RunRight();
        }
        else if (axisZ < 0 && _canMove)
        {
            RunLeft();
        }
        else
        {
            Stay();
        }

        if (Input.GetKeyDown(KeyCode.Space) && _isGrounded)
        {
            _direction.y = Mathf.Sqrt(_jumpHeight * -2 * _gravity);
            _animator.SetBool("IsJumping", true);
        }
        else
        {
            _animator.SetBool("IsJumping", false);
        }

        if (Input.GetButtonDown("Fire1"))
        {
            Attack();
        }

        _characterController.Move(_moveDirection * Time.deltaTime * _forwardSpeed);
        
        _direction.y += _gravity * Time.deltaTime;
        _characterController.Move(_direction * Time.deltaTime);


    }

    private void Attack() 
    {
        
        _canMove = false;
        if(_weapon.Hits < 3)
        {
            _animator.SetTrigger("Attack");
        }
        else
        {
            _animator.SetTrigger("Ability");
            _weapon.Hits = 0;
        }
        
    }

    private void Stay() 
    {
        _moveDirection = Vector3.zero;
        _animator.SetFloat("Blend", 0f, 0.1f, Time.deltaTime);
    }

    private void Die() 
    {
        _animator.SetTrigger("DieTrigger");
        IsDead = true;
        StopAllCoroutines();
        _gameOverPanel.SetActive(true);
    }

    private void RunForward()
    {
        _animator.SetFloat("Blend", 0.25f, 0.1f, Time.deltaTime);
    }


    private void RunBackward() 
    {
        _animator.SetFloat("Blend", 0.5f, 0.1f, Time.deltaTime);
    }

    private void RunLeft()
    {
        _animator.SetFloat("Blend", 0.75f, 0.1f, Time.deltaTime);
    }

    private void RunRight() 
    {
        _animator.SetFloat("Blend", 1f, 0.1f, Time.deltaTime);
    }

  
    private void SetRotation()
    {
        Plane PlayerPlane = new Plane(Vector3.up, transform.position);

        Ray Ray = _mainCamera.ScreenPointToRay(Input.mousePosition);
        if (PlayerPlane.Raycast(Ray, out float hitdist))
        {
            Vector3 TargetPoint = Ray.GetPoint(hitdist);
            Quaternion TargetRotation = Quaternion.LookRotation(TargetPoint - transform.position);
            transform.rotation = Quaternion.Slerp(transform.rotation, TargetRotation, _forwardSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if(other.gameObject.TryGetComponent(out Coin coin))
        {
            _score += coin.Score;
            _textScore.text = _score.ToString();
            Destroy(coin.gameObject);
        }

        if(other.gameObject.TryGetComponent(out Cristal cristal))
        {
            _hasPortalKey = true;
            _portal.PlayParticles();
            Destroy(cristal.gameObject);
        }

        if (other.gameObject.TryGetComponent(out Portal portal))
        {
            if (_hasPortalKey || _score > 25) 
            {
                if(SceneManager.GetActiveScene().buildIndex + 1 == SceneManager.sceneCountInBuildSettings)
                {
                    portal.Teleport(0);
                }
                else 
                {
                    portal.Teleport(SceneManager.GetActiveScene().buildIndex + 1);
                }

            }
        }

        if (other.gameObject.TryGetComponent(out LocalPortal localPortal))
        {
            transform.position = _teleportationPoint.transform.position;
        }


        if (other.gameObject.TryGetComponent(out Health health))
        {
            if(_currentHealth != _health)
            {
                _currentHealth += health.Value;

                if(_currentHealth > _health)
                {
                    _currentHealth = _health;
                }

                float f = _currentHealth / (float)_health;
                HealthBar.SetValue(f);
               
                Destroy(health.gameObject);
            }
        }

        if (other.gameObject.TryGetComponent(out Key key))
        {
            _keys.Add(key);
         
            Destroy(key.gameObject);          
        }

        if (other.gameObject.TryGetComponent(out Fence fence))
        {
            foreach(Key k in _keys)
            {
                if(k.ID == fence.ID)
                {
                    fence.Open();
                }
            }
        }
    }
}

