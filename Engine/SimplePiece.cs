using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Struct.  Represents a piece, in a simple manner.
	/// </summary>
	public struct SimplePiece
	{
		public int Color;
		public int Type;
		public int Index;
		public bool WasPawn;
		public bool IsNull;

		public static SimplePiece NULL
		{
			get
			{
				SimplePiece x = new SimplePiece();
				x.Color = Player.UNKNOWN;
				x.Type = Piece.UNKNOWN;
				x.Index = -1;
				x.WasPawn = false;
				x.IsNull = true;
				return x;
			}
		}

		public SimplePiece( int inColor, int inType, int inIndex, bool inWasPawn )
		{
			Color = inColor;
			Type = inType;
			Index = inIndex;
			WasPawn = inWasPawn;
			IsNull = false;
		}

		public SimplePiece( int inColor, int inType )
		{
			Color = inColor;
			Type = inType;
			Index = 0;
			WasPawn = false;
			IsNull = false;
		}

		public bool Equals( SimplePiece sp )
		{
			if ( IsNull == true && sp.IsNull == true )
				return true;

			return ( Color == sp.Color
				&& Type == sp.Type
				&& Index == sp.Index );
		}

		public string ToNormalString()
		{
			if ( IsNull )
				return "Null Piece";
			else
				return Player.PlayerToString[ Color ] + " " + Piece.TypeToString[ Type ];
		}

		public override string ToString()
		{
			if ( IsNull )
				return "Null Piece";
			else
				return Player.PlayerToString[ Color ] + " " + Piece.TypeToString[ Type ] + ( (WasPawn) ? " WasPawn " : " " ) + "#" + Index;
		}
	}

}
