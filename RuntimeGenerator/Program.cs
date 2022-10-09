using RuntimeGenerator.JOp;

var projectName = "juliapp";

var rootPath = Environment.CurrentDirectory.Substring(0, Environment.CurrentDirectory.IndexOf(projectName)) + projectName;

JOPCodeGenerator.GenerateOpCodeTable(rootPath);