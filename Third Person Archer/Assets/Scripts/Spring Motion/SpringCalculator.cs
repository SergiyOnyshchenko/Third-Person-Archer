using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct DampedSpringMotionParams
{
    public float PosPosCoef;
    public float PosVelCoef;
    public float VelPosCoef;
    public float VelVelCoef;
}

public static class SpringCalculator 
{
    public static void CalcDampedSpringMotionParams(
            ref DampedSpringMotionParams pOutParams,
            float deltaTime,
            float angularFrequency,
            float dampingRatio)
        {
            const float epsilon = 0.0001f;

            if (dampingRatio < 0.0f) dampingRatio = 0.0f;
            if (angularFrequency < 0.0f) angularFrequency = 0.0f;

            if (angularFrequency < epsilon)
            {
                pOutParams.PosPosCoef = 1.0f;
                pOutParams.PosVelCoef = 0.0f;
                pOutParams.VelPosCoef = 0.0f;
                pOutParams.VelVelCoef = 1.0f;
                return;
            }

            if (dampingRatio > 1.0f + epsilon)
            {
                float za = -angularFrequency * dampingRatio;
                float zb = angularFrequency * Mathf.Sqrt(dampingRatio * dampingRatio - 1.0f);
                float z1 = za - zb;
                float z2 = za + zb;

                float e1 = Mathf.Exp(z1 * deltaTime);
                float e2 = Mathf.Exp(z2 * deltaTime);

                float invTwoZb = 1.0f / (2.0f * zb);
                float e1_Over_TwoZb = e1 * invTwoZb;
                float e2_Over_TwoZb = e2 * invTwoZb;
                float z1e1_Over_TwoZb = z1 * e1_Over_TwoZb;
                float z2e2_Over_TwoZb = z2 * e2_Over_TwoZb;

                pOutParams.PosPosCoef = e1_Over_TwoZb * z2 - z2e2_Over_TwoZb + e2;
                pOutParams.PosVelCoef = -e1_Over_TwoZb + e2_Over_TwoZb;
                pOutParams.VelPosCoef = (z1e1_Over_TwoZb - z2e2_Over_TwoZb + e2) * z2;
                pOutParams.VelVelCoef = -z1e1_Over_TwoZb + z2e2_Over_TwoZb;
            }
            else if (dampingRatio < 1.0f - epsilon)
            {
                float omegaZeta = angularFrequency * dampingRatio;
                float alpha = angularFrequency * Mathf.Sqrt(1.0f - dampingRatio * dampingRatio);
                float expTerm = Mathf.Exp(-omegaZeta * deltaTime);
                float cosTerm = Mathf.Cos(alpha * deltaTime);
                float sinTerm = Mathf.Sin(alpha * deltaTime);
                float invAlpha = 1.0f / alpha;
                float expSin = expTerm * sinTerm;
                float expCos = expTerm * cosTerm;
                float expOmegaZetaSin_Over_Alpha = expTerm * omegaZeta * sinTerm * invAlpha;

                pOutParams.PosPosCoef = expCos + expOmegaZetaSin_Over_Alpha;
                pOutParams.PosVelCoef = expSin * invAlpha;
                pOutParams.VelPosCoef = -expSin * alpha - omegaZeta * expOmegaZetaSin_Over_Alpha;
                pOutParams.VelVelCoef = expCos - expOmegaZetaSin_Over_Alpha;
            }
            else
            {
                float expTerm = Mathf.Exp(-angularFrequency * deltaTime);
                float timeExp = deltaTime * expTerm;
                float timeExpFreq = timeExp * angularFrequency;

                pOutParams.PosPosCoef = timeExpFreq + expTerm;
                pOutParams.PosVelCoef = timeExp;
                pOutParams.VelPosCoef = -angularFrequency * timeExpFreq;
                pOutParams.VelVelCoef = -timeExpFreq + expTerm;
            }
        }

        public static void UpdateDampedSpringMotion(
            ref float pPos,
            ref float pVel,
            float equilibriumPos,
            DampedSpringMotionParams parameters)
        {
            float oldPos = pPos - equilibriumPos;
            float oldVel = pVel;

            pPos = oldPos * parameters.PosPosCoef + oldVel * parameters.PosVelCoef + equilibriumPos;
            pVel = oldPos * parameters.VelPosCoef + oldVel * parameters.VelVelCoef;
        }

        public static void UpdateDampedSpringMotion(
            ref Vector2 pPos,
            ref Vector2 pVel,
            Vector2 equilibriumPos,
            DampedSpringMotionParams parameters)
        {
            Vector2 oldPos = pPos - equilibriumPos;
            Vector2 oldVel = pVel;

            pPos = oldPos * parameters.PosPosCoef + oldVel * parameters.PosVelCoef + equilibriumPos;
            pVel = oldPos * parameters.VelPosCoef + oldVel * parameters.VelVelCoef;
        }

        public static void UpdateDampedSpringMotion(
            ref Vector3 pPos,
            ref Vector3 pVel,
            Vector3 equilibriumPos,
            DampedSpringMotionParams parameters)
        {
            Vector3 oldPos = pPos - equilibriumPos;
            Vector3 oldVel = pVel;

            pPos = oldPos * parameters.PosPosCoef + oldVel * parameters.PosVelCoef + equilibriumPos;
            pVel = oldPos * parameters.VelPosCoef + oldVel * parameters.VelVelCoef;
        }

        public static void UpdateDampedSpringMotion(
            ref Quaternion pRot,
            ref Quaternion pVel,
            Quaternion equilibriumRot,
            DampedSpringMotionParams parameters)
        {
            float rotX = pRot.x;
            float rotY = pRot.y;
            float rotZ = pRot.z;
            float rotW = pRot.w;

            float velX = pVel.x;
            float velY = pVel.y;
            float velZ = pVel.z;
            float velW = pVel.w;

            UpdateDampedSpringMotion(ref rotX, ref velX, equilibriumRot.x, parameters);
            UpdateDampedSpringMotion(ref rotY, ref velY, equilibriumRot.y, parameters);
            UpdateDampedSpringMotion(ref rotZ, ref velZ, equilibriumRot.z, parameters);
            UpdateDampedSpringMotion(ref rotW, ref velW, equilibriumRot.w, parameters);

            pRot = new Quaternion(rotX, rotY, rotZ, rotW);
            pVel = new Quaternion(velX, velY, velZ, velW);
        }
}
