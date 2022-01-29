using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace Geopoiesis
{
    public static class ExtensionMethods
    {
        static Vector3 AngleTo(Vector3 from, Vector3 location)
        {
            Vector3 angle = new Vector3();
            Vector3 v3 = Vector3.Normalize(location - from);

            angle.X = (float)Math.Asin(v3.Y);
            angle.Y = (float)Math.Atan2((double)-v3.X, (double)-v3.Z);

            return angle;
        }

        public static Quaternion SetFromToRotation(this Quaternion q, Vector3 from, Vector3 to)
        {

            Quaternion nq = q.Look(from, 1, Vector3.Forward);
            return nq.Look(to, 1, Vector3.Forward);
        }

        public static Vector3 GetEuler(this Quaternion q)
        {
            Vector3 rotationaxes = new Vector3();
            Vector3 forward = Vector3.Transform(Vector3.Forward, q);
            Vector3 up = Vector3.Transform(Vector3.Up, q);

            rotationaxes = AngleTo(new Vector3(), forward);

            if (rotationaxes.X == MathHelper.PiOver2)
            {
                rotationaxes.Y = (float)Math.Atan2((double)up.X, (double)up.Z);
                rotationaxes.Z = 0;
            }
            else if (rotationaxes.X == -MathHelper.PiOver2)
            {
                rotationaxes.Y = (float)Math.Atan2((double)-up.X, (double)-up.Z);
                rotationaxes.Z = 0;
            }
            else
            {
                up = Vector3.Transform(up, Matrix.CreateRotationY(-rotationaxes.Y));
                up = Vector3.Transform(up, Matrix.CreateRotationX(-rotationaxes.X));

                //rotationaxes.Z = (float)Math.Atan2((double)-up.Z, (double)up.Y);
                rotationaxes.Z = (float)Math.Atan2((double)-up.X, (double)up.Y);
            }

            return rotationaxes;
        }
        public static Quaternion SetEuler(this Quaternion q, Vector3 euler)
        {
            return Quaternion.CreateFromRotationMatrix(Matrix.CreateFromYawPitchRoll(euler.Y, euler.X, euler.Z));
        }

        public static Quaternion Rotate(this Quaternion q, Vector3 axis, float angle)
        {
            axis = Vector3.Transform(axis, Matrix.CreateFromQuaternion(q));
            return Quaternion.Normalize(Quaternion.CreateFromAxisAngle(axis, angle) * q);
        }

        public static Quaternion Look(this Quaternion q, Vector3 dir,  float speed, Vector3 fwd)
        {
            if (fwd == Vector3.Zero)
                fwd = Vector3.Forward;

            Vector3 ominusp = fwd;

            if (dir == Vector3.Zero)
                return q;

            dir.Normalize();

            float theta = (float)Math.Acos(Vector3.Dot(dir, ominusp));
            Vector3 cross = Vector3.Cross(ominusp, dir);

            if (cross == Vector3.Zero)
                return q;

            cross.Normalize();

            Quaternion targetQ = Quaternion.CreateFromAxisAngle(cross, theta);
            return Quaternion.Slerp(q, targetQ, speed);
        }
    }
}
