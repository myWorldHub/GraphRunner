using GraphConnectEngine;
using GraphConnectEngine.Graphs.Value;
using GraphConnectEngine.Nodes;

namespace GraphRunner
{
    public static class GraphExtension
    {
        //TODO 登録できるようにする
        public static IGraph? ToGraph(this GraphSetting setting,INodeConnector conn)
        {
            switch (setting.Type)
            {
                case "Updater":
                    return new MyUpdateGraph(conn, new SerialSender());
                case "DebugText":
                    return new MyDebugTextGraph(conn);
                case "Value":

                    if (!(setting.Setting.ContainsKey("Type") &&
                          setting.Setting.ContainsKey("Value")))
                        return null;

                    switch (setting.Setting["Type"])
                    {
                        case "Int":
                            return new ValueGraph<int>(conn, int.Parse(setting.Setting["Value"]));
                        case "String":
                            return new ValueGraph<string>(conn, setting.Setting["Value"]);
                    }

                    break;
            }

            return null;
        }
    }
}
