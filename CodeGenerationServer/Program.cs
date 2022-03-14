
global using GraphConnectEngine.Nodes;

using System.Diagnostics;
using System.Net;
using System.Security;
using System.Text.Json;

namespace GraphConnectEngine.CodeGen;
#nullable disable

//TODO
// 変数名固定
// シンタックスハイライト
// 

internal static class Program
{
    public static void Main(string[] args)
    {
        /*
        File.WriteAllText("sample_gen.json", JsonSerializer.Serialize(Sample.GeneratorSetting));
        File.WriteAllText("sample_top.json", JsonSerializer.Serialize(Sample.GraphTopologySetting));
        Environment.Exit(0);

        GraphConnectEngine.Logger.SetLogMethod(s=>System.Console.WriteLine(s));
        GraphConnectEngine.Logger.LogLevel = GraphConnectEngine.Logger.LevelDebug;
        */

        Console.WriteLine("Starting server...");

        if (!HttpListener.IsSupported)
        {
            Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
            return;
        }

        // Create a listener.
        var httpListener = new HttpListener();
        httpListener.Prefixes.Add($@"http://+:19136/");

        //open
        try
        {
            httpListener.Start();
            Console.WriteLine("Started.");
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.ToString());
            Environment.Exit(-1);
        }

        bool isRunning = true;

        Task.Run(async () =>
        {
            while (isRunning)
            {
                // Note: The GetContext method blocks while waiting for a request.
                HttpListenerContext context = await httpListener.GetContextAsync();
                HttpListenerRequest request = context.Request;

                Console.WriteLine($"Access from {context.Request.RemoteEndPoint} to {request.RawUrl}");

                // Obtain a response object.
                var response = context.Response;
                Stream output = response.OutputStream;

                response.AppendHeader("Access-Control-Allow-Origin", "*");
                response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept, X-Requested-With, access-control-allow-headers,access-control-allow-methods,access-control-allow-origin,content-type");
                response.AddHeader("Access-Control-Allow-Methods", "GET, POST");
                response.AddHeader("Access-Control-Max-Age", "128");

                if (request.HttpMethod == "OPTIONS")
                {
                    var buffer = System.Text.Encoding.UTF8.GetBytes("Preflight");
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = (int)HttpStatusCode.OK;
                    await output.WriteAsync(buffer, 0, buffer.Length);
                    output.Close();
                    response.Close();
                    continue;
                }

                if (request.Url == null)
                {
                    response.StatusCode = 404;
                    response.Close();

                    Console.WriteLine($"Completed 404 NotFound");
                    continue;
                }

                Listen(request, output, response);
            }
        });

        while (Console.ReadLine() != "quit")
        {
            isRunning = false;
            continue;
        }

    }
    async static void Listen(HttpListenerRequest request, Stream outputStream, HttpListenerResponse response)
    {
        //時間を計測する
        var sw = Stopwatch.StartNew();

        // Construct a response.
        string responseString = "";
        byte[] buffer;

        //設定をデシリアライズする
        GraphTopologySetting topology = null;
        GeneratorSetting generatorSetting = null;
        string startGraphId = null;

        try
        {
            var body = request.InputStream;
            var encoding = request.ContentEncoding;
            var reader = new StreamReader(body, encoding);

            Console.WriteLine("Start of post:");
            string post = reader.ReadToEnd();
            Console.WriteLine(post);
            Console.WriteLine("End of post");
            body.Close();
            reader.Close();

            var data = JsonSerializer.Deserialize<PostPack>(post);

            topology = data.GraphTopologySetting;
            generatorSetting = data.GeneratorSetting;
            startGraphId = data.StartGraphId;

            if (topology == null || 
                generatorSetting == null ||
                startGraphId == null)
            {
                throw new Exception();
            }
        }
        catch (Exception)
        {
            //パースでエラー
            sw.Stop();
            Console.WriteLine("Error : parse");
            Console.WriteLine($"Completed 400 BadRequest in {sw.ElapsedMilliseconds}ms");

            responseString = "400 BadRequest (Parse Error)";
            buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            await outputStream.WriteAsync(buffer, 0, buffer.Length);
            outputStream.Close();
            return;
        }

        //設定からグラフを読み込む
        var sw2 = Stopwatch.StartNew();

        var connector = new NodeConnector();
        var graphs = new Dictionary<string, AutoGraph>();

        //グラフを生成する
        foreach (var (id, setting) in topology.Graphs)
        {
            var graph = generatorSetting.CreateGraph(id, setting);
            if (graph != null)
            {
                graphs[id] = graph;
            }
            else
            {
                //グラフ生成でエラー
                sw.Stop();
                sw2.Stop();
                Console.WriteLine("Error : graph generation");
                Console.WriteLine($"Completed 400 BadRequest in {sw.ElapsedMilliseconds}ms");

                responseString = $"Failed to Instantiate Graph({id})";
                buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                response.StatusCode = (int)HttpStatusCode.BadRequest;
                await outputStream.WriteAsync(buffer, 0, buffer.Length);
                outputStream.Close();
                return;
            }
        }

        //最初のグラフが見つからない
        if (!graphs.ContainsKey(startGraphId))
        {
            sw.Stop();
            sw2.Stop();
            Console.WriteLine("Error : first graph");
            Console.WriteLine($"Completed 400 BadRequest in {sw.ElapsedMilliseconds}ms");

            responseString = $"StartGraph({startGraphId}) not Found";
            buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            response.StatusCode = (int)HttpStatusCode.BadRequest;
            await outputStream.WriteAsync(buffer, 0, buffer.Length);
            outputStream.Close();
            return;
        }

        //ノードをつなぐ
        foreach (var setting in topology.Connections)
        {
            if (GraphTopologySetting.TryParseConnection(setting, out var node1, out var node2))
            {
                if(!connector.ConnectNode(node1.ToNode(graphs[node1.GraphId]), node2.ToNode(graphs[node2.GraphId])))
                {
                    sw.Stop();
                    sw2.Stop();
                    Console.WriteLine("Error : type mismatch");
                    Console.WriteLine($"Completed 400 BadRequest in {sw.ElapsedMilliseconds}ms");

                    responseString = $"Failed to connect node.\nNode[{node1.GraphId}][{node1.NodeType}][{node1.Index}]\nNode[{node2.GraphId}][{node2.NodeType}][{node2.Index}]";
                    buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                    response.ContentLength64 = buffer.Length;
                    response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await outputStream.WriteAsync(buffer, 0, buffer.Length);
                    outputStream.Close();
                    return;
                }
            }
        }

        sw2.Stop();

        //生成する
        var sw3 = Stopwatch.StartNew();

        //TODO 二回生成してるのをどうにかする
        var generator = new SequentialCodeGenerator(generatorSetting);
        var code = generator.Generate(graphs[startGraphId], connector);
        var xml = generator.Generate(graphs[startGraphId], connector, (graph,str) =>
          {
              var escapedId = SecurityElement.Escape(graph.Id);
              return $"<GraphSyntaxHighLight {escapedId}>{str}</GraphSyntaxHighLight>";
          });

        var result = new ResultPack
        {
            GeneratedCode = code,
            SyntaxHighlight = xml
        };
        responseString = JsonSerializer.Serialize(result);

        sw3.Stop();

        //処理を終了して結果を返す
        sw.Stop();
        Console.WriteLine($"Completed 200 OK in {sw.ElapsedMilliseconds}ms (graph : {sw2.ElapsedMilliseconds}ms , gen : {sw3.ElapsedMilliseconds}ms)");

        buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        response.StatusCode = (int)HttpStatusCode.OK;
        await outputStream.WriteAsync(buffer, 0, buffer.Length);
        outputStream.Close();
        return;
    }
}