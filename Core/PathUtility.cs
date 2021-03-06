﻿/*
 * Copyright (c) 2014 Akilram Krishnan

 * Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated documentation files (the "Software"),
 * to deal in the Software without restriction, including without limitation the rights to use, copy, modify, merge, publish, distribute, sublicense,
 * and/or sell copies of the Software, and to permit persons to whom the Software is furnished to do so, subject to the following conditions:

 * The above copyright notice and this permission notice shall be included in all copies or substantial portions of the Software.

 * THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR
 * A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF
 * CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE SOFTWARE.
 *
 * Akilram.com
 */

/* ADDING NEW PATHS
 * When a new path is added to this library, update this PathHelper class by
 *      1) add the path's component class type to the ReadOnlyCollection<Type> PathTypes
 *      2) update the GetPath3D(GameObject g) function with support for your path component class
 */

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using UnityEngine;

namespace Paths
{
    /// <summary>
    /// Static class with helper functions when dealing with Path components.
    /// </summary>
    public static class PathUtility
    {
        /// <summary>
        /// Verified path types.
        /// </summary>
        public static readonly ReadOnlyCollection<Type> PathTypes =
            new ReadOnlyCollection<Type>(
                new Type[]
                {
                    typeof(QuadraticBezierPathComponent),
                    typeof(CubicBezierPathComponent),
                    typeof(DynamicBezierPathComponent),
                    typeof(LinePathComponent),
                    typeof(CirclePathComponent)
                }
            );

        #region Parse

        /// <summary>
        /// Searches g for a Path component's Path.
        /// </summary>
        public static Path3D GetPath3D(GameObject g)
        {
            Type pathType;
            return GetPath3D(g, out pathType);
        }

        public static Path3D GetPath3D(GameObject g, out Type pathType)
        {
            pathType = GetPathType(g);
            Path3D path = null;

            if (pathType == null)
                return path;

            // Based on the path type, use the appropriate logic to get the path.

            if (pathType.Equals(typeof(QuadraticBezierPathComponent)))
                path = g.GetComponent<QuadraticBezierPathComponent>().Path;
            else if (pathType.Equals(typeof(CubicBezierPathComponent)))
                path = g.GetComponent<CubicBezierPathComponent>().Path;
            else if (pathType.Equals(typeof(DynamicBezierPathComponent)))
                path = g.GetComponent<DynamicBezierPathComponent>().Path;
            else if (pathType.Equals(typeof(CirclePathComponent)))
                path = g.GetComponent<CirclePathComponent>().Path;
            else if (pathType.Equals(typeof(LinePathComponent)))
                path = g.GetComponent<LinePathComponent>().Path;

            return path;
        }

        /// <summary>
        /// Returns the type of path component attached to a GameObject.
        /// </summary>
        public static Type GetPathType(GameObject g)
        {
            List<Type> pathTypesInG = new List<Type>();
            foreach (Type t in PathTypes)
            {
                if (g.GetComponent(t) != null)
                    pathTypesInG.Add(t);
            }

            return CheckGetList<Type>(pathTypesInG, g);
        }

        /// <summary>
        /// Returns the path component attached to a GameObject.
        /// </summary>
        public static Component GetPathComponent(GameObject g)
        {
            List<Component> pathComponentsInG = new List<Component>();
            foreach (Type t in PathTypes)
            {
                Component candidate = g.GetComponent(t);
                if (candidate != null)
                    pathComponentsInG.Add(candidate);
            }

            return CheckGetList<Component>(pathComponentsInG, g);
        }

        private static T CheckGetList<T>(List<T> list, GameObject g) where T : class
        {
            //~ check for cases where the list doesn't contain 1 item
            if (list.Count == 0)
                return null;
            else if (list.Count > 1)
            {
                Debug.LogError(
                    "GameObject " + g.name + " contains more than one path component."
                    + "The first path component found will be used."
                );
            }

            return list[0];
        }

        #endregion Parse

        #region DebugDraw

        public static void DebugDrawSpline(ISpline spline, int numMidPoints)
        {
            if (numMidPoints < 0)
                throw new System.ArgumentOutOfRangeException("numMidPoints");

            for (float i = 0; i < numMidPoints; i++)
            {
                Debug.DrawLine(
                    spline.Evaluate(i / numMidPoints),
                    spline.Evaluate((i + 1.0f) / numMidPoints)
                    );
            }
        }

        public static void DebugDrawSpline(ISpline spline, int numMidPoints, Color color)
        {
            if (numMidPoints < 0)
                throw new System.ArgumentOutOfRangeException("numMidPoints");

            for (float i = 0; i < numMidPoints; i++)
            {
                Debug.DrawLine(
                    spline.Evaluate(i / numMidPoints),
                    spline.Evaluate((i + 1.0f) / numMidPoints),
                    color
                    );
            }
        }

        public static void DebugDrawSpline(ISpline spline, int numMidPoints, Color color, float duration)
        {
            if (numMidPoints < 0)
                throw new System.ArgumentOutOfRangeException("numMidPoints");

            for (float i = 0; i < numMidPoints; i++)
            {
                Debug.DrawLine(
                    spline.Evaluate(i / numMidPoints),
                    spline.Evaluate((i + 1.0f) / numMidPoints),
                    color,
                    duration
                    );
            }
        }

        public static void DebugDrawSpline(ISpline spline, int numMidPoints, Color color, float duration, bool depthTest)
        {
            if (numMidPoints < 0)
                throw new System.ArgumentOutOfRangeException("numMidPoints");

            for (float i = 0; i < numMidPoints; i++)
            {
                Debug.DrawLine(
                    spline.Evaluate(i / numMidPoints),
                    spline.Evaluate((i + 1.0f) / numMidPoints),
                    color,
                    duration,
                    depthTest
                    );
            }
        }

        #endregion DebugDraw

        #region Twist

        public static Quaternion Twist(ISpline core, float t_core, ISpline guide, float t_guide)
        {
            Vector3 corePoint = core.Evaluate(t_core);
            Vector3 rotationPoint = guide.Evaluate(t_guide);

            Vector3 tangentBasis = core.Tangent(t_core);
            tangentBasis = tangentBasis.normalized;

            Vector3 normalBasis = Vector3.Cross(tangentBasis, rotationPoint - corePoint);
            normalBasis = normalBasis.normalized;

            #region Uncomment this region to debug all Twist invocations

            /*
            //Vector3 binormalBasis = Vector3.Cross(normalBasis, tangentBasis);
            //binormalBasis = binormalBasis.normalized;

            //Debug.DrawLine(corePoint, corePoint + tangentBasis, Color.blue);
            //Debug.DrawLine(corePoint, corePoint + normalBasis, Color.green);
            //Debug.DrawLine(corePoint, corePoint + binormalBasis, Color.red);
            */

            #endregion Uncomment this region to debug all Twist invocations

            return Quaternion.LookRotation(tangentBasis, normalBasis);
        }

        #endregion Twist

    }
}