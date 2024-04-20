using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class. Represents a Bishop piece.
	/// </summary>
	public class Bishop : Chess.Piece
	{
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Sets the location of this piece to its default location
		/// </summary>
		public override void SetLocationFromIndex()
		{
			if ( id < 0 || id > 1 )
				throw new ApplicationException( "Invalid input to Bishop.SetLocationFromIndex()" );

			if ( color == Player.WHITE )
				location.SetLocation( id * 3 + 2, 0 );
			else
				location.SetLocation( id * 3 + 2, 7 );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Updates this piece with respect to the given direction.
		/// </summary>
		/// <param name="dir">The direction from which to update from</param>
		public override void FixMoves( int dir )
		{
			if ( dir == Directions.UpLeft || dir == Directions.UpRight || dir == Directions.DownLeft || dir == Directions.DownRight )
				GenerateMovesAndAttacks();
			else
				kingDirection = sightGrid.Find( board.Pieces[color][Piece.KING][0] );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override. Populates the attackBoard bitBoard with true values everywhere this
		/// piece has an attacking influence on.
		/// </summary>
		public override void GenerateAttacks()
		{
			GenerateMovesAndAttacks();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override. Populates the moveBoard bitBoard with true values everywhere this
		/// piece can move.
		/// </summary>
		public override void GenerateMoves()
		{
			GenerateMovesAndAttacks();
		}
		public void GenerateMovesAndAttacks()
		{
			attackBoard = BitBoard.EMPTY;
			moveBoard = BitBoard.EMPTY;

			MovesHelper( Directions.Cross );
			kingDirection = sightGrid.Find( board.Pieces[color][Piece.KING][0] );
		}
	}
}
