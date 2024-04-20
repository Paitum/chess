using System;
using System.Collections;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class.  Represents a Random chess player, that randomly selects VALID moves.
	/// </summary>
	public class AI_Random : Chess.AI
	{

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Picks a random VALID move and returns it.
		/// </summary>
		/// <param name="board">the board to use</param>
		/// <param name="turn">the player to represent</param>
		/// <returns>the move</returns>
		public override Move GetMove( Board board, int turn )
		{
			if ( board.Turn != turn )
				throw new ApplicationException( "AI_Random: It's " + Player.PlayerToString[ board.Turn ] + "'s turn" );
			if ( board.IsGameOver )
				throw new ApplicationException( "AI_Random: Cannot move because the game is over" );

			Move move;
			BoardLocations boardLocations;

			boardLocations = BitBoard.GetLocations( board.MoversBitBoard( board.Turn ), true );

			// Are there any Movers?
			if ( boardLocations.Count == 0 )
				throw new DataIntegrityException( "AI_Random: No pieces can move." );

			// Pick a mover
			move.From = ChooseRandomLocation( boardLocations );

			boardLocations = board[move.From.x, move.From.y].Moves;

			// Are there any Moves?
			if ( boardLocations.Count == 0 )
				throw new DataIntegrityException( "AI_Random: Selected piece has no moves" );

			// Pick a move
			move.To = ChooseRandomLocation( boardLocations );

			// If it's a pawn getting promoted, set the Promotion Type
			if ( move.To.y == Player.LAST_ROW[ turn ] && board[move.From.x, move.From.y].Type == Piece.PAWN )
				move.PromotionType = Piece.QUEEN;
			else
				move.PromotionType = Piece.UNKNOWN;

			return move;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Picks a random location from a bitboard.
		/// </summary>
		/// <param name="moves"></param>
		/// <returns></returns>
		private Location ChooseRandomLocation( BoardLocations moves )
		{
			return moves.Locations[randomizer.Next( moves.Count )];
		}
	}
}

