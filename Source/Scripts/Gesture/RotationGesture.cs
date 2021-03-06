﻿using System;
using System.Collections.Generic;
using UnityEngine;

namespace FairyGUI
{
	/// <summary>
	/// 手指反向操作的手势。
	/// </summary>
	public class RotationGesture : EventDispatcher
	{
		/// <summary>
		/// 当两个手指开始呈反向操作时派发该事件。
		/// </summary>
		public EventListener onBegin { get; private set; }
		/// <summary>
		/// 当其中一个手指离开屏幕时派发该事件。
		/// </summary>
		public EventListener onEnd { get; private set; }
		/// <summary>
		/// 当手势动作时派发该事件。
		/// </summary>
		public EventListener onAction { get; private set; }

		/// <summary>
		/// 总共旋转的角度。
		/// </summary>
		public float rotation;

		/// <summary>
		/// 从上次通知后的改变量。
		/// </summary>
		public float delta;

		/// <summary>
		/// 是否把变化量强制为整数。默认true。
		/// </summary>
		public bool snapping;

		GObject _host;
		Vector2 _startVector;
		float _lastRotation;
		int[] _touches;
		bool _started;

		public RotationGesture(GObject host)
		{
			_host = host;
			Enable(true);

			_touches = new int[2];
			snapping = true;

			onBegin = new EventListener(this, "onRotationBegin");
			onEnd = new EventListener(this, "onRotationEnd");
			onAction = new EventListener(this, "onRotationAction");
		}

		public void Dispose()
		{
			Enable(false);
			_host = null;
		}

		public void Enable(bool value)
		{
			if (value)
				_host.onTouchBegin.Add(__touchBegin);
			else
			{
				_started = false;
				_host.onTouchBegin.Remove(__touchBegin);
				Stage.inst.onTouchMove.Remove(__touchMove);
				Stage.inst.onTouchEnd.Remove(__touchEnd);
			}
		}

		void __touchBegin(EventContext context)
		{
			if (Stage.inst.touchCount == 2)
			{
				if (!_started)
				{
					Stage.inst.GetAllTouch(_touches);
					Vector2 pt1 = _host.GlobalToLocal(Stage.inst.GetTouchPosition(_touches[0]));
					Vector2 pt2 = _host.GlobalToLocal(Stage.inst.GetTouchPosition(_touches[1]));
					_startVector = pt1 - pt2;

					Stage.inst.onTouchMove.Add(__touchMove);
					Stage.inst.onTouchEnd.Add(__touchEnd);
				}
			}
		}

		void __touchMove(EventContext context)
		{
			InputEvent evt = context.inputEvent;
			Vector2 pt1 = _host.GlobalToLocal(Stage.inst.GetTouchPosition(_touches[0]));
			Vector2 pt2 = _host.GlobalToLocal(Stage.inst.GetTouchPosition(_touches[1]));
			Vector2 vec = pt1 - pt2;

			float rot = Mathf.Rad2Deg * ((Mathf.Atan2(vec.y, vec.x) - Mathf.Atan2(_startVector.y, _startVector.x)));
			if (snapping)
			{
				rot = Mathf.Round(rot);
				if (rot == 0)
					return;
			}

			if (!_started && rot > 5)
			{
				_started = true;
				rotation = 0;
				_lastRotation = 0;

				onBegin.Call(evt);
			}

			if (_started)
			{
				delta = rot - _lastRotation;
				_lastRotation = rot;
				this.rotation += delta;
				onAction.Call(evt);
			}
		}

		void __touchEnd(EventContext context)
		{
			Stage.inst.onTouchMove.Remove(__touchMove);
			Stage.inst.onTouchEnd.Remove(__touchEnd);

			if (_started)
			{
				_started = false;
				onEnd.Call(context.inputEvent);
			}
		}
	}
}
