using System;
using System.Threading;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Abstract Class. Provides the framework for player-classes to inherit from
	/// </summary>
	public abstract class Player
	{
		///////////////////////////////////////////////////////////////////////
		// Constants and static members used for readability and efficiency.
		public const int WHITE = 0;	// White must be 0
		public const int BLACK = 1;	// Black must be 1
		public const int UNKNOWN = 2;
		public const int FIRST_COLOR = WHITE;
		public const int LAST_COLOR = BLACK;
		public static readonly String[] PlayerToString = { "White", "Black", "UnknownColor" };
		public static readonly String[] PlayerToSmallString = { "W", "B", "U" };
		public static readonly int[] Inverse = { BLACK, WHITE, UNKNOWN };
		public static readonly int[] FIRST_ROW = { 0, 7 };
		public static readonly  int[] SECOND_ROW = { 1, 6 };
		public static readonly  int[] LAST_ROW = { 7, 0 };

		public enum ControllerTypes { Human, AI_Random, AI_MinMax };

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The color, or side, that this player is playing.
		/// </summary>
		protected int side;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The game this player is a part of.
		/// </summary>
		protected Game game;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The board to make moves on.
		/// </summary>
		private Board board;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This thread allows the player to "go-off" and think about what move to make
		/// without locking the rest of the program.
		/// </summary>
		protected Thread thread;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The AI this player is using.
		/// </summary>
		protected AI ai;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Associates this player with a game.
		/// </summary>
		/// <param name="inGame">the game</param>
		/// <param name="inSide">the side this player will play</param>
		public void JoinGame( Game inGame, int inSide )
		{
			if ( game != null )
				LeaveGame();
			side = inSide;
			game = inGame;
			if ( side == Player.WHITE )
				game.WhiteTurn += new TurnHandler( this.ReturnMove );
			else
				game.BlackTurn += new TurnHandler( this.ReturnMove );
			game.CancelMove += new EventHandler( this.CancelMove );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Disassociates this player with the game.
		/// </summary>
		public void LeaveGame()
		{
			game.CancelMove -= new EventHandler( this.CancelMove );
			if ( side == Player.WHITE )
				game.WhiteTurn -= new TurnHandler( this.ReturnMove );
			else
				game.BlackTurn -= new TurnHandler( this.ReturnMove );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Abstract.  Determines the move to make.
		/// </summary>
		/// <param name="board">the board to use</param>
		/// <returns>the move to make</returns>
		public abstract Move GetMove( Board board );

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The eventResponder to the TurnEvent from the game.  This method should begin the
		/// process of figuring out what move to do, and giving that data to the game.
		/// </summary>
		/// <param name="obj">the event sender (the game)</param>
		/// <param name="e">the TurnEventArgs</param>
		public virtual void ReturnMove( object obj, TurnEventArgs e )
		{
			StopThread();
			board = e.Board;
			thread = new Thread( new ThreadStart( MovePiece ) );
			thread.Start();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Virtual. Perform the entire operation of getting a move, and sending it to the game.
		/// </summary>
		public virtual void MovePiece() 
		{
			game.MovePiece( GetMove( board ) );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Aborts whatever action this player was doing.
		/// </summary>
		/// <param name="o">the event sender</param>
		/// <param name="e"></param>
		public virtual void CancelMove( object o, EventArgs e ) 
		{
			StopThread();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The mechanism to stop the thinking thread.
		/// </summary>
		public void StopThread()
		{
			if ( thread != null && thread.IsAlive )
				thread.Abort();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Property.  Gets or Sets the side this player is playing.
		/// </summary>
		public int Side
		{
			get
			{
				return side;
			}
			set
			{
				side = value;

			}
		}
	}
}
