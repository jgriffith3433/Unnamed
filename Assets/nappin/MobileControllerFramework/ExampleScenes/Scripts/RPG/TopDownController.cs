using UnityEngine;


namespace MobileControllerFramework
{
    [RequireComponent(typeof(Collider2D))]
    [RequireComponent(typeof(Rigidbody2D))]
    public class TopDownController : MonoBehaviour
    {
        [Header("Movement")]
        public Vector2 speed = Vector2.one;
        public float dashSpeed = 25f;
        public float dashDuration = 0.2f; // how long the dash lasts
        public float placeRadius = 3f;

        [Header("References")]
        public GameObject swordIn;
        public GameObject swordOut;
        public GameObject ballObj;
        public GameObject dustPrt;
        public Analog analog;
        public Dpad dpad;
        public ArrowKeys arrowKeys;
        public Animator anim;

        private Vector3 facingRight = Vector3.one;
        private Vector3 facingLeft = new Vector3(-1, 1, 1);

        private Rigidbody2D rb;
        private Vector2Int input;
        private GameObject ballRef;

        private bool canWalk = true;
        private bool moveBall = false;
        private bool isDashing = false;


        /**/


        #region Setup

        private void Start()
        {
            rb = GetComponent<Rigidbody2D>();
        }

        #endregion


        #region Update & Movement

        private void Update()
        {
            HandleInput();
            HandleFacing();
            HandleAnimation();

            if (moveBall)
                UpdateBallPosition();
        }


        private void FixedUpdate()
        {
            if (canWalk && !isDashing)
            {
                rb.linearVelocity = new Vector2(input.x * speed.x, input.y * speed.y);
            }
        }


        private void HandleInput()
        {
            input = Vector2Int.zero;

            if (dpad != null && dpad.gameObject.activeSelf)
                input = dpad.GetDpadDirection();
            else if (arrowKeys != null && arrowKeys.gameObject.activeSelf)
                input = arrowKeys.GetDirection();
        }


        private void HandleFacing()
        {
            if (input.x > 0) transform.localScale = facingRight;
            else if (input.x < 0) transform.localScale = facingLeft;
        }


        private void HandleAnimation()
        {
            bool isMoving = input.x != 0 || input.y != 0;

            if (anim != null)
                anim.Play(isMoving ? "Walk" : "Idle");

            if (dustPrt != null)
                dustPrt.SetActive(isMoving);
        }

        #endregion


        #region Dash

        public void DashLeft()
        {
            if (!canWalk || isDashing) return;
            StartCoroutine(Dash(Vector2.left, facingLeft));
        }


        public void DashRight()
        {
            if (!canWalk || isDashing) return;
            StartCoroutine(Dash(Vector2.right, facingRight));
        }


        private System.Collections.IEnumerator Dash(Vector2 direction, Vector3 facing)
        {
            isDashing = true;
            canWalk = false;

            // Face the right way
            transform.localScale = facing;

            // Reset velocity for consistent dash
            rb.linearVelocity = Vector2.zero;

            // Apply dash force
            rb.AddForce(direction * dashSpeed, ForceMode2D.Impulse);

            // Dash duration
            yield return new WaitForSeconds(dashDuration);

            // Stop dash movement smoothly
            rb.linearVelocity = Vector2.zero;

            isDashing = false;
            canWalk = true;
        }

        #endregion


        #region Sword

        public void ShowSword()
        {
            if (swordIn != null) swordIn.SetActive(false);
            if (swordOut != null) swordOut.SetActive(true);
        }


        public void HideSword()
        {
            if (swordIn != null) swordIn.SetActive(true);
            if (swordOut != null) swordOut.SetActive(false);
        }

        #endregion


        #region Ball

        public void InstantiateBall()
        {
            if (ballObj == null) return;

            ballRef = Instantiate(ballObj, transform.position, Quaternion.identity);
            moveBall = true;
        }


        private void UpdateBallPosition()
        {
            if (ballRef == null || analog == null) return;

            ballRef.transform.position = transform.position +
                                         (Vector3)analog.GetStickPosition() * placeRadius;
        }


        public void PlaceBall()
        {
            ballRef = null;
            moveBall = false;
        }

        #endregion
    }
}