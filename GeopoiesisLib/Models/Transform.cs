using Geopoiesis.Interfaces;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis.Models
{
    public class Transform : ITransform
    {
        public ITransform Parent { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Scale { get; set; }
        public Quaternion Rotation { get; set; }

        protected Matrix _world;
        public Matrix World
        {
            get
            {
                _world = Matrix.CreateScale(Scale) * Matrix.CreateFromQuaternion(Rotation) * Matrix.CreateTranslation(Position);

                if (Parent != null)
                    _world *= Parent.World;

                return _world;
            }
            set
            {
                _world = value;
            }
        }

        public Transform()
        {
            Position = Vector3.Zero;
            Scale = Vector3.One;
            Rotation = Quaternion.Identity;

            Parent = null;
        }

        public Transform(ITransform transform) : this()
        {
            Parent = transform;
        }


        public override string ToString()
        {
            return $"{{{Position} {Scale} {Rotation}}}";
        }

        public void Translate(Vector3 distance)
        {
            Position += Vector3.Transform(distance, Rotation);
        }

        public void Rotate(Vector3 axis, float angle)
        {
            axis = Vector3.Transform(axis, Matrix.CreateFromQuaternion(Rotation));
            Rotation = Quaternion.Normalize(Quaternion.CreateFromAxisAngle(axis, angle) * Rotation);
        }
    }
}
