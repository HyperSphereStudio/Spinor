namespace Core;

public interface Any {
    public Core.Type Type { get; }
    public bool Isa(Type t) => t.Isa(this);
}