using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class. Represents a Pawn piece.
	/// </summary>
	public class Pawn : Chess.Piece
	{
		public static readonly int[] Y_OFFSET = { 1, -1 };

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Sets the location back to its starting position.
		/// </summary>
		public override void SetLocationFromIndex()
		{
			if ( id < 0 || id > 8 )
				throw new ApplicationException( "Invalid input to Pawn.SetLocationFromIndex()" );
			if ( color == Player.WHITE )
				location.SetLocation( id, 1 );
			else
				location.SetLocation( id, 6 );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Makes sure that no Pawn is ever placed in the first or last row of 
		/// the board.
		/// </summary>
		/// <param name="x">x-coordinate to place the piece</param>
		/// <param name="y">y-coordinate to place the piece</param>
		public override void PutDownAt( int x, int y )
		{
			if ( ( color == Player.WHITE && y == 0 ) || ( color == Player.BLACK && y == 7 ) )
				throw new OutOfBoundsException( "Pawns can never reside behind their initial row. y = " + y );
			if ( ( color == Player.WHITE && y == 7 ) || ( color == Player.BLACK && y == 0 ) )
				throw new OutOfBoundsException( "Pawns can not exist in the last row. y = " + y );
			base.PutDownAt( x, y );
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
			int x = location.x, y = location.y;
			int oy = Y_OFFSET[color];

			attackBoard = BitBoard.EMPTY;

			if ( y == 0 || y == 7 )
				throw new DataIntegrityException( "Pawns cannot exist at row 0 or row 7" );

			// Attacks
			if ( x > 0 )
				attackBoard |= BitBoard.bit[x-1, y+oy];
			if ( x < 7 )
				attackBoard |= BitBoard.bit[x+1, y+oy];
		}
		
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Populates the moveBoard bitBoard with true values everywhere this
		/// piece can move.
		/// </summary>
		public override void GenerateMoves()
		{
			int x = location.x, y = location.y;
			int oy = Y_OFFSET[color];

			moveBoard = attackBoard & board.PiecesBitBoard[ Player.Inverse[ color ] ];
			
			// Forward One
			if ( board[x,y+oy] == null )
			{
				moveBoard |= BitBoard.bit[x, y+oy];
				// Forward Two
				if ( y == Player.SECOND_ROW[color] && board[x,y+oy+oy] == null )
					moveBoard |= BitBoard.bit[x, y+oy+oy];
			}
			
			kingDirection = sightGrid.Find( board.Pieces[color][Piece.KING][0] );
		}
	}
}
