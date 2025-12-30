using System;
using UnityEngine;

public partial class BalanceConfig
{
    [Serializable]
    public sealed class RoundingModule
    {
        [Header("Money Rounding")]
        [Tooltip("Minimum money returned by rounding (if positive).")]
        [SerializeField] private int _minMoney = 10;

        [Tooltip("Use magnitude-based nice steps (10/50/100/500/1000).")]
        [SerializeField] private bool _useNiceMoneySteps = true;

        [Header("Token Rounding")]
        [Tooltip("Minimum tokens returned by rounding (if positive).")]
        [SerializeField] private int _minTokens = 1;

        [Tooltip("Token rounding step for small values (<10).")]
        [SerializeField] private float _tokenStepSmall = 1f;

        [Tooltip("Token rounding step for medium values (<50).")]
        [SerializeField] private float _tokenStepMedium = 2f;

        [Tooltip("Token rounding step for large values (>=50).")]
        [SerializeField] private float _tokenStepLarge = 5f;

        public int RoundMoneyToNiceInt(float raw)
        {
            if (raw <= 0f) return 0;

            float v = raw;

            if (_useNiceMoneySteps)
            {
                float abs = Mathf.Abs(v);
                float step =
                    abs < 100f ? 10f :
                    abs < 1000f ? 50f :
                    abs < 10000f ? 100f :
                    abs < 100000f ? 500f :
                    1000f;

                v = Mathf.Round(v / step) * step;
            }
            else
            {
                v = Mathf.Round(v);
            }

            int result = Mathf.RoundToInt(v);
            return Mathf.Max(_minMoney, result);
        }

        public float RoundTokensToNice(float raw)
        {
            if (raw <= 0f) return 0f;

            float abs = Mathf.Abs(raw);
            float step =
                abs < 10f ? _tokenStepSmall :
                abs < 50f ? _tokenStepMedium :
                _tokenStepLarge;

            float v = Mathf.Round(raw / step) * step;

            if (v > 0f && v < _minTokens)
                v = _minTokens;

            return v;
        }
    }
}