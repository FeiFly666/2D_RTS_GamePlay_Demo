using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

namespace Assets.Scripts.Manager
{
    public class VisualManager : MonoSingleton<VisualManager>
    {
        [SerializeField] private GameObject MovementRayPrefab;
        [SerializeField] private RectTransform selectionBox;

        private List<MovementRay> MovementRays = new List<MovementRay>();

        private Pointer pointer;
        void Start()
        {
            GameObject pointer = Resources.Load<GameObject>("Prefab/Pointer");

            GameObject pointerObject = Instantiate(pointer, this.transform);

            this.pointer = pointerObject.GetComponent<Pointer>();

            this.pointer.ClosePointer();
        }

        public void DrawSelectionBox(Vector2 start, Vector2 end)
        {
            if (!selectionBox.gameObject.activeSelf)
            {
                selectionBox.gameObject.SetActive(true);
            }

            float minX = Mathf.Min(start.x, end.x);
            float minY = Mathf.Min(start.y, end.y);

            float maxX = Mathf.Max(start.x, end.x);
            float maxY = Mathf.Max(start.y, end.y);

            //设置位置为左下角
            selectionBox.position = new Vector3(minX, minY, selectionBox.position.z);

            float width = maxX - minX;
            float height = maxY - minY;

            selectionBox.sizeDelta = new Vector2(width, height);
        }

        public void HideSelectionBox()
        {
            selectionBox.sizeDelta = Vector2.zero;
            selectionBox.gameObject.SetActive(false);
        }

        public void UpdateMovementRay(Vector2 mousePos,Unit owner)
        {
            //ClearRays();
            GameObject rayGO = Instantiate(MovementRayPrefab, owner.transform);
            rayGO.name = "MovementRay";
            rayGO.GetComponent<MovementRay>().RayInit(owner.transform, mousePos);
            MovementRays.Add(rayGO.GetComponent<MovementRay>());
        }

        public void UpdatePointer(Vector2 mousePos,Unit owner, PointerMode pointerMode)
        {
            pointer.ClosePointer();

            SetPointer(mousePos,owner ,pointerMode);
        }


        private void SetPointer(Vector2 position, Unit owner, PointerMode pointerMode)
        {
            pointer.SetPointerMode(pointerMode);

            pointer.SetPointPositionAndShow(position, owner);
        }
        private void ClearRays()
        {
            if(MovementRays.Count > 0)
            {
                for (int i = MovementRays.Count - 1; i >= 0; i--)
                {
                    var ray = MovementRays[i];
                    if (ray != null)
                    {
                        Destroy(ray.gameObject);
                    }
                }
                MovementRays.Clear(); ;
            }
        }

        public void ClearAll()
        {
            ClearRays();
            this.pointer.ClosePointer();
        }
    }
}