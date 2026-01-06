namespace Core
{
	public interface IObserver
	{
		 void OnObjectChanged(IObservable observable);
	}
}