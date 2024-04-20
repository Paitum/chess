using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class.  A collection of constants and static members to enumerate
	/// the directions.
	/// </summary>
	public class Directions
	{
		public const int DownLeft = 0;
		public const int Left = 1;
		public const int UpLeft = 2;
		public const int Up = 3;
		public const int UpRight = 4;
		public const int Right = 5;
		public const int DownRight = 6;
		public const int Down = 7;
		public const int Unknown = 8;

		public const int FIRST_DIRECTION = 0;
		public const int LAST_DIRECTION = 8;

		public const int Cross = 0;
		public const int Plus = 1;

		public static readonly int[] XOffset = { -1, -1, -1, 0, 1, 1, 1, 0 };
		public static readonly int[] YOffset = { -1, 0, 1, 1, 1, 0, -1, -1 };
		public static readonly int[] Inverse = { UpRight, Right, DownRight, Down, DownLeft, Left, UpLeft, Up };
		public static readonly string[] DirectionToString = { "DownLeft", "Left", "UpLeft", "UpRight", "Right", "DownRight", "Down", "Unknown" };

	}
}
