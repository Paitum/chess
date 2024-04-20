using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class.  A datatype that is capable of representing all possible moves from a given board-state.
	/// </summary>
	public class AllMoves
	{
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The locations of the pieces that have moves.
		/// </summary>
		public BoardLocations Movers;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// An 8x8 array that contains the moves that can be made from pieces located on the board.
		/// Refer to the Movers to locate the pieces that have moves.
		/// </summary>
		private BoardLocations[,] moves;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Initializes the object.
		/// </summary>
		public AllMoves()
		{
			moves = new BoardLocations[8,8];
			Movers = new BoardLocations();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Adds a set of moves to a location on the board.
		/// </summary>
		/// <param name="l">the location of the piece that can move</param>
		/// <param name="inMoves">the move(s) where the piece can move</param>
		public void Add( Location l, BoardLocations inMoves )
		{
			if ( l.x < 0 || l.x > 7 || l.y < 0 || l.y > 7 )
				throw new ApplicationException( "AllMoves: Add: Location is out of bounds (" + l.x + "," + l.y + ")" );

			moves[l.x, l.y] = inMoves;
			Movers.Add( l );				
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Indexer.  Gets or sets the moves from a location.
		/// </summary>
		public BoardLocations this[Location l]
		{
			get
			{
				if ( l.x < 0 || l.x > 7 || l.y < 0 || l.y > 7 )
					throw new ApplicationException( "AllMoves: this[,]: Get: Array indices are out of bounds (" + l.x + "," + l.y + ")" );
				return moves[l.x,l.y];
			}
			set
			{
				if ( l.x < 0 || l.x > 7 || l.y < 0 || l.y > 7 )
					throw new ApplicationException( "AllMoves: this[,]: Set: Array indices are out of bounds (" + l.x + "," + l.y + ")" );
				moves[l.x,l.y] = value;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Indexer.  Gets or sets the moves from a location.
		/// </summary>
		public BoardLocations this[int x, int y]
		{
			get
			{
				if ( x < 0 || x > 7 || y < 0 || y > 7 )
					throw new ApplicationException( "AllMoves: this[,]: Get: Array indices are out of bounds (" + x + "," + y + ")" );
				return moves[x,y];
			}
			set
			{
				if ( x < 0 || x > 7 || y < 0 || y > 7 )
					throw new ApplicationException( "AllMoves: this[,]: Set: Array indices are out of bounds (" + x + "," + y + ")" );
				moves[x,y] = value;
			}
		}
	}
}
