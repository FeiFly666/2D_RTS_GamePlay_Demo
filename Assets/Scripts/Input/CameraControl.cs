using System.Collections;
using UnityEditor;
using UnityEngine;


public class CameraControl : MonoBehaviour
{
    private Camera cam;
    private float originZ;
    private Vector3 lastMousePos;
    [Header("通用设置")]
    [SerializeField] private float cameraMoveSpeed = 15f;
    [SerializeField] private float dragSpeed = 1.5f;

    [Header("边缘滚动设置")]
    [SerializeField] private bool useEdgeScrolling = true;
    [SerializeField] private int edgeSize = 20;

    [Header("缩放设置")]
    [SerializeField] private float zoomSpeed = 8f;
    [SerializeField] private float minZoom = 4f;
    [SerializeField] private float maxZoom = 20f;

    [Header("地图边界限制")]
    [SerializeField] private Vector2 minBounds;
    [SerializeField] private Vector2 maxBounds;

    private void Awake()
    {
        cam = Camera.main;
        originZ = cam.transform.position.z;
    }
    void Start()
    {
        BoundsInt walkableAreaBound = TilemapManager.Instance.WalkableTilemap.cellBounds;

        minBounds = new Vector2(walkableAreaBound.xMin, walkableAreaBound.yMin);

        maxBounds = new Vector2(walkableAreaBound.xMax, walkableAreaBound.yMax);
    }

    // Update is called once per frame
    void Update()
    {

    }
    private void LateUpdate()
    {
        if (!MyInputsystem.Instance.isGameStart) return;
        HandleMovement();
        HandleZoom();
        //ClampPosition();

    }
    private void HandleMovement()//边缘移动+中键移动
    {
        Vector3 finalMove = Vector3.zero;
        if (Input.GetMouseButtonDown(2))
        {
            lastMousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        }

        if(Input.GetMouseButton(2))
        {
            Vector3 currentPos = cam.ScreenToWorldPoint(Input.mousePosition);
            Vector3 mouseDelta = lastMousePos - currentPos;

            cam.gameObject.transform.position += mouseDelta;

            ClampPosition();
            lastMousePos = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        
        else
        {
            float h = Input.GetAxisRaw("Horizontal");
            float v = Input.GetAxisRaw("Vertical"); 
            Vector3 kbInput = new Vector3(h, v, 0);

            if (kbInput.sqrMagnitude > 0.01f)
            {
                finalMove = kbInput.normalized * cameraMoveSpeed * Time.deltaTime;
            }
            else
            {
                if (useEdgeScrolling && !CommonUtils.IsPointOverUIElement())
                {
                    Vector3 mousePos = Input.mousePosition;

                    if (mousePos.x <= edgeSize) finalMove.x -= 1;
                    else if (mousePos.x >= Screen.width - edgeSize) finalMove.x += 1;

                    if (mousePos.y <= edgeSize) finalMove.y -= 1;
                    else if (mousePos.y >= Screen.height - edgeSize) finalMove.y += 1;
                }
            }
            if (finalMove != Vector3.zero)
            {
                if (finalMove.sqrMagnitude <= 2.1f)
                {
                    finalMove = finalMove.normalized * cameraMoveSpeed * Time.deltaTime;
                }

                cam.transform.position += finalMove;
                ClampPosition();
                lastMousePos = cam.ScreenToWorldPoint(Input.mousePosition);
            }
        }


    }
    private void HandleZoom()//缩放
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (Mathf.Abs(scroll) > 0.01f)
        {
            cam.orthographicSize -= scroll * zoomSpeed;
            cam.orthographicSize = Mathf.Clamp(cam.orthographicSize, minZoom, maxZoom);
            ClampPosition();
        }
    }

    public void FocusOn(Vector3 pos)
    {
        transform.position = pos;
        ClampPosition();
    }
    private void ClampPosition()//限制位置
    {
        float camHeight = cam.orthographicSize;
        float camWidth = camHeight * cam.aspect;

        Vector3 pos = transform.position;

        pos.x = Mathf.Clamp(pos.x, minBounds.x + camWidth, maxBounds.x - camWidth);
        pos.y = Mathf.Clamp(pos.y, minBounds.y + camHeight, maxBounds.y - camHeight);
        pos.z = this.originZ;

        transform.position = pos;

    }
}