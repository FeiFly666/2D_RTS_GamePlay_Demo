using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Tilemaps;
using UnityEngine.UI;

namespace Assets.Scripts.Input
{
    public class MinimapControl : MonoBehaviour, IPointerClickHandler
    {

        [Header("相关配置")]
        [SerializeField] private RectTransform viewFrame;
        [SerializeField] private Camera minimapCam;

        private RawImage minimapImage;
        private RectTransform minimapRect;

        private Camera mainCam;
        private CameraControl mainCamControl;

        private Vector2 worldMin;
        private Vector2 worldSize;

        private Vector2 miniWorldMin;
        private Vector2 miniWorldSize;
        // Use this for initialization
        void Awake()
        {
            minimapImage = GetComponent<RawImage>();
            minimapRect = GetComponent<RectTransform>();

            mainCam = Camera.main;
            mainCamControl = mainCam.gameObject.GetComponent<CameraControl>();
        }
        private void Start()
        {
            var bounds = TilemapManager.Instance.WalkableTilemap.localBounds;

            worldMin = new Vector2(bounds.min.x, bounds.min.y);
            worldSize = new Vector2(bounds.size.x, bounds.size.y);

            SetMiniMapCam();
        }
        private void SetMiniMapCam()
        {
            if (minimapCam == null) return;

            Vector3 center = new Vector3(worldMin.x + worldSize.x / 2f, worldMin.y + worldSize.y / 2f, -100f);
            minimapCam.transform.position = center;

            float worldAspect = worldSize.x / worldSize.y;
            float screenAspect = minimapRect.rect.width / minimapRect.rect.height;

            if (screenAspect >= worldAspect)
            {
                //UI比地图更宽以高度为准
                minimapCam.orthographicSize = worldSize.y / 2f;
            }
            else
            {
                //UI比地图更高以宽度为准
                float size = worldSize.x / screenAspect / 2f;
                minimapCam.orthographicSize = size;
            }

            float camHeight = minimapCam.orthographicSize * 2f;
            float camWidth = camHeight * minimapCam.aspect;

            miniWorldSize = new Vector2(camWidth, camHeight);

            miniWorldMin = new Vector2(minimapCam.transform.position.x - camWidth * 0.5f,minimapCam.transform.position.y - camHeight * 0.5f);

        }
        public void OnPointerClick(PointerEventData eventData)
        {
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(minimapRect, eventData.position, eventData.pressEventCamera, out Vector2 localPoint))
            {
                float width = minimapRect.rect.width;
                float height = minimapRect.rect.height;

                float xPct = (localPoint.x + width * 0.5f) / width;
                float yPct = (localPoint.y + height * 0.5f) / height;

                float targetWorldX = miniWorldMin.x + xPct * miniWorldSize.x;
                float targetWorldY = miniWorldMin.y + yPct * miniWorldSize.y;

                Vector3 targetPos = new Vector3(targetWorldX, targetWorldY, mainCam.transform.position.z);

                mainCamControl.FocusOn(targetPos);
            }
        }
        // Update is called once per frame
        void Update()
        {
            UpdateViewFrame();
        }
        private void UpdateViewFrame()
        {
            if (viewFrame == null) return;

            float mapWidth = minimapRect.rect.width;
            float mapHeight = minimapRect.rect.height;

            float camHeight = mainCam.orthographicSize * 2f;
            float camWidth = camHeight * mainCam.aspect;

            float camLeft = mainCam.transform.position.x - camWidth * 0.5f;
            float camBottom = mainCam.transform.position.y - camHeight * 0.5f;

            float leftRatio = (camLeft - miniWorldMin.x) / miniWorldSize.x;
            float bottomRatio = (camBottom - miniWorldMin.y) / miniWorldSize.y;

            float frameWidth = mapWidth * (camWidth / miniWorldSize.x);
            float frameHeight = mapHeight * (camHeight / miniWorldSize.y);

            viewFrame.sizeDelta = new Vector2(frameWidth, frameHeight);

            float uiX = leftRatio * mapWidth + frameWidth * 0.5f - mapWidth * 0.5f;
            float uiY = bottomRatio * mapHeight + frameHeight * 0.5f - mapHeight * 0.5f;

            viewFrame.anchoredPosition = new Vector2(uiX, uiY);
        }


    }
}