using System;
using System.Threading;
using System.Threading.Tasks;

namespace Messenger
{
    public class RecieveThread
    {
        private readonly Task task;
        readonly CancellationTokenSource cancelTokenSource;
        readonly CancellationToken token;

        public RecieveThread(Action<CancellationToken> function)
        {
            cancelTokenSource = new CancellationTokenSource();
            token = cancelTokenSource.Token;
            task = new Task(() => function(token), token);
        }

        public void StartThread() =>
            task.Start();

        public void CancelThread()
        {
            cancelTokenSource.Cancel();
            //Тут ошибка выпадала
            //task.Wait();
            task.Dispose();
        }
    }
}
