#region Copyright
//
// Nini Configuration Project.
// Copyright (C) 2004 Brent R. Matzelle.  All rights reserved.
//
// This software is published under the terms of the MIT X11 license, a copy of 
// which has been included with this distribution in the LICENSE.txt file.
// 
#endregion

using System;
using System.IO;
using Nini.Ini;
using NUnit.Framework;

namespace Nini.Test.Ini
{
	[TestFixture]
	public class IniReaderTests
	{
		[Test]
		public void NormalComment ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("");
			writer.WriteLine (" ; Something");
			writer.WriteLine (" #   Some comment  ");
			writer.WriteLine (" ;");
			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			
			Assert.AreEqual (IniReadState.Initial, reader.ReadState);
			Assert.IsTrue (reader.Read ());
			Assert.AreEqual (IniReadState.Interactive, reader.ReadState);
			Assert.AreEqual (IniType.Empty, reader.Type);
			Assert.AreEqual ("", reader.Name);
			Assert.AreEqual (null, reader.Comment);
			
			Assert.IsTrue (reader.Read ());
			Assert.AreEqual (IniType.Empty, reader.Type);
			Assert.AreEqual ("Something", reader.Comment);
			
			Assert.IsTrue (reader.Read ());
			Assert.AreEqual (IniType.Empty, reader.Type);
			Assert.AreEqual ("Some comment", reader.Comment);
			
			Assert.IsTrue (reader.Read ());
			Assert.AreEqual ("", reader.Comment);
			
			Assert.IsFalse (reader.Read ());
		}
		
		[Test]
		public void NormalSectionAndKey ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Logging] ; Logging section");
			writer.WriteLine (" great logger =   log4net  ");
			writer.WriteLine ("  [Pets] ; pets comment  ");
			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			
			Assert.AreEqual (IniReadState.Initial, reader.ReadState);
			Assert.IsTrue (reader.Read ());
			Assert.AreEqual (IniReadState.Interactive, reader.ReadState);
			Assert.AreEqual (IniType.Section, reader.Type);
			Assert.AreEqual ("Logging", reader.Name);
			Assert.AreEqual ("", reader.Value);
			Assert.AreEqual ("Logging section", reader.Comment);
			
			Assert.IsTrue (reader.Read ());
			Assert.AreEqual (IniType.Key, reader.Type);
			Assert.AreEqual ("great logger", reader.Name);
			Assert.AreEqual ("log4net", reader.Value);
			Assert.AreEqual (null, reader.Comment);
			
			Assert.IsTrue (reader.Read ());
			Assert.AreEqual (IniType.Section, reader.Type);
			Assert.AreEqual ("Pets", reader.Name);
			Assert.AreEqual ("", reader.Value);
			Assert.AreEqual ("pets comment", reader.Comment);
		}
		
		[Test]
		public void KeyWithQuotes ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Nini]");
			writer.WriteLine ("  whitespace = \"  remove thing\"  ");
			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			
			Assert.IsTrue (reader.Read ());
			Assert.IsTrue (reader.Read ());
			Assert.AreEqual (IniType.Key, reader.Type);
			Assert.AreEqual ("whitespace", reader.Name);
			Assert.AreEqual ("  remove thing", reader.Value);
			Assert.AreEqual (null, reader.Comment);
			
			Assert.IsFalse (reader.Read ());
		}
		
		[Test]
		[ExpectedException (typeof (IniException))]
		public void SectionWithNoEndBracket ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Nini");
			writer.WriteLine ("");
			IniReader reader = new IniReader (new StringReader (writer.ToString ()));

			Assert.IsTrue (reader.Read ());
		}
		
		[Test]
		//[ExpectedException (typeof (IniException))]
		public void LinePositionAndNumber ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("; Test");
			writer.WriteLine ("# Test 1");
			writer.WriteLine ("[Nini Thing");
			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			
			Assert.IsTrue (reader.Read ());
			Assert.IsTrue (reader.Read ());
			
			try
			{
				reader.Read ();
			}
			catch(IniException e)
			{
				Assert.AreEqual (3, e.LineNumber);
				Assert.AreEqual (13, e.LinePosition);
			}
		}
		
		[Test]
		[ExpectedException (typeof (IniException))]
		public void KeysWithSameName ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Nini]");
			writer.WriteLine (" superkey = legal ");
			writer.WriteLine ("[Pets]");
			writer.WriteLine (" superkey = legal ");
			writer.WriteLine (" superkey = not legal ");
			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			
			Assert.IsTrue (reader.Read ());
			Assert.IsTrue (reader.Read ());
			Assert.IsTrue (reader.Read ());
			Assert.IsTrue (reader.Read ());
			
			reader.Read ();
		}
		
		[Test]
		[ExpectedException (typeof (IniException))]
		public void SectionsWithSameName ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Nini]");
			writer.WriteLine (" some key = \" something ");
			writer.WriteLine ("[Nini]");
			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			
			Assert.IsTrue (reader.Read ());
			Assert.IsTrue (reader.Read ());
			try
			{
				reader.Read ();
			}
			catch(IniException e)
			{
				Assert.AreEqual (3, e.LineNumber);
				Assert.AreEqual (6, e.LinePosition);
			}
		}

		[Test]
		public void IgnoreComments ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Nini] ; my comment 1");
			writer.WriteLine (" some key = something ; my comment 1");
			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			
			Assert.IsTrue (reader.Read ());
			Assert.AreEqual ("my comment 1", reader.Comment);
			reader.IgnoreComments = true;
			Assert.IsTrue (reader.Read ());
			Assert.AreEqual (null, reader.Comment);
		}
		
		[Test]
		[ExpectedException (typeof (IniException))]
		public void NoEndingQuote ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Nini]");
			writer.WriteLine (" some key = \" something ");
			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			
			Assert.IsTrue (reader.Read ());
			Assert.IsTrue (reader.Read ());
		}
		
		[Test]
		[ExpectedException (typeof (IniException))]
		public void KeyWithNoEquals ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Nini]");
			writer.WriteLine (" some key ");
			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			
			Assert.IsTrue (reader.Read ());
			Assert.IsTrue (reader.Read ());
		}
		
		[Test]
		public void MoveToNextSection ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("; Test");
			writer.WriteLine ("; Test 1");
			writer.WriteLine ("[Nini Thing]");
			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			
			Assert.IsTrue (reader.MoveToNextSection ());
			Assert.AreEqual (4, reader.LineNumber);
			Assert.AreEqual (IniType.Section, reader.Type);
			Assert.IsFalse (reader.MoveToNextSection ());
		}
		
		[Test]
		public void MoveToNextKey ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("; Test");
			writer.WriteLine ("; Test 1");
			writer.WriteLine ("[Nini Thing]");
			writer.WriteLine ("; Test");
			writer.WriteLine (" my key = new key");
			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			
			Assert.IsFalse (reader.MoveToNextKey ());
			Assert.AreEqual (4, reader.LineNumber);
			Assert.IsTrue (reader.MoveToNextKey ());
			Assert.AreEqual (6, reader.LineNumber);
			Assert.AreEqual (IniType.Key, reader.Type);
			Assert.AreEqual ("my key", reader.Name);
		}

		[Test]
		public void NoSectionsOrKeys ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("");

			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			reader.Read ();
			Assert.IsTrue (true);
		}

		#region No end of line tests
		[Test]
		public void NoEndOfLineComment ()
		{
			StringWriter writer = new StringWriter ();
			writer.Write (" #   Some comment  ");

			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			reader.Read ();
			Assert.IsTrue (true);
		}

		[Test]
		public void NoEndOfLineKey ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Nini Thing]");
			writer.Write (" somekey = key ");

			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			reader.Read ();
			Assert.IsTrue (true);
		}
		
		[Test]
		public void NoEndOfLineKeyNoValue ()
		{
			StringWriter writer = new StringWriter ();
			writer.WriteLine ("[Nini Thing]");
			writer.Write (" somekey = ");

			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			reader.Read ();
			Assert.IsTrue (true);
		}

		[Test]
		public void NoEndOfLineSection ()
		{
			StringWriter writer = new StringWriter ();
			writer.Write ("[Nini Thing]");

			IniReader reader = new IniReader (new StringReader (writer.ToString ()));
			reader.Read ();
			Assert.IsTrue (true);
		}
		#endregion
	}
}