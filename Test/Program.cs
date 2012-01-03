using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Test
{
	class Blah
	{
		public string X;
	}

	class Program
	{
		static void TestIt(ref Blah b)
		{
			b = null;
		}

		static void Main(string[] args)
		{
			Blah b = new Blah();
			TestIt(ref b);

			Console.WriteLine((b == null).ToString());
			Console.Read();
		}
	}
}
