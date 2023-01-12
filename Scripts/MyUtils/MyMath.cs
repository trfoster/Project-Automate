using System;
using UnityEngine;

namespace ProjectAutomate.MyUtils
{
	public static class MyMathf
	{
		public static float CorrectAngle(float angle)
		{
			switch (angle)
			{
				case >= 360f:
					angle -= 360f;
					break;
				case < 0f:
					angle += 360f;
					break;
				default:
					if (Math.Abs(angle - -180f) < 0.000f) angle = 180f;
					break;
			}
			return Mathf.Round(angle);
		}

		public static Vector3 RoundDirectional(Vector3 value, float direction)
		{
			float x = value.x;
			float y = value.y;
			float flooredX = Mathf.Floor(x);
			float flooredY = Mathf.Floor(y);
			switch (direction)
			{
				case 0f: case 90f:
					if (x - flooredX == 0.5f) x += 0.5f;
					if (y - flooredY == 0.5f) y -= 0.5f;
					return new Vector3(x, y, value.z);
				case 180f: case 270f:
					if (x - flooredX == 0.5f) x -= 0.5f;
					if (y - flooredY == 0.5f) y += 0.5f;
					return new Vector3(x, y, value.z);
				default: Debug.LogWarning("Function RoundDirectional has received an invalid rotation");
					return Vector3.zero;
			}
		}
		
		//this function returns the position in front of the building depending on the rotation. Eg. The position the belt is putting items into.
		public static Vector2 GetForwardVectorOffset(float rotation)
		{
			switch (rotation)
			{
				case 0f: return new Vector2(0f,-1f);
				case 180f: return new Vector2(0f ,1f);
				case 270f: return new Vector2(-1f ,0f);
				case 90f: return new Vector2(1f ,0f);
				default: Debug.LogWarning("Function GetForwardVector has received an invalid rotation");
					return Vector2.zero;
			}
		}
		

		/*public static Vector3 GetForwardVector3Offset(float rotation)
		{
			Vector2Int vector = GetForwardVector2Offset(rotation);
			return new Vector3(vector.x, vector.y,);
		}*/
		
		public static Vector2 GetBackwardVectorOffset(float rotation)
		{
			return GetForwardVectorOffset(rotation) * -1f;
		}
	}
}
