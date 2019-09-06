﻿using App.Cameras;
using Dom.Raytrace;
using Fou.Maths;
using System.Numerics;

namespace App.RayTrace
{
    public class BackgroundTracer : ITracer
    {
        public BackgroundTracer(ISceneAccessor scene, IRayTraceCamera camera, int resolutionX, int resolutionY)
        {
            Scene = scene;
            Camera = camera;
            ResolutionX = resolutionX;
            ResolutionY = resolutionY;
        }

        public ISceneAccessor Scene { get; }
        public IRayTraceCamera Camera { get; }
        public int ResolutionX { get; }
        public int ResolutionY { get; }

        public Frame Trace()
        {
            var frame = new Frame(ResolutionX, ResolutionY);

            for (int y = 0; y < ResolutionY; y++)
            {
                for (int x = 0; x < ResolutionX; x++)
                {
                    var color = Vector3.Zero;
                    for (var s = 0; s < 50; s++)
                    {
                        var u = (x + Utils.Rand()) / ResolutionX;
                        var v = (y + Utils.Rand()) / ResolutionY;

                        var ray = Camera.GetRay(u, v);
                        var newColor = GetBackgroundContribution(ray);
                        var traceray = new TraceRay(ray, 0, 0.001f, float.MaxValue, newColor, Vector3.Zero, Vector3.Zero, 0);


                        var resultRay = ThrowRay(traceray);
                        color += resultRay.Color;
                    }

                    color /= 50;
                    frame.AddPixel(new Pixel(color), x, y);
                }
            }

            return frame;
        }

        private TraceRay ThrowRay(TraceRay traceray)
        {
            foreach (var hitable in Scene.Hitables)
            {
                var result = hitable.Value.TryHit(ref traceray, out var hitpoint);
                if (result)
                {
                    if (traceray.T < traceray.TMax)
                    {
                        traceray.TMax = traceray.T;
                        var id = hitable.Key;
                        traceray.Normal = hitpoint.Normal;
                        traceray.HitPoint = hitpoint.Point;
                        traceray = Scene.Scatterables[id].Scatter(traceray);
                        traceray.Depth++;
                        if (traceray.Depth > 10)
                        {
                            traceray = new TraceRay(traceray.Ray, traceray.T, traceray.TMin, traceray.TMax, new Vector3(0.0f), traceray.Normal, traceray.HitPoint, traceray.Depth);
                            break;
                        }
                        return ThrowRay(traceray);
                    } 
                }
            }
            return traceray;
        }

        private Vector3 GetBackgroundContribution(Ray ray)
        {
            var unitDirection = Vector3.Normalize(ray.Direction);
            var t = 0.5f * (unitDirection.Y + 1.0f);
            return (1.0f - t) * new Vector3(1.0f, 1.0f, 1.0f) + t * Scene.BackgroundColor;
        }
    }
}
