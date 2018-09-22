namespace Scribd_Document_Downloader
{
    class Program {
        static void Main(string[] args) {
            System.Console.Write("Paste Scribd URL:");
            string url = System.Console.ReadLine();
            Scribd scribd = new Scribd(url);
            scribd.Start();
            System.Console.ReadKey();
        }
    }
}
