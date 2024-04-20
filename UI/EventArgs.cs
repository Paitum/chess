using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;

namespace Chess
{
	// Event Handlers
	public delegate void BoardNoticeEventHandler( object o, BoardEventArgs e );

	// Event Args
	public class LocationEventArgs : EventArgs
	{
		public Location location;

		public LocationEventArgs( Location inLoc )
		{
			location = inLoc;
		}
	}

	public class MoveEventArgs : EventArgs
	{
		public Move Move;

		public MoveEventArgs( Move inMove )
		{
			Move = inMove;
		}
	}

	public class BoardEventArgs : EventArgs
	{
		public Board board;

		public BoardEventArgs( Board inBoard )
		{
			board = inBoard;
		}
	}


}