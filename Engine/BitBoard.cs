using System;
using System.Collections;

namespace Chess
{

	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class.  The BitBoard class consists of static methods to assist
	/// in dealing with bitboards.  A BitBoard is a conceptual 8x8 grid
	/// that stores boolean values in each location.  It uses these 64
	/// boolean values to answer somekind of question relating to the board.
	/// Example: "Where are the white pieces?"
	/// Answer: (in the form of a bitboard)
	///		00000000
	///		00000000
	///		00000000
	///		00000000
	///		00000000
	///		00000000
	///		11111111
	///		11111111
	///	
	///	Since the bitboard needs 64 bits to represent the 64 boolean values,
	///	we use an unsigned-long (ulong).
	///	
	///	< - high order                                     low order - >
	///	1234567890123456789012345678901234567890123456789012345678901234
	///	\------/\------/\------/\------/\------/\------/\------/\------/
	///	   |       |       |       |       |       |       |       |
	///	00000000   |       |       |       |       |       |       |
	///	00000000 --/       |       |       |       |       |       |
	///	00000000 ----------/       |       |       |       |       |
	///	00000000 ------------------/       |       |       |       |
	///	00000000 --------------------------/       |       |       |
	///	00000000 ----------------------------------/       |       |
	///	00000000 ------------------------------------------/       |
	///	00000000 --------------------------------------------------/
	///	
	/// </summary>
	public class BitBoard
	{
		public static ulong[,] bit;
		public const ulong EMPTY = 0UL;
		public const ulong FULL = 18446744073709551615UL;

		static BitBoard()
		{
			ulong temp = 1;
			bit = new ulong[8,8];

			for( int y = 0; y < 8; y++ ) 
			{
				for( int x = 7; x >= 0; x-- )
				{
					bit[x,y] = temp;
					temp = temp << 1;	
				}
			}
		}

		public static ulong FromString( String input )
		{
			ulong value = 0UL;
			int length = 0;

			foreach( char c in input )
			{
				if ( c == '1' )
				{
					value = value << 1;
					value = value | 1;
					length++;
				}
				else if ( c == '0' )
				{
					value = value << 1;
					length++;
				}
				if ( length > 64 )
					throw new System.ApplicationException("Too many bits provided for the 8x8 bit array");
			}

			return value;
		}

		public static String ToString( ulong bb )
		{
			String s = "";

			for( int index = 1; index <= 64; index++ )
			{
				if ( (bb & 1) == 1 )
					s = '1' + s;
				else
					s = '0' + s;
				if ( index % 8 == 0 && index != 64)
					s = Environment.NewLine + s;
				bb = bb >> 1;
			}

			return s;
		}

		public static BoardLocations GetLocations( ulong board, bool value )
		{
			BoardLocations moves = new BoardLocations();
			ulong tempBoard = ( value ) ? board : ~board;
			int x, y;

			moves.Board = tempBoard;
			for( x = 0; x < 8; x++ )
				for( y = 0; y < 8; y++ )
					if ( ( tempBoard & bit[x,y] ) != BitBoard.EMPTY )
						moves.Locations[moves.Count++] = new Location(x, y);

			return moves;
		}

		public static ulong SetBit( ulong board, int x, int y, bool value )
		{
			if ( x >= 0 && x <= 7 && y >= 0 && y <= 7 )
				board = ( ( value ) ? ( board | bit[x,y] ) : ( board & ~bit[x,y] ) );
			return board;
		}
		/// <summary>
		/// Changes the values of all the surrounding bits to the boolean value
		/// </summary>
		/// <param name="x">x coordinate of the bit to surround</param>
		/// <param name="y">y coordinate of the bit to surround</param>
		/// <param name="value">The value to set the surrounding bits to</param>
		public static ulong SetSurroundingBits( ulong board, int x, int y, bool value )
		{
			board = BitBoard.SetBit( board, x - 1, y, value );
			board = BitBoard.SetBit( board, x + 1, y, value );
			board = BitBoard.SetBit( board, x, y - 1, value );
			board = BitBoard.SetBit( board, x, y + 1, value );
			board = BitBoard.SetBit( board, x - 1, y - 1, value );
			board = BitBoard.SetBit( board, x - 1, y + 1, value );
			board = BitBoard.SetBit( board, x + 1, y - 1, value );
			return BitBoard.SetBit( board, x + 1, y + 1, value );
		}
		public static ulong SetKnightBits( ulong board, int x, int y, bool value )
		{
			// Clockwise
			board = BitBoard.SetBit( board, x + 1, y + 2, value );
			board = BitBoard.SetBit( board, x + 2, y + 1, value );
			board = BitBoard.SetBit( board, x + 2, y - 1, value );
			board = BitBoard.SetBit( board, x + 1, y - 2, value );
			board = BitBoard.SetBit( board, x - 1, y - 2, value );
			board = BitBoard.SetBit( board, x - 2, y - 1, value );
			board = BitBoard.SetBit( board, x - 2, y + 1, value );
			return BitBoard.SetBit( board, x - 1, y + 2, value );
		}

		public static ulong SetRow( ulong board, int row, bool value )
		{
			if ( row < 0 || row > 7 )
				throw new System.ApplicationException("Row out of range.  Row must be [0-7]");
			ulong temp = 255UL << row * 8;
			if ( value )
				return board | temp;
			else
				return board & ~temp;
		}

		public static ulong SetColumn( ulong board, int column, bool value )
		{
			if ( column < 0 || column > 7 )
				throw new System.OverflowException("Column out of range.  Column must be [0-7]");
			ulong temp = 9259542123273814144UL >> column;
			if ( value )
				return board | temp;
			else
				return board & ~temp;
		}

		public static ulong SetSlash( ulong board, int x, int y, bool value )
		{
			ulong template = 0;
			if ( x > y )
			{
				switch ( x - y )
				{
					case 1: template = 283691315109952UL; break;
					case 2: template = 1108169199648UL; break;
					case 3: template = 4328785936UL; break;
					case 4: template = 16909320UL; break;
					case 5: template = 66052UL; break;
					case 6: template = 258UL; break;
					case 7: template = 1UL; break;
				}
			} 
			else 
			{
				switch ( y - x )
				{
					case 0: template = 72624976668147840UL; break;
					case 1: template = 145249953336295424UL; break;
					case 2: template = 290499906672525312UL; break;
					case 3: template = 580999813328273408UL; break;
					case 4: template = 1161999622361579520UL; break;
					case 5: template = 2323998145211531264UL; break;
					case 6: template = 4647714815446351872UL; break;
					case 7: template = 9223372036854775808UL; break;
				}
			}

			if ( template == 0 )
			{
				throw new ApplicationException( "x or y inputs were out of range [0-7]" );
			}
			else
			{
				if ( value )
					return board | template;
				else
					return board & ~template;
			}
		}

		public static ulong SetBackSlash( ulong board, int x, int y, bool value )
		{
			ulong template = 0;
			x = 7 - x;

			if ( x > y )
			{
				switch ( x - y )
				{
					case 1: template = 36099303471055874UL; break;
					case 2: template = 141012904183812UL; break;
					case 3: template = 550831656968UL; break;
					case 4: template = 2151686160UL; break;
					case 5: template = 8405024UL; break;
					case 6: template = 32832UL; break;
					case 7: template = 128UL; break;
				}
			} 
			else 
			{
				switch ( y - x )
				{
					case 0: template = 9241421688590303745UL; break;
					case 1: template = 4620710844295151872UL; break;
					case 2: template = 2310355422147575808UL; break;
					case 3: template = 1155177711073755136UL; break;
					case 4: template = 577588855528488960UL; break;
					case 5: template = 288794425616760832UL; break;
					case 6: template = 144396663052566528UL; break;
					case 7: template = 72057594037927936UL; break;
				}
			}

			if ( template == 0 )
			{
				throw new ApplicationException( "x or y inputs were out of range [0-7]" );
			}
			else
			{
				if ( value )
					return board | template;
				else
					return board & ~template;
			}
		}

		public static ulong SetLine( ulong board, int x, int y, int dir, bool value )
		{
			switch ( dir )
			{
				case Directions.Up:
				case Directions.Down:
					return BitBoard.SetColumn( board, x, value );
				case Directions.Right:
				case Directions.Left:
					return BitBoard.SetRow( board, y, value );
				case Directions.UpLeft:
				case Directions.DownRight:
					return BitBoard.SetBackSlash( board, x, y, value );
				case Directions.UpRight:
				case Directions.DownLeft:
					return BitBoard.SetSlash( board, x, y, value );
				default:
					throw new ApplicationException( "SetLine: Invalid Direction" );
			}
		}

		public static ulong SetPartialLine( ulong board, int x, int y, int dir, int untilX, int untilY, bool value )
		{
			int ox = Directions.XOffset[dir];
			int oy = Directions.YOffset[dir];
			ulong temp = 0;

			for( x += ox, y += oy; ; x += ox, y += oy)
			{
				if ( x < 0 || x > 7 || y < 0 || y > 7 )
					break;

				temp |= (ulong) BitBoard.bit[x,y];

				if ( x == untilX && y == untilY )
					break;
			}
			return board | ( ( value ) ? temp : ~temp );
		}


		public static ulong FlipHorizontally( ulong board )
		{
			ulong temp = 0;

			// Flip Horizontally
			temp = temp | ( (board >> 7) & 72340172838076673UL);
			temp = temp | ( (board >> 5) & 144680345676153346UL);
			temp = temp | ( (board >> 3) & 289360691352306692UL);
			temp = temp | ( (board >> 1) & 578721382704613384UL);
			temp = temp | ( (board << 1) & 1157442765409226768UL);
			temp = temp | ( (board << 3) & 2314885530818453536UL);
			temp = temp | ( (board << 5) & 4629771061636907072UL);
			temp = temp | ( (board << 7) & 9259542123273814144UL);

			return temp;
		}
		public static ulong FlipVertically( ulong board )
		{
			ulong temp = 0;

			// Flip Vertically
			temp = temp | ( (board >> 56) & 255UL);
			temp = temp | ( (board >> 40) & 65280UL);
			temp = temp | ( (board >> 24) & 16711680UL);
			temp = temp | ( (board >> 8) & 4278190080UL);
			temp = temp | ( (board << 8) & 1095216660480UL);
			temp = temp | ( (board << 24) & 280375465082880UL);
			temp = temp | ( (board << 40) & 71776119061217280UL);
			temp = temp | ( (board << 56) & 18374686479671623680UL);

			return temp;
		}
		public static ulong Rotate( ulong board )
		{
			return BitBoard.FlipVertically( BitBoard.FlipHorizontally( board ) );
		}

		public static bool IsEmpty( ulong board )
		{
			return board == 0;
		}

		public static bool IsNotEmpty( ulong board )
		{
			return board != 0;
		}

	}
}
