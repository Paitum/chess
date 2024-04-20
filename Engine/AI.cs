using System;
using System.Collections;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Abstract Class.  AI lays out the framework for AI classes to inherit from.
	/// </summary>
	public abstract class AI
	{

		#region Static Members
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// A public randomizer for subclasses to use.
		/// </summary>
		public static Random randomizer;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Static Constructor.  Initializes the static variablesof AI.
		/// </summary>
		static AI()
		{
			randomizer = new Random();
		}
		#endregion

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Event.  ProgressAnnouncement event is used to update the GUI's progress bar.
		/// </summary>
		public event ProgressEventHandler ProgressAnnouncement;


		public abstract Move GetMove( Board board, int turn );

		protected virtual Move SelectMoveFromMoves( Board board, Move[] bestMoves, int bestMovesCount )
		{
			return bestMoves[ randomizer.Next( bestMovesCount ) ];

//			// TODO: Make this method more intelligent
//			for( x = 0; x < bestMovesCount; x++ )
//			{
//				board.MovePiece( bestMoves[x].From.x, bestMoves[x].From.y, bestMoves[x].To.x, bestMoves[x].To.y, bestMoves[x].PromotionType );
//				// Get a quick heuristic.
//				board.Undo();
//			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Triggers the ProgressAnnouncement in a centralized place.
		/// </summary>
		/// <param name="e"></param>
		protected virtual void OnProgressAnnouncement( ProgressEventArgs e )
		{
			if ( ProgressAnnouncement != null )
				ProgressAnnouncement( this, e );
		}

	}
}
