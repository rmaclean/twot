namespace twot
{
    using System;

    class Spinner : ProgressMeter, IDisposable
    {
        int step;
        string[] steps = new[] { "|", "/", "-", "\\" };

        public Spinner() : base(200) { }

        internal override void UpdateUI(object? state)
        {
            Write(steps[step]);
            step++;
            if (step >= steps.Length)
            {
                step = 0;
            }
        }

        public void Done()
        {
            doneEvent?.Set();
        }
    }
}
