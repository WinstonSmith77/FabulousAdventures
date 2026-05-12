<Query Kind="Program" />

void Main()
{
	Console.WriteLine("An immutable queue");
	var qs = new List<IImQueue<int>> { ImQueue<int>.Empty };

	var current = qs.First();

	foreach (var element in Enumerable.Range(0, 3))
	{
		current = current.Enqueue(element);
		qs.Add(current);
	}

	foreach (var q in qs)
	{
		var q_ = q;
		while (!q_.IsEmpty)
		{
			q_.Peek().Dump();
			q_ = q_.Dequeue();
		}
		" ".Dump();
	}

	qs.Dump();


}

public interface IImStack<T> : IEnumerable<T>
{
	IImStack<T> Push(T item);
	T Peek();
	IImStack<T> Pop();
	bool IsEmpty { get; }
}



public class ImStack<T> : IImStack<T>
{
	private class EmptyStack : IImStack<T>
	{
		public EmptyStack() { }
		public IImStack<T> Push(T item) => new ImStack<T>(item, this);
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
	public IImStack<T> Push(T item) => new ImStack<T>(item, this);
	public T Peek() => item;
	public IImStack<T> Pop() => tail;
	public bool IsEmpty => false;
	public IEnumerator<T> GetEnumerator()
	{
		for (IImStack<T> s = this; !s.IsEmpty; s = s.Pop())
			yield return s.Peek();
	}
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}

interface IImQueue<T> : IEnumerable<T>
{
	IImQueue<T> Enqueue(T item);
	T Peek();
	IImQueue<T> Dequeue();
	bool IsEmpty { get; }
}

// Listing 2.7: An immutable queue interface

class ImQueue<T> : IImQueue<T>
{
	public static IImQueue<T> Empty { get; } = new ImQueue<T>(ImStack<T>.Empty, ImStack<T>.Empty);

	private readonly IImStack<T> enqueues;
	private readonly IImStack<T> dequeues;
	private ImQueue(IImStack<T> enqueues, IImStack<T> dequeues)
	{
		this.enqueues = enqueues;
		this.dequeues = dequeues;
	}
	public IImQueue<T> Enqueue(T item) =>
		this.IsEmpty ?
		new ImQueue<T>(this.enqueues, this.dequeues.Push(item)) :
		new ImQueue<T>(this.enqueues.Push(item), this.dequeues);
	public T Peek() => this.dequeues.Peek();
	public IImQueue<T> Dequeue()
	{
		IImStack<T> newdeq = this.dequeues.Pop();
		if (!newdeq.IsEmpty)
			return new ImQueue<T>(this.enqueues, newdeq);
		if (this.enqueues.IsEmpty)
			return Empty;
		return new ImQueue<T>(ImStack<T>.Empty, this.enqueues.Reverse());
	}
	public bool IsEmpty => this.dequeues.IsEmpty;
	public IEnumerator<T> GetEnumerator()
	{
		foreach (var item in this.dequeues)
			yield return item;
		foreach (var item in this.enqueues.Reverse())
			yield return item;
	}
	IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}


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
