using System;
using System.Linq;
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Utils {
    public static class Utility {
        public const int badIdx = -1;

        public static Color transparent = new Color(1f, 1f, 1f, 0f);

        public static bool maybe { get { return UnityEngine.Random.Range(-1, 1) == -1 ? true : false; } }

        public static void hide(GameObject go) {
            go.SetActive(false);
        }

        public static void show(GameObject go) {
            go.SetActive(true);
        }

        public static GameObject create(string tag, GameObject parent) {
            GameObject go = null;
            if (!parent.transform.Find(tag)) {
                go = new GameObject(tag);
                go.transform.parent = parent.transform;
            }
            return go;
        }

        public static void parentTo(GameObject go, GameObject parent) {
            go.transform.parent = parent.transform;
        }

        public static float map(this float value, float fromMin, float fromMax, float toMin, float toMax) {
            float percent = Mathf.InverseLerp(fromMin, fromMax, value);
            return Mathf.Lerp(toMin, toMax, percent);
        }

        public static IEnumerator _colorOverTime(SpriteRenderer sr, Color to, float duration) {
            float elapsed = 0f;
            float step = Time.deltaTime / duration;
            while(elapsed < 1f) {
                sr.color = Color.Lerp(sr.color, to, step * 2.25f);
                elapsed += step;
                yield return null;
            }

            sr.color = to;
        }

        public static IEnumerator _rotateOverTime(Transform target, float angle, float duration) {
            float elapsed = 0f;
            float step = Time.deltaTime / duration;
            while(elapsed < 1f) {
                target.Rotate(Vector3.forward, angle);
                elapsed += step;
                yield return null;
            }
        }

        public static IEnumerable<T> Shuffle<T>(this IEnumerable<T> source, System.Random rng) {
            T[] elements = source.ToArray();
            for (int i = elements.Length - 1; i >= 0; i--) {
                int swapIndex = rng.Next(i + 1);
                yield return elements[swapIndex];
                elements[swapIndex] = elements[i];
            }
        }
    }

    public static class CoroutineExtension {
        static private readonly Dictionary<string, int> Runners = new Dictionary<string, int>();

        private static IEnumerator _doParallel(IEnumerator coroutine, MonoBehaviour parent, string groupName) {
            yield return parent.StartCoroutine(coroutine);
            Runners[groupName]--;
        }

        public static void parallel(this IEnumerator coroutine, MonoBehaviour parent, string groupName) {
            if (!Runners.ContainsKey(groupName)) Runners.Add(groupName, 0);

            Runners[groupName]++;
            parent.StartCoroutine(_doParallel(coroutine, parent, groupName));
        }

        public static bool inProcess(string groupName) {
            return (Runners.ContainsKey(groupName) && Runners[groupName] > 0);
        }
    }
}
