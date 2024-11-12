﻿// MIT License - Copyright (C) The Mono.Xna Team
// This file is subject to the terms and conditions defined in
// file 'LICENSE.txt', which is part of this source code package.

// Copyright (C)2024 Nick Kastellanos

using System;
using System.Collections.Generic;

namespace Microsoft.Xna.Framework
{
    internal class IntersectsHelper
    {
        // adapted from http://www.scratchapixel.com/lessons/3d-basic-lessons/lesson-7-intersecting-simple-shapes/ray-box-intersection/
        internal static void BoundingBoxIntersectsRay(ref BoundingBox box, ref Ray ray, out float? result)
        {
            const float Epsilon = 1e-6f;

            float? tMin = null, tMax = null;

            if (Math.Abs(ray.Direction.X) < Epsilon)
            {
                if (ray.Position.X < box.Min.X || ray.Position.X > box.Max.X)
                {
                    result = null;
                    return;
                }
            }
            else
            {
                tMin = (box.Min.X - ray.Position.X) / ray.Direction.X;
                tMax = (box.Max.X - ray.Position.X) / ray.Direction.X;

                if (tMin > tMax)
                {
                    var temp = tMin;
                    tMin = tMax;
                    tMax = temp;
                }
            }

            if (Math.Abs(ray.Direction.Y) < Epsilon)
            {
                if (ray.Position.Y < box.Min.Y || ray.Position.Y > box.Max.Y)
                {
                    result = null;
                    return;
                }
            }
            else
            {
                var tMinY = (box.Min.Y - ray.Position.Y) / ray.Direction.Y;
                var tMaxY = (box.Max.Y - ray.Position.Y) / ray.Direction.Y;

                if (tMinY > tMaxY)
                {
                    var temp = tMinY;
                    tMinY = tMaxY;
                    tMaxY = temp;
                }

                if ((tMin.HasValue && tMin > tMaxY) || (tMax.HasValue && tMinY > tMax))
                {
                    result = null;
                    return;
                }

                if (!tMin.HasValue || tMinY > tMin) tMin = tMinY;
                if (!tMax.HasValue || tMaxY < tMax) tMax = tMaxY;
            }

            if (Math.Abs(ray.Direction.Z) < Epsilon)
            {
                if (ray.Position.Z < box.Min.Z || ray.Position.Z > box.Max.Z)
                {
                    result = null;
                    return;
                }
            }
            else
            {
                var tMinZ = (box.Min.Z - ray.Position.Z) / ray.Direction.Z;
                var tMaxZ = (box.Max.Z - ray.Position.Z) / ray.Direction.Z;

                if (tMinZ > tMaxZ)
                {
                    var temp = tMinZ;
                    tMinZ = tMaxZ;
                    tMaxZ = temp;
                }

                if ((tMin.HasValue && tMin > tMaxZ) || (tMax.HasValue && tMinZ > tMax))
                {
                    result = null;
                    return;
                }

                if (!tMin.HasValue || tMinZ > tMin) tMin = tMinZ;
                if (!tMax.HasValue || tMaxZ < tMax) tMax = tMaxZ;
            }

            // having a positive tMax and a negative tMin means the ray is inside the box
            // we expect the intesection distance to be 0 in that case
            if ((tMin.HasValue && tMin < 0) && tMax > 0)
            {
                result = 0;
                return;
            }

            // a negative tMin means that the intersection point is behind the ray's origin
            // we discard these as not hitting the AABB
            if (tMin < 0)
            {
                result = null;
                return;
            }

            result = tMin;
            return;
        }
    }
}
