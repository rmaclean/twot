namespace twot
{
    using System;

    internal class Spinner : ProgressMeter, IDisposable
    {
        private readonly string[][] spinners = new[]
        {
            new[] { "|", "/", "-", "\\" },
            new[] { "←", "↖", "↑", "↗", "→", "↘", "↓", "↙" },
            new[] { "⣾", "⣽", "⣻", "⢿", "⡿", "⣟", "⣯", "⣷" },
            new[] { "⠁", "⠂", "⠄", "⡀", "⢀", "⠠", "⠐", "⠈" },
            new[] { "◐", "◓", "◑", "◒" },
            new[] { "◴", "◷", "◶", "◵" },
            new[] { "◰", "◳", "◲", "◱" },
            new[] { "◢", "◣", "◤", "◥" },
            new[] { "┤", "┘", "┴", "└", "├", "┌", "┬", "┐" },
            new[] { "⠋", "⠙", "⠹", "⠸", "⠼", "⠴", "⠦", "⠧", "⠇", "⠏" },
        };

        private string[] steps;

        private int step;

        private bool messageUpdated = false;

        private string message = string.Empty;

        private Random random = new Random();

        public Spinner(string message)
            : base(200)
        {
            this.steps = this.spinners[0];
            this.Message = message;
        }

        public string Message
        {
            get => this.message;
            set
            {
                this.message = value;
                this.messageUpdated = true;
            }
        }

        internal override void UpdateUI(object? state)
        {
            if (this.messageUpdated)
            {
                this.steps = this.spinners[this.random.Next(this.spinners.Length)];
                this.Write(string.Empty.PadRight(Console.WindowWidth - 1, ' '));
                var existingColour = Console.ForegroundColor;
                Console.ForegroundColor = ConsoleColor.DarkBlue;
                this.Write(this.Message);
                Console.ForegroundColor = existingColour;
                this.messageUpdated = false;
            }

            this.step++;
            if (this.step >= this.steps.Length)
            {
                this.step = 0;
            }

            this.Write(this.steps[this.step], horizontalOffset: this.Message.Length + 1);
        }

        internal void Done()
        {
            this.DoneEvent?.Set();
        }
    }
}
