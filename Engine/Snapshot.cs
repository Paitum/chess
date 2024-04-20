using System;
using System.Collections;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Stores the least amount of data needed to continue a chess game in a lightweight
	/// structure.  It is intended to help in cloning a board and also for sending the
	/// board state in a compact manner over a network.
	/// 
	/// It stores the location of all pieces, the en passant board, the castle board,
	/// whos turn it is, and the numberOfMoves since last pawn or capture in 5 unsigned
	/// longs.  Resulting in (5ulong * 8byte/ulong) 40byte data structure.  The breakdown
	/// of the bits is located at the bottom of this file.
	/// </summary>
	public struct Snapshot
	{
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The variables that store the chess-state.  Each data element in the array
		/// is further subdivided using bitwise operations.  See the chart at the top
		/// for the breakdown of the bits.
		/// </summary>
		private ulong[] data;

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Static Property Psuedo-Constructor.  Returns a null snapshot object.
		/// </summary>
		public static Snapshot BLANK
		{
			get
			{
				Snapshot s = new Snapshot();
				s.data = new ulong[5];
				return s;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Constructor.  Creates a Snapshot from the supplied chess board.
		/// </summary>
		/// <param name="board">a chess board to "snapshot"</param>
		public Snapshot( Board board )
		{
			data = new ulong[5];
			Piece piece;

			// Setup the snapshot.
			EnPassantBoard = board.EnPassantBoard;
			CastleBits = board.CastleBits;
			WhosTurn = board.Turn;
			NumberOfTurnsSincePawnOrCapture = board.NumberOfTurnsSincePawnOrCapture;

			// Go through all the pieces and add them.
			for( int color = 0 ; color < 2 ; color++ )
				for ( int type = 0; type < 6; type++ )
					for( int id = 0; id < Piece.NUMBER_OF_PER_PLAYER[ type ]; id++ )
					{
						piece = board.Pieces[color][type][id];
						this[color,type,id] = new SnapshotElement( piece.IsOnBoard, piece.Location, piece.Type );
					}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Property.  Gets and Sets the EnPassantBoard variable.
		/// </summary>
		public ulong EnPassantBoard
		{
			get
			{
				int bits = (int) ( ( data[EN_PASSANT_DATA_INDEX] >> EN_PASSANT_BIT_OFFSET ) & SEVEN_BIT_BITBOARD );

				if ( ( bits & 1 ) == 0 )
					return BitBoard.EMPTY;

				Location l = LOCATIONS[ bits >> LOCATION_BIT_OFFSET ];
				return BitBoard.SetBit( BitBoard.EMPTY, l.x, l.y, true );
			}
			set
			{
				ulong bits = 0UL;
				if ( value != 0UL )
					bits = ( (ulong) Math.Round( Math.Log( value, 2.0 ), 14 ) << LOCATION_BIT_OFFSET ) | 1UL;
				data[EN_PASSANT_DATA_INDEX] = ( data[EN_PASSANT_DATA_INDEX] & ~( SEVEN_BIT_BITBOARD << EN_PASSANT_BIT_OFFSET ) ) | bits << EN_PASSANT_BIT_OFFSET;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Property.  Gets and Sets the CastleBits variable.
		/// </summary>
		public int CastleBits
		{
			get
			{
				return (int) (( data[CASTLE_DATA_INDEX] >> CASTLE_BIT_OFFSET ) & FOUR_BIT_BITBOARD);
			}
			set
			{
				data[CASTLE_DATA_INDEX] = ( data[CASTLE_DATA_INDEX] & ~( FOUR_BIT_BITBOARD << CASTLE_BIT_OFFSET ) ) | (ulong) ( value & 15 ) << CASTLE_BIT_OFFSET;
			}
		}
		
		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Property.  Gets and Sets the WhosTurn variable.
		/// </summary>
		public int WhosTurn
		{
			get
			{
				return (int) ( ( data[WHOSTURN_DATA_INDEX] >> WHOSTURN_BIT_OFFSET ) & 1 );
			}
			set
			{
				data[WHOSTURN_DATA_INDEX] = ( data[WHOSTURN_DATA_INDEX] & ~( 1UL << WHOSTURN_BIT_OFFSET ) | ( (ulong) value ) << WHOSTURN_BIT_OFFSET );
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Property.  Gets and Sets the NumberOfTurnsSincePawnOrCapture variable.
		/// </summary>
		public int NumberOfTurnsSincePawnOrCapture
		{
			get
			{
				return (int) ( ( data[NOTSPOC_DATA_INDEX] >> NOTSPOC_BIT_OFFSET ) & SIX_BIT_BITBOARD );
			}
			set
			{
				data[NOTSPOC_DATA_INDEX] = ( data[NOTSPOC_DATA_INDEX] & ~( SIX_BIT_BITBOARD << NOTSPOC_BIT_OFFSET ) ) | ( (ulong) value ) << NOTSPOC_BIT_OFFSET;
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Indexer.  Gets and Sets pieces from the snapshot.  [ color, type, id ]
		/// </summary>
		public SnapshotElement this[int color, int type, int id]
		{
			get
			{
				ulong MASK;
				if ( type == Piece.PAWN )
					MASK = TEN_BIT_BITBOARD;
				else
					MASK = SEVEN_BIT_BITBOARD;

				ulong bits = ( data[ DATA_INDEX[color][type][id] ] >> BIT_OFFSET[color][type][id] ) & MASK;

				SnapshotElement se = new SnapshotElement();
				se.isOnBoard = ( ( bits & 1UL ) == 0 ) ? false : true;
				se.location = LOCATIONS[ (int) ( ( bits >> LOCATION_BIT_OFFSET ) & SIX_BIT_BITBOARD ) ];
				if ( type == Piece.PAWN )
					se.type = (int) ( ( bits >> TYPE_BIT_OFFSET ) & THREE_BIT_BITBOARD );
				else
					se.type = type;

				return se;
			}
			set
			{
				ulong MASK;

				// Calculate Bits to Insert
				ulong bits = 0UL;
				if ( value.isOnBoard )
					bits |= 1UL;

				if ( value.location.x < 0 || value.location.x > 7 || value.location.y < 0 || value.location.y > 7 )
					throw new DataIntegrityException( "Snapshot: this[][][]: SnapshotElement has an invalid location " + value.location );

				bits |= ( ( (ulong) ( value.location.y * 8 + ( 7 - value.location.x ) ) ) & SIX_BIT_BITBOARD ) << LOCATION_BIT_OFFSET;

				if ( type == Piece.PAWN )
				{
					MASK = TEN_BIT_BITBOARD;
					bits |= ( ( (ulong) value.type ) & THREE_BIT_BITBOARD ) << TYPE_BIT_OFFSET;
				}
				else
					MASK = SEVEN_BIT_BITBOARD;


				// Insert the Bits
				int i = DATA_INDEX[color][type][id];
				int OFFSET = BIT_OFFSET[color][type][id];
				data[i] = ( data[i] & ~( MASK << OFFSET ) ) | ( bits << OFFSET );
			}
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Override.  Provides a multiline textual representation of this snapshot.
		/// </summary>
		/// <returns></returns>
		public override String ToString()
		{
			String output = "";
			ulong temp;

			output += @"T/enPas\/ WB1 \/ WB0 \/ WR1 \/ WR0 \/ WK1 \/ WK0 \/ WQ  \/WKing\" + Environment.NewLine;
			output += BinaryToString( data[0] ) + Environment.NewLine;
			output += @"  NumOfT/ BB1 \/ BB0 \/ BR1 \/ BR0 \/ BK1 \/ BK0 \/ BQ  \/BKing\" + Environment.NewLine;
			output += BinaryToString( data[1] ) + Environment.NewLine;
			output += @"CAST/  WP5   \/  WP4   \/  WP3   \/  WP2   \/  WP1   \/  WP0   \" + Environment.NewLine;
			output += BinaryToString( data[2] ) + Environment.NewLine;
			output += @"    /  BP5   \/  BP4   \/  BP3   \/  BP2   \/  BP1   \/  BP0   \" + Environment.NewLine;
			output += BinaryToString( data[3] ) + Environment.NewLine;
			output += @"                        /  BP7   \/  BP6   \/  WP7   \/  WP6   \" + Environment.NewLine;
			output += BinaryToString( data[4] ) + Environment.NewLine;
			output += "Turn: " + Player.PlayerToString[ WhosTurn ] + Environment.NewLine;
			output += "NOTSPOC: " + NumberOfTurnsSincePawnOrCapture + Environment.NewLine;
			output += "CastleBits: " + CastleBits + Environment.NewLine;
			temp = EnPassantBoard;
			output += "EnPessant Board: " + temp;
			if ( temp != 0UL )
				output += Environment.NewLine + BitBoard.ToString( temp );

			return output;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Static.  Converts a ulong to a string representation of 1s and 0s.
		/// This is used in the ToString() method of Snapshot
		/// </summary>
		/// <param name="bb">the ulong (bitboard) to "decode"</param>
		/// <returns>binary representation in a string format</returns>
		private static String BinaryToString( ulong bb )
		{
			String s = "";

			for( int index = 1; index <= 64; index++ )
			{
				if ( (bb & 1) == 1 )
					s = '1' + s;
				else
					s = '0' + s;
				bb = bb >> 1;
			}

			return s;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// Static.  Converts an int into a string representation of 1s and 0s.
		/// This is used in the ToString() method of Snapshot
		/// </summary>
		/// <param name="input">the int to "decode"</param>
		/// <returns>binary representation in a string format</returns>
		private static String BinaryToString( int input )
		{
			String s = "";

			for( int index = 1; index <= 32; index++ )
			{
				if ( (input & 1) == 1 )
					s = '1' + s;
				else
					s = '0' + s;
				input = input >> 1;
			}

			return s;
		}

		///////////////////////////////////////////////////////////////////////
		/// <summary>
		/// The snapshot struct uses many static variables to help in readability and
		/// flexibility.  The following region initializes all the static data.
		/// </summary>
		#region Static Fields (Helper Fields)
		private static int[][][] DATA_INDEX;
		private static int[][][] BIT_OFFSET;
		private static Location[] LOCATIONS;
		private static ulong TEN_BIT_BITBOARD = 1023UL;
		private static ulong SEVEN_BIT_BITBOARD = 127UL;
		private static ulong SIX_BIT_BITBOARD = 63UL;
		private static ulong FOUR_BIT_BITBOARD = 15UL;
		private static ulong THREE_BIT_BITBOARD = 7UL;
		private static int LOCATION_BIT_OFFSET = 1;
		private static int TYPE_BIT_OFFSET = 7;

		private static int EN_PASSANT_DATA_INDEX = 0;
		private static int EN_PASSANT_BIT_OFFSET = 56;
		private static int WHOSTURN_DATA_INDEX = 0;
		private static int WHOSTURN_BIT_OFFSET = 63;
		private static int NOTSPOC_DATA_INDEX = 1;
		private static int NOTSPOC_BIT_OFFSET = 56;
		private static int CASTLE_DATA_INDEX = 2;
		private static int CASTLE_BIT_OFFSET = 60;


		// Static Field Constructor
		static Snapshot()
		{
			// DATA_INDEX is an array that is accessed by [color][piece][id] and
			//	returns the data-index to use in the snapshot.
			#region Initialize DATA_INDEX
			DATA_INDEX = new int[2][][];

			DATA_INDEX[Player.WHITE] = new int[6][];
			DATA_INDEX[Player.BLACK] = new int[6][];

			DATA_INDEX[Player.WHITE][Piece.PAWN] = new int[8];
			DATA_INDEX[Player.WHITE][Piece.ROOK] = new int[2];
			DATA_INDEX[Player.WHITE][Piece.KNIGHT] = new int[2];
			DATA_INDEX[Player.WHITE][Piece.BISHOP] = new int[2];
			DATA_INDEX[Player.WHITE][Piece.QUEEN] = new int[1];
			DATA_INDEX[Player.WHITE][Piece.KING] = new int[1];

			DATA_INDEX[Player.BLACK][Piece.PAWN] = new int[8];
			DATA_INDEX[Player.BLACK][Piece.ROOK] = new int[2];
			DATA_INDEX[Player.BLACK][Piece.KNIGHT] = new int[2];
			DATA_INDEX[Player.BLACK][Piece.BISHOP] = new int[2];
			DATA_INDEX[Player.BLACK][Piece.QUEEN] = new int[1];
			DATA_INDEX[Player.BLACK][Piece.KING] = new int[1];

			DATA_INDEX[Player.WHITE][Piece.KING][0] = 0;
			DATA_INDEX[Player.WHITE][Piece.QUEEN][0] = 0;
			DATA_INDEX[Player.WHITE][Piece.BISHOP][0] = 0;
			DATA_INDEX[Player.WHITE][Piece.BISHOP][1] = 0;
			DATA_INDEX[Player.WHITE][Piece.KNIGHT][0] = 0;
			DATA_INDEX[Player.WHITE][Piece.KNIGHT][1] = 0;
			DATA_INDEX[Player.WHITE][Piece.ROOK][0] = 0;
			DATA_INDEX[Player.WHITE][Piece.ROOK][1] = 0;

			DATA_INDEX[Player.BLACK][Piece.KING][0] = 1;
			DATA_INDEX[Player.BLACK][Piece.QUEEN][0] = 1;
			DATA_INDEX[Player.BLACK][Piece.BISHOP][0] = 1;
			DATA_INDEX[Player.BLACK][Piece.BISHOP][1] = 1;
			DATA_INDEX[Player.BLACK][Piece.KNIGHT][0] = 1;
			DATA_INDEX[Player.BLACK][Piece.KNIGHT][1] = 1;
			DATA_INDEX[Player.BLACK][Piece.ROOK][0] = 1;
			DATA_INDEX[Player.BLACK][Piece.ROOK][1] = 1;

			DATA_INDEX[Player.WHITE][Piece.PAWN][0] = 2;
			DATA_INDEX[Player.WHITE][Piece.PAWN][1] = 2;
			DATA_INDEX[Player.WHITE][Piece.PAWN][2] = 2;
			DATA_INDEX[Player.WHITE][Piece.PAWN][3] = 2;
			DATA_INDEX[Player.WHITE][Piece.PAWN][4] = 2;
			DATA_INDEX[Player.WHITE][Piece.PAWN][5] = 2;
			DATA_INDEX[Player.WHITE][Piece.PAWN][6] = 4;
			DATA_INDEX[Player.WHITE][Piece.PAWN][7] = 4;

			DATA_INDEX[Player.BLACK][Piece.PAWN][0] = 3;
			DATA_INDEX[Player.BLACK][Piece.PAWN][1] = 3;
			DATA_INDEX[Player.BLACK][Piece.PAWN][2] = 3;
			DATA_INDEX[Player.BLACK][Piece.PAWN][3] = 3;
			DATA_INDEX[Player.BLACK][Piece.PAWN][4] = 3;
			DATA_INDEX[Player.BLACK][Piece.PAWN][5] = 3;
			DATA_INDEX[Player.BLACK][Piece.PAWN][6] = 4;
			DATA_INDEX[Player.BLACK][Piece.PAWN][7] = 4;
			#endregion

			// BIT_OFFSET is an array that is accessed by [color][piece][id] and
			//  returns the bit-offset in the data (ulong) where to find the data
			//  on the piece.
			#region Initialize BIT_OFFSET
			BIT_OFFSET = new int[2][][];

			BIT_OFFSET[Player.WHITE] = new int[6][];
			BIT_OFFSET[Player.BLACK] = new int[6][];
			
			BIT_OFFSET[Player.WHITE][Piece.PAWN] = new int[8];
			BIT_OFFSET[Player.WHITE][Piece.ROOK] = new int[2];
			BIT_OFFSET[Player.WHITE][Piece.KNIGHT] = new int[2];
			BIT_OFFSET[Player.WHITE][Piece.BISHOP] = new int[2];
			BIT_OFFSET[Player.WHITE][Piece.QUEEN] = new int[1];
			BIT_OFFSET[Player.WHITE][Piece.KING] = new int[1];

			BIT_OFFSET[Player.BLACK][Piece.PAWN] = new int[8];
			BIT_OFFSET[Player.BLACK][Piece.ROOK] = new int[2];
			BIT_OFFSET[Player.BLACK][Piece.KNIGHT] = new int[2];
			BIT_OFFSET[Player.BLACK][Piece.BISHOP] = new int[2];
			BIT_OFFSET[Player.BLACK][Piece.QUEEN] = new int[1];
			BIT_OFFSET[Player.BLACK][Piece.KING] = new int[1];

			int offset;

			BIT_OFFSET[Player.WHITE][Piece.PAWN][0] = offset = 0;
			BIT_OFFSET[Player.WHITE][Piece.PAWN][1] = offset+=10;
			BIT_OFFSET[Player.WHITE][Piece.PAWN][2] = offset+=10;
			BIT_OFFSET[Player.WHITE][Piece.PAWN][3] = offset+=10;
			BIT_OFFSET[Player.WHITE][Piece.PAWN][4] = offset+=10;
			BIT_OFFSET[Player.WHITE][Piece.PAWN][5] = offset+=10;

			BIT_OFFSET[Player.BLACK][Piece.PAWN][0] = offset = 0;
			BIT_OFFSET[Player.BLACK][Piece.PAWN][1] = offset+=10;
			BIT_OFFSET[Player.BLACK][Piece.PAWN][2] = offset+=10;
			BIT_OFFSET[Player.BLACK][Piece.PAWN][3] = offset+=10;
			BIT_OFFSET[Player.BLACK][Piece.PAWN][4] = offset+=10;
			BIT_OFFSET[Player.BLACK][Piece.PAWN][5] = offset+=10;

			BIT_OFFSET[Player.WHITE][Piece.PAWN][6] = offset = 0;
			BIT_OFFSET[Player.WHITE][Piece.PAWN][7] = offset+=10;
			BIT_OFFSET[Player.BLACK][Piece.PAWN][6] = offset+=10;
			BIT_OFFSET[Player.BLACK][Piece.PAWN][7] = offset+=10;

			BIT_OFFSET[Player.WHITE][Piece.KING][0] = offset = 0;
			BIT_OFFSET[Player.WHITE][Piece.QUEEN][0] = offset+=7;
			BIT_OFFSET[Player.WHITE][Piece.KNIGHT][0] = offset+=7;
			BIT_OFFSET[Player.WHITE][Piece.KNIGHT][1] = offset+=7;
			BIT_OFFSET[Player.WHITE][Piece.ROOK][0] = offset+=7;
			BIT_OFFSET[Player.WHITE][Piece.ROOK][1] = offset+=7;
			BIT_OFFSET[Player.WHITE][Piece.BISHOP][0] = offset+=7;
			BIT_OFFSET[Player.WHITE][Piece.BISHOP][1] = offset+=7;

			BIT_OFFSET[Player.BLACK][Piece.KING][0] = offset = 0;
			BIT_OFFSET[Player.BLACK][Piece.QUEEN][0] = offset+=7;
			BIT_OFFSET[Player.BLACK][Piece.KNIGHT][0] = offset+=7;
			BIT_OFFSET[Player.BLACK][Piece.KNIGHT][1] = offset+=7;
			BIT_OFFSET[Player.BLACK][Piece.ROOK][0] = offset+=7;
			BIT_OFFSET[Player.BLACK][Piece.ROOK][1] = offset+=7;
			BIT_OFFSET[Player.BLACK][Piece.BISHOP][0] = offset+=7;
			BIT_OFFSET[Player.BLACK][Piece.BISHOP][1] = offset+=7;


			#endregion

			// LOCATIONS is accessed by [bit-location] and provides the Location
			//  of that bit in a bitboard.
			#region Initialize LOCATIONS
			LOCATIONS = new Location[65];
			for( int y = 0; y < 8; y++ )
			{
				for( int x = 7; x >= 0; x-- )
				{
					LOCATIONS[ y * 8 + ( 7 - x ) ] = new Location(x,y);
				}
			}
			#endregion

		}
		#endregion

	}
}

/* (Bitwise) Layout of the snapshot structure:

[0] @/enPas\/ WB1 \/ WB0 \/ WR1 \/ WR0 \/ WK1 \/ WK0 \/ WQ  \/WKing\
	3210987654321098765432109876543210987654321098765432109876543210
[1]   NumOfT/ BB1 \/ BB0 \/ BR1 \/ BR0 \/ BK1 \/ BK0 \/ BQ  \/BKing\
	3210987654321098765432109876543210987654321098765432109876543210
[2] CAST/  WP5   \/  WP4   \/  WP3   \/  WP2   \/  WP1   \/  WP0   \
	3210987654321098765432109876543210987654321098765432109876543210
[3]     /  BP5   \/  BP4   \/  BP3   \/  BP2   \/  BP1   \/  BP0   \
	3210987654321098765432109876543210987654321098765432109876543210
[4]                         /  BP7   \/  BP6   \/  WP7   \/  WP6   \
	3210987654321098765432109876543210987654321098765432109876543210

@ = Turn

OTHER PIECES: 7 bits
/Loca\ ?
654321 0

PAWNS: 10 bits
/T\ /Loca\ ?
987 654321 0
			
*/
