using System;
using System.Drawing;
using System.Windows.Forms;
using System.Collections;
using Chess;

namespace Chess.UI
{
	/// <summary>
	/// Enumeration.  Used by BoardPanel to provide different functionality.
	/// </summary>
	public enum BoardPanelMode { Move, Select };

	/// <summary>
	/// The primary user-interface of the chess game.  An 8x8 grid, pieces, and the ability to
	/// move a piece.  Along with some other bells and whistles.
	/// </summary>
	public class BoardPanel : UserControl
	{
		private SolidBrush DarkGrayBrush = new SolidBrush( Color.DarkGray );
		private SolidBrush LightGrayBrush = new SolidBrush( Color.LightGray );
		private SolidBrush GrayBrush = new SolidBrush( Color.Gray );
		private SolidBrush BlueBrush = new SolidBrush( Color.Blue );
		private SolidBrush RedBrush = new SolidBrush( Color.Red );
		private Pen YellowPen = new Pen( Color.Yellow );
		private Pen BluePen = new Pen( Color.Blue );

		private Timer animationTimer;
		private ContextMenu contextMenu;
		private MenuItem showMovers_MenuItem;
		private MenuItem showMoves_MenuItem;
		
		private SimplePiece[,] board;
		private AllMoves moves;
		private ulong moversBitBoard;
		private ulong movesBitBoard;
		private BoardPanelMode mode;
		private Location mouseLocation;
		private bool isMouseOver;
		private Location selectedPieceLocation;
		private bool haveSelectedPiece;
		private bool isLocked;


		// Preferences
		private bool showMovers;
		private bool showMoves;

		public BoardPanel()
		{
			this.SetStyle(ControlStyles.AllPaintingInWmPaint, true );
			this.SetStyle(ControlStyles.DoubleBuffer, true );
			
			Initialize();

		}

		public void Initialize()
		{
			animationTimer = new Timer();
			animationTimer.Tick += new EventHandler( this.Animate );
			animationTimer.Stop();

			contextMenu = new ContextMenu();
			showMovers_MenuItem = new MenuItem( "Highlight Movers", new EventHandler( this.ShowMoversMenuItem_Click ) );
			showMoves_MenuItem = new MenuItem( "Highlight Moves", new EventHandler( this.ShowMovesMenuItem_Click ) );
			contextMenu.MenuItems.Add( showMovers_MenuItem );
			contextMenu.MenuItems.Add( showMoves_MenuItem );
			this.ContextMenu = contextMenu;
			
			board = new SimplePiece[8,8];
			InitializeBoard();
			moversBitBoard = BitBoard.EMPTY;
			movesBitBoard = BitBoard.EMPTY;
			mode = BoardPanelMode.Move;
			isMouseOver = false;
			isLocked = false;
			haveSelectedPiece = false;
			showMovers = true;
			showMoves = true;

			showMovers_MenuItem.Checked = showMovers;
			showMoves_MenuItem.Checked = showMoves;
		}

		public void InitializeBoard()
		{
			for( int x = 0; x < 8; x++ )
			{
				for( int y = 0; y < 8; y++ )
				{
					board[x,y].Color = Player.UNKNOWN;
					board[x,y].Type = Piece.UNKNOWN;
				}           
			}
		}

		protected override void OnPaint( PaintEventArgs e )
		{

			base.OnPaint( e );
			Graphics g = e.Graphics;

			//			DateTime begin, end;
			//			int COUNT = 1;
			//			begin = DateTime.Now;
			//
			//			for( int loop = 0; loop < COUNT; loop++ )
			//			{

			// focusLocation represents the place where there is a piece that we're interested in
			Location focusLocation;
			bool haveFocusLocation = true;
			if ( haveSelectedPiece )
				focusLocation = selectedPieceLocation;
			else if ( isMouseOver )
				focusLocation = Tile.FromPixel( mouseLocation );
			else
			{
				focusLocation = new Location( -1, -1 );
				haveFocusLocation = false;
			}

			// Draw Board
			for( int x = 0; x < 400; x += Tile.WIDTH )
				for( int y = 0; y < 400; y += Tile.HEIGHT )
					if ( ( ( x + y ) % ( Tile.WIDTH + Tile.HEIGHT ) ) == 0 )
						g.FillRectangle( DarkGrayBrush, x, y, x + Tile.WIDTH, y + Tile.WIDTH );
					else
						g.FillRectangle( LightGrayBrush, x, y, x + Tile.HEIGHT, y + Tile.HEIGHT );

			// Draw the selectors
			if ( mode == BoardPanelMode.Move 
				&& ( showMovers || showMoves )
				&& ! isLocked
				&& ( moversBitBoard != BitBoard.EMPTY || movesBitBoard != BitBoard.EMPTY ) )
			{
				int px, py;
				for( int x = 0; x < 8; x++ )
					for( int y = 0; y < 8; y++ )
					{
						px = Tile.XFromBoard( x );
						py = Tile.YFromBoard( y );

						// Highlight the Movers 
						if ( showMovers && ! haveSelectedPiece && ( moversBitBoard & BitBoard.bit[x,y] ) != BitBoard.EMPTY )
						{
							g.FillRectangle( BlueBrush, px + 7, py + Tile.HEIGHT - 5, Tile.WIDTH - 14, 2 );
						}

						// Highlight the Moves
						if ( haveFocusLocation && ! board[focusLocation.x,focusLocation.y].IsNull && showMoves && ( movesBitBoard & BitBoard.bit[x,y] ) != BitBoard.EMPTY )
						{
							if ( board[focusLocation.x,focusLocation.y].Color == Player.Inverse[ board[x,y].Color ] )
								g.FillRectangle( RedBrush, px + 7, py + Tile.HEIGHT - 5, Tile.WIDTH - 14, 2 );
							else
								g.DrawImage( Tile.SHADOW[board[focusLocation.x,focusLocation.y].Color][board[focusLocation.x,focusLocation.y].Type], px, py );
						}
					}
			}

			// Draw the pieces on the board
			for( int x = 0; x < 8; x++ )
				for( int y = 0; y < 8; y++ )
					if ( board[x,y].Color != Player.UNKNOWN && board[x,y].Type != Piece.UNKNOWN  )
						if ( ! haveSelectedPiece || ! ( selectedPieceLocation.x == x && selectedPieceLocation.y == y ) )
						{
							g.DrawImage( Tile.PIECES[board[x,y].Color][board[x,y].Type], x * Tile.WIDTH, (7 - y) * Tile.HEIGHT );
						}

			// Pawn Promotion
			if ( isMouseOver && haveSelectedPiece && board[selectedPieceLocation.x,selectedPieceLocation.y].Type == Piece.PAWN )
			{
				Location location = Tile.FromPixel( mouseLocation );

				if ( location.y == Player.LAST_ROW[board[selectedPieceLocation.x,selectedPieceLocation.y].Color]
					&& ( movesBitBoard & BitBoard.bit[location.x,location.y] ) != BitBoard.EMPTY )
				{
//					g.FillRectangle( new SolidBrush( Color.FromArgb( 128, Color.Blue ) ), mouseLocation.x, mouseLocation.y, 200, 50 );
					g.DrawImage( Tile.PIECES[board[selectedPieceLocation.x,selectedPieceLocation.y].Color][Piece.QUEEN], mouseLocation.x + Tile.WIDTH / 5, mouseLocation.y + Tile.HEIGHT / 5, Tile.WIDTH / 2, Tile.HEIGHT / 2 );
				}
			}


			// Draw the selectedPiece where ever the mouse is
			if ( haveSelectedPiece )
			{
				g.DrawImage( Tile.PIECES[board[selectedPieceLocation.x,selectedPieceLocation.y].Color][board[selectedPieceLocation.x,selectedPieceLocation.y].Type]
					, mouseLocation.x - Tile.WIDTH / 2
					, mouseLocation.y - Tile.HEIGHT / 2 );
			}

			//			}
			//			end = DateTime.Now;
			//			System.Console.WriteLine( "Time: " + (end - begin ).TotalSeconds / (double) COUNT + " FPS: " + ( 1.0D / ((end - begin ).TotalSeconds / (double) COUNT)) );

		}

		public void Animate( object obj, EventArgs e )
		{
			throw new ApplicationException( "Animation Timer Ticked" );
		}

		protected override void OnMouseDown( MouseEventArgs e )
		{
			base.OnMouseDown( e );

			Location location = Tile.FromPixel( new Location( e.X, e.Y ) );

			if ( Chess.Location.IsOutOfBounds( location.x, location.y ) )
			{
				isMouseOver = false;
				return;
			}

			if ( e.Button == MouseButtons.Left && isLocked == false )
			{
				switch ( mode )
				{
					case BoardPanelMode.Move:
						if ( haveSelectedPiece )
						{
							haveSelectedPiece = false;
							movesBitBoard = BitBoard.EMPTY;

							// FIX THIS... ADD THE PROMOTION INFO
							if ( PieceMove != null )
								PieceMove( this, new MoveEventArgs( new Move( selectedPieceLocation, location, Piece.QUEEN ) ) );
						}
						else
						{
							if ( ( MoversBitBoard & BitBoard.bit[location.x,location.y] ) != BitBoard.EMPTY )
							{
								movesBitBoard = moves[location.x,location.y].Board;
								selectedPieceLocation = location;
								haveSelectedPiece= true;
							}
						}
						break;
					case BoardPanelMode.Select:
						if ( PieceMove != null )
							PieceSelect( this, new LocationEventArgs( location ) );
						break;
				}
			} 
			else if ( e.Button == MouseButtons.Right )
			{
				haveSelectedPiece = false;
				if ( ( MoversBitBoard & BitBoard.bit[location.x,location.y] ) != BitBoard.EMPTY )
					movesBitBoard = moves[location.x,location.y].Board;
				else
					movesBitBoard = BitBoard.EMPTY;
			}

			Refresh();
		}

		protected override void OnMouseMove( MouseEventArgs e )
		{
			base.OnMouseMove( e );

			Location newMouseLocation = new Location( e.X, e.Y );
			Location location = Tile.FromPixel( newMouseLocation );

			if ( Chess.Location.IsOutOfBounds( location.x, location.y ) )
			{
				isMouseOver = false;
				return;
			}

			// Detect movement INTO another tile.
			if ( ! isMouseOver || ! Tile.FromPixel( mouseLocation ).Equals( Tile.FromPixel( newMouseLocation ) ) )
			{
				if ( ! haveSelectedPiece )
				{
					if ( ( MoversBitBoard & BitBoard.bit[location.x,location.y] ) != BitBoard.EMPTY )
						movesBitBoard = moves[location.x,location.y].Board;
					else
						movesBitBoard = BitBoard.EMPTY;
				}

				if ( MouseMoveIntoSquare != null )
					MouseMoveIntoSquare( this, new LocationEventArgs( location ) );

				isMouseOver = true;
				mouseLocation = newMouseLocation;

				Refresh();
			}
			else
			{
				isMouseOver = true;
				mouseLocation = newMouseLocation;

				if ( haveSelectedPiece )
					Refresh();
			}
		}

		protected override void OnMouseLeave( EventArgs e )
		{
			base.OnMouseLeave( e );

			isMouseOver = false;
			haveSelectedPiece = false;
			movesBitBoard = BitBoard.EMPTY;
			Refresh();
		}

		public void ChessApp_BoardNoticeResponder( object o, BoardEventArgs e )
		{
			Board board = e.board;

			for( int x = 0; x < 8; x++ )
				for( int y = 0; y < 8; y++ )
					if ( board[x,y] != null )
						AddPiece( new SimplePiece( board[x,y].Color, board[x,y].Type), new Location( x, y ) );
					else
						AddPiece( SimplePiece.NULL, new Location( x, y ) );

			moves = board.Moves();

			moversBitBoard = board.MoversBitBoard( board.Turn );
			movesBitBoard = BitBoard.EMPTY;

			haveSelectedPiece = false;
			Location location = Tile.FromPixel( mouseLocation );
			if ( ( MoversBitBoard & BitBoard.bit[location.x,location.y] ) != BitBoard.EMPTY )
				movesBitBoard = moves[location.x,location.y].Board;
			else
				movesBitBoard = BitBoard.EMPTY;

			Refresh();
		}

		private void ShowMoversMenuItem_Click( object obj, EventArgs e )
		{
			showMovers = ! showMovers;
			showMovers_MenuItem.Checked = showMovers;
		}

		private void ShowMovesMenuItem_Click( object obj, EventArgs e )
		{
			showMoves = ! showMoves;
			showMoves_MenuItem.Checked = showMoves;
		}

		public void LockBoard()
		{
			if ( ! isLocked )
			{
				isLocked = true;
				haveSelectedPiece = false;
			}

			Refresh();
		}

		public void UnLockBoard()
		{
			isLocked = false;
			Refresh();
		}

		public void AddPiece( SimplePiece sp, Location location )
		{
			if ( location.IsOutOfBounds() )
				return;
			board[location.x, location.y] = sp;

		}
		public void RemovePiece( Location location )
		{
			if ( location.IsOutOfBounds() )
				return;
			board[location.x, location.y].Color = Player.UNKNOWN;
			board[location.x, location.y].Type = Piece.UNKNOWN;
		}
		/// <summary>
		/// Puts this instance into a "Select" mode.  This mode is used when a single 
		/// location selection should be announced.
		/// </summary>
		public void SelectMode()
		{
			mode = BoardPanelMode.Select;
			haveSelectedPiece = false;
		}

		private void InitializeComponent()
		{
			// 
			// BoardPanel
			// 
			this.Name = "BoardPanel";
			this.Size = new System.Drawing.Size(536, 536);

		}
		/// <summary>
		/// Puts this instance into a "Move" mode.  This mode is used when a move (aka
		/// two locations) should be announced.
		/// </summary>
		public void MoveMode()
		{
			mode = BoardPanelMode.Move;
		}

		public ulong MovesBitBoard
		{
			get
			{
				return movesBitBoard;
			}
			set
			{
				if ( movesBitBoard != value )
				{
					movesBitBoard = value;
					Refresh();
				}
			}
		}
		public ulong MoversBitBoard
		{
			get
			{
				return moversBitBoard;
			}
			set
			{
				if ( moversBitBoard != value )
				{
					moversBitBoard = value;
					Refresh();
				}
			}
		}

		// EventHandlers
		public delegate void LocationEventHandler( object o, LocationEventArgs e );
		public delegate void MoveEventHandler( object o, MoveEventArgs e );

		// Events
		public event LocationEventHandler PieceSelect;
		public event MoveEventHandler PieceMove;
		/// <summary>
		/// Event.  Invoked when the cursor moves over a different square.
		/// </summary>
		public event LocationEventHandler MouseMoveIntoSquare;
	}

}
