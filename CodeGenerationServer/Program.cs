namespace GraphConnectEngine.CodeGen;

using GraphConnectEngine.Nodes;
using System.Diagnostics;
using System.Net;
using System.Text.Json;

#nullable disable

internal static class Program
{
    public static void Main(string[] args)
    {

        System.IO.File.WriteAllText("sample.yaml",JsonSerializer.Serialize(GeneratorSetting.Sample));
        Environment.Exit(0);

        Console.WriteLine("Starting server...");

        if (!HttpListener.IsSupported)
        {
            Console.WriteLine("Windows XP SP2 or Server 2003 is required to use the HttpListener class.");
            return;
        }

        // Create a listener.
        var httpListener = new HttpListener();
        httpListener.Prefixes.Add($@"http://localhost:80/");

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

                if (request.Url == null)
                {
                    var res = context.Response;
                    res.StatusCode = 400;
                    res.Close();

                    Console.WriteLine($"Completed 404 NotFound");
                    continue;
                }

                var path = request.Url.LocalPath;

                // Obtain a response object.
                var response = context.Response;
                Stream output = response.OutputStream;

                response.Headers.Add("Access-Control-Allow-Origin", "*");
                response.Headers.Add("Access-Control-Allow-Methods", "POST, GET");

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

        //設定をパラメータから取得する
        string topologyJson = null;
        string generatorSettingJson = null;
        string startGraphId = null;

        for (int i = 0; i < request.QueryString.Count; i++)
        {
            var key = request.QueryString.GetKey(i);
            if (key == "t")
            {
                topologyJson = request.QueryString[i];
            }
            else if(key == "g")
            {
                generatorSettingJson = request.QueryString[i];
            }
            else if(key == "s")
            {
                startGraphId = request.QueryString[i];
            }
        }

        // Construct a response.
        string responseString = "";
        byte[] buffer;

        if (topologyJson == null || generatorSettingJson == null || startGraphId == null)
        {
            sw.Stop();
            Console.WriteLine($"Completed 400 BadRequest in {sw.ElapsedMilliseconds}ms");

            responseString = "One or more parameters are missing.";
            buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            await outputStream.WriteAsync(buffer, 0, buffer.Length);
            outputStream.Close();
            return;
        }

        //設定をデシリアライズする
        GraphTopologySetting topology = null;
        GeneratorSetting generatorSetting = null;

        try
        {
            topology = JsonSerializer.Deserialize<GraphTopologySetting>(topologyJson);
            generatorSetting = JsonSerializer.Deserialize<GeneratorSetting>(generatorSettingJson);
        }
        catch (Exception)
        {
            //パースでエラー
            sw.Stop();
            Console.WriteLine($"Completed 400 BadRequest in {sw.ElapsedMilliseconds}ms");

            responseString = "ParseError";
            buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            await outputStream.WriteAsync(buffer, 0, buffer.Length);
            outputStream.Close();
        }

        //設定からグラフを読み込む
        var sw2 = Stopwatch.StartNew();

        var connector = new NodeConnector();
        var graphs = new Dictionary<string,AutoGraph>();
        
        //グラフを生成する
        foreach(var (id, setting) in topology.Graphs)
        {
            var graph = generatorSetting.CreateGraph(id,setting);
            if (graph != null)
            {
                graphs[id] = graph;
            }
            else 
            { 
                //グラフ生成でエラー
                sw.Stop();
                sw2.Stop();
                Console.WriteLine($"Completed 400 BadRequest in {sw.ElapsedMilliseconds}ms");

                responseString = $"Failed to Instantiate Graph({id})";
                buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
                response.ContentLength64 = buffer.Length;
                await outputStream.WriteAsync(buffer, 0, buffer.Length);
                outputStream.Close();
            }
        }

        //最初のグラフが見つからない
        if (!graphs.ContainsKey(startGraphId))
        {
            sw.Stop();
            sw2.Stop();
            Console.WriteLine($"Completed 400 BadRequest in {sw.ElapsedMilliseconds}ms");

            responseString = $"StartGraph({startGraphId}) is not Found";
            buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
            response.ContentLength64 = buffer.Length;
            await outputStream.WriteAsync(buffer, 0, buffer.Length);
            outputStream.Close();
        }

        //ノードをつなぐ
        foreach(var setting in topology.Connections)
        {
            if(GraphTopologySetting.TryParseConnection(setting,out var node1,out var node2))
            {
                connector.ConnectNode(node1.ToNode(graphs[node1.GraphId]), node2.ToNode(graphs[node2.GraphId]));
            }
        }

        sw2.Stop();

        //生成する
        var sw3 = Stopwatch.StartNew();

        var gen = new SequentialCodeGenerator(generatorSetting);
        responseString = gen.Generate(graphs[startGraphId], connector);

        sw3.Stop();

        //処理を終了して結果を返す
        sw.Stop();
        Console.WriteLine($"Completed 200 OK in {sw.ElapsedMilliseconds}ms (graph : {sw2.ElapsedMilliseconds}ms , gen : {sw3.ElapsedMilliseconds}ms)");

        buffer = System.Text.Encoding.UTF8.GetBytes(responseString);
        response.ContentLength64 = buffer.Length;
        await outputStream.WriteAsync(buffer, 0, buffer.Length);
        outputStream.Close();
    }
}