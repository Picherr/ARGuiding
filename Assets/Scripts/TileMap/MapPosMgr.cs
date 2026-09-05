using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// 处理二维地图的平移、缩放和回到当前位置交互。
/// 地图瓦片位于 UI 坐标系，路线由正交相机渲染，因此两者需要同步移动和缩放。
/// </summary>
public class MapPosMgr : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IScrollHandler
{
    [SerializeField]
    private GameObject Map;

    [SerializeField]
    [Range(0.75f, 1f)]
    private float minZoom = 0.85f;

    [SerializeField]
    [Range(1f, 3f)]
    private float maxZoom = 2f;

    [SerializeField]
    private float pinchSensitivity = 0.004f;

    private RectTransform mapRect;
    private LocationMap locationMap;
    private Canvas parentCanvas;
    private Camera routeCamera;
    private Vector3 baseMapScale;
    private Vector3 baseRouteCameraPosition;
    private float baseRouteCameraSize;
    private float zoom = 1f;
    private Vector2 pendingPan;
    private bool isDragging;
    private bool routeCameraInitialized;

    private void Start()
    {
        GameObject mapObject = Map != null ? Map : gameObject;
        mapRect = mapObject.GetComponent<RectTransform>();
        locationMap = mapObject.GetComponent<LocationMap>();
        parentCanvas = GetComponentInParent<Canvas>();
        baseMapScale = mapRect != null ? mapRect.localScale : Vector3.one;

        EventCenter.GetInstance().AddEventListener(EventName.LocationUpdated, OnLocationUpdated);
        TryResolveRouteCamera();
        ApplyZoom(1f);
    }

    private void Update()
    {
        TryResolveRouteCamera();

        if (Input.touchCount != 2 || mapRect == null)
        {
            return;
        }

        Touch first = Input.GetTouch(0);
        Touch second = Input.GetTouch(1);
        if (!IsTouchOverMap(first.position) || !IsTouchOverMap(second.position))
        {
            return;
        }

        Vector2 previousFirst = first.position - first.deltaPosition;
        Vector2 previousSecond = second.position - second.deltaPosition;
        float previousDistance = Vector2.Distance(previousFirst, previousSecond);
        float currentDistance = Vector2.Distance(first.position, second.position);
        float canvasScale = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
        float delta = (currentDistance - previousDistance) / Mathf.Max(canvasScale, 0.01f);
        ApplyZoom(zoom + delta * pinchSensitivity);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (Input.touchCount < 2)
        {
            isDragging = true;
        }
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging || Input.touchCount >= 2 || mapRect == null || locationMap == null)
        {
            return;
        }

        float canvasScale = parentCanvas != null ? parentCanvas.scaleFactor : 1f;
        Vector2 delta = eventData.delta / Mathf.Max(canvasScale, 0.01f);
        mapRect.anchoredPosition += delta;
        pendingPan += delta;

        RecycleTilesAcrossBoundaries();
        SyncRouteCamera();
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    public void OnScroll(PointerEventData eventData)
    {
        ApplyZoom(zoom + eventData.scrollDelta.y * 0.12f);
    }

    /// <summary>
    /// 回到最新定位点，并保留用户当前的缩放级别。
    /// </summary>
    public void Recenter()
    {
        if (mapRect == null || locationMap == null)
        {
            return;
        }

        mapRect.anchoredPosition = Vector2.zero;
        pendingPan = Vector2.zero;
        locationMap.RecenterOnCurrentLocation();
        SyncRouteCamera();
    }

    private void OnLocationUpdated(object sender, System.EventArgs e)
    {
        LocationUpdatedEventArgs data = e as LocationUpdatedEventArgs;
        if (data == null || !data.recenterRequested || mapRect == null)
        {
            return;
        }

        mapRect.anchoredPosition = Vector2.zero;
        pendingPan = Vector2.zero;
        SyncRouteCamera();
    }

    private void ApplyZoom(float requestedZoom)
    {
        if (mapRect == null)
        {
            return;
        }

        float nextZoom = Mathf.Clamp(requestedZoom, minZoom, maxZoom);
        if (Mathf.Approximately(nextZoom, zoom) && mapRect.localScale == baseMapScale * nextZoom)
        {
            return;
        }

        zoom = nextZoom;
        mapRect.localScale = baseMapScale * zoom;
        RecycleTilesAcrossBoundaries();
        SyncRouteCamera();
    }

    private void RecycleTilesAcrossBoundaries()
    {
        if (locationMap == null)
        {
            return;
        }

        float tileSize = LocationMap.TileWidthAndHeigth * LocationMap.TileScale * zoom;
        if (tileSize <= 0f)
        {
            return;
        }

        int safetyCounter = 0;
        while (pendingPan.x >= tileSize && safetyCounter++ < 32)
        {
            locationMap.MapUpdate(Direction.left);
            pendingPan.x -= tileSize;
        }
        while (pendingPan.x <= -tileSize && safetyCounter++ < 32)
        {
            locationMap.MapUpdate(Direction.right);
            pendingPan.x += tileSize;
        }
        while (pendingPan.y <= -tileSize && safetyCounter++ < 32)
        {
            locationMap.MapUpdate(Direction.up);
            pendingPan.y += tileSize;
        }
        while (pendingPan.y >= tileSize && safetyCounter++ < 32)
        {
            locationMap.MapUpdate(Direction.down);
            pendingPan.y -= tileSize;
        }
    }

    private bool IsTouchOverMap(Vector2 screenPosition)
    {
        Camera eventCamera = parentCanvas != null && parentCanvas.renderMode != RenderMode.ScreenSpaceOverlay
            ? parentCanvas.worldCamera
            : null;
        return RectTransformUtility.RectangleContainsScreenPoint(mapRect, screenPosition, eventCamera);
    }

    private void TryResolveRouteCamera()
    {
        if (routeCameraInitialized)
        {
            return;
        }

        GameObject routeCameraObject = GameObject.Find("RouteCamera");
        if (routeCameraObject == null)
        {
            return;
        }

        routeCamera = routeCameraObject.GetComponent<Camera>();
        if (routeCamera == null)
        {
            return;
        }

        baseRouteCameraPosition = routeCamera.transform.position;
        baseRouteCameraSize = routeCamera.orthographicSize;
        routeCameraInitialized = true;
        SyncRouteCamera();
    }

    private void SyncRouteCamera()
    {
        if (!routeCameraInitialized || routeCamera == null || mapRect == null)
        {
            return;
        }

        Vector2 offset = mapRect.anchoredPosition / Mathf.Max(zoom, 0.01f);
        routeCamera.transform.position = baseRouteCameraPosition + new Vector3(-offset.x, 0f, -offset.y);
        routeCamera.orthographicSize = baseRouteCameraSize / Mathf.Max(zoom, 0.01f);
    }

    private void OnDestroy()
    {
        EventCenter.GetInstance().RemoveEventListener(EventName.LocationUpdated, OnLocationUpdated);
    }
}
