using System;
using System.Collections;

namespace Chess
{

	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class.  An implementation of the MinMax algorithm.
	/// </summary>
	public class AI_MinMax : Chess.AI
	{
		private const int BEST_MOVE_ARRAY_SIZE = 512;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// How many moves ahead to look.
		/// </summary>
		int depth;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The board to move pieces on.
		/// </summary>
		Board board;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The player that we are representing.
		/// </summary>
		int turn;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The array that will hold all the good moves we find.
		/// </summary>
		Move[] bestMoves;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// A count of how many good moves we have in the bestMoves array
		/// </summary>
		int bestMovesCount;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Creates a AI_MinMax object.
		/// </summary>
		/// <param name="inDepth">the desired look-ahead level</param>
		public AI_MinMax( int inDepth )
		{
			depth = inDepth;
			bestMoves = new Move[ BEST_MOVE_ARRAY_SIZE ];
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Runs the MinMax algorithm and returns the move it deemed best.
		/// </summary>
		/// <param name="inBoard"></param>
		/// <param name="inTurn"></param>
		/// <returns></returns>
		public override Move GetMove( Board inBoard, int inTurn )
		{
			board = inBoard;
			turn = inTurn;

			if ( board.Turn != turn )
				throw new ApplicationException( "AI_MinMax: It's " + Player.PlayerToString[ board.Turn ] + "'s turn" );
			if ( board.IsGameOver )
				throw new ApplicationException( "AI_MinMax: Cannot move because the game is over" );

			bestMovesCount = 0;

			MinMax( depth );

			return SelectMoveFromMoves( board, bestMoves, bestMovesCount );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Performs the actual recursive searching for the best moves.  This algorithm
		/// fills the bestMoves array with equal-weighted moves.  If the array only contains
		/// one entry, then it is the best move, otherwise you will have to pick from
		/// the list.
		/// </summary>
		/// <param name="inDepth"></param>
		/// <returns></returns>
		private int MinMax( int inDepth )
		{
			int bestValue = int.MinValue, tempValue;

			// Get ALL of the possible moves from the board.
			AllMoves allMoves = board.Moves();
			Location moverLocation, l2;
			BoardLocations moves;

			// Iterate through all the pieces THAT HAVE MOVES.
			for( int pi = 0; pi < allMoves.Movers.Count; pi++ )
			{
				moverLocation = allMoves.Movers.Locations[pi];
				moves = allMoves[ moverLocation ];

				// Iterate through all of the moves OF THIS PIECE.
				for( int li = 0; li < moves.Count; li++ )
				{
					l2 = moves.Locations[li];

					// Is move valid?  ( This check is necessary.  
					// For more info check the BoardLocations struct.
					if ( ( moves.Board & BitBoard.bit[l2.x, l2.y] ) == BitBoard.EMPTY )
						continue;

					board.MovePiece( moverLocation.x, moverLocation.y, l2.x, l2.y, Piece.QUEEN );

					if ( board.GameState == Board.State.BlackCheckMate || board.GameState == Board.State.WhiteCheckMate )
						tempValue = int.MaxValue;
					else if ( inDepth <= 1 || board.GameState == Board.State.Stalemate )
						tempValue = -board.BoardValue;
					else
						tempValue = -MinMax( inDepth - 1 );

					board.Undo();

					if ( bestValue <= tempValue )
					{
						if ( inDepth == depth )
						{
							if  ( bestValue < tempValue )
								bestMovesCount = 0;
							bestMoves[ bestMovesCount ].From = moverLocation;
							bestMoves[ bestMovesCount ].To = l2;
							bestMoves[ bestMovesCount ].PromotionType = Piece.QUEEN;
							bestMovesCount++;
						}
						bestValue = tempValue;
					} 
				}

				if ( inDepth == depth )
					OnProgressAnnouncement( new ProgressEventArgs( pi * 100 / allMoves.Movers.Count ) );
			}

			return bestValue;
		}

	}
}

