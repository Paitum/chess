using System;

namespace Chess
{
	///////////////////////////////////////////////////////////////////////
	/// <summary>
	/// Customized Exceptions.
	/// </summary>

	public class DataIntegrityException : ApplicationException
	{
		public DataIntegrityException( String s ) : base( s ) {}
	}
	public class AddPieceException : ApplicationException
	{
		public AddPieceException( String s ) : base( s ) {}
	}
	public class OutOfBoundsException : ApplicationException
	{
		public OutOfBoundsException( String s ) : base( s ) {}
	}
	public class EmptyLocationException : ApplicationException
	{
		public EmptyLocationException( String s ) : base( s ) {}
	}
	public class OutOfTurnException : ApplicationException
	{
		public OutOfTurnException( String s ) : base( s ) {}
	}
	public class IllegalMoveException : ApplicationException
	{
		public IllegalMoveException( String s ) : base( s ) {}
	}
	public class InvalidPromotionTypeException : ApplicationException
	{
		public InvalidPromotionTypeException( String s ) : base( s ) {}
	}
	public class LimitReachedException : ApplicationException
	{
		public LimitReachedException( String s ) : base( s ) {}
	}
	public class ImportException : ApplicationException
	{
		public ImportException( String s ) : base( s ) {}
	}
	
}
