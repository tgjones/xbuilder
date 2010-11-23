using System;
using Microsoft.Xna.Framework;

namespace XnaInspector.Xna.Camera
{
	/// <summary>
	/// With many thanks to <see cref="http://www.codeproject.com/KB/openGL/virtualtrackball.aspx"/>
	/// </summary>
	public class TrackBallController
	{
		private readonly float _radius;
		private Quaternion _currentQuat;
		private float _winWidth;
		private float _winHeight;
		private float _xprev;
		private float _yprev;
		private bool _mouseButtonDown;

		public Quaternion CurrentQuaternion
		{
			get { return _currentQuat; }
		}

		public TrackBallController(float rad, int width, int height)
			: this(rad, new Quaternion(Vector3.UnitX, 0), width, height)
		{

		}

		public TrackBallController(float rad, Quaternion initialOrient, int width, int height)
		{
			_winWidth = width;
			_winHeight = height;
			_mouseButtonDown = false;
			_xprev = _yprev = 0.0f;

			_radius = MathHelper.Clamp(rad, 0.1f, 1);
			_currentQuat = Quaternion.Normalize(initialOrient);
		}

		public void ResetOrientation()
		{
			_currentQuat = Quaternion.Identity;
		}

		public void Resize(int width, int height)
		{
			_winWidth = width;
			_winHeight = height;
		}

		public void MouseDown(Point location)
		{
			_xprev = (2 * location.X - _winWidth) / _winWidth;
			_yprev = (2 * location.Y - _winHeight) / _winHeight;
			_mouseButtonDown = true;
		}

		public void MouseUp(Point location)
		{
			_mouseButtonDown = false;
			_xprev = _yprev = 0.0f;
		}

		public void MouseMove(Point location)
		{
			float xcurr = (2 * location.X - _winWidth) / _winWidth;
			float ycurr = (2 * location.Y - _winHeight) / _winHeight;
			Vector3 vfrom = new Vector3(-_xprev, _yprev, 0);
			Vector3 vto = new Vector3(-xcurr, ycurr, 0);
			if (_mouseButtonDown)
			{
				// find the two points on sphere according to the projection method
				ProjectOnSphere(ref vfrom);
				ProjectOnSphere(ref vto);
				// get the corresponding unitquaternion
				Quaternion lastQuat = RotationFromMove(vfrom, vto);
				_currentQuat *= lastQuat;
				_xprev = xcurr;
				_yprev = ycurr;
			}
		}

		private void ProjectOnSphere(ref Vector3 v)
		{
			float rsqr = _radius * _radius;
			float dsqr = v.X * v.X + v.Y * v.Y;
			// if relatively "inside" sphere project to sphere else on hyperbolic sheet
			if (dsqr < (rsqr * 0.5f)) v.Z = (float)Math.Sqrt(rsqr - dsqr);
			else v.Z = rsqr / (2 * (float)Math.Sqrt(dsqr));
		}

		private static Quaternion RotationFromMove(Vector3 vfrom, Vector3 vto)
		{
			Quaternion q;
			q.X = vfrom.Z * vto.Y - vfrom.Y * vto.Z;
			q.Y = vfrom.X * vto.Z - vfrom.Z * vto.X;
			q.Z = vfrom.Y * vto.X - vfrom.X * vto.Y;
			q.W = Vector3.Dot(vfrom, vto);
			return Quaternion.Normalize(q);
		}
	}
}