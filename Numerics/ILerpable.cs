namespace XYEngine;

public interface ILerpable<T>
{
	public T Lerp(T other, float t);
}