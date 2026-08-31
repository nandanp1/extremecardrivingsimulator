namespace UnityEditor.iOS.Xcode.PBX
{
	internal enum TokenType
	{
		EOF = 0,
		Invalid = 1,
		String = 2,
		QuotedString = 3,
		Comment = 4,
		Semicolon = 5,
		Comma = 6,
		Eq = 7,
		LParen = 8,
		RParen = 9,
		LBrace = 10,
		RBrace = 11
	}
}
