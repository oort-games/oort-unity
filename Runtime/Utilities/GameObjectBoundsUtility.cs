using UnityEngine;

namespace OortUnity.Utilities
{
    /// <summary>
    /// GameObject 계층의 Renderer Bounds와 UI RectTransform 크기를 계산합니다.
    /// </summary>
    public static class GameObjectBoundsUtility
    {
        #region Type

        /// <summary>
        /// GameObject가 RectTransform과 CanvasRenderer를 포함하는 UI 계층인지 확인합니다.
        /// </summary>
        /// <param name="source">확인할 GameObject입니다.</param>
        /// <returns>UI 계층으로 판단되면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool IsUIObject(GameObject source)
        {
            return source != null
                && source.GetComponent<RectTransform>() != null
                && source.GetComponentInChildren<CanvasRenderer>(true) != null;
        }

        #endregion

        #region Bounds

        /// <summary>
        /// 활성 계층에 포함된 활성 Renderer의 월드 Bounds를 하나로 결합합니다.
        /// </summary>
        /// <param name="root">Renderer를 검색할 루트 GameObject입니다.</param>
        /// <param name="bounds">결합된 월드 Bounds입니다.</param>
        /// <returns>하나 이상의 활성 Renderer를 찾았으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryGetRendererBounds(GameObject root, out Bounds bounds)
        {
            bounds = default;

            if (root == null)
            {
                return false;
            }

            Renderer[] renderers = root.GetComponentsInChildren<Renderer>(false);
            bool hasBounds = false;

            foreach (Renderer renderer in renderers)
            {
                if (renderer == null || !renderer.enabled)
                {
                    continue;
                }

                if (!hasBounds)
                {
                    bounds = renderer.bounds;
                    hasBounds = true;
                    continue;
                }

                bounds.Encapsulate(renderer.bounds);
            }

            return hasBounds;
        }

        /// <summary>
        /// GameObject의 RectTransform 크기를 조회합니다.
        /// Rect 크기를 사용할 수 없으면 sizeDelta를 사용합니다.
        /// </summary>
        /// <param name="source">크기를 조회할 UI GameObject입니다.</param>
        /// <param name="size">조회된 RectTransform 크기입니다.</param>
        /// <returns>너비와 높이가 모두 유효한 크기를 조회했으면 true, 그렇지 않으면 false를 반환합니다.</returns>
        public static bool TryGetRectSize(GameObject source, out Vector2 size)
        {
            size = default;

            if (source == null || source.transform is not RectTransform rectTransform)
            {
                return false;
            }

            Canvas.ForceUpdateCanvases();

            Rect rect = rectTransform.rect;
            size = new Vector2(Mathf.Abs(rect.width), Mathf.Abs(rect.height));

            if (size.x > Mathf.Epsilon && size.y > Mathf.Epsilon)
            {
                return true;
            }

            size = new Vector2(
                Mathf.Abs(rectTransform.sizeDelta.x),
                Mathf.Abs(rectTransform.sizeDelta.y)
            );

            return size.x > Mathf.Epsilon && size.y > Mathf.Epsilon;
        }

        #endregion
    }
}
