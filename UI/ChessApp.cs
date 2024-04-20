using System;
using System.Windows.Forms;
using System.Drawing;
using System.ComponentModel;

namespace Chess.UI
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class.  ChessApp contains everything needed to coordinate a chess
	/// game.  It has a board, a game, and a UI (chessFrom).  This is the
	/// centralized message center for an actual game.
	/// </summary>
	public class ChessApp
	{
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// the board to play with.
		/// </summary>
		protected Board board;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// the game to coordinate the players
		/// </summary>
		protected Game game;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The window that will display the game.
		/// </summary>
		protected ChessForm chessForm;

		public ChessApp()
		{
			Initialize( new Board() );
		}

		public ChessApp( Board inBoard )
		{
			Initialize( inBoard );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Sets up the game by initializing the variables and "attaching"
		/// several event-handlers to their respective events.
		/// </summary>
		/// <param name="inBoard">the board to play the game on.</param>
		public void Initialize( Board inBoard )
		{
			board = inBoard;
			chessForm = new ChessForm( this );
			chessForm.Closing += new CancelEventHandler( ChessForm_Closing );

			game = new Game( board );
			game.LockBoard += new EventHandler( chessForm.LockBoard );
			game.UnLockBoard += new EventHandler( chessForm.UnLockBoard );
			game.TurnChange += new BoardNoticeEventHandler( Game_TurnChange );
			game.ProgressAnnouncement += new ProgressEventHandler( chessForm.AI_ProgressAnnouncementResponder );
			
			OnBoardNotice();
			System.Windows.Forms.Application.Run( chessForm );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Event Handler.  Called when the UI wants to close.
		/// </summary>
		public void ChessForm_Closing( object o, CancelEventArgs e )
		{
			game.Stop();
			System.Console.WriteLine("ChessApp: Shutting Down" );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Used to initialize or change a player's controller to a
		/// different controller.
		/// </summary>
		/// <param name="color">the player to change</param>
		/// <param name="type">the controller to assign</param>
		public void PlayerChange( int color, Player.ControllerTypes type )
		{
			Player newPlayer = null;
			game.Stop();

			switch ( type )
			{
				case Player.ControllerTypes.Human:
					break;
				case Player.ControllerTypes.AI_Random:
					newPlayer = new Player_AI_Random();
					break;
				case Player.ControllerTypes.AI_MinMax:
					newPlayer = new Player_AI_MinMax( chessForm.GetDepth( color ) );
					break;
			}

			game.SetPlayer( color, newPlayer );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Event Handler.  Called when the Start button is clicked.
		/// </summary>
		public void PlayButton_Click( object o, EventArgs e )
		{
			game.Start();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Event Handler.  Called when the Stop button is clicked.
		/// </summary>
		public void StopButton_Click( object o, EventArgs e )
		{
			StopGame();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Stops the game, can also be considered pausing.
		/// </summary>
		public void StopGame()
		{
			game.Stop();
			OnBoardNotice();
		}
		
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Event Handler.  Called when the game changes turns.
		/// </summary>
		public void Game_TurnChange( object o, BoardEventArgs e )
		{
			OnBoardNotice( e );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Event Handler.  Called when a piece is moved in the UI.
		/// </summary>
		public void BoardPanel_PieceMove( object o, MoveEventArgs e )
		{
			game.MovePiece( e.Move );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Performs a number of Undoes or Redoes on the board.
		/// </summary>
		/// <param name="times">the number of moves to travel.  A positive
		/// number symbolizes an undo, a negative symbolizes a redo.</param>
		public void UndoRedoSeveral( int times )
		{
			game.Stop();
			board.UndoRedo( times );
			OnBoardNotice();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Event Handler.  Called when the UndoAll button is clicked
		/// </summary>
		public void UndoAllButton_Click( object o, EventArgs e )
		{
			if ( board.CanUndo )
			{
				try
				{
					game.Stop();
					board.UndoAll();
					OnBoardNotice();
				}
				catch
				{
					Error = "UndoAll Exception";
				}
			} 
			else
			{
				chessForm.EnableUndo = false;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Event Handler.  Called when the Undo button is clicked
		/// </summary>
		public void UndoButton_Click( object o, EventArgs e )
		{
			if ( board.CanUndo )
			{
				try
				{
					game.Stop();
					board.Undo();
					OnBoardNotice();
				}
				catch
				{
					Error = "Undo Exception";
					chessForm.EnableUndo = false;
				}
			}
			else
			{
				chessForm.EnableUndo = false;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Event Handler.  Called when the RedoAll button is clicked
		/// </summary>
		public void RedoAllButton_Click( object o, EventArgs e )
		{
			if ( board.CanRedo )
			{
				try
				{
					game.Stop();
					board.RedoAll();
					OnBoardNotice();
				}
				catch
				{
					Error = "RedoAll Exception";
				}
			} 
			else
			{
				chessForm.EnableRedo = false;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Event Handler.  Called when the Redo button is clicked
		/// </summary>
		public void RedoButton_Click( object o, EventArgs e )
		{
			if ( board.CanRedo )
			{
				try
				{
					game.Stop();
					board.Redo();
					OnBoardNotice();
				}
				catch
				{
					Error = "Redo Exception";
					chessForm.EnableRedo = false;
				}
			}
			else
			{
				chessForm.EnableRedo = false;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Overloaded.  Centralized place to trigger a BoardNotice event.
		/// </summary>
		protected void OnBoardNotice()
		{
			OnBoardNotice( new BoardEventArgs( board ) );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Centralized place to trigger a BoardNotice Event.
		/// </summary>
		protected void OnBoardNotice( BoardEventArgs e )
		{
//			chessForm.MessageText = "----- History -----" + Environment.NewLine + board.History.DumpToString() + Environment.NewLine + "-------------------";
			
			chessForm.EnableUndo = ( board.CanUndo ) ? true : false;
			chessForm.EnableRedo = ( board.CanRedo ) ? true : false;
			chessForm.UndoRedoMessageText = board.History.Current + "/" + board.History.Count;

			chessForm.TurnText = Player.PlayerToString[ board.Turn ] + "'s Turn";
			if ( board.GameState == Board.State.Play )
				chessForm.StateText = "";
			else
				chessForm.StateText = Board.StateToString[ (int) board.GameState ];

			if ( BoardNotice != null )
				BoardNotice( this, new BoardEventArgs( board ) );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Property.  Informs the user of an error.
		/// </summary>
		public string Error
		{
			set
			{
				chessForm.MessageAppend = "Error: " + value;
			}
		}

		// Events
		public event BoardNoticeEventHandler BoardNotice;
	}


}

