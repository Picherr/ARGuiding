using UnityEngine;
using UnityEngine.EventSystems;

public class MapPosMgr : MonoBehaviour//, IDragHandler, IBeginDragHandler, IEndDragHandler
{
    [SerializeField]
    private GameObject Map;

    //上次拖拽的位置
    Vector2 DragPos;
    private bool isDrag = false;

    //更新 地图的 位置边界
    private float lx;
    private float rx;
    private float ty;
    private float by;

    //每次位置变化大小
    private float changeSzie;

    //是否移动
    //private float DragFlag = 50f;

    //地图
    private LocationMap map;

    //渲染瓦片地图的相机
    private GameObject routeCamera;

    private void Start()
    {
        map = Map.GetComponent<LocationMap>();

        routeCamera = GameObject.Find("RouteCamera");

        changeSzie = LocationMap.TileWidthAndHeigth * LocationMap.TileScale;

        //初始化 边界
        lx = changeSzie;
        rx = -changeSzie;
        ty = -changeSzie;
        by = changeSzie;

        MonoMgr.GetInstance().AddUpdateListener(mapUpdate);
    }

    private void mapUpdate()
    {
        //注意 没有滑动时 不计算
        if (!isDrag) return;

        if (Map.transform.localPosition.x > lx)
        {
            map.MapUpdate(Direction.left);
            lx += changeSzie;
            rx += changeSzie;
        }
        if (Map.transform.localPosition.x < rx)
        {
            map.MapUpdate(Direction.right);
            rx -= changeSzie;
            lx -= changeSzie;
        }
        if (Map.transform.localPosition.y < ty)
        {
            map.MapUpdate(Direction.up);
            ty -= changeSzie;
            by -= changeSzie;

        }
        if (Map.transform.localPosition.y > by)
        {
            map.MapUpdate(Direction.down);
            by += changeSzie;
            ty += changeSzie;
        }
    }

    /*public void OnBeginDrag(PointerEventData eventData)
    {
        DragPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (Input.touchCount == 2) return;

        Vector2 offset = (eventData.position - DragPos).normalized;
        Map.transform.position += new Vector3(offset.x, offset.y, 0) * DragFlag;
        routeCamera.transform.position -= new Vector3(offset.x, 0, offset.y) * DragFlag;
        DragPos = eventData.position;
        isDrag = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDrag = false;
    }*/
}