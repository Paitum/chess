using System;
using System.Collections;

namespace Chess
{

	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Summary description for Board.
	/// </summary>
	public class Board
	{
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Enum.  Defines the possible game states that the board can be in.
		/// </summary>
		public enum State { Play = 0, BlackCheck, WhiteCheck, BlackCheckMate, WhiteCheckMate, Stalemate };

		#region Static Members
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// static. State Descriptions
		/// </summary>
		public static string[] StateToString = { "Normal Play", "Black is in Check", "White is in Check", "White Wins", "Black Wins", "Stalemate" };

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// HashKeyComponent & HashLockComponent are used to create unique hashKeys and 
		/// hashLocks, respectively, for a given board.
		/// </summary>
		private static int[][,] HashKeyComponent;
		private static int[][,] HashLockComponent;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes HashKeyComponent and HashLockComponent with random numbers for
		/// every piece at every position.
		/// </summary>
		static Board()
		{
			Random r = new Random();
			HashKeyComponent = new int[12][,];
			HashLockComponent = new int[12][,];

			for( int p = 0; p < 12; p++ )
			{
				HashKeyComponent[p] = new int[8,8];
				HashLockComponent[p] = new int[8,8];
				for( int y = 0; y < 8; y++ )
				{
					for( int x = 0; x < 8; x++ )
					{

						HashKeyComponent[p][x,y] = r.Next();
						HashLockComponent[p][x,y] = r.Next();
					}
				}
			}
		}
		#endregion

		#region Data Fields
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// pieces stores all 32 pieces in a jagged array whose format is: 
		/// pieces[ color ][ type ][ id ].  ( It is possible to find a different kind of
		/// piece-type located in pieces[][][].  This is necessary for PAWN-Upgrading )
		/// </summary>
		protected Piece[][][] pieces;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// board is an 8x8 array that points to pieces.  Spaces that are empty remain null.
		/// </summary>
		protected Piece[,] board;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// border manages 36 psuedo-pieces along the rim of the board.  Essentially acting as 
		/// a wall of pieces to simplify code.
		/// </summary>
		protected Border border;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// pieceBitBoard contains a bitboard for each Player.  It contains true values
		/// in the locations where there are pieces of the respective Player.
		/// </summary>
		protected ulong[] piecesBitBoard;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// checkRestrictionBoard is a bit board used when either king is in check.  This
		/// board represents locations that pieces must occupy in order to save the king
		/// from check.  The king ignores the checkRestrictionBoard.
		/// </summary>
		protected ulong checkRestrictionBoard;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// enPassantBoard is a bit board used to allow pawns the "en passant" move
		/// when applicable.  It stores a true value in the space that the pawn jumped
		/// over.
		/// </summary>
		protected ulong enPassantBoard;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// castleBits is used to store which way the kings can castle.
		/// WXYZ bits ( W left BK, X right BK, Y left WK, Z right WK )
		/// </summary>
		protected int castleBits;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// history maintains the move-history of a game.  It is therefore possible to undo moves
		/// and to review the moves in a game.
		/// </summary>
		protected History history;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// numberOfTurnsSincePawnOrCapture assists in enforcing the rule that after 50 moves
		/// without a capture or pawn move that the game ends in a stalemate.
		/// </summary>
		protected int numberOfTurnsSincePawnOrCapture;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// state stores chess related states.  Like check, check-mate, stalemate, or play.
		/// </summary>
		protected State state;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// turn keeps track of which Player should make the next move.
		/// </summary>
		protected int turn;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// When isReady is true, the board can play chess immediately.  When false,
		/// the board knows it must "GetReady" before playing.
		/// </summary>
		protected bool isReady;
		/// <summary>
		/// A numeric approximation of the value of the board (from White's perspective).
		/// Higher values indicated a better board for White.
		/// </summary>
		protected int boardValue;

		#endregion

		///////////////////////////////////////////////////////////////////////
		//  ____            _       _   _        
		// |  _ \   _   _  | |__   | | (_)   ___ 
		// | |_) | | | | | | '_ \  | | | |  / __|
		// |  __/  | |_| | | |_) | | | | | | (__ 
		// |_|      \__,_| |_.__/  |_| |_|  \___|
		//

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Creates a ready-to-play chess board.
		/// </summary>
		public Board() : this( true )
		{
		}
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Creates an initialized board with the option of leaving the
		/// board empty or setting up the pieces
		/// </summary>
		/// <param name="setupDefault">If true the pieces are placed, otherwise the board remains empty</param>
		public Board( bool setupDefault )
		{
			Initialize();
			if ( setupDefault )
				Reset();
		}
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Initializes a new board from a Snapshot.
		/// </summary>
		/// <param name="snapshot">a "snapshot" of a previous game to reference</param>
		public Board( Snapshot snapshot ) : this( false )
		{
			enPassantBoard = snapshot.EnPassantBoard;
			castleBits = snapshot.CastleBits;
			turn = snapshot.WhosTurn;
			numberOfTurnsSincePawnOrCapture = snapshot.NumberOfTurnsSincePawnOrCapture;

			SnapshotElement se;
			for( int color = 0 ; color < 2 ; color++ )
			{
				for ( int pieceType = 0; pieceType < 6; pieceType++ )
				{
					for( int i = 0; i < Piece.NUMBER_OF_PER_PLAYER[ pieceType ]; i++ )
					{
						se = snapshot[color, pieceType, i];
						Piece piece = pieces[color][pieceType][i];
						Location pieceLoc = piece.Location;

						// If the piece is on the board, pick it up.
						if ( piece.IsOnBoard )
							RemovePiece( pieceLoc.x, pieceLoc.y );

						// If we need a different kind of piece, replace the piece
						if ( se.type != piece.Type )
						{
							piece = PieceFactory.CreatePiece( this, se.type, piece.Color, piece.Id, true);
							pieces[color][pieceType][i] = piece;
						}

						// Put the piece on the board where SnapshotElement says it belongs
						if ( se.isOnBoard )
							PutDownPiece( piece, se.location.x, se.location.y);
						else
							piece.Location = se.location;
					}
				}
			}
		}
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// static psuedo-Constructor.  Returns an empty board ready for pieces to be added.
		/// </summary>
		public static Board EmptyBoard()
		{
			return new Board( false );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Puts all the pieces on the board in their starting positions.
		/// </summary>
		public void Reset()
		{
			Clear();
			PlacePiecesOnBoard();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Clears the board of pieces.
		/// </summary>
		public void Clear()
		{
			board = new Piece[8,8];
			border.Reset();
			history.Reset();
			turn = Player.WHITE;
			state = State.Play;
			castleBits = 15;
			numberOfTurnsSincePawnOrCapture = 0;
			enPassantBoard = BitBoard.EMPTY;
			checkRestrictionBoard = BitBoard.EMPTY;
			piecesBitBoard[Player.WHITE] = BitBoard.EMPTY;
			piecesBitBoard[Player.BLACK] = BitBoard.EMPTY;
			boardValue = 0;

			// Go through every piece and Reset() it.
			for( int color = 0 ; color < 2 ; color++ )
				for ( int pieceType = 0; pieceType < 6; pieceType++ )
					for( int i = 0; i < Piece.NUMBER_OF_PER_PLAYER[ pieceType ]; i++ )
						// Check for upgraded pawns and turn them back to pawns
						if ( pieces[color][pieceType][i].WasPawn )
							pieces[color][pieceType][i] = PieceFactory.CreatePiece( this, Piece.PAWN, color, i, false);
						else
							pieces[color][pieceType][i].Reset();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Puts a piece on the board.
		/// </summary>
		/// <param name="color">The color of the piece</param>
		/// <param name="type">The type of piece</param>
		/// <param name="x">x coordinate</param>
		/// <param name="y">y coordinate</param>
		public void AddPiece( int color, int type, int x, int y )
		{
			int desiredType = type;
			int id = -1;

			// Attmept 1: Find a piece of the color & type that is not on the board
			foreach( Piece piece in pieces[color][type] )
			{
				if ( ! piece.IsOnBoard )
					id = piece.Id;
			}

			// Attmept 2: If no pieces of the desiredType are on the board, then 
			//         find any free pawn of "color", and upgrade it (if applicable).
			if ( id == -1 && ( type != Piece.PAWN && type != Piece.KING ) )
			{
				type = Piece.PAWN;
				foreach( Piece piece in pieces[color][Piece.PAWN] )
					if ( ! piece.IsOnBoard )
						id = piece.Id;
			} 

			// If we do not have a valid id at this point, then we cannot add this piece.
			if ( id == -1 )
				throw new AddPieceException( "Unable to add anymore " + Piece.TypeToString[ type ] + "s" );

			// If the Piece object isn't of the right subclass, then create one that is.
			if ( desiredType == Piece.PAWN && pieces[color][type][id].WasPawn )
				pieces[color][type][id] = PieceFactory.CreatePiece( this, Piece.PAWN, color, id, false);
			else if ( desiredType != type )
				pieces[color][type][id] = PieceFactory.CreatePiece( this, desiredType, color, id, true);

			// Place the game in a not-ready state.
			Unready();

			// Finish Adding the piece
			RegisterPiece( pieces[color][type][id], x, y );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Takes a piece off the board.
		/// </summary>
		/// <param name="x">x coordinate</param>
		/// <param name="y">y coordinate</param>
		public void RemovePiece( int x, int y )
		{
			if ( x < 0 || x > 7 || y < 0 || y > 7 )
				throw new OutOfBoundsException( "Coordinates out of bounds (" + x + "," + y + ")" );
			if ( board[x,y] == null )
				throw new EmptyLocationException( "No piece found at (" + x + "," + y + ")" );

			PickUpPiece( x, y );

			Unready();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Performs a few chores before allowing the game to begin.
		/// </summary>
		public void GetReady()
		{
			if ( ! isReady )
			{
				// Must set isReady to true or else infinite recursion will occur.
				isReady = true;

				try
				{
					Validate();
					RefreshAllPieces();
					PostMoveChecks();
					history.Reset();
				}
				catch
				{
					Unready();
					throw;
				}
			}
		}

		//////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Performs only legal chess moves.  If the attempted move is not legal an exception
		/// will be thrown.
		/// </summary>
		/// <param name="fx">From X</param>
		/// <param name="fy">From Y</param>
		/// <param name="tx">To X</param>
		/// <param name="ty">To Y</param>
		public void MovePiece( int fx, int fy, int tx, int ty )
		{
			MovePiece( fx, fy, tx, ty, Piece.UNKNOWN );
		}
			
		//////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Performs only legal chess moves.  If the attempted move is not legal an exception
		/// will be thrown.  The upgradeType must be set to either Queen, Rook, Knight or Bishop
		/// if this move will cause a pawn to reach the final row.  The upgradeType is ignored
		/// for all other moves.
		/// </summary>
		/// <param name="fx">From X</param>
		/// <param name="fy">From Y</param>
		/// <param name="tx">To X</param>
		/// <param name="ty">To Y</param>
		/// <param name="upgradeType">the piece type to upgrade the pawn to, if applicable</param>
		public void MovePiece( int fx, int fy, int tx, int ty, int upgradeType )
		{
			Piece piece1 = null, piece2 = null, piece3 = null;

			if ( ! isReady )
				GetReady();

			// Step 1a: Validate arguments
			if ( fx < 0 || fx > 7 || fy < 0 || fy > 7 || tx < 0 || tx > 7 || ty < 0 || ty > 7 )
				throw new OutOfBoundsException( "Invalid Move Coordinate (" + fx + "," + fy + ") or (" + tx + "," + ty + ")");
			if ( board[fx, fy] == null )
				throw new EmptyLocationException( "No Piece at (" + fx + "," + fy + ") to move." );

			// Make "piece1" point to the moving-piece.
			piece1 = board[fx, fy];

			// Step 1b: Validate arguments
			if ( piece1.Color != turn )
				throw new OutOfTurnException( "It is not " + Player.PlayerToString[ piece1.Color ] + "'s turn"  );
			if ( fx == tx && fy == ty )
				throw new IllegalMoveException( "Can't move to your own location (" + fx + "," + fy + ") to (" + tx + "," + ty + ")");
			if ( ! piece1.IsMovableTo(tx, ty) )
				throw new IllegalMoveException( "Illegal Move: (" + fx + "," + fy + ")-(" + tx + "," + ty + ")" );
			if ( piece1.Color == Player.WHITE && state == State.BlackCheck )
				throw new DataIntegrityException( "Impossible Situation:  Black is in check, and it's White's turn" );
			if ( piece1.Color == Player.BLACK && state == State.WhiteCheck )
				throw new DataIntegrityException( "Impossible Situation:  White is in check, and it's Black's turn" );
			if ( piece1.Type == Piece.PAWN && ty == Player.LAST_ROW[ piece1.Color ] 
				&& upgradeType != Piece.QUEEN && upgradeType != Piece.KNIGHT && upgradeType != Piece.ROOK && upgradeType != Piece.BISHOP )
				throw new InvalidPromotionTypeException( "Invalid Promotion Type" );

			// PICK UP the moving-piece and place it into "piece1" (again).
			piece1 = PickUpPiece( fx, fy );	// piece1 is the moving piece
			piece3 = piece1;
			HistoryEvent historyEvent = HistoryEvent.Move;
			// Remember the state of the EnPassantBoard & CastleBits for the history.
			ulong oldEnPassantBoard = enPassantBoard;
			int oldCastleBits = castleBits;
			enPassantBoard = BitBoard.EMPTY;

			// Step 2: Remove Castling ability if a King or Rook moves
			if ( piece1.Type == Piece.KING )
			{
				castleBits &= ( piece1.Color == Player.WHITE ) ? 12 : 3;
			}
			else if ( piece1.Type == Piece.ROOK )
			{
				if ( piece1.Color == Player.WHITE )
					castleBits &= ( piece1.Id == 0 ) ? 13 : 14;
				else
					castleBits &= ( piece1.Id == 0 ) ? 7 : 11;
			}

			// Step 3: Process the Move
			if ( board[tx, ty] != null )
			{
				// Simple Capture
				if ( piece1.Color == board[tx, ty].Color )
					throw new IllegalMoveException( Player.PlayerToString[ piece1.Color ] + " tried to take its own piece" );

				piece2 = PickUpPiece( tx, ty );
				historyEvent = HistoryEvent.Capture;
			} 
			else if ( piece1.Type == Piece.PAWN )
			{
				// If we are dealing with a Pawn move
				if ( fx != tx && board[tx,fy] != null )
				{
					// En Passant
					piece2 = PickUpPiece( tx, fy );
					historyEvent = HistoryEvent.EnPassant;
				} 
				else if ( Math.Abs( fy - ty) == 2 )
				{
					// Double Pawn Move - Enable EnPassantBoard for one turn
					enPassantBoard = BitBoard.bit[tx, ( fy + ty ) / 2 ];
				}
			} 
			else if ( piece1.Type == Piece.KING )
			{
				if ( fx - tx > 1 )
				{
					// Left Castle
					int homeRow = Player.FIRST_ROW[ piece1.Color ];
					PutDownPiece( PickUpPiece( 0, homeRow ), 2, homeRow );
					historyEvent = HistoryEvent.Castle;
				} 
				else if ( tx - fx > 1 )
				{
					// Right Castle
					int homeRow = Player.FIRST_ROW[ piece1.Color ];
					PutDownPiece( PickUpPiece( 7, homeRow ), 4, homeRow );
					historyEvent = HistoryEvent.Castle;
				}
			}

			if ( piece1.Type == Piece.PAWN && ty == (( piece1.Color == Player.WHITE ) ? 7 : 0) )
			{
				// Pawn Upgrade
				piece1 = PieceFactory.CreatePiece( this, upgradeType, piece3.Color, piece3.Id, true );
				pieces[piece3.Color][piece3.Type][piece3.Id] = piece1;
				historyEvent = HistoryEvent.PawnUpgrade;
			}

			// Step 4: Record the event in the history
			SimplePiece capturedSimplePiece;
			if ( piece2 != null )
				capturedSimplePiece = new SimplePiece( piece2.Color, piece2.Type, piece2.Id, piece2.WasPawn );
			else
				capturedSimplePiece = SimplePiece.NULL;

			history.Add( new HistoryEntry( new Location( fx, fy )
				, new Location( tx, ty )
				, new SimplePiece( piece1.Color, piece1.Type, piece1.Id, piece1.WasPawn)
				, capturedSimplePiece
				, historyEvent
				, state
				, numberOfTurnsSincePawnOrCapture
				, checkRestrictionBoard
				, oldEnPassantBoard
				, oldCastleBits ) );

			// Step 5: Move piece on board
			PutDownPiece( piece1, tx, ty );
			turn = Player.Inverse[ turn ];

			// Step 6: Perform post-move chores
			if ( piece3.Type == Piece.PAWN || historyEvent == HistoryEvent.Capture )
				numberOfTurnsSincePawnOrCapture = 0;
			else
				numberOfTurnsSincePawnOrCapture++;

			PostMoveChecks();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Undoes or Redoes a sepcified number of times.  (If positive then REDO
		/// else UNDO )
		/// </summary>
		public void UndoRedo( int times )
		{
			if ( times > 0 )
				while( times-- > 0 && CanRedo )
					Redo();
			else
				while( times++ < 0 && CanUndo)
					Undo();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Goes through the history and undoes every entry.
		/// </summary>
		public void UndoAll()
		{
			while( history.CanUndo )
				Undo();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// If the history is not empty, this will undo the most recent move.
		/// </summary>
		public void Undo()
		{
			if ( ! isReady )
				GetReady();
			if ( ! history.CanUndo )
				throw new LimitReachedException( "Cannot undo further" );

			HistoryEntry he = history.Undo();
			Piece movedPiece, capturedPiece = null;

			movedPiece = PickUpPiece( he.to.x, he.to.y );

			// "Regenerate" the piece that was captured
			if ( ! he.capturedPiece.IsNull )
			{
				capturedPiece = PieceFactory.CreatePiece( this, he.capturedPiece.Type, he.capturedPiece.Color, he.capturedPiece.Index, he.capturedPiece.WasPawn );
				if ( capturedPiece.WasPawn )
					pieces[capturedPiece.Color][Piece.PAWN][capturedPiece.Id] = capturedPiece;
				else
					pieces[capturedPiece.Color][capturedPiece.Type][capturedPiece.Id] = capturedPiece;
			}

			// Copy back the many state variables.
			numberOfTurnsSincePawnOrCapture = he.numberOfTurnsSincePawnOrCapture;
			checkRestrictionBoard = he.checkRestrictionBoard;
			enPassantBoard = he.enPassantBoard;
			castleBits = he.castleBits;
			state = he.state;
			turn = Player.Inverse[ turn ];

			// Reverse the move
			switch ( he.historyEvent )
			{
				case HistoryEvent.Capture:
					PutDownPiece( movedPiece, he.from.x, he.from.y );
					PutDownPiece( capturedPiece, he.to.x, he.to.y );
					break;
				case HistoryEvent.Castle:
					Piece rookPiece;
					PutDownPiece( movedPiece, he.from.x, he.from.y );
					if ( he.to.x == 1 )
					{
						rookPiece = PickUpPiece( 2, he.from.y );
						PutDownPiece( rookPiece, 0, he.from.y );
					}
					else
					{
						rookPiece = PickUpPiece( 4, he.from.y );
						PutDownPiece( rookPiece, 7, he.from.y );
					}
					break;
				case HistoryEvent.EnPassant:
					PutDownPiece( movedPiece, he.from.x, he.from.y );
					PutDownPiece( capturedPiece, he.to.x, he.from.y );
					break;
				case HistoryEvent.Move:
					PutDownPiece( movedPiece, he.from.x, he.from.y );
					break;
				case HistoryEvent.PawnUpgrade:
					movedPiece = PieceFactory.CreatePiece( this, Piece.PAWN, he.movedPiece.Color, he.movedPiece.Index, false );
					pieces[movedPiece.Color][Piece.PAWN][movedPiece.Id] = movedPiece;
					PutDownPiece( movedPiece, he.from.x, he.from.y );
					if ( ! he.capturedPiece.IsNull )
						PutDownPiece( capturedPiece, he.to.x, he.to.y );
					break;
				default:
					throw new DataIntegrityException( "Undo: Unknown HistoryEvent" );
			}
	
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Redoes until it can't redo further
		/// </summary>
		public void RedoAll()
		{
			while( history.CanRedo )
				Redo();
		}
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Gets the next move from the history and performs it.
		/// </summary>
		public void Redo()
		{
			if ( ! isReady )
				GetReady();
			if ( ! history.CanRedo )
				throw new ApplicationException( "Cannot Redo Further" );

			HistoryEntry he = history.Redo();

			if ( he.historyEvent == HistoryEvent.PawnUpgrade )
				MovePiece( he.from.x, he.from.y, he.to.x, he.to.y, he.movedPiece.Type );
			else
				MovePiece( he.from.x, he.from.y, he.to.x, he.to.y );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates a bit board that represents everywhere that a given color
		/// has attacking presence over.  (Doesn't mean it can attack)
		/// </summary>
		/// <param name="color">the side to create an AttackBitBoard from</param>
		/// <returns>A BitBoard that has true values everywhere that this color
		/// has attacking presence over.</returns>
		public ulong AttackBitBoard( int color )
		{
			ulong temp = BitBoard.EMPTY;

			foreach ( Piece[] pieceArray in pieces[color] )
				foreach ( Piece piece in pieceArray )
					if ( piece.IsOnBoard )
						temp |= piece.AttackBoard;

			return temp;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates a bit board that represents everywhere that a given color
		/// can move a piece to.  If this bit board IsEmpty then the given color
		/// has no moves.
		/// </summary>
		/// <param name="color">the side to create a MoveBitBoard from</param>
		/// <returns>A BitBaord that has true values everywhere that this color
		/// can move a piece to.</returns>
		public ulong MoveBitBoard( int color )
		{
			ulong temp = BitBoard.EMPTY;

			foreach ( Piece[] pieceArray in pieces[color] )
				foreach ( Piece piece in pieceArray )
					if ( piece.IsOnBoard )
						temp |= piece.MoveBoard;

			return temp;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates a bit board that represents everywhere that a given color
		/// has a piece that can move.  If this bit board IsEmpty then the given
		/// color has no moves.  
		/// </summary>
		/// <param name="color">the side to create a MoversBitBoard from</param>
		/// <returns>A BitBoard that has true values everywhere that this color
		/// has a piece that can move.</returns>
		public ulong MoversBitBoard( int color )
		{
			ulong temp = BitBoard.EMPTY;

			foreach ( Piece[] pieceArray in pieces[color] )
				foreach ( Piece piece in pieceArray )
					if ( piece.IsOnBoard && piece.MoveBoard != 0 )
						temp |= BitBoard.bit[piece.Location.x, piece.Location.y];
						
			return temp;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Fills a Moves data-structure with all of the possible moves and returns it.
		/// </summary>
		/// <returns>All the valid moves</returns>
		public AllMoves Moves()
		{
			AllMoves temp = new AllMoves();

			foreach ( Piece[] pieceArray in pieces[turn] )
				foreach ( Piece piece in pieceArray )
					if ( piece.IsOnBoard && piece.HasMoves )
						temp.Add( piece.Location, piece.Moves );

			return temp;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Answers the question:  Does player X have any moves?
		/// </summary>
		/// <param name="color">the Player in question</param>
		/// <returns>true if the player has moves, false if not</returns>
		public bool HasMoves( int color )
		{
			foreach ( Piece[] pieceArray in pieces[color] )
				foreach ( Piece piece in pieceArray )
					if ( piece.IsOnBoard && piece.MoveBoard != 0 )
						return true;
			return false;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates a new Board completely unrelated to this one.
		/// </summary>
		/// <returns></returns>
		public Board Clone()
		{
			Board b = new Board( true );
			b.ImportHistoryDump( HistoryDump() );
			return b;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates a new similar Board, without the history.
		/// </summary>
		/// <returns></returns>
		public Board IncompleteClone()
		{
			return new Board( new Snapshot( this ) );
		}

		public ArrayList HistoryDump()
		{
			ArrayList moves = new ArrayList();
			int count = history.Current;
			HistoryEntry he;
			for( int x = 0; x < count ; x++ )
			{
				he = history.Peek( x );
				if ( he.historyEvent == HistoryEvent.PawnUpgrade )
					moves.Add( new Move( he.from, he.to, he.movedPiece.Type ) );
				else
					moves.Add( new Move( he.from, he.to, Piece.UNKNOWN ) );
			}
			return moves;
		}

		public void ImportHistoryDump( ArrayList moves )
		{
			int count = moves.Count;
			Move move;
			try
			{
				for( int x = 0; x < count ; x++ )
				{
					move = (Move) moves[x];
					MovePiece( move.From.x, move.From.y, move.To.x, move.To.y, move.PromotionType );
				}
			}
			catch
			{
                throw new ImportException( "Could not complete the import" );                
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates a count-list of piece with respect to their players.
		/// </summary>
		/// <returns>A jagged array of ints.  First index is player, Second is piece type</returns>
		public int[][] CountPieces()
		{
			int[][] values = new int[2][];

			for( int color = 0 ; color < 2 ; color++ )
			{
				values[color] = new int[6];
				for ( int pieceType = 0; pieceType < 6; pieceType++ )
				{
					for( int i = 0; i < Piece.NUMBER_OF_PER_PLAYER[ pieceType ]; i++ )
					{
						if ( pieces[color][pieceType][i].IsOnBoard )
							values[color][pieceType]++;
					}
				}
			}

			return values;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Property. Creates a hash key for the board using the HashKeyComponents.
		/// </summary>
		public int CreateHashKey
		{
			get
			{
				int hash = 0;

				foreach ( Piece[][] piecesInColor in pieces )
					foreach ( Piece[] piecesArray in piecesInColor )
						foreach( Piece piece in piecesArray )
							if ( piece.IsOnBoard )
								hash ^= HashKeyComponent[piece.Type + piece.Color * 6][piece.Location.x,piece.Location.y];
				// hash ^= (int) ( ( ( enPassantBoard >> 32 ) & 65280UL ) & ( enPassantBoard >> 16 ) & 255UL );
				// hash ^= (int) ( ( ( castleBoard >> 56 ) & 65280UL ) & ( castleBoard & 255UL ) );
				return hash;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Property.  Creates a hash lock for the board using the HashLockComponents
		/// </summary>
		public int CreateHashLock
		{
			get
			{
				int hash = 0;

				foreach ( Piece[][] piecesInColor in pieces )
					foreach ( Piece[] piecesArray in piecesInColor )
						foreach( Piece piece in piecesArray )
							if ( piece.IsOnBoard )
								hash ^= HashLockComponent[piece.Type + piece.Color * 6][piece.Location.x,piece.Location.y];

				return hash;
			}
		}

		public State GameState
		{
			get
			{
				if ( ! isReady )
					GetReady();
				return state;
			}
		}
		// Fix this to return a SimpleHistory
		public History History
		{
			get
			{
				if ( ! isReady )
					GetReady();
				return history;
			}
		}
		public bool IsGameOver
		{
			get
			{
				if ( ! isReady )
					GetReady();
				return ( state == State.BlackCheckMate 
					|| state == State.WhiteCheckMate
					|| state == State.Stalemate );
			}
		}
		public int Turn
		{
			get
			{
				return turn;
			}
			set
			{
				Unready();
				turn = value;
			}
		}
		public ulong CheckRestrictionBoard
		{
			get
			{
				return checkRestrictionBoard;
			}
		}
		public ulong EnPassantBoard
		{
			get
			{
				return enPassantBoard;
			}
			set
			{
				enPassantBoard = value ;
			}
		}
		public int CastleBits
		{
			get
			{
				return castleBits;
			}
			set
			{
				castleBits = value ;
			}
		}
		public int NumberOfTurnsSincePawnOrCapture
		{
			get
			{
				return numberOfTurnsSincePawnOrCapture;
			}
			set
			{
				numberOfTurnsSincePawnOrCapture = value;
			}
		}
		public bool IsReady
		{
			get
			{
				return isReady;
			}
		}
		public bool CanUndo
		{
			get
			{
				return history.CanUndo;
			}
		}
		public bool CanRedo
		{
			get
			{
				return history.CanRedo;
			}
		}
		public int BoardValue
		{
			get
			{
				return ( turn == Player.WHITE ) ? boardValue : -boardValue ;
			}
		}
		public override String ToString()
		{
			String output = "";
			for( int y = 7; y >= 0; y-- )
			{
				for( int x = 0; x < 8; x++ )
				{
					if ( board[x,y] != null )
						output += board[x,y].ToSmallString();
					else
						output += "..";

					output += ( x < 7 ) ? " " : Environment.NewLine;
				}
			}
			return output;
		}

		
		
		
		
		
		
		
		
		public Piece[][][] Pieces
		{
			get
			{
				return pieces;
			}
		}
		public ulong[] PiecesBitBoard
		{
			get
			{
				return piecesBitBoard;
			}
		}
		public Piece this[ int x, int y]
		{
			get
			{
				return board[x,y];
			}
		}



		//  ____           _                   _          
		// |  _ \   _ __  (_) __   __   __ _  | |_    ___ 
		// | |_) | | '__| | | \ \ / /  / _` | | __|  / _ \
		// |  __/  | |    | |  \ V /  | (_| | | |_  |  __/
		// |_|     |_|    |_|   \_/    \__,_|  \__|  \___|
		//    

		/// <summary>
		/// Declares this board unready.  Being "unready" means that important
		/// variables have not been initialized and that the board is unplayable
		/// until the board becomes "ready" again.  The primary purpose of this
		/// mechanism is to allow the board to be altered multiple times without
		/// having the board recalculate everything for every alteration.  This way
		/// the board will just enter the unready state during the alterations, and
		/// when an important action needs to take place, then the board will ready
		/// itself automatically.  (in theory :)
		/// </summary>
		private void Unready()
		{
			isReady = false;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// This effectively initializes the pieces so they know where they can move.
		/// This method should be called after all the pieces are placed on the board.
		/// </summary>
		private void RefreshAllPieces()
		{
			foreach ( Piece[][] piecesInColor in pieces )
				foreach ( Piece[] piecesArray in piecesInColor )
					foreach( Piece piece in piecesArray )
						if ( piece.IsOnBoard )
							piece.GenerateMoves();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Checks for necessary data.
		/// </summary>
		private void Validate()
		{
			Piece WK = pieces[Player.WHITE][Piece.KING][0];
			Piece BK = pieces[Player.BLACK][Piece.KING][0];
			if ( ! WK.IsOnBoard )
				throw new ApplicationException( "White King is missing" );
			if ( ! BK.IsOnBoard )
				throw new ApplicationException( "Black King is missing" );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates and initializes the 'pieces' jagged array and the 'board' array
		/// </summary>
		private void Initialize()
		{
			border = new Border();
			history = new History();
			checkRestrictionBoard = BitBoard.EMPTY;
			enPassantBoard = BitBoard.EMPTY;
			castleBits = 0;

			pieces = new Piece[2][][];
			board = new Piece[8,8];

			// Initialize the piecesBitBoards.  These are used to track where White or 
			// Black pieces reside, but not what kinds of pieces they are.
			piecesBitBoard = new ulong[2];
			piecesBitBoard[ Player.WHITE ] = BitBoard.EMPTY;
			piecesBitBoard[ Player.BLACK ] = BitBoard.EMPTY;

			// Initialize all of the pieces.  After Initialization there should never be
			// a null piece in the pieces[][][] array.  The following 3 nested for loops
			// cycle through the colors, piece-types, and index.  The index goes from 0
			// to number of pieces a player should have.
			for( int color = 0 ; color < 2 ; color++ )
			{
				pieces[color] = new Piece[6][];
				for ( int pieceType = 0; pieceType < 6; pieceType++ )
				{
					pieces[color][pieceType] = new Piece[ Piece.NUMBER_OF_PER_PLAYER[ pieceType ] ];
					for( int i = 0; i < Piece.NUMBER_OF_PER_PLAYER[ pieceType ] ; i++ )
					{
						pieces[color][pieceType][i] = PieceFactory.CreatePiece( this, pieceType, color, i, false);
					}
				}
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Puts all the pieces onto the board in their default positions.
		/// </summary>
		private void PlacePiecesOnBoard()
		{
			// PlacePieces
			foreach ( Piece[][] piecesInColor in pieces )
				foreach ( Piece[] piecesArray in piecesInColor )
					foreach( Piece piece in piecesArray )
						RegisterPiece( piece );

			RefreshAllPieces();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Determines Check, CheckMate, or Stalemate conditions
		/// </summary>
		private void PostMoveChecks()
		{
			bool isWhiteInCheck = false, isBlackInCheck = false;
			Piece whiteKing = pieces[Player.WHITE][Piece.KING][0];
			Piece blackKing = pieces[Player.BLACK][Piece.KING][0];

			if ( ! whiteKing.IsOnBoard )
				throw new DataIntegrityException( "White king is not on board" );
			if ( ! blackKing.IsOnBoard )
				throw new DataIntegrityException( "Black king is not on board" );

			// Assume Play unless otherwise changed
			state = State.Play;

			// Check for White Check
			if ( ( AttackBitBoard( Player.BLACK ) & BitBoard.bit[whiteKing.Location.x, whiteKing.Location.y] ) != 0)
				isWhiteInCheck = true;
			// Check for Black Check
			if ( ( AttackBitBoard( Player.WHITE ) & BitBoard.bit[blackKing.Location.x, blackKing.Location.y] ) != 0 )
				isBlackInCheck = true;
			if ( isWhiteInCheck && isBlackInCheck )
				throw new DataIntegrityException( "White and Black kings are in check" );

			if ( isWhiteInCheck )
			{
				state = State.WhiteCheck;
				CreateCheckRestrictionBoard( Player.WHITE );
				if ( MoveBitBoard( Player.WHITE ) == 0 )
					state = State.WhiteCheckMate;
			} 
			else if ( isBlackInCheck )
			{
				state = State.BlackCheck;
				CreateCheckRestrictionBoard( Player.BLACK );
				if ( MoveBitBoard( Player.BLACK ) == 0 )
					state = State.BlackCheckMate;
			} 
			else if ( ! HasMoves( turn )
				|| ( ( piecesBitBoard[Player.WHITE] & ~BitBoard.bit[whiteKing.Location.x, whiteKing.Location.y] ) == BitBoard.EMPTY
				&& ( piecesBitBoard[Player.BLACK] & ~BitBoard.bit[blackKing.Location.x, blackKing.Location.y] ) == BitBoard.EMPTY ) )
			{
				state = State.Stalemate;
			}

			if ( numberOfTurnsSincePawnOrCapture >= 50 )
				state = State.Stalemate;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates a BitBoard that is used to restrict the movements of pieces when their
		/// king is in check.  Either the enemy piece must be destroyed or a piece must get
		/// in it's line of sight.  (The King does NOT use the CheckRestrictionBoard)
		/// </summary>
		/// <param name="color">The color of the side that is in check.</param>
		private void CreateCheckRestrictionBoard( int color )
		{
			if ( ! isReady )
				GetReady();

			Piece king = pieces[color][ Piece.KING][0];
			SightGrid sightGrid = king.SightGrid;
			int x = king.Location.x;
			int y = king.Location.y;
			int enemyColor = Player.Inverse[ color ];
			ulong kingsLocationBB = BitBoard.bit[x,y];

			int pawnY = ( color == Player.WHITE ) ? y + 1 : y - 1;
			if ( pawnY < 0 || pawnY > 7 )
				pawnY = -1;
			int numberOfAttackers = 0;
			Piece tempPiece;
			int roughDir;

			// Step 1: Clear the bit board
			checkRestrictionBoard = BitBoard.EMPTY;

			// Step 2a: Check for Knight Attackers
			Piece knight1 = pieces[enemyColor][Piece.KNIGHT][0];
			Piece knight2 = pieces[enemyColor][Piece.KNIGHT][1];
			if ( knight1.IsOnBoard && ( knight1.AttackBoard & kingsLocationBB ) != 0UL ) 
			{
				checkRestrictionBoard |= BitBoard.bit[knight1.Location.x, knight1.Location.y];
				numberOfAttackers++;
			}
			if ( knight2.IsOnBoard && ( knight2.AttackBoard & kingsLocationBB ) != 0UL )
			{
				checkRestrictionBoard |= BitBoard.bit[knight2.Location.x, knight2.Location.y];
				numberOfAttackers++;
			}
			
			// Step 2b: Check for Line-Of-Sight Attackers & Pawn Attackers
			for( int d = 0; d < 8; d++ )
			{
				if ( ! sightGrid[d].IsReal )
					continue;

				tempPiece = sightGrid[ d ];
				roughDir = d % 2;	// Approximate direction to either x (0) or + (1)
				if ( tempPiece.Color == color )
					continue;
				if ( ( roughDir == Directions.Cross && tempPiece.Type == Piece.BISHOP )
					|| ( roughDir == Directions.Plus && tempPiece.Type == Piece.ROOK )
					|| tempPiece.Type == Piece.QUEEN )
				{
					checkRestrictionBoard = BitBoard.SetPartialLine( checkRestrictionBoard, x, y, d, tempPiece.Location.x, tempPiece.Location.y, true );
					numberOfAttackers++;
				}
				else if ( tempPiece.Type == Piece.PAWN 
					&& roughDir == 0
					&& tempPiece.Location.y == pawnY )
				{
					checkRestrictionBoard |= BitBoard.bit[tempPiece.Location.x, tempPiece.Location.y];
					numberOfAttackers++;
				}
			}

			// Assumption:  If the king is being attacked by more than 1 attacker,
			//              then no piece can help the king.
			if ( numberOfAttackers > 1 )
				checkRestrictionBoard = BitBoard.EMPTY;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Refreshes the movement boards of the knights that are 
		/// within 1-knight move from the given location.
		/// </summary>
		/// <param name="x">x coordinate</param>
		/// <param name="y">y coordinate</param>
		private void RefreshKnights( int x, int y )
		{
			int nx, ny1, ny2, ox;
			for( ox = -2; ox < 3; ox = ( ox == 0 ) ? 1 : ox + 1 )
			{
				nx = x + ox;
				ny1 = y + (( ox == 1 || ox == -1 ) ? 2 : 1);
				ny2 = y - (( ox == 1 || ox == -1 ) ? 2 : 1);
				if ( nx >= 0 && nx <= 7 
					&& ny1 >= 0 && ny1 <= 7 
					&& board[nx,ny1] != null 
					&& board[nx,ny1].Type == Piece.KNIGHT )
				{
					board[nx,ny1].GenerateMoves();
				}
				if ( nx >= 0 && nx <= 7 
					&& ny2 >= 0 && ny2 <= 7 
					&& board[nx,ny2] != null 
					&& board[nx,ny2].Type == Piece.KNIGHT )
				{
					board[nx,ny2].GenerateMoves();
				}
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Places a piece onto the board according to where the piece says it should go.
		/// </summary>
		/// <param name="piece">The piece to put on the board</param>
		private void RegisterPiece( Piece piece )
		{
			piece.SetLocationFromIndex();
			RegisterPiece( piece, piece.Location.x, piece.Location.y );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Places a piece onto the board.  The piece is only notified to update its location
		/// and DOES NOT update anything else.
		/// </summary>
		/// <param name="piece">The piece to put on the board</param>
		/// <param name="x">x coordinate to place the piece</param>
		/// <param name="y">y coordinate to place the piece</param>
		internal void RegisterPiece( Piece piece, int x, int y )
		{
			if ( x < 0 || x > 7 || y < 0 || y > 7 )
				throw new OutOfBoundsException( "Invalid Coordinate (" + x + "," + y + ")" );
			if ( piece.IsOnBoard )
				throw new DataIntegrityException( "Attempting to Register a piece that is on the board already" );

			piece.SightGrid = CreateSightGrid( x, y, piece );
			piece.PutDownAt( x, y );
			board[x,y] = piece;
			piece.SightGrid.Link();

			// Update the piecesBitBoard to reflect the added piece
			if ( ( piecesBitBoard[piece.Color] & BitBoard.bit[x,y] ) != 0 )
				throw new DataIntegrityException( "piecesBitBoard[" + Player.PlayerToString[ piece.Color ] + "] says a piece is already at " + piece.Location );
			piecesBitBoard[piece.Color] |= BitBoard.bit[x,y];
			boardValue = boardValue + ( ( piece.Color == Player.WHITE) ? Piece.VALUE[ piece.Type ] : - Piece.VALUE[ piece.Type ] );
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Places a piece on the board, and also refreshes it.  This is the primary
		/// way to put a piece on the board after all the pieces have been placed.
		/// </summary>
		/// <param name="piece">The piece to put on the board</param>
		/// <param name="x">x coordinate to place the piece</param>
		/// <param name="y">y coordinate to place the piece</param>
		internal void PutDownPiece( Piece piece, int x, int y )
		{
			if ( x < 0 || x > 7 || y < 0 || y > 7 )
				throw new OutOfBoundsException( "Invalid coordinates to PutDownPiece" );

			RegisterPiece( piece, x, y );
			if ( piece.Type != Piece.KING )
				piece.GenerateMoves();
			RefreshPieces( piece.SightGrid, x, y );

			// Refresh Both Kings
			pieces[Player.BLACK][Piece.KING][0].GenerateMoves();
			pieces[Player.WHITE][Piece.KING][0].GenerateMoves();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Takes a piece off of the board.
		/// </summary>
		/// <param name="x">x coordinate of the piece</param>
		/// <param name="y">y coordinate of the piece</param>
		/// <returns>The piece that was on the board</returns>
		internal Piece PickUpPiece( int x, int y )
		{
			Piece piece;
			SightGrid sightGrid;

			if ( x < 0 || x > 7 || y < 0 || y > 7 )
				throw new OutOfBoundsException( "Coordinate out of bounds" );
			if ( board[x,y] == null )
				throw new EmptyLocationException( "No Piece Located at (" + x + "," + y + ")" );

			piece = board[x,y];
			sightGrid = piece.SightGrid;
			board[x,y] = null;

			// Update the piecesBitBoard to reflect the picked up piece
			if ( ( piecesBitBoard[piece.Color] & BitBoard.bit[x,y] ) == 0 )
				throw new DataIntegrityException( "PickUpPiece: piecesBitBoard[" + Player.PlayerToString[ piece.Color ] + "] says there is no piece at " + piece.Location );
			piecesBitBoard[piece.Color] &= ~BitBoard.bit[x,y];
			boardValue = boardValue + ( ( piece.Color == Player.WHITE) ? - Piece.VALUE[ piece.Type ] : Piece.VALUE[ piece.Type ] );

			piece.PickUp();
			sightGrid.Unlink();

			RefreshPieces( sightGrid, x, y );

			return piece;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Updates all pieces that are "seen" by the sightGrid.
		/// </summary>
		/// <param name="sightGrid">The sightGrid to use</param>
		/// <param name="x">x coordinate of the piece</param>
		/// <param name="y">y coordinate of the piece</param>
		private void RefreshPieces( SightGrid sightGrid, int x, int y )
		{
			// Refresh Affected Knights
			RefreshKnights( x, y );
			// Refresh Line-of-Sight pieces
			for( int dir = 0, odir; dir < 8; dir++ )
				if ( sightGrid[dir].IsReal )
				{
					odir = Directions.Inverse[dir];
					sightGrid[dir].FixMoves( odir );
				}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Creates a SightGrid for a given position.  A SightGrid is used to keep
		/// track of Line-Of-Sight pieces.
		/// </summary>
		/// <param name="x">x coordinate to create a SightGrid from</param>
		/// <param name="y">y coordinate to create a SightGrid from</param>
		/// <returns>The new SightGrid</returns>
		private SightGrid CreateSightGrid( int x, int y, Piece piece )
		{
			SightGrid sightGrid = new SightGrid( piece );

			if ( board[x,y] != null )
				throw new DataIntegrityException( "Cannot CreateSightGrid at " + new Location( x, y ) + " because " + board[x,y] + " is already there");

			// Have SightGridBuilder head out in "statistically" better directions.  (Instead of
			// always just going Up, we decide which side it is closer to and head in that way).
			SightGridBuilder( sightGrid, x, y, ( x - y < 0 ) ? Directions.UpLeft : Directions.DownRight );
			SightGridBuilder( sightGrid, x, y, (   y > 3   ) ? Directions.Up : Directions.Down );
			SightGridBuilder( sightGrid, x, y, ( 7-x-y < 0 ) ? Directions.UpRight : Directions.DownLeft );
			SightGridBuilder( sightGrid, x, y, (   x > 3   ) ? Directions.Right : Directions.Left );

			return sightGrid;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Explores the board in a certain direction and from a given point to find a piece.
		/// When it finds a piece, it registers that piece with the sightGrid and also registers
		/// the piece that is the other direction.
		/// </summary>
		/// <param name="sightGrid">the sightGrid to update</param>
		/// <param name="x">x coordinate to start from</param>
		/// <param name="y">y coordinate to start from</param>
		/// <param name="dir">direction to look for a piece</param>
		private void SightGridBuilder( SightGrid sightGrid, int x, int y, int dir)
		{
			if ( dir == Directions.Unknown )
				throw new ApplicationException( "Invalid Direction" );
			
			int odir = Directions.Inverse[dir];
			int ox = Directions.XOffset[dir];
			int oy = Directions.YOffset[dir];

			// Head out to find a piece
			for( int mx = x + ox, my = y + oy ; ; mx += ox, my += oy )
			{
				if ( mx < 0 || mx > 7 || my < 0 || my > 7 )
				{
					// If we couldn't find a piece on the board, then link it to the border
					sightGrid[dir] = border.GetBorderPart( x, y, dir );
					break;
				}
				else if ( board[mx, my] != null )
				{
					sightGrid[dir] = board[mx, my];
					break;
				}
			}

			if ( sightGrid[dir].SightGrid == null )
				throw new OutOfBoundsException( "null SightGrid from a " + sightGrid[dir].ToString() );
			if ( sightGrid[dir].SightGrid[odir] == null )
				throw new OutOfBoundsException( "null SightGrid element towards: " + odir + " from: (" + x + "," + y + ")" );

			// Register the piece in other-direction
			sightGrid[odir] = sightGrid[dir].SightGrid[odir];
		}

		#region Debug Tools
		public void ValidateIntegrity()
		{
			for( int x = 0; x < 8; x++ )
			{
				for( int y = 0; y < 8; y++ )
				{
					if ( board[x,y] != null )
						board[x,y].SightGrid.Validate();
				}
			}

			if ( (piecesBitBoard[Player.WHITE] & piecesBitBoard[Player.BLACK]) != 0 )
				throw new DataIntegrityException( "There exists a black and white piece at the same place" );

		}
		public void Debug()
		{
			String output = "";
			for( int y = 7; y >= 0; y-- )
			{
				for( int d = 0; d < 3; d++ )
				{
					for( int x = 0; x < 8; x++ )
					{
						switch ( d )
						{
							case 0:
								output += DebugHelper( x, y, 2 );
								output += DebugHelper( x, y, 3 );
								output += DebugHelper( x, y, 4 );
								break;
							case 1:
								output += DebugHelper( x, y, 1 );
								output += " ";
								output += DebugHelper( x, y, 5 );
								break;
							case 2:
								output += DebugHelper( x, y, 0 );
								output += DebugHelper( x, y, 7 );
								output += DebugHelper( x, y, 6 );
								break;
						}					
						output += " ";
					}
					output += Environment.NewLine;
				}
				output += Environment.NewLine;
			}
			System.Console.Out.WriteLine( output );
		}
		public String DebugHelper( int x, int y, int d)
		{
			if ( board[x,y] == null )
				return ".";
			if ( board[x,y].SightGrid == null )
				return "?";
			if ( board[x,y].SightGrid[d] == null )
				return "?";
			return board[x,y].SightGrid[d].ToSmallString()[1] + "";
		}
		#endregion
        
	}
}

