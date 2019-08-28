﻿using System.Numerics;

namespace App.RayTrace
{
    public abstract class Entity : IPositionable
    {
        protected Vector3 _translation;

        public Vector3 Translation { get => _translation; set => _translation = value; }
    }
}