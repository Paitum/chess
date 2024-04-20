using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class. Represents a Rook piece.
	/// </summary>
	public class Rook : Chess.Piece
	{
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Sets the location back to its starting position.
		/// </summary>
		public override void SetLocationFromIndex()
		{
			if ( id < 0 || id > 1 )
				throw new ApplicationException( "Invalid input to Rook.SetLocationFromIndex()" );

			if ( color == Player.WHITE )
				location.SetLocation( id * 7 + 0, 0 );
			else
				location.SetLocation( id * 7 + 0, 7 );
		}
		
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Updates this piece with respect to the given direction.
		/// </summary>
		/// <param name="dir">The direction from which to update from</param>
		public override void FixMoves( int dir )
		{
			if ( dir == Directions.Left || dir == Directions.Up || dir == Directions.Right || dir == Directions.Down )
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
		/// Populates the moveBoard bitBoard with true values everywhere this
		/// piece can move.
		/// </summary>
		public override void GenerateMoves()
		{
			GenerateMovesAndAttacks();
		}
		private void GenerateMovesAndAttacks()
		{
			attackBoard = BitBoard.EMPTY;
			moveBoard = BitBoard.EMPTY;

			MovesHelper( Directions.Plus );
			kingDirection = sightGrid.Find( board.Pieces[color][Piece.KING][0] );
		}
	}
}
