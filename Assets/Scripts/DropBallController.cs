using System;
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(CircleCollider2D))]
[RequireComponent(typeof(Ball))]
public sealed class DropBallController : MonoBehaviour
{
    private GameManager gameManager;
    
    private Rigidbody2D ballRigidbody;
    private CircleCollider2D ballCollider;
    private Camera mainCamera;

    private PlayAreaBounds playAreaBounds;

    private float aimingY;
    private bool isAiming;
    private bool pointerWasPressed;
    private Ball _ball;
    
    private static readonly List<RaycastResult>
        UiRaycastResults = new List<RaycastResult>();

    public event Action<DropBallController> Dropped;

    private void Awake()
    {
        ballRigidbody = GetComponent<Rigidbody2D>();
        ballCollider = GetComponent<CircleCollider2D>();
        _ball = GetComponent<Ball>();
        
        mainCamera = Camera.main;
    }

    public void SetGameManager(
        GameManager value
    )
    {
        gameManager = value;
    }
    
    public bool BeginAiming(
        PlayAreaBounds bounds
    )
    {
        if (bounds == null || !bounds.IsValid)
        {
            Debug.LogError(
                "Некорректный PlayAreaBounds.",
                this
            );

            return false;
        }

        if (mainCamera == null)
        {
            Debug.LogError(
                "Не найдена камера с тегом MainCamera.",
                this
            );

            return false;
        }
        
        if (gameManager == null)
        {
            Debug.LogError(
                "В DropBallController не передан GameManager.",
                this
            );

            return false;
        }

        playAreaBounds = bounds;

        aimingY = transform.position.y;
        isAiming = true;
        pointerWasPressed = false;

        ballRigidbody.bodyType =
            RigidbodyType2D.Dynamic;

        ballRigidbody.linearVelocity =
            Vector2.zero;

        ballRigidbody.angularVelocity = 0f;
        ballRigidbody.simulated = false;

        enabled = true;

        return true;
    }

    public void DisableControl()
    {
        isAiming = false;
        pointerWasPressed = false;

        Dropped = null;
        enabled = false;
    }

    private void Update()
    {
        if (!isAiming ||
            Pointer.current == null)
        {
            return;
        }

        /*
         * Во время паузы забываем любое удерживание,
         * которое могло начаться раньше.
         */
        if (gameManager == null ||
            !gameManager.CanUseGameplayInput)
        {
            pointerWasPressed = false;
            return;
        }

        Pointer pointer = Pointer.current;

        if (pointer.press.wasPressedThisFrame)
        {
            Vector2 screenPosition =
                pointer.position.ReadValue();

            /*
             * Если нажатие началось над кнопкой или
             * другим UI, шар не получает управление.
             */
            if (IsPointerOverUI(screenPosition))
            {
                pointerWasPressed = false;
                return;
            }

            pointerWasPressed = true;
        }

        if (pointerWasPressed &&
            pointer.press.isPressed)
        {
            Vector2 screenPosition =
                pointer.position.ReadValue();

            MoveToPointer(screenPosition);
        }

        if (pointerWasPressed &&
            pointer.press.wasReleasedThisFrame)
        {
            Drop();
        }
    }
    
    private static bool IsPointerOverUI(
        Vector2 screenPosition
    )
    {
        if (EventSystem.current == null)
        {
            return false;
        }

        PointerEventData eventData =
            new PointerEventData(
                EventSystem.current
            )
            {
                position = screenPosition
            };

        UiRaycastResults.Clear();

        EventSystem.current.RaycastAll(
            eventData,
            UiRaycastResults
        );

        return UiRaycastResults.Count > 0;
    }

    private void MoveToPointer(
        Vector2 screenPosition
    )
    {
        float distanceFromCamera = Mathf.Abs(
            mainCamera.transform.position.z -
            transform.position.z
        );

        Vector3 worldPosition =
            mainCamera.ScreenToWorldPoint(
                new Vector3(
                    screenPosition.x,
                    screenPosition.y,
                    distanceFromCamera
                )
            );

        float scaleX =
            Mathf.Abs(transform.lossyScale.x);

        float scaleY =
            Mathf.Abs(transform.lossyScale.y);

        float worldRadius =
            ballCollider.radius *
            Mathf.Max(scaleX, scaleY);

        float minimumX =
            playAreaBounds.LeftX + worldRadius;

        float maximumX =
            playAreaBounds.RightX - worldRadius;

        float newX = Mathf.Clamp(
            worldPosition.x,
            minimumX,
            maximumX
        );

        transform.position = new Vector3(
            newX,
            aimingY,
            transform.position.z
        );
    }

    private void Drop()
    {
        if (!isAiming)
        {
            return;
        }

        isAiming = false;
        pointerWasPressed = false;
        
        _ball.MarkReleased();

        ballRigidbody.simulated = true;

        Action<DropBallController> handlers =
            Dropped;

        Dropped = null;
        enabled = false;

        handlers?.Invoke(this);
    }
}