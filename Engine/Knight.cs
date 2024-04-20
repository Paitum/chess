using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class. Represents a Knight piece.
	/// </summary>
	public class Knight : Chess.Piece
	{

		#region Static Constructor
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// movetemplates is a static variable that stores the precalculated
		/// locations of where a Knight can attack/move.
		/// </summary>
		public static ulong[,] moveTemplates;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Static Constructor.  Initializes the static variables.
		/// </summary>
		static Knight()
		{
			moveTemplates = new ulong[8,8];
			for( int x = 0; x < 8; x++ )
			{
				for( int y = 0; y < 8; y++ )
				{
					moveTemplates[x,y] = BitBoard.SetKnightBits( BitBoard.EMPTY, x, y, true );
				}
			}
		}
		#endregion

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Sets the location back to its starting position.
		/// </summary>
		public override void SetLocationFromIndex()
		{
			if ( id < 0 || id > 1 )
				throw new ApplicationException( "Invalid input to Knight.SetLocationFromIndex()" );

			if ( color == Player.WHITE )
				location.SetLocation( id * 5 + 1, 0 );
			else
				location.SetLocation( id * 5 + 1, 7 );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Updates this piece with respect to the given direction.
		/// </summary>
		/// <param name="dir">The direction from which to update from</param>
		public override void FixMoves( int dir )
		{
			GenerateMoves();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override. Populates the attackBoard bitBoard with true values everywhere this
		/// piece has an attacking influence on.
		/// </summary>
		public override void GenerateAttacks()
		{
			attackBoard = moveTemplates[location.x, location.y];
		}
		
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Populates the moveBoard bitBoard with true values everywhere this
		/// piece can move.
		/// </summary>
		public override void GenerateMoves()
		{
			moveBoard = attackBoard & ~( board.PiecesBitBoard[color] );
			kingDirection = sightGrid.Find( board.Pieces[color][Piece.KING][0] );
		}
	}
}
