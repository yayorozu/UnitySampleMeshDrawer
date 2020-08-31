using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

namespace DrawMesh
{
	public class Drawer : MonoBehaviour, IDragHandler, IBeginDragHandler, IEndDragHandler, IPointerDownHandler, IPointerUpHandler
	{
		[SerializeField]
		private float _width = 0f;

		[SerializeField]
		private Camera _targetCamera = null;

		[SerializeField]
		private Color _color = Color.white;

		[SerializeField]
		private MeshFilter _meshFilter = null;

		private Vector2 _prevPos;
		private RectTransform _rect;

		private void Awake()
		{
			_rect = transform.root as RectTransform;
			if (_meshFilter.sharedMesh == null)
				_meshFilter.sharedMesh = new Mesh();
		}

		/// <summary>
		/// 最初に作る場合は4頂点必要なので判定用
		/// </summary>
		private bool _isNew;

		public void OnPointerDown(PointerEventData eventData)
		{
			if (_isNew)
				return;

			_isNew = true;
			_prevPos = eventData.position;
		}

		public void OnDrag(PointerEventData eventData)
		{
			var pos = eventData.position;

			// 一定距離ドラッグしたら描画する
			if ((_prevPos - pos).magnitude < 2f)
				return;

			CreateMesh(_prevPos, pos, _isNew);
			_prevPos = eventData.position;
			_isNew = false;
		}

		public void OnBeginDrag(PointerEventData eventData)
		{
			if (_isNew)
				return;

			_isNew = true;
			_prevPos = eventData.position;
		}

		public void OnEndDrag(PointerEventData eventData)
		{
			_isNew = false;
		}

		public void OnPointerUp(PointerEventData eventData)
		{
			_isNew = false;
		}

		/// <summary>
		/// Mesh を作成
		/// </summary>
		/// <param name="prev"></param>
		/// <param name="end"></param>
		/// <param name="isNew"></param>
		private void CreateMesh(Vector2 prev, Vector2 end, bool isNew = false)
		{
			// Screen 座標から 対象カメラの座標へ変換
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, prev, _targetCamera, out var w_prev);
			RectTransformUtility.ScreenPointToLocalPointInRectangle(_rect, end, _targetCamera, out var w_end);

			// Canvas に Scale がかかってるので適応
			w_prev *= transform.lossyScale;
			w_end *= transform.lossyScale;

			// 進行方向に垂直な法線を取得
			var normal = (Vector2)(Quaternion.Euler( 0, 0, 90) * (w_prev - w_end).normalized);

			// 太さ分の座標を確保
			var v = normal * _width / 2 * transform.lossyScale;

			var new1 = w_end + v;
			var new2 = w_end - v;

			var mesh = _meshFilter.sharedMesh;
			// Mesh 情報をキャッシュ
			var vert = mesh.vertices.ToList();
			var tri = mesh.triangles.ToList();
			var colors = mesh.colors.ToList();

			var beginIndex = vert.Count;

			// 初期状態
			if (isNew)
			{
				var prev1 = w_prev + v;
				var prev2 = w_prev - v;
				vert.AddRange(new Vector3[] {prev1, prev2, new1, new2});
				tri.AddRange(new []
				{
					beginIndex, beginIndex + 1, beginIndex + 2,
					beginIndex + 1, beginIndex + 3, beginIndex + 2
				});

				colors.AddRange(new []{_color, _color, _color, _color});
			}
			else
			{
				vert.AddRange(new Vector3[] {new1, new2});
				tri.AddRange(new[]
				{
					beginIndex - 2, beginIndex - 1, beginIndex,
					beginIndex - 1, beginIndex + 1, beginIndex
				});
				colors.AddRange(new []{_color, _color});
			}

			mesh.vertices = vert.ToArray();
			mesh.triangles = tri.ToArray();
			mesh.colors = colors.ToArray();
		}
	}
}
