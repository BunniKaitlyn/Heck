﻿using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Heck.Animation
{
    public class Vector4PointDefinition : PointDefinition<Vector4>
    {
        internal Vector4PointDefinition(IReadOnlyCollection<object> points)
            : base(points)
        {
        }

        internal static Vector3 DivideByComponent(Vector4 val1, Vector4 val2)
        {
            return new Vector3(val1.x / val2.x, val1.y / val2.y, val1.z / val2.z);
        }

        protected override bool Compare(Vector4 val1, Vector4 val2) => val1.EqualsTo(val2);

        protected override Vector4 InterpolatePoints(List<IPointData> points, int l, int r, float time)
        {
            PointData pointRData = (PointData)points[r];
            Vector4 pointL = points[l].Point;
            Vector4 pointR = pointRData.Point;
            if (!pointRData.HsvLerp)
            {
                return Vector4.LerpUnclamped(pointL, pointR, time);
            }

            Color.RGBToHSV(pointL, out float hl, out float sl, out float vl);
            Color.RGBToHSV(pointR, out float hr, out float sr, out float vr);
            Color lerped = Color.HSVToRGB(Mathf.LerpUnclamped(hl, hr, time), Mathf.LerpUnclamped(sl, sr, time), Mathf.LerpUnclamped(vl, vr, time));
            return new Vector4(lerped.r, lerped.g, lerped.b, Mathf.LerpUnclamped(pointL.w, pointR.w, time));
        }

        private protected override Modifier<Vector4> CreateModifier(float[] floats, Modifier<Vector4>[] modifiers, Operation operation)
        {
            Vector4 result = new(floats[0], floats[1], floats[2], floats[3]);
            return new Modifier(result, modifiers, operation);
        }

        private protected override IPointData CreatePointData(float[] floats, string[] flags, Modifier<Vector4>[] modifiers, Functions easing)
        {
            Vector4 result = new(floats[0], floats[1], floats[2], floats[3]);
            return new PointData(result, flags.Any(n => n == "lerpHSV"), floats[4], modifiers, easing);
        }

        private class PointData : Modifier, IPointData
        {
            internal PointData(Vector4 point, bool hsvLerp, float time, Modifier<Vector4>[] modifiers, Functions easing)
                : base(point, modifiers, default)
            {
                HsvLerp = hsvLerp;
                Time = time;
                Easing = easing;
            }

            public bool HsvLerp { get; }

            public float Time { get; }

            public Functions Easing { get; }
        }

        private class Modifier : Modifier<Vector4>
        {
            internal Modifier(Vector4 point, Modifier<Vector4>[] modifiers, Operation operation)
                : base(point, modifiers, operation)
            {
            }

            public override Vector4 Point
            {
                get
                {
                    return Modifiers.Aggregate(OriginalPoint, (current, modifier) => modifier.Operation switch
                    {
                        Operation.opAdd => current + modifier.Point,
                        Operation.opSub => current - modifier.Point,
                        Operation.opMult => Vector4.Scale(current, modifier.Point),
                        Operation.opDivide => DivideByComponent(current, modifier.Point),
                        _ => throw new InvalidOperationException($"[{modifier.Operation}] cannot be performed on type Vector3.")
                    });
                }
            }

            protected override string FormattedValue => $"{OriginalPoint.x}, {OriginalPoint.y}, {OriginalPoint.z}, {OriginalPoint.w}";
        }
    }
}