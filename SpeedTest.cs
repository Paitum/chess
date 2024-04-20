using System;
using Chess.UI;

namespace Chess
{
	/// <summary>
	/// This class was created to gauge the efficiency of the chess program.  It
	/// moves pieces randomly and records how fast it can make moves.
	/// </summary>
	public class SpeedTest
	{
		public SpeedTest()
		{
			Board b = new Board();
			Board temp = new Board();
			AI ai = new AI_Random();
			Move move = new Move( new Location(0,0), new Location(0,0) );

			DateTime begin, mark;
			ulong gamesPlayed = 0;
			ulong totalMoves = 0;
			int moves = 0;
			double seconds, totalSeconds = 0;
			double movesPerSecond = 0;

			System.Console.Out.WriteLine( "Games \tMoves/Sec = Ave.Moves/Sec" );

			begin = DateTime.Now;

			while( true )
			{
				b.Reset();

				mark = DateTime.Now;

				try
				{
					while( ! b.IsGameOver )
					{
						// MOVE WHITE
						move = ai.GetMove( b, Player.WHITE );

						b.MovePiece( move.From.x, move.From.y, move.To.x, move.To.y, move.PromotionType );

						if ( b.IsGameOver )
							break;

						// MOVE BLACK
						move = ai.GetMove( b, Player.BLACK );

						b.MovePiece( move.From.x, move.From.y, move.To.x, move.To.y, move.PromotionType );

					}					
				} 
				catch ( Exception ex ) 
				{
					System.Console.WriteLine( ex );
//					System.Console.Out.WriteLine( "Times: " + gamesPlayed);
//					System.Console.Out.WriteLine( "IsGameOver: " + b.IsGameOver );
//					System.Console.Out.WriteLine( "State: " + b.GameState );
//					System.Console.Out.WriteLine( "Turn: " + Player.PlayerToString[ b.Turn ] );
//					System.Console.Out.WriteLine( "Board:" + Environment.NewLine + b );
//					if ( b.GameState != Board.State.Play )
//						System.Console.Out.WriteLine( "CRB:" + Environment.NewLine + BitBoard.ToString( b.CheckRestrictionBoard ) );
//					System.Console.Out.WriteLine( Player.PlayerToString[ b.Turn ] + " b Movers:" + Environment.NewLine + BitBoard.ToString( b.MoversBitBoard( b.Turn ) ) );
//					System.Console.Out.WriteLine( Player.PlayerToString[ b.Turn ] + " b Moves:" + Environment.NewLine + BitBoard.ToString( b.MoveBitBoard( b.Turn ) ) );
//					System.Console.Out.WriteLine( "pieceBitBoard[" + Player.PlayerToString[ Player.WHITE ] + "]:" + Environment.NewLine + BitBoard.ToString( b.PiecesBitBoard[Player.WHITE] ) );
//					System.Console.Out.WriteLine( "pieceBitBoard[" + Player.PlayerToString[ Player.BLACK ] + "]:" + Environment.NewLine + BitBoard.ToString( b.PiecesBitBoard[Player.BLACK] ) );
//					System.Console.Out.WriteLine( b.History.DumpToString() );
					// System.Console.Out.WriteLine( "This guys move board on b:" + Environment.NewLine + BitBoard.ToString( b[move.from.x, move.from.y].MoveBoard ) );
					ChessApp app = new ChessApp( b );
					return;
				}

				seconds = (double) ( DateTime.Now - mark ).TotalSeconds;
				totalSeconds += seconds;
				if ( seconds > 0.0 ) // Don't divide by 0
				{
					moves = b.History.Current;
					totalMoves += (ulong) moves;
					movesPerSecond = (double) moves / seconds;
				}

				if ( ++gamesPlayed % 250 == 0 ) // Print only once for every 250 games
				{
					System.Console.WriteLine( gamesPlayed + "\t" + totalMoves + "/" + Math.Round( totalSeconds, 3 ) + " = " + totalMoves / totalSeconds );
				}
			}


		}
	}
}
