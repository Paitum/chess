using System;
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Reflection;

namespace Chess.UI
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Class.  This class generates all of the piece-bitmaps from the
	/// Chess Font.
	/// </summary>
	public class Tile
	{
		public static int HEIGHT = 50;
		public static int WIDTH = 50;

		public static Bitmap[][] PIECES;	// [color][type]
		public static Bitmap[][] SHADOW;	// [type]

		static Tile()
		{
			Graphics g;
			PIECES = new Bitmap[2][];
			SHADOW = new Bitmap[2][];

			SolidBrush BlackBrush = new SolidBrush( Color.Black );
			SolidBrush WhiteBrush = new SolidBrush( Color.White );
			SolidBrush ShadowBrush = new SolidBrush( Color.Gray );
			Font ChessFont = new Font("Chess Cases", 35);

			// If the font OS doesn't have the font, check locally
			if ( ChessFont.Name != "Chess Cases" )
			{
				try
				{
					PrivateFontCollection pfc = new PrivateFontCollection();
					pfc.AddFontFile( "CASEFONT.TTF" );
					FontFamily ff = new FontFamily( "Chess Cases", pfc );
					System.Console.WriteLine( ff.Name  );
					ChessFont = new Font( ff, 35 );
				}
				catch ( FileNotFoundException )
				{
				}
			}

			if ( ChessFont.Name == "Chess Cases" )
			{
				string pieceLetter = "otmvwl";
				for( int color = 0 ; color < 2 ; color++ )
				{
					PIECES[color] = new Bitmap[6];
					SHADOW[color] = new Bitmap[6];
					for ( int type = 0; type < 6; type++ )
					{
						PIECES[color][type] = new Bitmap(WIDTH, HEIGHT);
						g = Graphics.FromImage( PIECES[color][type] );
						g.DrawString( pieceLetter[ type ].ToString()
							, ChessFont
							, ( color == Player.WHITE ) ? WhiteBrush : BlackBrush
							, -7
							, 0 );

						SHADOW[color][type] = new Bitmap(WIDTH, HEIGHT);
						g = Graphics.FromImage( SHADOW[color][type] );
						g.DrawString( pieceLetter[ type ].ToString()
							, ChessFont
							, ShadowBrush
							, -7
							, 0 );
					}
				}
			} 
			else
			{
				Font ArialFont = new Font("Arial", 18 );
				for( int color = 0 ; color < 2 ; color++ )
				{
					PIECES[color] = new Bitmap[6];
					SHADOW[color] = new Bitmap[6];
					for ( int type = 0; type < 6; type++ )
					{
						PIECES[color][type] = new Bitmap(WIDTH, HEIGHT);
						g = Graphics.FromImage( PIECES[color][type] );
						g.DrawString( Player.PlayerToSmallString[ color ] + Piece.TypeToSmallString[ type ]
							, ArialFont
							, ( color == Player.WHITE ) ? WhiteBrush : BlackBrush
							, 2
							, 5 );
						SHADOW[color][type] = new Bitmap(WIDTH, HEIGHT);
						g = Graphics.FromImage( SHADOW[color][type] );
						g.DrawString( Player.PlayerToSmallString[ color ] + Piece.TypeToSmallString[ type ]
							, ArialFont
							, ShadowBrush
							, 2
							, 5 );
					}
				}
			}
		}

		public static Location FromPixel( Location l )
		{
			return new Location( XFromPixel( l.x ), YFromPixel( l.y ) );
		}
		public static Location FromBoard( Location l )
		{
			return new Location( XFromBoard( l.x ), YFromBoard( l.y ));
		}
		public static int XFromPixel( int inX )
		{
			return inX / WIDTH;
		}
		public static int XFromBoard( int inX )
		{
			return inX * WIDTH;
		}
		public static int YFromPixel( int inY )
		{
			return 7 - (inY / HEIGHT);
		}
		public static int YFromBoard( int inY )
		{
			return (7 - inY) * HEIGHT;
		}


		public Tile()
		{
		}
	}
}
