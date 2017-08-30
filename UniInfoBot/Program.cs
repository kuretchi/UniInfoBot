using System.Threading.Tasks;
using SimpleInjector;

namespace UniInfoBot
{
    public static class Program
    {
        public static void Main()
        {
            Execute().GetAwaiter().GetResult();
        }

        public static async Task Execute()
        {
            var container = new Container();
            container.Register<IMusicDataManger, MusicDataManager>();
            container.Register<ITwitterManager, TwitterManager>();
            container.Verify();

            var worker = new Worker(container);
            await worker.Start();
        }
    }
}
