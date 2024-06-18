using UnityEngine;

[SerializeField]
public class MultiPathScript : MonoBehaviour
{
    private Vector3[] directions = new Vector3[4] { Vector3.forward, Vector3.right, -Vector3.forward, Vector3.left };

    [Tooltip("màu sắc của 1 waypoint")]
    public Color ColorOfPoint;
    [Tooltip("màu sắc của đường nối waypoint")]
    public Color ColorOfWay;
    [Tooltip("độ lớn của hình biểu diễn cho waypoint")]
    public float pointScale = 1.5f;
    [Tooltip("độ dài khi spawm 1 gameObject")]
    public float distance = 30f;
    [Tooltip("đặt là true nếu khi xóa 1 nhánh đi các nhánh khác không có cha là gốc sẽ bị xóa")]
    public bool haveRoot = false;
    [Tooltip("số lượng các phần tử hiện tại")]
    public int WayPointObjs;
    [Tooltip("độ rộng của đường")]
    public float WayWidth;

    private void OnDrawGizmos()
    {
        Gizmos.color = ColorOfPoint;
        if (transform.childCount == 0)
        {
            Debug.Log("đã reset lại");
            WayPoint.NewWayPoint(transform.position, transform);
            WayPointObjs = transform.childCount;
        }
        if (transform.childCount != WayPointObjs)
        {
            int m = transform.childCount;
            for (int i = 0; i < m; i++)
            {
                var p = transform.GetChild(i).GetComponent<WayPoint>();
                p.gameObject.name = "Waypoint_" + i;
            }
            WayPointObjs = m;
        }

        var n = transform.childCount;
        for (int i = 0; i < transform.childCount; i++)
        {
            if (n != transform.childCount) return;

            // vẽ khối cầu điều khiển
            var wayPoint = transform.GetChild(i).GetComponent<WayPoint>();
            Gizmos.color = ColorOfPoint;
            Gizmos.DrawWireSphere(wayPoint.transform.position, pointScale);

            var n_count = 0;
            for (int d = 0; d < 4; d++)
            {
                if (wayPoint.Next[d] != null) n_count++;
            }


            // vẽ khung đường để đi            
            if (n_count == 1)
            {
                for (int d = 0; d < 4; d++)
                {
                    if (wayPoint.Next[d] != null)
                    {
                        Vector3 v3;
                        //                                                                                          đây dùng để đảo dấu
                        v3 = Vector3.Cross((wayPoint.transform.position - wayPoint.Next[d].transform.position) * (d == 0 || d == 3 ? 1 : -1), Vector3.up).normalized;

                        Gizmos.color = Color.green;
                        Gizmos.DrawWireSphere(wayPoint.transform.position + v3 * WayWidth, pointScale * 0.5f);
                        //bên phải
                        wayPoint.local[0] = wayPoint.local[3] = wayPoint.transform.position + v3 * WayWidth;

                        Gizmos.color = Color.red;
                        Gizmos.DrawWireSphere(wayPoint.transform.position - v3 * WayWidth, pointScale * 0.5f);
                        //bên trái
                        wayPoint.local[2] = wayPoint.local[1] = wayPoint.transform.position - v3 * WayWidth;
                        break;
                    }
                }
            }
            else
            {

                for (int d = 0; d < 4; d++)
                {
                    if (wayPoint.Next[d] == null) continue;
                    bool stop = false;
                    int d1 = (d + 1) % 4;
                    while (wayPoint.Next[d1] == null)
                    {
                        d1 = (d1 + 1) % 4;
                        if (d1 == d)
                        {
                            stop = true;
                            break;
                        }
                    }
                    if (stop) break;

                    var vd = Vector3.Cross((wayPoint.transform.position - wayPoint.Next[d].transform.position), Vector3.up).normalized;
                    var vd1 = Vector3.Cross((wayPoint.transform.position - wayPoint.Next[d1].transform.position), Vector3.up).normalized;

                    var A1d = wayPoint.Next[d].transform.position + vd * WayWidth;
                    var A2d = wayPoint.Next[d].transform.position + vd * WayWidth + (wayPoint.transform.position - wayPoint.Next[d].transform.position).normalized * 10f;
                    var B1d = wayPoint.Next[d1].transform.position - vd1 * WayWidth;
                    var B2d = wayPoint.Next[d1].transform.position - vd1 * WayWidth + (wayPoint.transform.position - wayPoint.Next[d1].transform.position).normalized * 10f;

                    var thisVector = GetIntersectionPointCoordinates(new Vector2(A1d.x, A1d.z), new Vector2(A2d.x, A2d.z), new Vector2(B1d.x, B1d.z), new Vector2(B2d.x, B2d.z));

                    // trường hợp song song của n_count = 2 và 3
                    if (thisVector == Vector2.zero)
                    {
                        if (n_count == 2)
                        {
                            var thisVec = Vector3.Cross((wayPoint.transform.position - wayPoint.Next[d].transform.position), Vector3.up).normalized;
                            Gizmos.color = (d == 3 || d == 0) ? Color.green : Color.red;
                            Gizmos.DrawWireSphere(wayPoint.transform.position + thisVec * WayWidth, pointScale * 0.5f);
                            wayPoint.local[d] = wayPoint.transform.position + thisVec * WayWidth;
                        }
                        else
                        if (n_count == 3)
                        {
                            var thisVec = Vector3.Cross((wayPoint.transform.position - wayPoint.Next[d].transform.position), Vector3.up).normalized;
                            wayPoint.local[d] = wayPoint.transform.position + thisVec * WayWidth;
                        }
                    }
                    else
                    {
                        var PointTrans = new Vector3(thisVector.x, wayPoint.transform.position.y, thisVector.y);

                        wayPoint.local[d] = PointTrans;

                        if (n_count != 3 || d==(d1-1+4)%4 )
                        {
                            Gizmos.color = (d == 3 || d == 0) ? Color.green : Color.red;
                            Gizmos.DrawWireSphere(PointTrans, pointScale * 0.5f);
                        }                            
                    }
                }

                // ngoại lệ n_count = 2 tự thêm 2 đường không dùng vào
                if (n_count == 2)
                {
                    for (int d = 0; d < 4; d++)
                    {
                        if (wayPoint.Next[d] == null)
                        {
                            wayPoint.local[d] = wayPoint.local[(d - 1 + 4) % 4];
                        }
                    }
                }

                // ngoại lệ n_count = 3 thay đổi đường nối
                if (n_count == 3)
                {
                    int d;
                    for (d = 0; wayPoint.Next[d] != null && d < 4; d++) ;
                    wayPoint.local[d] = wayPoint.local[(d - 1 + 4) % 4];
                    if (d == 0 || d == 2)
                    {
                        wayPoint.local[d].x = wayPoint.local[(d + 1) % 4].x;
                        wayPoint.local[(d - 1 + 4) % 4].x = wayPoint.local[(d - 2 + 4) % 4].x;
                    }
                    else
                    if (d == 1 || d == 3)
                    {
                        wayPoint.local[d].z = wayPoint.local[(d + 1) % 4].z;
                        wayPoint.local[(d - 1 + 4) % 4].z = wayPoint.local[(d - 2 + 4) % 4].z;
                    }
                    Gizmos.color = (d == 3 || d == 0) ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(wayPoint.local[d], pointScale * 0.5f);
                    d = (d - 1 + 4) % 4;
                    Gizmos.color = (d == 3 || d == 0) ? Color.green : Color.red;
                    Gizmos.DrawWireSphere(wayPoint.local[d], pointScale * 0.5f);
                }
            }

            // vẽ đường nối giữa các khối cầu điều khiển
            foreach (var next in wayPoint.Next)
            {
                if (next != null)
                {
                    Gizmos.color = ColorOfWay;
                    Gizmos.DrawLine(wayPoint.transform.position, next.transform.position);
                }
            }
        }

        // kiểm tra các điểm
        for (int i = 0; i < n; i++)
        {
            Gizmos.color = Color.magenta;
            var wayPoint = transform.GetChild(i).GetComponent<WayPoint>();
            for (int d = 0; d < 4; d++)
            {
                if (wayPoint.local[d] != null)
                {
                    Gizmos.DrawWireSphere(wayPoint.local[d], pointScale * 1.1f);
                }
            }
        }
    }


    /// <summary>
    /// Gets the coordinates of the intersection point of two lines.
    /// </summary>
    /// <param name="A1">A point on the first line.</param>
    /// <param name="A2">Another point on the first line.</param>
    /// <param name="B1">A point on the second line.</param>
    /// <param name="B2">Another point on the second line.</param>
    /// <param name="found">Is set to false of there are no solution. true otherwise.</param>
    /// <returns>The intersection point coordinates. Returns Vector2.zero if there is no solution.</returns>
    public Vector2 GetIntersectionPointCoordinates(Vector2 A1, Vector2 A2, Vector2 B1, Vector2 B2)
{
    float tmp = (B2.x - B1.x) * (A2.y - A1.y) - (B2.y - B1.y) * (A2.x - A1.x);

    if (tmp == 0)
    {
        // No solution!
        // found = false;
        return Vector2.zero;
    }

    float mu = ((A1.x - B1.x) * (A2.y - A1.y) - (A1.y - B1.y) * (A2.x - A1.x)) / tmp;

    //found = true;

    return new Vector2(
        B1.x + (B2.x - B1.x) * mu,
        B1.y + (B2.y - B1.y) * mu
    );
}
}

// cái code củ lìn 4 ngày + thức trắng 2 đêm mới code xong cáu zl
