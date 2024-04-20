using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class. Represents a "bunch" of locations of the board.  It stores this information in 2 ways.  First
	/// it keeps a bitboard with true values of where locations are.  And secondly it stores an array of
	/// Locations.
	/// 
	/// WARNING.  Always check the "Board" bitboard to see if a move is still valid.  You may have locations that 
	///  are not currently valid, so check the bitboard.  This situation arises when the king is checked,
	///  effectively nullifying many moves from many pieces.  Instead of refreshing every piece during this
	///  situation, the Piece.RealMoveBoard() just negates invalid regions of the bitboard.
	/// </summary>
	public class BoardLocations
	{
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// A bitboard representing where locations are being pointed to.
		/// </summary>
		public ulong Board;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// An array of locations
		/// </summary>
		public Location[] Locations;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The number of locations in the Locations array.
		/// </summary>
		public int Count;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Initializes the members.
		/// </summary>
		public BoardLocations()
		{
			Board = BitBoard.EMPTY;
			Locations = new Location[64];
			Count = 0;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Adds a location.
		/// </summary>
		/// <param name="l">the location to add</param>
		public void Add( Location l )
		{
			Board |= BitBoard.bit[l.x, l.y];
			Locations[Count++] = l;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Returns a multiline string representation of this object.
		/// </summary>
		/// <returns>the multiline string representation</returns>
		public override string ToString()
		{
			string output = "";
			output += BitBoard.ToString( Board ) + Environment.NewLine;
			for( int x = 0; x < Count; x++ )
				output += Locations[x] + Environment.NewLine;
			output += BitBoard.ToString( Board );
			return output;
		}
	}
}
