using System.Collections;
using UnityEngine;

namespace NitroxClient.Unity.Helper
{
    public class CoroutineWithData
    {
        public Coroutine Coroutine { get; }
        public object Result;
        private readonly IEnumerator target;

        public CoroutineWithData(MonoBehaviour owner, IEnumerator enumerator)
        {
            target = enumerator;
            Coroutine = owner.StartCoroutine(Run());
        }

        private IEnumerator Run()
        {
            while (target.MoveNext())
            {
                Result = target.Current;
                yield return Result;
            }
        }
    }
}
