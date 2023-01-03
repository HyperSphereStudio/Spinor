/*
   * Author : Johnathan Bizzano
   * Created : Monday, January 2, 2023
   * Last Modified : Monday, January 2, 2023
*/

namespace Core;

public interface ILineNumberNode {
    public int Line { get; }
    public string File { get; }
}

public class LineNumberNode : SystemAny, ILineNumberNode{
    public int Line { get; }
    public string File { get; }

    public LineNumberNode(int line, string file) {
        Line = line;
        File = file;
    }

    public override string ToString() => "#= " + File + ":" + Line + " =#";
    public override Type Type => Types.LineNumberNode;
}