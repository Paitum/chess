using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// A lightweight datatype that assists in accessing a snapshot object.
	/// </summary>
	public struct SnapshotElement
	{
		public bool isOnBoard;
		public Location location;
		public int type;

		public SnapshotElement( bool inIsOnBoard, Location inLocation, int inType )
		{
			isOnBoard = inIsOnBoard;
			location = inLocation;
			type = inType;
		}
	}
}
