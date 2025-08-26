using System;

namespace Script.Define
{
    public class LoadingProgressResult : ICloneable
    {
        public float amount;

        public object Clone()
        {
            LoadingProgressResult result = new LoadingProgressResult();
            result.amount = amount;
            return result;
        }
    }
}