<Query Kind="Program" />

void Main()
{
	CovImmutableStack.SampleCode();
}


// Fabulous Adventures in Data Structures and Algorithms
// Eric Lippert
// Chapter 2

// Handy extension methods
static class Extensions
{
	public static string Comma<T>(this IEnumerable<T> items) =>
		string.Join(',', items);
	public static string Bracket<T>(this IEnumerable<T> items) =>
		"[" + items.Comma() + "]";

	// Original version of Reverse
	//
	// public static IImStack<T> Reverse<T>(this IImStack<T> stack)
	// {
	//    var result = ImStack<T>.Empty;
	//    for (; !stack.IsEmpty; stack = stack.Pop())
	//        result = result.Push(stack.Peek());
	//    return result;
	// }
	//
	// Refactored into ReverseOnto helper:

	public static IImStack<T> ReverseOnto<T>(this IImStack<T> stack, IImStack<T> tail)
	{
		var result = tail;
		for (; !stack.IsEmpty; stack = stack.Pop())
			result = result.Push(stack.Peek());
		return result;
	}
	public static IImStack<T> Reverse<T>(this IImStack<T> stack) =>
		stack.ReverseOnto(ImStack<T>.Empty);
	public static IImStack<T> Concatenate<T>(this IImStack<T> xs, IImStack<T> ys) =>
		ys.IsEmpty ? xs : xs.Reverse().ReverseOnto(ys);
	public static IImStack<T> Append<T>(this IImStack<T> stack, T item) =>
		stack.Concatenate(ImStack<T>.Empty.Push(item));
}


	static class CovImmutableStack
	{
		public static void SampleCode()
		{
			Console.WriteLine("A covariant immutable stack");

			// By making Push a static method of the class and
			// adding an extension method to maintain our fluent user
			// interface, we can create a covariant version of the 
			// immutable stack.

			IImStack<Tiger> s1 = ImStack<Tiger>.Empty;
			IImStack<Tiger> s2 = s1.Push(new Tiger());
			IImStack<Tiger> s3 = s2.Push(new Tiger());
			IImStack<Animal> s4 = s3; // Legal because of covariance.
			IImStack<Animal> s5 = s4.Push(new Giraffe());

			s5.Bracket().Dump();
		}
	}

	class Animal
	{
		public override string ToString() => this.GetType().Name;
	}
	class Tiger : Animal { }
	class Giraffe : Animal { }

	public interface IImStack<out T> : IEnumerable<T>
	{
		T Peek();
		IImStack<T> Pop();
		bool IsEmpty { get; }
	}

	public class ImStack<T> : IImStack<T>
	{
		private class EmptyStack : IImStack<T>
		{
			public EmptyStack() { }
			public T Peek() => throw new InvalidOperationException();
			public IImStack<T> Pop() => throw new InvalidOperationException();
			public bool IsEmpty => true;
			public IEnumerator<T> GetEnumerator()
			{
				yield break;
			}
			IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
		}
		public static IImStack<T> Empty { get; } = new EmptyStack();

		private readonly T item;
		private readonly IImStack<T> tail;
		private ImStack(T item, IImStack<T> tail)
		{
			this.item = item;
			this.tail = tail;
		}
		public static IImStack<T> Push(T item, IImStack<T> tail) => new ImStack<T>(item, tail);
		public T Peek() => item;
		public IImStack<T> Pop() => tail;
		public bool IsEmpty => false;
		public IEnumerator<T> GetEnumerator()
		{
			IImStack<T> s = this;
			for (; !s.IsEmpty; s = s.Pop())
				yield return s.Peek();
		}
		IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
	}

	static class ExtensionsImStack
	{
		public static IImStack<T> Push<T>(this IImStack<T> stack, T item) =>
			ImStack<T>.Push(item, stack);
	}

