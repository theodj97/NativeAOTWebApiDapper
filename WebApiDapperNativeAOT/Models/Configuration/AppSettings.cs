namespace WebApiDapperNativeAOT.Models.Configuration;

public partial class AppSettings { public ConnectionStrings ConnectionStrings { get; set; } = new(); }
public partial class ConnectionStrings { public string TodoDB { get; set; } = ""; }