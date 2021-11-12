using NUnit.Framework;
using AppUpdater;
using System.Threading.Tasks;

namespace AppUpdater.Tests
{
  public class Tests
  {
    private Updater Updater {get;set;}
    [SetUp]
    public void Setup()
    {
      var directory = @"D:\печать полисов\PolisCardPrinter\win-x86";
      Updater = new Updater(directory);
    }

    [Test]
    public async Task UpdateTestAsync()
    {
      Updater.GitCatalog = "http://10.18.100.100:3000/94000_it5/PolisCardPrinter-x86Unpacked/raw/master";
      var result = await Updater.Update();
      Assert.IsTrue(result);
    }

    [Test]
    public async Task CreateMd5()
    {
      await Updater.WriteInFileAsync();
    }
  }
}