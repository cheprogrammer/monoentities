using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using MonoGame.Extended;

namespace MonoEntities
{
    public class Transform2DComponent : Component, IEnumerable<Transform2DComponent>
    {
        [Flags]
        private enum TransformFlags : byte
        {
            WorldMatrixIsDirty = 1,
            LocalMatrixIsDirty = 2,
            All = LocalMatrixIsDirty | WorldMatrixIsDirty
        }

        private TransformFlags _flags = TransformFlags.All;
        private Vector2 _scale = Vector2.One;
        private Matrix2D _localMatrix = Matrix2D.Identity;
        private Transform2DComponent _parent = null;
        private Matrix2D _worldMatrix = Matrix2D.Identity;
        private Vector2 _position;
        private Vector2 _origin;
        private float _rotation;
        private int _zIndex = 0;

        /// <summary>
        /// Gets the world position of object
        /// </summary>
        public Vector2 WorldPosition => WorldMatrix.Translation;

        /// <summary>
        /// Gets the world scale of object
        /// </summary>
        public Vector2 WorldScale => WorldMatrix.Scale;

        /// <summary>
        /// Gets the world rotation of object
        /// </summary>
        public float WorldRotation => WorldMatrix.Rotation;

        /// <summary>
        /// Gets or sets local position
        /// </summary>
        public Vector2 Position
        {
            get { return _position; }
            set
            {
                _position = value;
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        /// <summary>
        /// Gets or sets local origin position
        /// </summary>
        public Vector2 Origin
        {
            get { return _origin; }
            set
            {
                _origin = value;
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        /// <summary>
        /// Gets or sets local rotation in radians
        /// </summary>
        public float Rotation
        {
            get { return _rotation; }
            set
            {
                _rotation = value;
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        public float RotationDegrees
        {
            get { return MathHelper.ToDegrees(_rotation); }
            set
            {
                float radians = MathHelper.ToRadians(value);
                Rotation = radians;
            }
        }

        public override void Reset()
        {
            Position = Vector2.Zero;
            Origin = Vector2.Zero;
            Rotation = 0;
            Scale = Vector2.One;
            ZIndex = 0;
        }

        /// <summary>
        /// Gets or sets local Z Index
        /// </summary>
        public int ZIndex
        {
            get => _zIndex;
            set
            {
                if (_zIndex == value)
                    return;

                _zIndex = value;

                if (Service.IsDrawing)
                    throw new EcsWorkflowException("Cannot change Z Index during Draw phase");

                Service.Tree.UpdateSiblingsZIndex(Entity);
            }
        }

        /// <summary>
        /// Gets or sets local scale
        /// </summary>
        public Vector2 Scale
        {
            get => _scale;
            set
            {
                _scale = value;
                LocalMatrixBecameDirty();
                WorldMatrixBecameDirty();
            }
        }

        /// <summary>
        /// Gets the local matrix
        /// </summary>
        public Matrix2D LocalMatrix
        {
            get
            {
                RecalculateLocalMatrixIfNecessary();
                return _localMatrix;
            }
        }

        /// <summary>
        /// Gets the world matrix
        /// </summary>
        public Matrix2D WorldMatrix
        {
            get
            {
                RecalculateWorldMatrixIfNecessary();
                return _worldMatrix;
            }
        }

        /// <summary>
        /// Gets or sets parent transform component
        /// </summary>
        public Transform2DComponent Parent
        {
            get => _parent;
            set
            {
                if (_parent == value)
                    return;

                Transform2DComponent parent = Parent;
                _parent = value;
                OnParentChanged(parent, value);

                if(Service.IsDrawing)
                    throw new EcsWorkflowException("Cannot change parent during Draw phase");

                Service.Tree.ChangeParent(Entity, parent?.Entity, value?.Entity);
            }
        }

        internal event Action TransformBecameDirty;

        public void GetLocalMatrix(out Matrix2D matrix)
        {
            RecalculateLocalMatrixIfNecessary();
            matrix = _localMatrix;
        }

        public void GetWorldMatrix(out Matrix2D matrix)
        {
            RecalculateWorldMatrixIfNecessary();
            matrix = _worldMatrix;
        }

        protected internal void LocalMatrixBecameDirty()
        {
            _flags = _flags | TransformFlags.LocalMatrixIsDirty;
        }

        protected internal void WorldMatrixBecameDirty()
        {
            _flags = _flags | TransformFlags.WorldMatrixIsDirty;
            Action transformBecameDirty = TransformBecameDirty;
            transformBecameDirty?.Invoke();
        }

        private void OnParentChanged(Transform2DComponent oldParent, Transform2DComponent newParent)
        {
            for (Transform2DComponent transform2DComponent = oldParent;
                transform2DComponent != null;
                transform2DComponent = transform2DComponent.Parent)
                transform2DComponent.TransformBecameDirty -= ParentOnTransformBecameDirty;
            for (Transform2DComponent transform2DComponent = newParent;
                transform2DComponent != null;
                transform2DComponent = transform2DComponent.Parent)
                transform2DComponent.TransformBecameDirty += ParentOnTransformBecameDirty;
        }

        private void ParentOnTransformBecameDirty()
        {
            _flags = _flags | TransformFlags.All;
        }

        private void RecalculateWorldMatrixIfNecessary()
        {
            if ((_flags & TransformFlags.WorldMatrixIsDirty) == 0)
                return;
            RecalculateLocalMatrixIfNecessary();
            RecalculateWorldMatrix(ref _localMatrix, out _worldMatrix);
            _flags = _flags & ~TransformFlags.WorldMatrixIsDirty;
        }

        private void RecalculateLocalMatrixIfNecessary()
        {
            if ((_flags & TransformFlags.LocalMatrixIsDirty) == 0)
                return;
            RecalculateLocalMatrix(out _localMatrix);
            _flags = _flags & ~TransformFlags.LocalMatrixIsDirty;
            WorldMatrixBecameDirty();
        }

        private void RecalculateWorldMatrix(ref Matrix2D localMatrix, out Matrix2D matrix)
        {
            if (Parent != null)
            {
                Parent.GetWorldMatrix(out matrix);
                Matrix2D.Multiply(ref localMatrix, ref matrix, out matrix);
            }
            else
                matrix = localMatrix;
        }

        private void RecalculateLocalMatrix(out Matrix2D matrix)
        {
            matrix = Matrix2D.CreateScale(_scale) * Matrix2D.CreateRotationZ(_rotation) *
                     Matrix2D.CreateTranslation(_position - (_origin * _scale));
        }

        public IEnumerator<Transform2DComponent> GetEnumerator()
        {
            return Service.Tree.FindNode(Entity).ChildNodes.Select(e => e.Entity.Transform).ToList().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }
    }
}
