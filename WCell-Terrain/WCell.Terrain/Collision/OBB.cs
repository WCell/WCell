using System;
using WCell.Util.Graphics;

namespace WCell.Terrain.Collision
{
    /// <summary>
    /// Orientated Bounding Box
    /// </summary>
    public class OBB
    {
        private BoundingBox _aabb;
        private Vector3 _bounds;
        private Vector3 _center;
        private DirtyFlags _dirtyFlags;
        private Matrix _inverseRotation;
        private Matrix _rotation;
        private Matrix _worldTransform;


        /// <summary>
        /// Constructor
        /// </summary>
        public OBB()
            : this(Vector3.Zero, new Vector3(1.0f, 1.0f, 1.0f), Matrix.Identity)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OBB(Vector3 center, Vector3 bounds)
            : this(center, bounds, Matrix.Identity)
        {
        }

        /// <summary>
        /// Constructor
        /// </summary>
        public OBB(Vector3 center, Vector3 bounds, Matrix rotation)
        {
            _center = center;
            _bounds = bounds;

            _rotation = rotation;

            _inverseRotation = Matrix.Identity;

            _worldTransform = Matrix.Identity;

            _aabb = new BoundingBox(Vector3.Zero, Vector3.Zero);

            _dirtyFlags = DirtyFlags.WorldTransformDirty |
                          DirtyFlags.InverseRotationDirty |
                          DirtyFlags.LocalAABBDirty;
        }

        /// <summary>
        /// Bounds of this OBB. A single Vector 3 representing the distance from the center point to the edge of the box on all three axis if it were an AABB.
        /// </summary>
        public Vector3 Bounds
        {
            get { return _bounds; }
            set
            {
                _bounds = value;
                _dirtyFlags |= DirtyFlags.LocalAABBDirty;
            }
        }

        /// <summary>
        /// Vector3 center of this OBB
        /// </summary>
        public Vector3 Center
        {
            get { return _center; }
            set
            {
                _center = value;
                _dirtyFlags |= DirtyFlags.WorldTransformDirty;
            }
        }

        /// <summary>
        /// Rotation matrix for this OBB (the rotation matrix used from the AABB to get to the OBB);
        /// </summary>
        public Matrix Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                _dirtyFlags |= DirtyFlags.InverseRotationDirty;
                _dirtyFlags |= DirtyFlags.WorldTransformDirty;
            }
        }

        /// <summary>
        /// Inverse rotation for this OBB
        /// </summary>
        public Matrix InverseRotation
        {
            get
            {
                if ((_dirtyFlags & DirtyFlags.InverseRotationDirty) ==
                    DirtyFlags.InverseRotationDirty)
                {
                    _inverseRotation = Matrix.Invert(_rotation);
                    _dirtyFlags ^= DirtyFlags.InverseRotationDirty;
                }
                return _inverseRotation;
            }
        }

        /// <summary>
        /// Not sure on this one
        /// </summary>
        public Matrix WorldTransform
        {
            get
            {
                if ((_dirtyFlags & DirtyFlags.WorldTransformDirty) == DirtyFlags.WorldTransformDirty)
                {
                    _worldTransform = Rotation*Matrix.CreateTranslation(_center);
                    _dirtyFlags ^= DirtyFlags.WorldTransformDirty;
                }
                return _worldTransform;
            }
        }

        /// <summary>
        /// Not sure on this one...
        /// </summary>
        public BoundingBox LocalBoundingBox
        {
            get
            {
                if ((_dirtyFlags & DirtyFlags.LocalAABBDirty) == DirtyFlags.LocalAABBDirty)
                {
                    _aabb.Max.X = _bounds.X;
                    _aabb.Max.Y = _bounds.Y;
                    _aabb.Max.Z = _bounds.Z;

                    _aabb.Min.X = -_bounds.X;
                    _aabb.Min.Y = -_bounds.Y;
                    _aabb.Min.Z = -_bounds.Z;

                    _dirtyFlags ^= DirtyFlags.LocalAABBDirty;
                }
                return _aabb;
            }
        }

        /// <summary>
        /// Intersection test between this OBB and another OBB
        /// </summary>
        /// <param name="b">OBB to test against</param>
        /// <returns>Boolean result if they intersect or not</returns>
        public bool Intersects(OBB b)
        {
            Matrix matB = b._rotation*InverseRotation;
            Vector3 vPosB = Vector3.Transform(b._center - _center, _inverseRotation);

            var xAxis = new Vector3(matB.M11, matB.M21, matB.M31);
            var yAxis = new Vector3(matB.M12, matB.M22, matB.M32);
            var zAxis = new Vector3(matB.M13, matB.M23, matB.M33);

            //15 tests

            //1 (Ra)x
            if (Math.Abs(vPosB.X) >
                (_bounds.X +
                 b._bounds.X*Math.Abs(xAxis.X) +
                 b._bounds.Y*Math.Abs(xAxis.Y) +
                 b._bounds.Z*Math.Abs(xAxis.Z)))
            {
                return false;
            }

            //2 (Ra)y
            if (Math.Abs(vPosB.Y) >
                (_bounds.Y +
                 b._bounds.X*Math.Abs(yAxis.X) +
                 b._bounds.Y*Math.Abs(yAxis.Y) +
                 b._bounds.Z*Math.Abs(yAxis.Z)))
            {
                return false;
            }

            //3 (Ra)z
            if (Math.Abs(vPosB.Z) >
                (_bounds.Z +
                 b._bounds.X*Math.Abs(zAxis.X) +
                 b._bounds.Y*Math.Abs(zAxis.Y) +
                 b._bounds.Z*Math.Abs(zAxis.Z)))
            {
                return false;
            }

            //4 (Rb)x
            if (Math.Abs(vPosB.X*xAxis.X +
                         vPosB.Y*yAxis.X +
                         vPosB.Z*zAxis.X) >
                (b._bounds.X +
                 _bounds.X*Math.Abs(xAxis.X) +
                 _bounds.Y*Math.Abs(yAxis.X) +
                 _bounds.Z*Math.Abs(zAxis.X)))
            {
                return false;
            }

            //5 (Rb)y
            if (Math.Abs(vPosB.X*xAxis.Y +
                         vPosB.Y*yAxis.Y +
                         vPosB.Z*zAxis.Y) >
                (b._bounds.Y +
                 _bounds.X*Math.Abs(xAxis.Y) +
                 _bounds.Y*Math.Abs(yAxis.Y) +
                 _bounds.Z*Math.Abs(zAxis.Y)))
            {
                return false;
            }

            //6 (Rb)z
            if (Math.Abs(vPosB.X*xAxis.Z +
                         vPosB.Y*yAxis.Z +
                         vPosB.Z*zAxis.Z) >
                (b._bounds.Z +
                 _bounds.X*Math.Abs(xAxis.Z) +
                 _bounds.Y*Math.Abs(yAxis.Z) +
                 _bounds.Z*Math.Abs(zAxis.Z)))
            {
                return false;
            }

            //7 (Ra)x X (Rb)x
            if (Math.Abs(vPosB.Z*yAxis.X -
                         vPosB.Y*zAxis.X) >
                (_bounds.Y*Math.Abs(zAxis.X) +
                 _bounds.Z*Math.Abs(yAxis.X) +
                 b._bounds.Y*Math.Abs(xAxis.Z) +
                 b._bounds.Z*Math.Abs(xAxis.Y)))
            {
                return false;
            }

            //8 (Ra)x X (Rb)y
            if (Math.Abs(vPosB.Z*yAxis.Y -
                         vPosB.Y*zAxis.Y) >
                (_bounds.Y*Math.Abs(zAxis.Y) +
                 _bounds.Z*Math.Abs(yAxis.Y) +
                 b._bounds.X*Math.Abs(xAxis.Z) +
                 b._bounds.Z*Math.Abs(xAxis.X)))
            {
                return false;
            }

            //9 (Ra)x X (Rb)z
            if (Math.Abs(vPosB.Z*yAxis.Z -
                         vPosB.Y*zAxis.Z) >
                (_bounds.Y*Math.Abs(zAxis.Z) +
                 _bounds.Z*Math.Abs(yAxis.Z) +
                 b._bounds.X*Math.Abs(xAxis.Y) +
                 b._bounds.Y*Math.Abs(xAxis.X)))
            {
                return false;
            }

            //10 (Ra)y X (Rb)x
            if (Math.Abs(vPosB.X*zAxis.X -
                         vPosB.Z*xAxis.X) >
                (_bounds.X*Math.Abs(zAxis.X) +
                 _bounds.Z*Math.Abs(xAxis.X) +
                 b._bounds.Y*Math.Abs(yAxis.Z) +
                 b._bounds.Z*Math.Abs(yAxis.Y)))
            {
                return false;
            }

            //11 (Ra)y X (Rb)y
            if (Math.Abs(vPosB.X*zAxis.Y -
                         vPosB.Z*xAxis.Y) >
                (_bounds.X*Math.Abs(zAxis.Y) +
                 _bounds.Z*Math.Abs(xAxis.Y) +
                 b._bounds.X*Math.Abs(yAxis.Z) +
                 b._bounds.Z*Math.Abs(yAxis.X)))
            {
                return false;
            }

            //12 (Ra)y X (Rb)z
            if (Math.Abs(vPosB.X*zAxis.Z -
                         vPosB.Z*xAxis.Z) >
                (_bounds.X*Math.Abs(zAxis.Z) +
                 _bounds.Z*Math.Abs(xAxis.Z) +
                 b._bounds.X*Math.Abs(yAxis.Y) +
                 b._bounds.Y*Math.Abs(yAxis.X)))
            {
                return false;
            }

            //13 (Ra)z X (Rb)x
            if (Math.Abs(vPosB.Y*xAxis.X -
                         vPosB.X*yAxis.X) >
                (_bounds.X*Math.Abs(yAxis.X) +
                 _bounds.Y*Math.Abs(xAxis.X) +
                 b._bounds.Y*Math.Abs(zAxis.Z) +
                 b._bounds.Z*Math.Abs(zAxis.Y)))
            {
                return false;
            }

            //14 (Ra)z X (Rb)y
            if (Math.Abs(vPosB.Y*xAxis.Y -
                         vPosB.X*yAxis.Y) >
                (_bounds.X*Math.Abs(yAxis.Y) +
                 _bounds.Y*Math.Abs(xAxis.Y) +
                 b._bounds.X*Math.Abs(zAxis.Z) +
                 b._bounds.Z*Math.Abs(zAxis.X)))
            {
                return false;
            }

            //15 (Ra)z X (Rb)z
            if (Math.Abs(vPosB.Y*xAxis.Z -
                         vPosB.X*yAxis.Z) >
                (_bounds.X*Math.Abs(yAxis.Z) +
                 _bounds.Y*Math.Abs(xAxis.Z) +
                 b._bounds.X*Math.Abs(zAxis.Y) +
                 b._bounds.Y*Math.Abs(zAxis.X)))
            {
                return false;
            }

            return true;
        }

        #region Nested type: DirtyFlags

        [Flags]
        private enum DirtyFlags
        {
            None = 0,
            InverseRotationDirty = 1,
            WorldTransformDirty = 2,
            LocalAABBDirty = 4,
        }

        #endregion
    }
}