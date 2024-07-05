using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Stickman : MonoBehaviour
{
    [Header("Sprite Player")]
    [SerializeField] Sprite ballSprite;
    [SerializeField] Sprite stopSprite;
    [SerializeField] Sprite goSprite;
    [SerializeField] Sprite backSprite;
    [SerializeField] Sprite winSprite;
    
    [Header("Componets")]
    private HingeJoint2D hJoint;
    private Rigidbody2D rb;
    private LineRenderer lineRenderer;
    private SpriteRenderer spriteRenderer;

    [Header("Anchor")]
    [SerializeField] private GameObject anchor;

    [Header("Variable private")]
    private int lastBestPosJoint;
    private int lastBestPosSelected;
    private int touches;
    private int bestPos;
    private float bestDistance;
    private Vector3 actualJointPos;


    [Header("public variable")]
    [SerializeField] private float GravityRope = 2f;
    [SerializeField] private float GravityAir = 0.5f;
    [SerializeField] private float factorX = 1.2f;
    [SerializeField] private float factorY = 1f;


    [Header("Bool")]
    private bool sticked =false;


    private void Start()
    {
        hJoint = GetComponent<HingeJoint2D>();
        rb = GetComponent<Rigidbody2D>();
        lineRenderer = GetComponent<LineRenderer>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        
        //initialize
        lastBestPosJoint = 0;
        lastBestPosSelected = 0;
        touches = 0;

        anchor.transform.GetChild(lastBestPosSelected).gameObject.GetComponent<JointAnchor>().Selected();
    }

    private void Update()
    {
        bestPos = 0;
        bestDistance = float.MaxValue;

        //in current scene we have two joints
        for(int i = 0; i < anchor.transform.childCount; i++)
        {
            
            float actualDistance = Vector2.Distance(gameObject.transform.position, anchor.transform.GetChild(i).transform.position);
            if(actualDistance < bestDistance)
            {
                bestPos = i;
                bestDistance = actualDistance;
            }
        }
            
        CheckInput();

        if(sticked)
        {
            lineRenderer.SetPosition(0, gameObject.transform.position);
            lineRenderer.SetPosition(1, actualJointPos);


            //change the sprite 
            ChangeSprite();
        }

        if(lastBestPosSelected != bestPos)
        {
            anchor.transform.GetChild(lastBestPosSelected).gameObject.GetComponent<JointAnchor>().Unselected();
            anchor.transform.GetChild(bestPos).gameObject.GetComponent<JointAnchor>().Selected();
        }
        lastBestPosSelected = bestPos;
    }

    private void CheckInput()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.Space) || ((Input.touchCount > 0) && (touches == 0)))
        {
            lineRenderer.enabled = true; 
            hJoint.enabled = true;
            rb.gravityScale = GravityRope;


            //connect the joint rb to hinge
            hJoint.connectedBody = anchor.transform.GetChild(bestPos).transform.GetChild(0).gameObject.GetComponent<Rigidbody2D>();
            actualJointPos = anchor.transform.GetChild(bestPos).gameObject.transform.position;
            anchor.transform.GetChild(bestPos).gameObject.GetComponent<JointAnchor>().SetSticked();
            anchor.transform.GetChild(bestPos).gameObject.GetComponent<JointAnchor>().Unselected();

            lastBestPosJoint = bestPos;
            rb.angularVelocity = 0f;
            sticked = !sticked;
        }

        if (Input.GetMouseButtonUp(0) || Input.GetKeyUp(KeyCode.Space) || ((Input.touchCount == 0) && (touches > 0)))
        {
            lineRenderer.enabled = false;
            hJoint.enabled = false;
            rb.velocity = new Vector2 (rb.velocity.x * factorX, rb.velocity.y + factorY);
            rb.gravityScale = GravityAir;

            anchor.transform.GetChild(lastBestPosJoint).gameObject.GetComponent<JointAnchor>().SetUnsticked();

            if (bestPos == lastBestPosJoint)
            {
                anchor.transform.GetChild(bestPos).gameObject.GetComponent<JointAnchor>().Selected();
                anchor.transform.GetChild(lastBestPosSelected).gameObject.GetComponent<JointAnchor>().Unselected();
            }

            spriteRenderer.sprite = ballSprite;
            rb.AddTorque(-rb.velocity.magnitude);
            sticked = !sticked;
        }

        touches = Input.touchCount;
    }

    public void ChangeSprite()
    {
        //play is loocking/moving right
        //by default our player is loocking right all the sprites
        //so this is for flip the sprite
        if(rb.velocity.x > 0f)
        {
            spriteRenderer.flipX = false;
        }
        else //for left
        {
            spriteRenderer.flipX = true;
        }

        if(rb.velocity.x < 0.7f && rb.velocity.x > -0.7f && gameObject.transform.position.y < actualJointPos.y)
        {
            spriteRenderer.sprite = stopSprite;
        }
        else
        {
            if(rb.velocity.y < 0)
            {
                spriteRenderer.sprite = goSprite;
            }
            else
            {
                spriteRenderer.sprite = backSprite;
            }
        }

        gameObject.transform.eulerAngles = LookAt2d(actualJointPos - gameObject.transform.position);
    }

    public Vector3 LookAt2d(Vector3 vec)
    {
        return new Vector3(gameObject.transform.eulerAngles.x, gameObject.transform.eulerAngles.y, Vector2.SignedAngle(Vector2.up, vec));
    }
}
