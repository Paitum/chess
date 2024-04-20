using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class. Represents a Queen piece.
	/// </summary>
	public class Queen : Chess.Piece
	{
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Sets the location back to its starting position.
		/// </summary>
		public override void SetLocationFromIndex()
		{
			if ( color == Player.WHITE )
				location.SetLocation( 4, 0 );
			else
				location.SetLocation( 4, 7 );
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
			MovesHelper( Directions.Cross );
			kingDirection = sightGrid.Find( board.Pieces[color][Piece.KING][0] );
		}
	}
}
