using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Struct. Represents a location on the board.
	/// </summary>
	public struct Location
	{
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// the x-coordinate
		/// </summary>
		private int xVal;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// the y-coordinate
		/// </summary>
		private int yVal;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Sets the location
		/// </summary>
		/// <param name="inX">x-coordiante</param>
		/// <param name="inY">y-coordiante</param>
		public Location( int inX, int inY )
		{
			xVal = inX;
			yVal = inY;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Sets the location
		/// </summary>
		/// <param name="inX">x-coordiante</param>
		/// <param name="inY">y-coordiante</param>
		public void SetLocation( int inX, int inY )
		{
			xVal = inX;
			yVal = inY;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Compares this location with another location.
		/// </summary>
		/// <param name="inLoc">the location to compare</param>
		/// <returns>true if they both represent the same location</returns>
		public bool Equals( Location inLoc )
		{
			return ( xVal == inLoc.x && yVal == inLoc.y );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Compares this location with another location through coordinates
		/// </summary>
		/// <param name="inX">x-coordiante</param>
		/// <param name="inY">y-coordiante</param>
		/// <returns>true if the locations are the same</returns>
		public bool Equals( int inX, int inY )
		{
			return ( xVal == inX && yVal == inY );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Static. Determines whether this coordinate is off the board.
		/// </summary>
		/// <param name="x">x-coordiante</param>
		/// <param name="y">y-coordiante</param>
		/// <returns>True if out-of-bounds, false if not.</returns>
		public static bool IsOutOfBounds( int x, int y )
		{
			if ( x < 0 || x > 7 || y < 0 || y > 7 )
				return true;
			return false;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether this Location object is off the board.
		/// </summary>
		/// <returns>True if out-of-bounds, false if not.</returns>
		public bool IsOutOfBounds()
		{
			if ( xVal < 0 || xVal > 7 || yVal < 0 || yVal > 7 )
				return true;
			return false;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Property.  Provides access the the x-coordinate
		/// </summary>
		public int x
		{
			get
			{
				return xVal;
			}
			set
			{
				xVal = value;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Property.  Provides access the the y-coordinate
		/// </summary>
		public int y
		{
			get
			{
				return yVal;
			}
			set
			{
				yVal = value;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  A string representation of this location.
		/// </summary>
		/// <returns>a '(x,y)' string representation</returns>
		public override string ToString()
		{
			return ( String.Format("({0},{1})", x, y ) );
		}
	}
}
