using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Windows.Forms;
using System.Xml;

namespace GiderosPlayerRemote {
    public enum LogType {
      PlayerOutput,
      Info,
      Warning
    }

    public class RemoteController {
      Socket soc;
      NetworkStream networkStream;
      int nextSeqId = 1;
      string projectFileName;
      string giderosPath;
      IRemoteControllerListener logger;
      List<KeyValuePair<string, string>> fileList = new List<KeyValuePair<string, string>>();
      Dictionary<string, KeyValuePair<DateTime, byte[]>> md5 = new Dictionary<string, KeyValuePair<DateTime, byte[]>>();
      DependencyGraph dependencyGraph = new DependencyGraph();
      MD5 md5calculator = MD5.Create();
      ProjectProperties properties_ = new ProjectProperties();

      public bool TryStart(string addr, int port, string gprojPath, IRemoteControllerListener logger) {
        this.projectFileName = gprojPath;
        this.logger = logger;

        if (soc != null) {
          soc.Dispose();
        }
        soc = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
        try {
          soc.Connect(new IPEndPoint(IPAddress.Parse(addr), 15000));
        } catch (SocketException e) {
          if (e.ErrorCode == 10061) {
            return false;
          } else {
            throw;
          }
        }
        soc.ReceiveBufferSize = 1024 * 1024;
        soc.SendBufferSize = 1024 * 1024;
        networkStream = new NetworkStream(soc);
        Play();
        return true;           
      }

      public void ReadLoop() {
        var reader = new GiderosMessageReader(soc);

        try {

          while (true) {

            ReceivedGiderosMessage msg = reader.TryTakeMessageFromBuffer();

            if (msg == null) {
              if (reader.ReceiveMore() == 0) {
                return;
              }
              continue;
            }

            byte msgType = msg.ReadByte();

            switch (msgType) {

              case GiderosMessageType.Output:
                HandleOutput(msg);
                break;

              case GiderosMessageType.FileList:
                HandleFileList(msg);
                break;

              default:
                logger.X_Log(LogType.Warning, "unknown message:" + msgType);
                break;
            }
          }

        } catch (Exception) {

          throw new Exception("Random Exception :D");

        }
      }

      // Set the Gideros installation path which was passed by DebugAdapter.exe
      public void SetGiderosPath(string path) {
        giderosPath = path;
      }
    
      // Output Gideros print() statements to VSCode debug console
      void HandleOutput(ReceivedGiderosMessage msg)
      {
        /*
          * 5/1/2021 - Changed output type to "Info" because "PlayerOutput" 
          * does not append new lines for some reason.
        */
        //logger.X_Log(LogType.PlayerOutput, msg.ReadString());
        logger.X_Log(LogType.Info, msg.ReadString());
      }

      void HandleFileList(ReceivedGiderosMessage msg)
      {
        // void Application::dataReceived(const QByteArray& d)
        // in gdrdeamon/application.cpp

        Queue<KeyValuePair<string, string>> fileQueue = new Queue<KeyValuePair<string, string>>();

        Dictionary<string, string> localFileMap = new Dictionary<string, string>();
        Dictionary<string, string> localFileMapReverse = new Dictionary<string, string>();
        foreach (var e in fileList) {
          localFileMap[e.Key] = e.Value;
          localFileMapReverse[e.Value] = e.Key;
        }

        Dictionary<string, KeyValuePair<int, byte[]>> remoteFileMap = new Dictionary<string, KeyValuePair<int, byte[]>>();
        while (msg.IsEOB() == false) {
          var file = msg.ReadString();
          if (file[0] == 'F') {
            int age = msg.ReadInt();
            byte[] md5 = msg.ReadBytes(16);
            remoteFileMap[file.Substring(1)] = new KeyValuePair<int, byte[]>(age, md5);
          } else if (file[0] == 'D') {
          // Well... what happens here?
            logger.X_Log(LogType.Info, "what happens here???\n");
          }
        }

        // Delete unused files
        foreach (var iter in remoteFileMap) {
          if (localFileMap.ContainsKey(iter.Key) == false) {
            //printf("deleting: %s\n", qPrintable(iter->first));
            NewMessage(GiderosMessageType.DeleteFile).AppendString(iter.Key).Send();
            logger.X_Log(LogType.Info, "delete " + iter.Key + "\n");
          }
        }

        // upload files
        string path = Path.GetDirectoryName(projectFileName);
        foreach (var iter in localFileMap) {
          KeyValuePair<int, byte[]>? riter = remoteFileMap.Find(iter.Key);

          string localfile = Path.Combine(path, iter.Value);

          bool send = false;
          if (riter.HasValue == false) {
            //printf("always upload: %s\n", qPrintable(iter->first));
            send = true;
          } else {
            int localage = Util.FileAge(localfile);
            int remoteage = riter.Value.Key;
            byte[] localmd5 = md5[iter.Value].Value;
            byte[] remotemd5 = riter.Value.Value;

            if (localage < remoteage || localmd5.SequenceEqual(remotemd5) == false) {
              //printf("upload new file: %s\n", qPrintable(iter->first));
              send = true;
            }
          }

          if (send == true) {
            fileQueue.Enqueue(new KeyValuePair<string, string>(iter.Key, localfile));
          } else {
            //printf("don't upload: %s\n", qPrintable(iter->first));
          }
        }

        List<KeyValuePair<string, bool>> topologicalSort = dependencyGraph.TopologicalSort();

        List<string> luaFilesToPlay = topologicalSort.Where(x => x.Value == false).Select(x => localFileMapReverse[x.Key]).ToList();

        //---------------------------------------------------------
        // From here void Application::timer()
        var cfolderSent = new HashSet<string>();

        while (fileQueue.Count > 0) {
          string s1 = fileQueue.Peek().Key;
          string s2 = fileQueue.Peek().Value;
          fileQueue.Dequeue();

          // create remote directories
          var dir = Path.GetDirectoryName(s1);
          if (cfolderSent.Contains(dir)) {
            // pass
          } else {
            cfolderSent.Add(dir);
            NewMessage(GiderosMessageType.CreateFolder).AppendString(dir).Send();
            //logger(LogType.Info, "cfolder " + dir);
          }

          string fileName = Path.Combine(path, s2);

          try {
            byte[] bytes = File.ReadAllBytes(fileName);

            NewMessage(GiderosMessageType.File).AppendString(s1).AppendByteArray(bytes).Send();
            logger.X_Log(LogType.Info, "Uploading " + s1 + "\r\n");

          } catch (FileNotFoundException) {
          // md5 계산에서 이미 경고를 냈으므로
          // 여기에선 무시한다

          logger.X_Log(LogType.Info, "Unmanaged error.\r\n");

          // Since the md5 calculation already gave a warning
          // Ignore here
        }
      }
        logger.X_Log(LogType.Info, "Uploading complete.\r\n");

        SendProjectProperties();

        //-----------------------------------------------
        var playMsg = NewMessage(GiderosMessageType.Play);
        foreach (string f in luaFilesToPlay) {
          //logger(LogType.Info, "play " + f);
          playMsg.AppendString(f);
        }
        playMsg.Send();
      }

      void SendProjectProperties() {
        var m = NewMessage(GiderosMessageType.SendProjectProperties);

        m.AppendInt(properties_.scaleMode);
        m.AppendInt(properties_.logicalWidth);
        m.AppendInt(properties_.logicalHeight);

        m.AppendInt((int)properties_.imageScales.Count);
        foreach (var s in properties_.imageScales) {
          m.AppendString(s.Key); // Suffix
          m.AppendFloat((float)s.Value); // Scale
        }
        /*
        m.AppendInt((int)properties_.plugins.Count);
        foreach (var p in properties_.plugins) {
          m.AppendString(p.Key); // Name
          m.AppendString(p.Value); // Enabled
        }
        */
        m.AppendInt(properties_.orientation);
        m.AppendInt(properties_.fps);

        m.AppendInt(properties_.retinaDisplay);
        m.AppendInt(properties_.autorotation);

        m.AppendInt(properties_.mouseToTouch ? 1 : 0);
        m.AppendInt(properties_.touchToMouse ? 1 : 0);
        m.AppendInt(properties_.mouseTouchOrder);

        m.AppendInt(properties_.windowWidth);
        m.AppendInt(properties_.windowHeight);

        m.Send();
      }

      void Play() {
        XmlDocument doc = new XmlDocument();
        doc.Load(projectFileName);

      var root = doc.DocumentElement;
      var properties = (XmlElement)root.SelectSingleNode("properties");

      // Read properties
      {

        properties_.Clear();

          // 
          // Plugin options
          //
          /*
          var plugins = properties.SelectSingleNode("plugins");
          foreach (var p in plugins.ChildNodes) {
            var plugin = (XmlElement)p;
            properties_.plugins.Add(new KeyValuePair<string, string>(plugin.GetAttribute("name"), plugin.GetAttribute("enabled")));
          }
          */

          // 
          // Graphics options
          // 

          var imageScales = properties.SelectSingleNode("imageScales");
          foreach (var n in imageScales.ChildNodes) {
            var scale = (XmlElement)n;
            properties_.imageScales.Add(new KeyValuePair<string, double>(scale.GetAttribute("suffix"), double.Parse(scale.GetAttribute("scale"))));
          }

          if (properties.HasAttribute("scaleMode")) {
            properties_.scaleMode = int.Parse(properties.GetAttribute("scaleMode"));
          }

          if (properties.HasAttribute("logicalWidth"))
          {
            properties_.logicalWidth = int.Parse(properties.GetAttribute("logicalWidth"));
          }

          if (properties.HasAttribute("logicalHeight")) {
            properties_.logicalHeight = int.Parse(properties.GetAttribute("logicalHeight"));
          }

          if (properties.HasAttribute("orientation")) {
            properties_.orientation = int.Parse(properties.GetAttribute("orientation"));
          }

          if (properties.HasAttribute("fps")) {
            properties_.fps = int.Parse(properties.GetAttribute("fps"));
          }

          // 
          // IOS options
          // 

          if (properties.HasAttribute("retinaDisplay")) {
            properties_.retinaDisplay = int.Parse(properties.GetAttribute("retinaDisplay"));
          }

          if (properties.HasAttribute("autorotation")) {
            properties_.autorotation = int.Parse(properties.GetAttribute("autorotation"));
          }

          // 
          // Export options
          // 

          if (properties.HasAttribute("architecture")) {
            properties_.architecture = int.Parse(properties.GetAttribute("architecture"));
          }

          if (properties.HasAttribute("exportMode")) {
            properties_.exportMode = int.Parse(properties.GetAttribute("exportMode"));
          }

          if (properties.HasAttribute("iosDevice")) {
            properties_.iosDevice = int.Parse(properties.GetAttribute("iosDevice"));
          }

          if (properties.HasAttribute("packageName")) {
            properties_.packageName = properties.GetAttribute("packageName");
          }
        }

      // 
      // Populate file list and dependency graph
      // 

      {
        fileList.Clear();
        dependencyGraph.Clear();

        var dependencies = new List<KeyValuePair<string, string>>();

        var stack = new Stack<XmlElement>();

        stack.Push(doc.DocumentElement);

        var dir = new List<string>();

        while (stack.Count > 0) {

          XmlElement e = stack.Pop();
          if (e == null) {
            dir.RemoveAt(dir.Count - 1);
            continue;
          }

          string type = e.Name;
          string fileName;

          if (type == "file") {
            fileName = e.HasAttribute("source") ? e.GetAttribute("source") : e.GetAttribute("name");

            string name = Path.GetFileName(fileName);

            string n = "";
            foreach (string d in dir) {
              n += d + "/";
            }
            n += name;

            fileList.Add(new KeyValuePair<string, string>(n, fileName));

            if (fileName.ToLower().EndsWith(".lua")) { // Only process .lua files

              bool excludeFromExecution = (e.HasAttribute("excludeFromExecution")) && (int.Parse(e.GetAttribute("excludeFromExecution")) != 0);

              if (!excludeFromExecution) { // Only examine the files content if the excludeFromExecution attribute is not present
                try {
                  // If the "excludeFromExecution" attribute is not present, check if the first line of the file ends 
                  // with the string "!NOEXEC", and if it does, specify that the file should not be executed.
                  string path = Path.GetDirectoryName(projectFileName);
                  string line1 = File.ReadLines(Path.GetFullPath(path) + "/" + fileName).First(); // Read first line of file
                  if (line1.EndsWith("!NOEXEC")) {
                    excludeFromExecution = true;
                  }
                } catch (Exception) {
                  // Just skip any processing if the file is empty
                  logger.X_Log(LogType.Info, fileName + " is empty... skipping\n");
                }
              }

              dependencyGraph.AddCode(fileName, excludeFromExecution);
            }
            continue;
          } // end file type check

          if (type == "folder") {
            string name = e.GetAttribute("name");
            dir.Add(name);

            string n = "";
            foreach (string d in dir) {
              n += d + "/";
            }
            stack.Push(null);
          }

          if (type == "dependency") {
            dependencies.Add(new KeyValuePair<string, string>(e.GetAttribute("from"), e.GetAttribute("to")));
          }

          var childNodes = e.ChildNodes;
          foreach (var c in childNodes) {
            stack.Push((XmlElement)c);
          }
        }

        // Plugin stuff

        string pluginPath = giderosPath + "/Plugins/";

        var plugins = properties.SelectSingleNode("plugins");
        foreach (var p in plugins.ChildNodes) {
          var plugin = (XmlElement)p;
          string name = plugin.GetAttribute("name");
          bool enabled = (plugin.HasAttribute("enabled")) && (int.Parse(plugin.GetAttribute("enabled")) != 0);

          if (enabled) {
            string type = "DLL";

            if (!File.Exists(pluginPath + name + ".dll")) {
              type = "LUA";
              fileList.Add(new KeyValuePair<string, string>(name.ToLower() + ".lua", giderosPath + "/All Plugins/" + name + "/luaplugin/" + name.ToLower() + ".lua"));
              //            dependencyGraph.AddCode(name, true);
            }
            logger.X_Log(LogType.Info, "plugin:" + name + " enabled:" + enabled.ToString() + " (" + type + " plugin)\n");


            /*
            if (File.Exists(pluginPath + name + ".dll"))
            {
              logger.X_Log(LogType.Info, "plugin:" + name + " enabled:" + enabled.ToString() + " (DLL plugin)\n");
            }
            else
            {
              logger.X_Log(LogType.Info, "plugin:" + name + " enabled:" + enabled.ToString() + " (LUA plugin)\n");
              fileList.Add(new KeyValuePair<string, string>(name.ToLower() + ".lua", giderosPath + "/All Plugins/" + name + "/luaplugin/" + name.ToLower() + ".lua"));
              //            dependencyGraph.AddCode(name, true);
            }
            */
          
          } else {
            logger.X_Log(LogType.Info, "skipping disabled plugin:" + name + "\n");

          }





        }


        foreach (var d in dependencies) {
          dependencyGraph.AddDependency(d.Key, d.Value);
        }

      }

      UpdateMD5();

      SendStop();

      NewMessage(GiderosMessageType.SetProjectName).AppendString(Path.GetFileNameWithoutExtension(projectFileName)).Send();

      NewMessage(GiderosMessageType.SendFileList).Send();
    }

    public void SendStop() {
      NewMessage(GiderosMessageType.Stop).Send();
    }

    void UpdateMD5()
    {
      //var begin = DateTime.Now;

      // because the .tmp/~.md5 file format depends on QDataStream
      // Skip the implementation of .md5 caching because it's cumbersome to reverse engineer.
      // I don't know how slow it is to recalculate the entire md5 on PC these days
      // Implement without a cache, run the calculation at a dual scale, and cache if it is slow..

      string path = Path.GetDirectoryName(projectFileName);

      foreach (var f in fileList) {
        var filename = f.Value;

        // Create the absolute path name, and replace "/" chars with "\" chars
        var absfilename = Path.Combine(path, filename);//.Replace(@"/", "\\");



        //logger.X_Log(LogType.Info, "val: " + filename + " key: " + p + "\n");

        DateTime mtime = File.GetLastWriteTime(absfilename);
        ////if (File.Exists(absfilename)){
          //if (iter == md5_.end() || mtime != iter.value().first)
          //{
          md5[filename] = new KeyValuePair<DateTime, byte[]>(mtime, MD5FromFile(absfilename));
          //}

        ////}

      }

      //logger("md5 time: " + (DateTime.Now - begin).TotalSeconds.ToString());
    }

    byte[] MD5FromFile(string path) {
      //logger.X_Log(LogType.Warning, "MD5FromFile(" + path + ")\n");
      try {
        using (var stream = File.OpenRead(path)) {
          return md5calculator.ComputeHash(stream);
        }

      } catch (FileNotFoundException) {

        logger.X_Log(LogType.Warning, "File not found: " + path + "\n");
        return new byte[16];
      }
    }

    GiderosMessageToSend NewMessage(byte ty) {
      var rv = new GiderosMessageToSend(nextSeqId++, networkStream);
      rv.AppendByte(ty);
      return rv;
    }

  }
}
