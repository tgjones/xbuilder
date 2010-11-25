using Microsoft.Xna.Framework;

namespace XBuilder.ContentPreview.Rendering
{
	/// <summary>
	/// Very simple camera implementation
	/// - mouse movement in x rotates around y axis
	/// - mouse movement in y rotates around x axis
	/// </summary>
	public class CameraController
	{
		private bool _mouseButtonDown;

		private Point _mouseDownPoint;
		private float _xAngle;
		private float _yAngle;

		public Matrix CurrentOrientation
		{
			get { return Matrix.CreateRotationY(_xAngle) * Matrix.CreateRotationX(_yAngle); }
		}

		public void ResetOrientation()
		{
			_xAngle = _yAngle = 0.0f;
		}

		public void MouseDown(Point location)
		{
			_mouseButtonDown = true;
			_mouseDownPoint = location;
		}

		public void MouseUp(Point location)
		{
			_mouseDownPoint = Point.Zero;
			_mouseButtonDown = false;
		}

		public void MouseMove(Point location)
		{
			if (_mouseButtonDown)
			{
				_xAngle += (location.X - _mouseDownPoint.X)/100.0f;
				_yAngle += (location.Y - _mouseDownPoint.Y)/100.0f;

				_mouseDownPoint = location;
			}
		}
	}
}