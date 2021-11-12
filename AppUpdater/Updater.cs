using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace AppUpdater
{
  public class Updater
  {
    public string Folder { get; private set; }
    public List<string> Paths { get; set; }
    public List<(string, string)> PathHash {get;set; }
    public List<(string, string)> GitPathHash{get;set;}
    public string GitCatalog {get;set;}

    public Updater(string directory)
    {
      this.Folder = directory;
      this.Paths = new List<string>();
      this.PathHash = new List<(string, string)>();
      FillPaths(Folder);
      CountHashes();
    }

    public async Task<bool> Update()
    {
      DownloadMD5();
      await ReadGitMD5Async();
      return DownloadNew();
    }

    private bool DownloadMD5()
    {
      using var webClient = new WebClient();
      webClient.Headers.Add("a", "a");
      try
      {
        webClient.DownloadFile($"{GitCatalog}/MD5.txt", @$"{Folder}\MD5.txt");
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }
   
    private static string CalculateMD5(string filename)
    {
      using var md5 = MD5.Create();
      using var stream = File.OpenRead(filename);
      var hash = md5.ComputeHash(stream);
      return BitConverter.ToString(hash).Replace("-", "").ToLowerInvariant();
    }

    private void CountHashes()
    {
      foreach (var path in this.Paths)
      {
        this.PathHash.Add((path.Replace(Folder,"")[1..], CalculateMD5(path)));
      }
    }

    private void FillPaths(string folder)
    {
      Paths.AddRange(Directory.GetFiles(folder));
      var folders = Directory.GetDirectories(folder);
      foreach (var dir in folders)
      {
        FillPaths(dir);
      }
    }

    public async Task<bool> WriteInFileAsync()
    {
      try
      {
        using var sw =
          new StreamWriter($"{Folder}\\MD5.txt", false, Encoding.Default);
        foreach(var (path,hash) in PathHash)
        {
          await sw.WriteLineAsync($"{path};{hash}");
        }
        return true;
      }
      catch (Exception)
      {
        return false;
      }
    }

    private async Task ReadGitMD5Async()
    {
      GitPathHash = new List<(string, string)>();
      using var sr = new StreamReader($"{Folder}/MD5.txt", Encoding.Default);
      string line;
      while ((line = await sr.ReadLineAsync()) != null)
      {
        var lineParts = line.Split(';');
        GitPathHash.Add((lineParts[0],lineParts[1]));
      }
    }

    private bool DownloadNew()
    {
      var toDownload = new List<string>();
      foreach (var (path, hash) in GitPathHash)
      {
        if (PathHash.Contains((path, hash))) continue;
        toDownload.Add(path);
      }
      foreach (var file in toDownload)
      {
        using var webClient = new WebClient();
        webClient.Headers.Add("a", "a");
        try
        {
          webClient.DownloadFile($"{GitCatalog}\\{file}", @$"{Folder}\{file}");
        }
        catch (Exception)
        {
          return false;
        }
      }
      return true;
    }
  }
}
