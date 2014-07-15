using System;

namespace JungleTimers
{
    internal class CallOnce
    {
        public Action A(Action action)
        {
            var context = new Context();
            Action ret = () =>
            {
                if (!context.AlreadyCalled)
                {
                    action();
                    context.AlreadyCalled = true;
                }
            };

            return ret;
        }

        private class Context
        {
            public bool AlreadyCalled;
        }
    }
}