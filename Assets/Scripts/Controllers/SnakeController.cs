using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using System;

public class SnakeController : MonoBehaviour
{
    [SerializeField] Sprite endTail;
    [SerializeField] Sprite normalTail;
    [SerializeField] int speed = 5;
    [SerializeField] GameObject tailGO;

    private Vector2 currentHeadDir = Vector2.right;
    private GameObject head;
    private HeadController headController;
    private Vector2 posOffset = Vector2.zero;
    private int headRot = -90;

    private Dictionary<Vector3, int> angleAtPos = new Dictionary<Vector3, int>();
    private Dictionary<Vector3, Vector2> dirAtPos = new Dictionary<Vector3, Vector2>();
    private Dictionary<Vector3, Vector2> offsetAtPos = new Dictionary<Vector3, Vector2>();

    private List<GameObject> tailParts = new List<GameObject>();

    private void Awake()
    {
        GameManager.OnStateChanged += OnStateChanged;
    }

    private void OnDestroy()
    {
        GameManager.OnStateChanged -= OnStateChanged;
        head.GetComponent<HeadController>().AteFood -= AddTail;
    }

    private void Start()
    {
        head = transform.GetChild(0).gameObject;
        headController = head.GetComponent<HeadController>();
        headController.AteFood += AddTail;
        head.GetComponent<Rigidbody2D>().velocity = currentHeadDir * speed;
    }

    private void Update()
    {
        if (GameManager.instance.state != GameState.Playing)
            return;
        GetNewDir();
    }

    private void FixedUpdate()
    {
        if (GameManager.instance.state != GameState.Playing)
            return;
        if (angleAtPos.Count != 0)
        {
            head.GetComponent<Rigidbody2D>().velocity = currentHeadDir * speed;
            RotateTailPart(head, new Vector3(0, 0, headRot));
            UpdateTailDir();
        }
        
        if(posOffset != Vector2.zero) {
            head.transform.position += (Vector3)posOffset;
            posOffset = Vector2.zero;
        }
    }

    private void OnStateChanged(GameState state)
    {
        if(state == GameState.Ending)
        {
            head.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            foreach (GameObject tail in tailParts)
            {
                tail.GetComponent<Rigidbody2D>().velocity = Vector2.zero;
            }
        }
    }

    private Vector3 ToVec3Rounded(Vector3 vec)
    {
        return new Vector3((float)Math.Round(vec.x, 4), (float)Math.Round(vec.y, 4), (float)Math.Round(vec.z, 4));
    }

    private void UpdateTailDir()
    {
        if (tailParts.Count == 0)
        {
            angleAtPos.Clear();
            dirAtPos.Clear();
            offsetAtPos.Clear();
        }

        foreach (GameObject tail in tailParts)
        {
            Vector3 posToRemove = ToVec3Rounded(tail.transform.position);
            if (angleAtPos.ContainsKey(posToRemove))
            {
                tail.GetComponent<Rigidbody2D>().velocity = dirAtPos[posToRemove] * speed;
                RotateTailPart(tail, new Vector3(0, 0, angleAtPos[posToRemove]));
                tail.transform.position += (Vector3)offsetAtPos[posToRemove];
                if (tailParts.Last() == tail)
                {
                    angleAtPos.Remove(posToRemove);
                    dirAtPos.Remove(posToRemove);
                    offsetAtPos.Remove(posToRemove);
                }
            }
        }
    }
    private void GetNewDir()
    {
        Vector2 newDir = new Vector2(-1, -1);
        Vector3 headVec = head.transform.position;
        if (Input.GetKeyDown(KeyCode.RightArrow) && currentHeadDir != Vector2.right && currentHeadDir != Vector2.left) { 
            dirAtPos.Add(ToVec3Rounded(headVec), Vector2.right);
            headRot = -90;
            posOffset = new Vector2(.2f, 0);
            newDir =  Vector2.right;
        } else if(Input.GetKeyDown(KeyCode.LeftArrow) && currentHeadDir != Vector2.right && currentHeadDir != Vector2.left) {
            dirAtPos.Add(ToVec3Rounded(headVec), Vector2.left);
            posOffset = new Vector2(-.2f, 0);
            headRot = 90;
            newDir = Vector2.left;
        } else if(Input.GetKeyDown(KeyCode.UpArrow) && currentHeadDir != Vector2.up && currentHeadDir != Vector2.down) {
            dirAtPos.Add(ToVec3Rounded(headVec), Vector2.up);
            posOffset = new Vector2(0, .2f);
            headRot = 0;
            newDir = Vector2.up;
        } else if (Input.GetKeyDown(KeyCode.DownArrow) && currentHeadDir != Vector2.up && currentHeadDir != Vector2.down) {
            dirAtPos.Add(ToVec3Rounded(headVec), Vector2.down);
            posOffset = new Vector2(0, -.2f);
            headRot = 180;
            newDir = Vector2.down;
        }
        if(newDir != new Vector2(-1, -1))
        {
            currentHeadDir = newDir;
            angleAtPos.Add(ToVec3Rounded(headVec), headRot);
            offsetAtPos.Add(ToVec3Rounded(headVec), posOffset);
        }
    }

    private void RotateTailPart(GameObject tail, Vector3 newRot)
    {
        tail.GetComponent<Transform>().localRotation = Quaternion.Euler(newRot);
    }

    private void AddTail()
    {
        Vector3 lastTailPosition;
        GameObject newTail = Instantiate(tailGO);
        GameObject lastTailRef;

        newTail.gameObject.transform.SetParent(transform);

        if (tailParts.Count == 0)
        {
            lastTailRef = head;
        } else
        {
            tailParts.Last().GetComponent<SpriteRenderer>().sprite = normalTail;
            lastTailRef = tailParts.Last();
        }

        newTail.GetComponent<SpriteRenderer>().sprite = endTail;
        lastTailPosition = lastTailRef.transform.localPosition;
        Vector2 lastTailRefVelocity = lastTailRef.GetComponent<Rigidbody2D>().velocity.normalized;

        newTail.gameObject.transform.localPosition = new Vector3(lastTailPosition.x + (1.2f * - lastTailRefVelocity.x), lastTailPosition.y + (1.2f * - lastTailRefVelocity.y), lastTailPosition.z);
        
        newTail.gameObject.transform.localScale = lastTailRef.transform.localScale;
        newTail.gameObject.transform.localRotation = lastTailRef.transform.localRotation;

        newTail.GetComponent<Rigidbody2D>().velocity = lastTailRef.GetComponent<Rigidbody2D>().velocity;

        tailParts.Add(newTail);
    }
}
