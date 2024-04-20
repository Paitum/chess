using System;

namespace Chess
{

	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class. This class encapsulates the idea of a 'game'.  A game consists of 2 players and a board.
	/// The players take turns moving pieces until the game is over.
	/// </summary>
	public class Game
	{
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// White's Controller
		/// </summary>
		protected Player whitePlayer;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Black's Controller.
		/// </summary>
		protected Player blackPlayer;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The board to play on.
		/// </summary>
		protected Board board;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Instantiates the board for play.
		/// </summary>
		public Game()
		{
			board = new Board();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Uses the passed board as the board to play on.
		/// </summary>
		/// <param name="inBoard">the board to use</param>
		public Game( Board inBoard )
		{
			board = inBoard;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns the game back to its starting environment.
		/// </summary>
		public void Reset()
		{
			board.Reset();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Sets a player.
		/// </summary>
		/// <param name="color">the player to set</param>
		/// <param name="inPlayer">the player to use</param>
		public void SetPlayer( int color, Player inPlayer )
		{
			if ( color == Player.WHITE )
				SetWhite( inPlayer );
			else
				SetBlack( inPlayer );

			if ( IsAITurn() )
				OnLockBoard();
			else
				OnUnLockBoard();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Sets the white player
		/// </summary>
		/// <param name="inPlayer">the player to use</param>
		private void SetWhite( Player inPlayer )
		{
			if ( whitePlayer != null )
				whitePlayer.LeaveGame();
			whitePlayer = inPlayer;
			if ( whitePlayer != null )
				whitePlayer.JoinGame( this, Player.WHITE );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Sets the black player
		/// </summary>
		/// <param name="inPlayer">the player to use</param>
		private void SetBlack( Player inPlayer )
		{
			if ( blackPlayer != null )
				blackPlayer.LeaveGame();
			blackPlayer = inPlayer;
			if ( blackPlayer != null )
				blackPlayer.JoinGame( this, Player.BLACK );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Begins the game or continues it.
		/// </summary>
		public void Start()
		{
			NextMove();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Stops the game.
		/// </summary>
		public void Stop()
		{
			UpdateProgress( 0 );
			if ( CancelMove != null )
				CancelMove( this, new EventArgs() );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Informs the player (who's turn it is) to move OR unlocks the board so that 
		/// a human can interface and make a move.
		/// </summary>
		private void NextMove()
		{
			UpdateProgress( 0 );
			if ( board.IsGameOver )
				return;
			if ( board.Turn == Player.WHITE && WhiteTurn != null )
			{
				OnLockBoard();
				WhiteTurn( this, new TurnEventArgs( board.IncompleteClone() ) );
			}
			else if ( board.Turn == Player.BLACK && BlackTurn != null )
			{
				OnLockBoard();
				BlackTurn( this, new TurnEventArgs( board.IncompleteClone() ) );
			}
			else
				OnUnLockBoard();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// MovePiece is the interface provided to move a piece.
		/// </summary>
		/// <param name="move">the move to make</param>
		public void MovePiece( Move move )
		{
			try
			{
				board.MovePiece( move.From.x, move.From.y, move.To.x, move.To.y, move.PromotionType ); 
				if ( TurnChange != null )
					TurnChange( this, new BoardEventArgs( board ) );
				NextMove();
			}
			catch
			{
				if ( InvalidMove != null )
					InvalidMove( this, new EventArgs() );
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// A common location to trigger the LockBoard event.
		/// </summary>
		public virtual void OnLockBoard()
		{
			if ( LockBoard != null )
				LockBoard( this, new EventArgs() );   
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// A common location to trigger the UnLockBoard event.
		/// </summary>
		public virtual void OnUnLockBoard()
		{
			if ( UnLockBoard != null )
				UnLockBoard( this, new EventArgs() );   
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Answers the question, "Is the player, who's turn it is, an AI player?"
		/// </summary>
		/// <returns>true if it is an AIs turn, false otherwise</returns>
		public bool IsAITurn()
		{
			if ( board.Turn == Player.WHITE && whitePlayer != null )
				return true;
			if ( board.Turn == Player.BLACK && blackPlayer != null )
				return true;
			return false;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Provides a mechanism to broadcast AI progress.
		/// </summary>
		/// <param name="percent">the percent complete</param>
		public void UpdateProgress( int percent )
		{
			if ( ProgressAnnouncement != null )
				ProgressAnnouncement( this, new ProgressEventArgs( percent ) );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Property.  Provides access to the internal board.
		/// </summary>
		public Board Board
		{
			get
			{
				return board;
			}
			set
			{
				board = value;
			}
		}

		///////////////////////////////////////////////////////////////////////
		// Events
		public event TurnHandler WhiteTurn;
		public event TurnHandler BlackTurn;
		public event EventHandler LockBoard;
		public event EventHandler UnLockBoard;
		public event BoardNoticeEventHandler TurnChange;
		public event EventHandler InvalidMove;
		public event EventHandler CancelMove;
		public event ProgressEventHandler ProgressAnnouncement;
	}

	// Event Handlers
	public delegate void TurnHandler( object o, TurnEventArgs e );
	public delegate void ProgressEventHandler( object o, ProgressEventArgs e );

	// Event Args
	public class TurnEventArgs : EventArgs
	{
		public Board Board;

		public TurnEventArgs( Board inBoard )
		{
			Board = inBoard;
		}
	}
	public class ProgressEventArgs : EventArgs
	{
		public int percent;

		public ProgressEventArgs( int inPercent)
		{
			percent = inPercent;
		}
	}
}
