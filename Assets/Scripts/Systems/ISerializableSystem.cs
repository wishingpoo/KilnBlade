public interface ISerializableSystem
{
    object CaptureState();
    void RestoreState(object state);
}
