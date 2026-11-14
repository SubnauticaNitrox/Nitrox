using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class InvokeHelper : MonoBehaviour
{
    private static InvokeHelper instance = null!;
    private readonly List<Action> runNextFrame = [];

    public void Awake()
    {
        instance = this;
    }

    public void LateUpdate()
    {
        List<Action> nextFrame = [.. runNextFrame];
        runNextFrame.Clear();

        foreach (Action callback in nextFrame)
        {
            try
            {
                callback();
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }
    }

    public static void InvokeNextFrame(Action callback)
    {
        instance.runNextFrame.Add(callback);
    }
}
