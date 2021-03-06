// CiDocLexer.cs - Ci documentation lexer
//
// Copyright (C) 2011-2019  Piotr Fusik
//
// This file is part of CiTo, see https://github.com/pfusik/cito
//
// CiTo is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
//
// CiTo is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// You should have received a copy of the GNU General Public License
// along with CiTo.  If not, see http://www.gnu.org/licenses/

namespace Foxoft.Ci
{

public enum CiDocToken
{
	EndOfFile,
	Char,
	CodeDelimiter,
	Bullet,
	Para,
	Period
}

public class CiDocLexer
{
	readonly CiLexer CiLexer;
	bool CheckPeriod = true;
	public CiDocToken CurrentToken;
	public int CurrentChar = '\n';

	public CiDocLexer(CiLexer ciLexer)
	{
		this.CiLexer = ciLexer;
		NextToken();
	}

	int PeekChar()
	{
		return this.CiLexer.PeekChar();
	}

	int ReadChar()
	{
		int c = this.CiLexer.ReadChar();
		if (c == '\n') {
			this.CiLexer.NextToken();
			if (!this.CiLexer.See(CiToken.DocComment))
				return -1;
		}
		return c;
	}

	CiDocToken ReadToken()
	{
		int lastChar = this.CurrentChar;
		for (;;) {
			int c = ReadChar();
			this.CurrentChar = c;
			switch (c) {
			case -1:
				return CiDocToken.EndOfFile;
			case '`':
				return CiDocToken.CodeDelimiter;
			case '*':
				if (lastChar == '\n' && PeekChar() == ' ') {
					ReadChar();
					return CiDocToken.Bullet;
				}
				return CiDocToken.Char;
			case '\r':
				continue;
			case '\n':
				if (this.CheckPeriod && lastChar == '.') {
					this.CheckPeriod = false;
					return CiDocToken.Period;
				}
				if (lastChar == '\n')
					return CiDocToken.Para;
				return CiDocToken.Char;
			default:
				return CiDocToken.Char;
			}
		}
	}

	protected void NextToken()
	{
		this.CurrentToken = ReadToken();
	}

	protected bool See(CiDocToken token)
	{
		return this.CurrentToken == token;
	}

	protected bool Eat(CiDocToken token)
	{
		if (See(token)) {
			NextToken();
			return true;
		}
		return false;
	}

	protected void Expect(CiDocToken expected)
	{
		if (!See(expected))
			throw this.CiLexer.ParseException("Expected {0}, got {1}", expected, this.CurrentToken);
		NextToken();
	}
}

}
