﻿using UnityEngine;

namespace LSCore.Extensions.Unity
{
    public static class Vector2Extensions
    {
        public static Vector2 Project(this in Vector2 a, Vector2 b)
        {
            float num1 = b.x * b.x + b.y * b.y; //Vector2.Dot
            if (num1 == 0f) return Vector2.zero;
            float num2 = a.x * b.x + a.y * b.y; //Vector2.Dot
            num2 /= num1;
            b.x *= num2;
            b.y *= num2;
            return b;
        }
        
        public static float UnclampedInverseLerp(this in Vector2 value, Vector2 a, Vector2 b)
        {
            b.x -= a.x;
            b.y -= a.y;
            a.x = value.x - a.x;
            a.y = value.y - a.y;
            
            //Vector3.Project
            float num1 = b.x * b.x + b.y * b.y; //Vector3.Dot
            if (num1 == 0f) return 0f;
            float num2 = a.x * b.x + a.y * b.y; //Vector3.Dot
            a.x = b.x * num2 / num1;
            a.y = b.y * num2 / num1;
            //Vector3.Project
            
            float t = a.magnitude / b.magnitude;
            
            if (b.x * a.x + b.y * a.y < 0f) //Vector3.Dot
                t *= -1;
            
            return t;
        }
        
        public static float InverseLerp(this in Vector2 value, Vector2 a, Vector2 b)
        {
            b.x -= a.x;
            b.y -= a.y;
            a.x = value.x - a.x;
            a.y = value.y - a.y;
            
            //Vector3.Project
            float num1 = b.x * b.x + b.y * b.y; //Vector3.Dot
            if (num1 == 0f) return 0f;
            float num2 = a.x * b.x + a.y * b.y; //Vector3.Dot
            a.x = b.x * num2 / num1;
            a.y = b.y * num2 / num1;
            //Vector3.Project

            float bMagnitude = b.magnitude;
            float t = a.magnitude / bMagnitude;
            
            if (b.x * a.x + b.y * a.y < 0) //Vector3.Dot
                t *= -1;
            
            return Mathf.InverseLerp(0f, bMagnitude, t * bMagnitude);
        }
        
        
        public static Vector2 Rotate(this Vector2 v, in float degrees)
        {
            float radianAngle = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radianAngle);
            float cos = Mathf.Cos(radianAngle);
            
            float x = v.x;
            float y = v.y;
            v.x = cos * x - sin * y;
            v.y = sin * x + cos * y;
            return v;
        }
        
        public static Vector2 RotateAroundPoint(this Vector2 point, in Vector2 pivot, in float degrees)
        {
            point.x -= pivot.x;
            point.y -= pivot.y;
            
            //Rotate
            float radianAngle = degrees * Mathf.Deg2Rad;
            float sin = Mathf.Sin(radianAngle);
            float cos = Mathf.Cos(radianAngle);

            float x = point.x;
            float y = point.y;
            point.x = cos * x - sin * y;
            point.y = sin * x + cos * y;
            //Rotate
            
            point.x += pivot.x;
            point.y += pivot.y;

            return point;
        }


        public static float GetAspect(this in Vector2 target)
        {
            if (ScreenExt.IsPortrait)
            {
                return target.x / target.y;
            }
            
            return target.y / target.x;
        }
        
        public static float GetAspect(this in Vector2 target, ScreenOrientation orientation)
        {
            if (orientation == ScreenOrientation.Portrait)
            {
                return target.x / target.y;
            }
            
            return target.y / target.x;
        }
    }
}