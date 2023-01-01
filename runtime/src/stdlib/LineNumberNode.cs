namespace Core;

public class LineNumberNode : SystemAny {
    public int Line;
    public string File;

    public LineNumberNode(int line, string file) {
        Line = line;
        File = file;
    }

    public override string ToString() => "#= " + File + ":" + Line + " =#";
    public override Type Type => Types.LineNumberNode;
}