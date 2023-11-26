using System;
using UnityEngine;

namespace ZSTUnity.Avocode.Exceptions
{
    public class Fail : Exception
    {
        public GameObject GameObject { get; private set; }
        public override string StackTrace => $"In Game Object: {GameObject.name}";

        public Fail(string message, GameObject gameObject) : base(message)
        {
            GameObject = gameObject;
        }
    }
}