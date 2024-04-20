using System;
using System.Collections;

namespace Chess
{

	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Abstract Class.  Manages the required data to represent a piece and its possible attacks and moves.
	/// </summary>
	public abstract class Piece
	{

		#region Static Members
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Piece Types Constants
		/// </summary>
		public const int PAWN = 0;
		public const int ROOK = 1;
		public const int KNIGHT = 2;
		public const int BISHOP = 3;
		public const int QUEEN = 4;
		public const int KING = 5;
		public const int BORDER = 6;
		public const int UNKNOWN = 7;

		public const int FIRST_PIECE = PAWN;
		public const int LAST_PIECE = KING;

		public static readonly int[] NUMBER_OF_PER_PLAYER = { 8, 2, 2, 2, 1, 1, 36, 0};
		public static readonly int[] VALUE = { 1, 5, 3, 3, 9, 0 };
		public static readonly string[] TypeToString = { "Pawn", "Rook", "Knight", "Bishop", "Queen", "King", "Border", "Unknown" };
		public static readonly string[] TypeToSmallString = { "P", "R", "N", "B", "Q", "K", "B", "U"};
		#endregion

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The board this piece is associated with.
		/// </summary>
		protected Board board;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Is this piece a real piece or is it a helper piece (like a BorderPart)?
		/// </summary>
		protected bool isReal = true;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The sightgrid, which stores what pieces are in the line-of-sight.
		/// </summary>
		protected SightGrid sightGrid;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The color of this piece
		/// </summary>
		protected int color;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The type of piece it is.
		/// </summary>
		protected int type;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Identifier to help distinguish between similar pieces.  e.g. Pawn0, Pawn1, ... , Rook1
		/// </summary>
		protected int id;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The location on the board (if on the board, refer to isOnBoard)
		/// </summary>
		protected Location location;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// True if this piece is currently on the board, false if not.
		/// </summary>
		protected bool isOnBoard;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The available moves this piece can make.
		/// </summary>
		protected ulong moveBoard;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// the attack bitboard that this piece has "attacking influence" over.
		/// </summary>
		protected ulong attackBoard;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The direction that king (of same color) is.
		/// If the king is not "visible" then kingDirection is Unknown
		/// </summary>
		protected int kingDirection;
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Stores whether this piece used to be a pawn that was promoted.
		/// </summary>
		protected bool wasPawn;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Initializes this piece with some immutable data values.  Once this piece is
		/// assigned certain values, like type and color, it will never change them.
		/// </summary>
		/// <param name="b">the Board that this piece belongs to</param>
		/// <param name="p">the type of piece</param>
		/// <param name="c">the color of this piece</param>
		/// <param name="i">the id for this piece.  Used to tell apart similar pieces</param>
		/// <param name="inWasPawn">true if this piece was a pawn that was upgraded</param>
		public void Initialize( Board b, int p, int c, int i, bool inWasPawn)
		{
			board = b;
			type = p;
			color = c;
			id = i;
			wasPawn = inWasPawn;

			Reset();
		}
		
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Resets game play data.  This method effectively places the piece "outside" of
		/// the board, as if it is waiting to be placed onto the board.
		/// </summary>
		public void Reset()
		{
			sightGrid = null;
			location = new Location( -1, -1 );
			isOnBoard = false;
			moveBoard = BitBoard.EMPTY;
			attackBoard = BitBoard.EMPTY;
		}
		
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Internally updates information that is needed to represent being picked up.
		/// </summary>
		public void PickUp()
		{
			isOnBoard = false;
		}
		
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Internally updates information that is needed to represent being placed
		/// at a location on the board.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public virtual void PutDownAt( int x, int y )
		{
			location.SetLocation( x, y );
			isOnBoard = true;

			// RefreshKingDirection();
			GenerateAttacks();
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Updates this piece with respect to the given direction.
		/// </summary>
		/// <param name="dir">The direction from which to update from</param>
		public abstract void FixMoves( int dir );

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Populates the attackBoard bitBoard with true values everywhere this
		/// piece has an attacking influence on.
		/// </summary>
		public abstract void GenerateAttacks();

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Populates the moveBoard bitBoard with true values everywhere this
		/// piece can move.
		/// </summary>
		public abstract void GenerateMoves();

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Sets the location of this piece to its default location
		/// </summary>
		public abstract void SetLocationFromIndex();

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Used by Queen, Bishop, and Rook to create their line-of-sight moves & attacks.
		/// </summary>
		/// <param name="dir">The direction to head</param>
		public void MovesHelper( int dir )
		{
			int ox, oy;

			for( dir = dir % 2; dir < Directions.LAST_DIRECTION ; dir += 2 )
			{
				ox = Directions.XOffset[dir];
				oy = Directions.YOffset[dir];

				for( int x = location.x + ox, y = location.y + oy
						 ; x >= 0 && x <= 7 && y >= 0 && y <= 7
					; x += ox, y += oy )
				{
					attackBoard |= BitBoard.bit[x, y];
					if ( board[x,y] != null )
					{
						if ( board[x,y].color != color )
						{
							moveBoard |= BitBoard.bit[x, y];
							// Special Case:  If the enemy king is found, then we must go one
							//                space past the king to ensure that the king can't
							//                take a step away, which would still leave him in
							//                check.   (For attackBoard only)
							if ( board[x,y].type == Piece.KING )
							{
								x += ox;
								y += oy;
								if ( x >= 0 && x <= 7 && y >= 0 && y <= 7 )
									attackBoard |= BitBoard.bit[x, y];
							}
						}
						break;
					}
					moveBoard |= BitBoard.bit[x, y];
				}
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Returns true if this piece can move to (x, y)
		/// and false otherwise.
		/// </summary>
		/// <param name="x">x coordinate to check</param>
		/// <param name="y">y coordinate to check</param>
		/// <returns>true if the move is legal, false if it's not</returns>
		public bool IsMovableTo( int x, int y )
		{
			return ( RealMoveBoard() & BitBoard.bit[x,y] ) != 0;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Further refines the possible moves of this piece if needed.
		/// If the king is in check, for instance, then this piece can only
		/// move if it can save the king.
		/// </summary>
		/// <returns>the real bitBoard to use for movement checks</returns>
		public ulong RealMoveBoard()
		{
			ulong moves;

			if ( ! board.IsReady )
				board.GetReady();

			if ( board.IsGameOver )
				return BitBoard.EMPTY;

			moves = moveBoard;

			// Enable "En Passant" move if applicable
			if ( type == Piece.PAWN && location.y == ( ( color == Player.WHITE ) ? 4 : 3 ) )
			{
				ulong enPassantBoard = board.EnPassantBoard;
				if ( enPassantBoard != BitBoard.EMPTY && ( attackBoard & enPassantBoard ) != BitBoard.EMPTY )
				{
					bool allowEnPassant = true;

					// This pawn is eligible for En Passant BUT we must first check that we will
					// not be endangering the king by doing it.  The "normal" method of checking
					// for king endangerment does not work here because two pieces are being
					// manipulated.
					// If the king is on this row, then check for possible king endangerment situations
					if ( board.Pieces[color][Piece.KING][0].Location.y == location.y )
					{
						int pawnDirection, otherDirection;
						int kingDir = Directions.Unknown;
						int dangerDir = Directions.Unknown;

						// We know there is a pawn next to us.  Let's find a pawn and use it
						// as the basis of our king endangerment algorithm.
						if ( sightGrid[ Directions.Left ].Type != Piece.PAWN )
							pawnDirection = Directions.Right;
						else
							pawnDirection = Directions.Left;
						otherDirection = Directions.Inverse[ pawnDirection ];

						// Now let's look in the otherDirection from that Pawn to look for either
						// our King or an enemy piece that MIGHT-BE threaten our king.
						if( sightGrid[otherDirection].Color == color 
							&& sightGrid[otherDirection].Type == Piece.KING )
						{
							kingDir = otherDirection;
						}
						else if( sightGrid[otherDirection].Color != color 
							&& ( sightGrid[otherDirection].Type == Piece.QUEEN 
							|| sightGrid[otherDirection].Type == Piece.ROOK ) )
						{
							dangerDir = otherDirection;
						}

						// English:  If there is danger in the "otherDirection" and our king is 
						// beyond the pawn OR if our king is in the "otherDirection" and there is
						// danger beyond the pawn, then don't allow the en passant.
						Piece piecePastPawn = sightGrid[pawnDirection].SightGrid[pawnDirection];
						if ( ( dangerDir == otherDirection 
							&& piecePastPawn.Color == color 
							&& piecePastPawn.Type == Piece.KING ) 
							||
							( kingDir == otherDirection 
							&& piecePastPawn.Color != color 
							&& ( piecePastPawn.Type == Piece.QUEEN 
							|| piecePastPawn.Type == Piece.ROOK ) ) )
						{
							allowEnPassant = false;
						}
					}

					if ( allowEnPassant )
						moves |= enPassantBoard;
				}
			}

			// Prevent King Endangerment:
			// If the king (of same color) can be seen by this piece, then make sure that we are
			// not going to put the king in check by moving this piece.
			// ToDo:  Evaluate whether this should be moved into the GenerateMove method instead.
			if ( type != Piece.KING && kingDirection != Directions.Unknown )
			{
				Piece dangerPiece = sightGrid[ Directions.Inverse[ kingDirection ] ];
				int kingDir = kingDirection % 2;	// Approximate direction to either x (0) or + (1)

				if ( dangerPiece.Color != color 
					&& ( ( kingDir == Directions.Cross && dangerPiece.Type == Piece.BISHOP )
					|| ( kingDir == Directions.Plus && dangerPiece.Type == Piece.ROOK )
					|| dangerPiece.Type == Piece.QUEEN ) )
				{
					moves &= BitBoard.SetLine( BitBoard.EMPTY, location.x, location.y, kingDirection, true );
				}
			}

			// If king is in check, then all the pieces (except the king) must use the
			// "CheckRestrictionBoard" to limit their moves only king protection moves.
			if ( type != Piece.KING 
				&& ( ( board.GameState == Board.State.WhiteCheck && color == Player.WHITE )
					|| ( board.GameState == Board.State.BlackCheck && color == Player.BLACK ) ) )
			{
				moves &= board.CheckRestrictionBoard;
			}


			return moves;
		}


		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Returns a two character string description
		/// </summary>
		/// <returns>a two character description</returns>
		public String ToSmallString()
		{
			return Player.PlayerToSmallString[ color ] + Piece.TypeToSmallString[ type ];
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Returns a string description.
		/// </summary>
		/// <returns>the description</returns>
		public override string ToString()
		{
			String output = Player.PlayerToString[ color ] + " " + Piece.TypeToString[ type ];

			if ( NUMBER_OF_PER_PLAYER[ type ] > 1 )
				output += "#" + id;

			if ( isReal )
			{
				if ( isOnBoard )
					output += " (" + location.x + "," + location.y + ")";
				else
					output += " (OFF-BOARD)";
			}

			if ( kingDirection != Directions.Unknown )
				output += " kD=" + kingDirection;

			return output;
		}



		///////////////////////////////////////////////////////////////////////
		// Properties.  The following properties provide access to their respective members.
		
		public SightGrid SightGrid
		{
			get
			{
				return sightGrid;
			}
			set
			{
				sightGrid = value;
			}
		}
		public bool IsReal
		{
			get
			{
				return isReal;
			}
			set
			{
				isReal = value;
			}
		}
		public int Color
		{
			get
			{
				return color;
			}
		}
		public int Type
		{
			get
			{
				return type;
			}
		}
		public int Id
		{
			get
			{
				return id;
			}
		}
		public Location Location
		{
			get
			{
				return location;
			}
			set
			{
				location = value;
			}
		}
		public bool IsOnBoard
		{
			get
			{
				return isOnBoard;
			}
			set
			{
				isOnBoard = value;
			}
		}
		public  ulong AttackBoard
		{
			get
			{
				return attackBoard;
			}
		}
		public ulong MoveBoard
		{
			get
			{
				return RealMoveBoard();
			}
		}
		public bool HasMoves
		{
			get
			{
				return ( RealMoveBoard() != BitBoard.EMPTY );
			}
		}
		public BoardLocations Moves
		{
			get
			{
				ulong realMoves = RealMoveBoard();
				if ( realMoves == BitBoard.EMPTY )
					return null;
				return BitBoard.GetLocations( realMoves, true );
			}
		}
		public bool WasPawn
		{
			get
			{
				return wasPawn;
			}
			set
			{			
				wasPawn = value;
			}
		}

	}
}

