#if UNITY_EDITOR

using System;
using System.Collections;
using UnityEngine;

namespace OortUnity.Editor
{
    [AddComponentMenu("")]
    public sealed class GameViewScreenshotCaptureRunner : MonoBehaviour
    {
        private Action _onFinished;
        private bool _finished;

        public static void Run(IEnumerator routine, Action onFinished)
        {
            if (routine == null)
            {
                throw new ArgumentNullException(nameof(routine));
            }

            var gameObject = new GameObject("Oort Game View Screenshot Capture Runner")
            {
                hideFlags = HideFlags.HideAndDontSave,
            };
            var runner = gameObject.AddComponent<GameViewScreenshotCaptureRunner>();

            runner._onFinished = onFinished;
            runner.StartCoroutine(runner.Execute(routine));
        }

        private IEnumerator Execute(IEnumerator routine)
        {
            yield return StartCoroutine(routine);

            Finish();
            Destroy(gameObject);
        }

        private void OnDestroy()
        {
            Finish();
        }

        private void Finish()
        {
            if (_finished)
            {
                return;
            }

            _finished = true;
            _onFinished?.Invoke();
            _onFinished = null;
        }
    }
}

#endif
