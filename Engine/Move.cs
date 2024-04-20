using System;
using System.Collections;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Structure.  Represents a move.
	/// </summary>
	public struct Move
	{
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The location to find the piece to move.
		/// </summary>
		public Location From;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The location to move the piece to
		/// </summary>
		public Location To;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The pawn-promotion type.  (If not applicable then use Piece.UNKNOWN
		/// </summary>
		public int PromotionType;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Initializes the move, without a promotion type
		/// </summary>
		/// <param name="f">location to find the piece</param>
		/// <param name="t">location to move the piece to</param>
		public Move( Location f, Location t )
		{
			From = f;
			To = t;
			PromotionType = Piece.UNKNOWN;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Initializes the move, with a promotion type
		/// </summary>
		/// <param name="f">location to find the piece</param>
		/// <param name="t">location to move the piece to</param>
		/// <param name="inPromotionType">the promotion type of the move</param>
		public Move( Location f, Location t, int inPromotionType)
		{
			From = f;
			To = t;
			PromotionType = inPromotionType;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Initializes the move, with a promotion type
		/// </summary>
		/// <param name="fx">the from x-coordinate</param>
		/// <param name="fy">the from y-coordinate</param>
		/// <param name="tx">the to x-coordinate</param>
		/// <param name="ty">the to y-coordinate</param>
		/// <param name="inPromotionType">the promotion type of the move</param>
		public Move( int fx, int fy, int tx, int ty, int inPromotionType)
		{
			From = new Location( fx, fy );
			To = new Location( tx, ty );
			PromotionType = inPromotionType;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines whether this move is the same as another move.
		/// </summary>
		/// <param name="m">the move to compare with</param>
		/// <returns>true if the same move</returns>
		public bool Equals( Move m )
		{
			return ( From.x == m.From.x 
				&& From.y == m.From.y 
				&& To.x == m.To.x 
				&& To.y == m.To.y
				&& PromotionType == m.PromotionType );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  A string representation of the move
		/// </summary>
		/// <returns>a string representation of the move</returns>
		public override string ToString()
		{
            return From + "-" + To + " PT: " + Piece.TypeToString[ PromotionType ];
		}
	}
}
