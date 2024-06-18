using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public class Extention
{

}

#region ---------------------------------------------  DATA STRUCTURE ------------------------------------------------------
public class Pair<T, U>
{
    public Pair() { }
    public Pair(T first, U second)
    {
        this.First = first;
        this.Second = second;
    }
    public T First { set; get; }
    public U Second { set; get; }
}

public class RecoilValue
{
    public float x, y, z, returnSpeed, snappiness;
    public static RecoilValue None = new RecoilValue(0, 0, 0, -1, -1);
    public RecoilValue(float x, float y, float z, float returnSpeed, float snappiness)
    {
        this.x = Mathf.Abs(x);
        this.y = Mathf.Abs(y);
        this.z = Mathf.Abs(z);
        this.returnSpeed = returnSpeed;
        this.snappiness = snappiness;
    }
    public Vector3 GetRecoil()
    {
        return new Vector3(-x, Random.Range(-y, y), Random.Range(-z, z));
    }
}

#endregion

#region ---------------------------------------------  FUNCTION ------------------------------------------------------
public static class BehaviorExt
{
    #region Tìm kiếm kẻ thù sử dụng tia chiếu từ phía kẻ thù chiếu đến các vật thể và lấy điểm đằng sau (xem thêm chú thích ở dưới)

    // Việc tìm kiếm này chỉ hiệu quả với các vật thể đồng nhất và hình dạng đơn giản nếu không sẽ dễ gây ra lỗi

    /// <summary>
    /// Tìm điểm ẩn nấp
    /// </summary>
    /// <param name="enterPoint">Điểm bắt đầu</param>
    /// <param name="direction">Hướng bắn</param>
    /// <param name="count">Số lượng điểm ẩn nấp mong muốn</param>
    /// <param name="angleDiv">Khoảng chia độ quét</param>
    /// <param name="angleMax">Độ quét tối đa, tính từ vector.forward sang một trong hai bên</param>
    /// <param name="MaxDistance">Khoảng cách raycast tối đa</param>
    /// <param name="CheckDistance">Độ dày tối đa của vật thể kiểm tra</param>
    public static Pair<Vector3, Vector3>[] FindCoverFromEnemy(Vector3 origin, Vector3 direction, ref int count, int angleDiv, int angleMax, float MaxDistance, float CheckDistance)
        => FindCoverFromEnemy(new Ray(origin, direction), ref count, angleDiv, angleMax, MaxDistance, CheckDistance, ~(1 << 31), false);
    /// <summary>
    /// Tìm điểm ẩn nấp
    /// </summary>
    /// <param name="enterPoint">Điểm bắt đầu</param>
    /// <param name="direction">Hướng bắn</param>
    /// <param name="count">Số lượng điểm ẩn nấp mong muốn</param>
    /// <param name="angleDiv">Khoảng chia độ quét</param>
    /// <param name="angleMax">Độ quét tối đa, tính từ vector.forward sang một trong hai bên</param>
    /// <param name="MaxDistance">Khoảng cách raycast tối đa</param>
    /// <param name="CheckDistance">Độ dày tối đa của vật thể kiểm tra</param>
    /// <param name="layer"> Các Layer có thể va chạm </param>
    /// <returns> Trả về danh sách các điểm (Cover,Show) tìm được với số lượng count </returns>
    public static Pair<Vector3, Vector3>[] FindCoverFromEnemy(Vector3 origin, Vector3 direction, ref int count, int angleDiv, int angleMax, float MaxDistance, float CheckDistance, LayerMask layer)
        => FindCoverFromEnemy(new Ray(origin, direction), ref count, angleDiv, angleMax, MaxDistance, CheckDistance, layer, false);
    /// <summary>
    /// Tìm điểm ẩn nấp
    /// </summary>
    /// <param name="enterPoint">Điểm bắt đầu</param>
    /// <param name="direction">Hướng bắn</param>
    /// <param name="count">Số lượng điểm ẩn nấp mong muốn</param>
    /// <param name="angleDiv">Khoảng chia độ quét</param>
    /// <param name="angleMax">Độ quét tối đa, tính từ vector.forward sang một trong hai bên</param>
    /// <param name="MaxDistance">Khoảng cách raycast tối đa</param>
    /// <param name="CheckDistance">Độ dày tối đa của vật thể kiểm tra</param>
    /// <param name="layer"> Các Layer có thể va chạm </param>
    /// <param name="_debug">Có thực hiện chế độ Debug hay không</param>
    /// <returns> Trả về danh sách các điểm (Cover,Show) tìm được với số lượng count </returns>
    public static Pair<Vector3, Vector3>[] FindCoverFromEnemy(Vector3 origin, Vector3 direction, ref int count, int angleDiv, int angleMax, float MaxDistance, float CheckDistance, LayerMask layer, bool _debug)
        => FindCoverFromEnemy(new Ray(origin, direction), ref count, angleDiv, angleMax, MaxDistance, CheckDistance, layer, _debug);
    /// <summary>
    /// Tìm điểm ẩn nấp
    /// </summary>
    /// <param name="ray">Tia tới</param>
    /// <param name="count">Số lượng điểm ẩn nấp mong muốn</param>
    /// <param name="angleDiv">Khoảng chia độ quét</param>
    /// <param name="angleMax">Độ quét tối đa, tính từ vector.forward sang một trong hai bên</param>
    /// <param name="MaxDistance">Khoảng cách raycast tối đa</param>
    /// <param name="CheckDistance">Độ dày tối đa của vật thể kiểm tra</param>
    /// <param name="layer"> Các Layer có thể va chạm </param>
    /// <param name="_debug">Có thực hiện chế độ Debug hay không</param>
    /// <returns> Trả về danh sách các điểm (Cover,Show) tìm được với số lượng count </returns>
    public static Pair<Vector3, Vector3>[] FindCoverFromEnemy(Ray ray, ref int count, int angleDiv, int angleMax, float MaxDistance, float CheckDistance, LayerMask layer, bool _debug)
    {
        var CoverPoints = new Pair<Vector3, Vector3>[count];

        var Org = ray.origin;
        int n = 0;

        Transform _current = null;
        Vector3? _coverPoint = null;
        Ray? _before = null;

        //Left
        for (int i = 0; i <= angleMax; i += angleDiv)
        {
            if (n >= count) break;
            var r = new Ray(ray.origin, Quaternion.AngleAxis(i, Vector3.up) * ray.direction);

            if (_debug) Debug.DrawLine(r.origin, r.origin + r.direction.normalized * MaxDistance, Color.yellow);

            if (RayCastExt.GetExitHit(r, out var hit, MaxDistance, CheckDistance, layer))
            {
                if (_debug) Debug.DrawLine(hit.point, hit.point + Vector3.up * 10, Color.red);

                if (_current != null && _current != hit.transform)
                {
                    var disBefore = ((Vector3)_coverPoint - Org).magnitude;
                    var disCurrent = (hit.point - Org).magnitude;
                    if (_before != null)
                    {
                        if (disBefore > disCurrent)
                        {
                            CoverPoints[n++] = new Pair<Vector3, Vector3>(hit.point, ((Ray)_before).GetPoint(disCurrent));
                        }
                        else
                        {
                            CoverPoints[n++] = new Pair<Vector3, Vector3>((Vector3)_coverPoint, r.GetPoint(disBefore));
                        }
                    }
                }
                else
                if (_current == null && _before != null)
                {
                    var disCurrent = (hit.point - Org).magnitude;
                    CoverPoints[n++] = new Pair<Vector3, Vector3>(hit.point, ((Ray)_before).GetPoint(disCurrent));
                }

                _current = hit.transform;
                _coverPoint = hit.point;
            }
            else
            {
                if (_current != null)
                {
                    var disBefore = ((Vector3)_coverPoint - Org).magnitude;
                    CoverPoints[n++] = new Pair<Vector3, Vector3>((Vector3)_coverPoint, r.GetPoint(disBefore));
                }
                _current = null;
                _coverPoint = null;
            }

            _before = r;
        }

        _current = null;
        _coverPoint = null;
        _before = null;

        //Right
        for (int i = 0; i <= angleMax; i += angleDiv)
        {
            if (n >= count) break;
            var r = new Ray(ray.origin, Quaternion.AngleAxis(-i, Vector3.up) * ray.direction);

            if (_debug) Debug.DrawLine(r.origin, r.origin + r.direction.normalized * MaxDistance, Color.yellow);

            if (RayCastExt.GetExitHit(r, out var hit, MaxDistance, CheckDistance, layer))
            {
                if (_debug) Debug.DrawLine(hit.point, hit.point + Vector3.up * 10, Color.red);

                if (_current != null && _current != hit.transform)
                {
                    var disBefore = ((Vector3)_coverPoint - Org).magnitude;
                    var disCurrent = (hit.point - Org).magnitude;
                    if (_before != null)
                    {
                        if (disBefore > disCurrent)
                        {
                            CoverPoints[n++] = new Pair<Vector3, Vector3>(hit.point, ((Ray)_before).GetPoint(disCurrent));
                        }
                        else
                        {
                            CoverPoints[n++] = new Pair<Vector3, Vector3>((Vector3)_coverPoint, r.GetPoint(disBefore));
                        }
                    }
                }
                else
                if (_current == null && _before != null)
                {
                    var disCurrent = (hit.point - Org).magnitude;
                    CoverPoints[n++] = new Pair<Vector3, Vector3>(hit.point, ((Ray)_before).GetPoint(disCurrent));
                }


                _current = hit.transform;
                _coverPoint = hit.point;
            }
            else
            {
                if (_current != null)
                {
                    var disBefore = ((Vector3)_coverPoint - Org).magnitude;
                    CoverPoints[n++] = new Pair<Vector3, Vector3>((Vector3)_coverPoint, r.GetPoint(disBefore));
                }
                _current = null;
                _coverPoint = null;
            }

            _before = r;
        }

        if (n < count) count = n;

        return CoverPoints;
    }
    #endregion

    #region Tìm kiếm kẻ thù sử dụng tia chiếu từ phía chủ thể đến các vật thể (xem thêm chú thích ở dưới)

    /// <summary>
    /// Tìm điểm ẩn nấp từ phía chủ thể
    /// </summary>
    /// <param name="ray">Tia tới</param>
    /// <param name="count">Số lượng điểm ẩn nấp mong muốn</param>
    /// <param name="MaxDistance">Khoảng cách raycast tối đa</param>
    /// <param name="layer"> Các Layer có thể va chạm </param>
    /// <param name="radius"> Bán kính tròn của nhân vật chiếu tia </param>
    /// <param name="EnemyPos"> Vị trí kẻ thù </param>
    /// <param name="EnemyTag"> Tag của kẻ thù </param>
    /// <param name="heigh"> Chiều cao khi đứng của nhân vật </param>
    /// <returns> Trả về danh sách các điểm (Cover,Show) tìm được với số lượng count </returns>
    public static Pair<Vector3, Vector3>[] FindCoverFormItseft(Ray ray, ref int count, float MaxDistance, LayerMask layer,
                                                              float heigh, float radius, Vector3 EnemyPos, string EnemyTag)
        => FindCoverFormItseft(ray, ref count, 5, 60, MaxDistance, layer, false, heigh, radius, EnemyPos, EnemyTag, false);

    /// <summary>
    /// Tìm điểm ẩn nấp từ phía chủ thể
    /// </summary>
    /// <param name="ray">Tia tới</param>
    /// <param name="count">Số lượng điểm ẩn nấp mong muốn</param>
    /// <param name="angleDiv">Khoảng chia độ quét</param>
    /// <param name="angleMax">Độ quét tối đa, tính từ vector.forward sang một trong hai bên</param>
    /// <param name="MaxDistance">Khoảng cách raycast tối đa</param>
    /// <param name="layer"> Các Layer có thể va chạm </param>
    /// <param name="_debugOfRayCast">Có thực hiện chế độ Debug của Ray Cast hay không hay không (các tia chiếu tới để tìm vị trí) </param>
    /// <param name="radius"> Bán kính tròn của nhân vật chiếu tia </param>
    /// <param name="EnemyPos"> Vị trí kẻ thù </param>
    /// <param name="EnemyTag"> Tag của kẻ thù </param>
    /// <param name="_debugOfCheckCover"> Có thực hiện chế độ Debug của tia kiểm tra vị trí cover an toàn hay không </param>
    /// <param name="heigh"> Chiều cao khi đứng của nhân vật </param>
    /// <returns> Trả về danh sách các điểm (Cover,Show) tìm được với số lượng count </returns>
    public static Pair<Vector3, Vector3>[] FindCoverFormItseft(Ray ray, ref int count, int angleDiv, int angleMax, float MaxDistance,
        LayerMask layer, bool _debugOfRayCast, float heigh, float radius, Vector3 EnemyPos, string EnemyTag, bool _debugOfCheckCover)
    {
        var ListCoverPoints = new List<Pair<Vector3, Vector3>>();

        int n = 0;

        RaycastHit? _coverPoint = null;
        Ray? _before = null;

        ////Left
        for (int i = 0; i <= angleMax; i += angleDiv)
        {

            if (n >= count) break;
            var r = new Ray(ray.origin, Quaternion.AngleAxis(i, Vector3.up) * ray.direction);

            if (_debugOfRayCast) Debug.DrawLine(r.origin, r.origin + r.direction.normalized * MaxDistance, Color.yellow);

            if (Physics.Raycast(r, out var hit, MaxDistance, layer))
            {
                //if (_debugOfRayCast) Debug.DrawLine(hit.point, hit.point + Vector3.up * 10, Color.red);

                var p = CanBeSeenByEnemy(hit.point + hit.normal.normalized * radius,
                    Vector3.up, radius, EnemyPos, EnemyTag, _debugOfCheckCover);

                //1
                // Xét tia ở lần này:
                //      - Nếu tia lần này ở vị trí phòng thủ nhưng khi chủ thể đứng lên sẽ ở trạng thái tấn công
                // => Thêm vị trí vào
                if (p == -1)
                {
                    if (CanBeSeenByEnemy(hit.point + hit.normal.normalized*radius + Vector3.up * heigh,
                                            Vector3.up, 0, EnemyPos, EnemyTag, _debugOfCheckCover) != -1)
                    {
                        ListCoverPoints.Add(new Pair<Vector3, Vector3>(hit.point + hit.normal.normalized * radius,
                            hit.point + hit.normal.normalized * radius + Vector3.up * heigh));
                    }
                }

                //2
                // Xem xét 2 tia, tia ở lần trước và tia ở lần này:
                //      - Vì có khả năng tia lần này tay bắn trúng nhưng nếu lấy độ dài bằng độ dài của tia lần trước thì thỏa mãn
                //       đang ở vị trí tấn công
                //      - Nếu có tồn tại vị trí phòng thủ từ trước
                // => Thêm vị trí vào
                if (_coverPoint != null)
                {
                    var h = (RaycastHit)_coverPoint;
                    var dis = ((h.point + h.normal.normalized * radius) - r.GetPoint(0)).magnitude;
                    dis = (Mathf.Cos((i - angleDiv) * Mathf.PI / 180) * dis) / Mathf.Cos(i * Mathf.PI / 180);

                    if (!Physics.Linecast(r.GetPoint(0), r.GetPoint(dis), layer) &&
                        CanBeSeenByEnemy(r.GetPoint(dis), Vector3.up, 0, EnemyPos, EnemyTag, _debugOfCheckCover) != -1)
                    {
                        ListCoverPoints.Add(new Pair<Vector3, Vector3>(h.point + h.normal.normalized*radius, r.GetPoint(dis)));

                        if (_debugOfRayCast) Debug.DrawLine(hit.point, hit.point + Vector3.up * 5f, Color.green);
                        if (_debugOfRayCast) Debug.DrawLine(r.GetPoint(dis), r.GetPoint(dis) + Vector3.up * 5f, Color.blue);
                    }
                }

                //3
                // Nếu tia lần này xác nhận là điểm phòng thủ:
                //  - Nếu tia trước đó (với độ dài bằng tia lần này) thỏa mãn vị trí tấn công
                // => Thêm vị trí vào
                if (p == -1)
                {
                    if (_before != null)
                    {
                        var ray_t = (Ray)_before;
                        var dis = ((hit.point + hit.normal.normalized * radius) - r.GetPoint(0)).magnitude;
                        dis = (Mathf.Cos(i * Mathf.PI / 180) * dis) / Mathf.Cos((i - angleDiv) * Mathf.PI / 180);

                        if (!Physics.Linecast(ray_t.GetPoint(0), ray_t.GetPoint(dis), layer) &&
                            CanBeSeenByEnemy(ray_t.GetPoint(dis), Vector3.up, 0, EnemyPos, EnemyTag, _debugOfCheckCover) != -1)
                        {
                            ListCoverPoints.Add(new Pair<Vector3, Vector3>
                                (hit.point + hit.normal.normalized * radius, ray_t.GetPoint(dis)));

                            if (_debugOfRayCast) Debug.DrawLine(hit.point, hit.point + Vector3.up * 5f, Color.green);
                            if (_debugOfRayCast) Debug.DrawLine(ray_t.GetPoint(dis), ray_t.GetPoint(dis) + Vector3.up * 5f, Color.blue);
                        }
                    }
                    _coverPoint = hit;
                }
                else
                //4
                // Nếu tia lần này không phải điểm phòng thủ:
                //  - Nếu tồn tại điểm phòng thủ
                // => Thêm vị trí vào
                {
                    if (_coverPoint != null && (_coverPoint.Value.point - hit.point).magnitude <= 5.0f)
                    {
                        ListCoverPoints.Add(new Pair<Vector3, Vector3>
                            (_coverPoint.Value.point + _coverPoint.Value.normal.normalized*radius, 
                                                            hit.point + hit.normal.normalized * radius));

                        if (_debugOfRayCast) Debug.DrawLine(_coverPoint.Value.point, _coverPoint.Value.point + Vector3.up * 5f, Color.green);
                        if (_debugOfRayCast) Debug.DrawLine(hit.point + hit.normal.normalized * radius,
                                hit.point + hit.normal.normalized * radius + Vector3.up, Color.blue);

                    }
                    _coverPoint = null;
                }
            }
            else
            {
                //5
                // Nếu tia lần này không bắn trúng gì:
                //  - Nếu tồn tại điểm phòng thủ
                //  - Nếu địa điểm được lấy theo độ dài từ chủ thể đến điểm phòng thủ thỏa mãn là vị trí tấn công
                // => Thêm vị trí vào
                if (_coverPoint != null)
                {
                    var h = (RaycastHit)_coverPoint;
                    var dis = ((h.point + h.normal.normalized * radius) - r.GetPoint(0)).magnitude;
                    dis = (Mathf.Cos((i - angleDiv) * Mathf.PI / 180) * dis) / Mathf.Cos(i * Mathf.PI / 180);

                    if (!Physics.Linecast(r.GetPoint(0), r.GetPoint(dis), layer) &&
                        CanBeSeenByEnemy(r.GetPoint(dis), Vector3.up, 0, EnemyPos, EnemyTag, _debugOfCheckCover) != -1)
                    {
                        ListCoverPoints.Add(new Pair<Vector3, Vector3>(h.point + h.normal.normalized * radius, r.GetPoint(dis)));

                        if (_debugOfRayCast) Debug.DrawLine(h.point, h.point + Vector3.up * 5f, Color.green);
                        if (_debugOfRayCast) Debug.DrawLine(r.GetPoint(dis), r.GetPoint(dis) + Vector3.up * 5f, Color.blue);
                    }
                }
                _coverPoint = null;
            }

            _before = r;
        }

        _coverPoint = null;
        _before = null;

        //Right
        for (int i = 0; i >= -angleMax; i -= angleDiv)
        {
            if (n >= count) break;
            var r = new Ray(ray.origin, Quaternion.AngleAxis(i, Vector3.up) * ray.direction);

            if (_debugOfRayCast) Debug.DrawLine(r.origin, r.origin + r.direction.normalized * MaxDistance, Color.yellow);

            if (Physics.Raycast(r, out var hit, MaxDistance, layer))
            {
                //if (_debugOfRayCast) Debug.DrawLine(hit.point, hit.point + Vector3.up * 10, Color.red);

                var p = CanBeSeenByEnemy(hit.point + hit.normal.normalized * radius,
                    Vector3.up, radius, EnemyPos, EnemyTag, _debugOfCheckCover);
                
                //1
                // Xét tia ở lần này:
                //      - Nếu tia lần này ở vị trí phòng thủ nhưng khi chủ thể đứng lên sẽ ở trạng thái tấn công
                // => Thêm vị trí vào
                if (p == -1)
                {
                    if (CanBeSeenByEnemy(hit.point + hit.normal.normalized * radius + Vector3.up * heigh,
                                            Vector3.up, 0, EnemyPos, EnemyTag, _debugOfCheckCover) != -1)
                    {
                        ListCoverPoints.Add(new Pair<Vector3, Vector3>(hit.point + hit.normal.normalized * radius,
                            hit.point + hit.normal.normalized * radius + Vector3.up * heigh));
                    }
                }

                //2
                //Xem xét 2 tia, tia ở lần trước và tia ở lần này:
                //  - Vì có khả năng tia lần này tay bắn trúng nhưng nếu lấy độ dài bằng độ dài của tia lần trước thì thỏa mãn
                //    đang ở vị trí tấn công
                //  - Nếu có tồn tại vị trí phòng thủ từ trước thì ta thêm vị trí vào
                if (_coverPoint != null)
                {
                    var h = (RaycastHit)_coverPoint;
                    var dis = ((h.point + h.normal.normalized * radius) - r.GetPoint(0)).magnitude;
                    dis = (Mathf.Cos((i + angleDiv) * Mathf.PI / 180) * dis) / Mathf.Cos(i * Mathf.PI / 180);

                    if (!Physics.Linecast(r.GetPoint(0), r.GetPoint(dis), layer) &&
                        CanBeSeenByEnemy(r.GetPoint(dis), Vector3.up, radius, EnemyPos, EnemyTag, _debugOfCheckCover) != -1)
                    {
                        ListCoverPoints.Add(new Pair<Vector3, Vector3>(h.point + h.normal.normalized * radius, r.GetPoint(dis)));

                        if (_debugOfRayCast) Debug.DrawLine(hit.point, hit.point + Vector3.up * 5f, Color.green);
                        if (_debugOfRayCast) Debug.DrawLine(r.GetPoint(dis), r.GetPoint(dis) + Vector3.up * 5f, Color.blue);

                    }
                }

                //3
                // Nếu tia lần này xác nhận là điểm phòng thủ:
                //  - Nếu tia trước đó (với độ dài bằng tia lần này) thỏa mãn vị trí tấn công thì thêm vị trí vào
                if (p == -1)
                {

                    if (_before != null)
                    {
                        var ray_t = (Ray)_before;
                        var dis = ((hit.point + hit.normal.normalized * radius) - r.GetPoint(0)).magnitude;
                        dis = (Mathf.Cos(i * Mathf.PI / 180) * dis) / Mathf.Cos((i + angleDiv) * Mathf.PI / 180);

                        if (!Physics.Linecast(ray_t.GetPoint(0), ray_t.GetPoint(dis), layer) &&
                            CanBeSeenByEnemy(ray_t.GetPoint(dis), Vector3.up, 0, EnemyPos, EnemyTag, _debugOfCheckCover) != -1)
                        {
                            ListCoverPoints.Add(new Pair<Vector3, Vector3>
                                (hit.point + hit.normal.normalized*radius, ray_t.GetPoint(dis)));

                            if (_debugOfRayCast) Debug.DrawLine(hit.point, hit.point + Vector3.up * 5f, Color.green);
                            if (_debugOfRayCast) Debug.DrawLine(ray_t.GetPoint(dis), ray_t.GetPoint(dis) + Vector3.up * 5f, Color.blue);

                        }
                    }
                    _coverPoint = hit;
                }
                else
                //4
                // Nếu tia lần này không phải điểm phòng thủ:
                //  - Nếu tồn tại điểm phòng thủ thì ta thêm vị trí vào
                {
                    if (_coverPoint != null && (_coverPoint.Value.point - hit.point).magnitude <= 5.0f)
                    {
                        ListCoverPoints.Add(new Pair<Vector3, Vector3>
                            (_coverPoint.Value.point + _coverPoint.Value.normal.normalized*radius, 
                                                            hit.point + hit.normal.normalized * radius));


                        if (_debugOfRayCast) Debug.DrawLine(_coverPoint.Value.point, _coverPoint.Value.point + Vector3.up * 5f, Color.green);
                        if (_debugOfRayCast) Debug.DrawLine(hit.point + hit.normal.normalized * radius,
                                hit.point + hit.normal.normalized * radius + Vector3.up, Color.blue);
                    }
                    _coverPoint = null;
                }
            }
            else
            {
                //5
                // Nếu tia lần này không bắn trúng gì:
                //  - Nếu tồn tại điểm phòng thủ
                //  - Nếu địa điểm được lấy theo độ dài từ chủ thể đến điểm phòng thủ thỏa mãn là vị trí tấn công thì thêm vị trí vào
                if (_coverPoint != null)
                {
                    var h = (RaycastHit)_coverPoint;
                    var dis = ((h.point + h.normal.normalized * radius) - r.GetPoint(0)).magnitude;
                    dis = (Mathf.Cos((i + angleDiv) * Mathf.PI / 180) * dis) / Mathf.Cos(i * Mathf.PI / 180);

                    if (!Physics.Linecast(r.GetPoint(0), r.GetPoint(dis), layer) &&
                        CanBeSeenByEnemy(r.GetPoint(dis), Vector3.up, 0, EnemyPos, EnemyTag, _debugOfCheckCover) != -1)
                    {
                        ListCoverPoints.Add(new Pair<Vector3, Vector3>
                            (h.point + h.normal.normalized * radius, r.GetPoint(dis)));


                        if (_debugOfRayCast) Debug.DrawLine(h.point, h.point + Vector3.up * 5f, Color.green);
                        if (_debugOfRayCast) Debug.DrawLine(r.GetPoint(dis), r.GetPoint(dis) + Vector3.up * 5f, Color.blue);
                    }
                }
                _coverPoint = null;
            }

            _before = r;
        }

        count = ListCoverPoints.Count;

        return ListCoverPoints.ToArray();
    }

    #endregion

    #region Kiểm tra kẻ thù và các hành vi liên quan đến phản ứng của kẻ thù

    /// <summary>
    /// Thử xem vị trí này có bị nhìn thấy bởi người chơi không
    /// </summary>
    /// <param name="ThisPos">Vị trí cần kiểm tra</param>
    /// <param name="EnemyPos">Vị trí kẻ thù</param>
    /// <param name="radius">Bán kính kiểm tra</param>
    /// <returns>-1 : không bị lộ  <br></br>
    ///           0 : bị lộ ở phía chính giữa <br></br>
    ///           1 : bị lộ ở phía trái <br></br>
    ///           2 : bị lộ ở phía phải <br></br>
    /// </returns>
    public static short CanBeSeenByEnemy(Vector3 ThisPos, Vector3 ThisVectorUp, float radius, Vector3 EnemyPos, string EnemyTag, bool _debug)
    {
        var dirOrg = EnemyPos - ThisPos;

        //Mid
        var point = ThisPos;
        var dir = EnemyPos - point;

        if (_debug) Debug.DrawLine(point, point + dir, Color.green);

        if (Physics.Raycast(point, dir, out var Hit, dir.magnitude) && Hit.collider.CompareTag(EnemyTag))
        {
            return 0;
        }

        if (_debug) Debug.DrawLine(point, dir + point, Color.red);

        if (radius == 0) return -1;

        //Left 
        point = ThisPos + Vector3.Cross(dirOrg, ThisVectorUp).normalized * radius;
        dir = EnemyPos - point;

        if (_debug) Debug.DrawLine(point, point + dir, Color.green);

        if (Physics.Raycast(point, dir, out Hit, dir.magnitude) && Hit.collider.CompareTag(EnemyTag))
        {
            return 1;
        }

        if (_debug) Debug.DrawLine(point, dir + point, Color.red);

        //Right
        point = ThisPos - Vector3.Cross(dirOrg, ThisVectorUp).normalized * radius;
        dir = EnemyPos - point;

        if (_debug) Debug.DrawLine(point, point + dir, Color.green);

        if (Physics.Raycast(point, dir, out Hit, dir.magnitude) && Hit.collider.CompareTag(EnemyTag))
        {
            return 2;
        }

        if (_debug) Debug.DrawLine(point, dir + point, Color.red);

        return -1;
    }
    #endregion
}

public static class RayCastExt
{
    /// <summary>
    /// Lấy điểm mà tia sẽ thoát ra sau khi bắn vào một vật thể, <br></br>
    /// <b>Lưu ý:</b> <i>CheckDistance > độ dày vật bắn vào</i>
    /// </summary>
    /// <param name="enterPoint">Điểm bắt đầu</param>
    /// <param name="direction">Hướng bắn</param>
    /// <param name="Hit">Điểm trúng</param>
    /// <param name="MaxDistance">Khoảng cách tối đa để tia bắn vào</param>
    /// <param name="CheckDistance">Độ dày vật thể</param>
    public static bool GetExitHit(Vector3 origin, Vector3 direction, out RaycastHit Hit, float MaxDistance, float CheckDistance, LayerMask layer)
        => GetExitHit(new Ray(origin, direction), out Hit, MaxDistance, CheckDistance, layer);
    /// <summary>
    /// Lấy điểm mà tia sẽ thoát ra sau khi bắn vào một vật thể, <br></br>
    /// <b>Lưu ý:</b> <i>CheckDistance > độ dày vật bắn vào</i>
    /// </summary>
    /// <param name="ray">Tia bắn tới</param>
    /// <param name="Hit">Điểm trúng</param>
    /// <param name="MaxDistance">Khoảng cách tối đa để tia bắn vào</param>
    /// <param name="CheckDistance">Độ dày vật thể</param>
    public static bool GetExitHit(Ray ray, out RaycastHit Hit, float MaxDistance, float CheckDistance, LayerMask layer)
    {
        Hit = new RaycastHit();
        if (!Physics.Raycast(ray, out Hit, MaxDistance, layer)) return false;
        //Lấy điểm gốc cách điểm đầu một đoạn bằng checkDistance
        ray.origin = ray.GetPoint((Hit.point - ray.origin).magnitude + CheckDistance);
        //Đảo ngược điểm
        ray.direction = -ray.direction;
        //Collider.RayCast sẽ bỏ qua tất cả các Collider khác với Collider đang xét
        if (Hit.collider.Raycast(ray, out var ExitHit, MaxDistance))
        {
            Hit = ExitHit;
            return true;
        }
        else return false;
    }
}

public static class Rand
{ 
    /// <summary>
    /// Xác xuất để trả về đúng
    /// </summary>
    /// <param name="change">Cơ hội để trả về đúng</param>
    /// <param name="In">Không gian mẫu của xác suất</param>
    /// <returns></returns>
    public static bool RandPer(int change,int In) => (Random.Range(0, In*10 + 1) <= change*10);
    /// <summary>
    /// Trả về Random các số từ [L,R] <br></br>
    /// Chú ý là từ L đến R chứ không phải từ L đén nhỏ hơn R
    /// </summary>
    /// <param name="L">Số bé nhất</param>
    /// <param name="R">Số lớn nhất</param>
    /// <param name="r">Nhiễu ngẫu nhiên</param>
    /// <returns></returns>
    public static int RandSpc(int L, int R, int rand) =>
        (Random.Range(L, R + 1) + Random.Range(L, R + 1) + Random.Range(L, R + 1) + rand) % (R + 1);

    public static float RandSpc(float L, float R, float rand) =>
        (Random.Range(L, R + 1) + Random.Range(L, R + 1) + Random.Range(L, R + 1) + rand) % (R + 1);
}


#endregion