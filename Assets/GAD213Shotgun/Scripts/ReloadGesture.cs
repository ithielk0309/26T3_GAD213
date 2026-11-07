namespace GAD213.Shotgun
{
    public struct ReloadIntent
    {
        public bool Pump;
        public bool ReloadHeld;
    }

    // A single press becomes either a tap OR a hold, never both.
    public sealed class ReloadGesture
    {
        private bool buttonWasPressed;
        private bool holdRecognised;
        private bool waitForRelease;
        private float buttonDownTime;
        public void Reset(bool keyHeld)
        {
            buttonWasPressed = false;
            holdRecognised = false;
            waitForRelease = keyHeld;
        }

        public ReloadIntent Step(bool held, bool blocked, float now, float threshold)
        {
            if (blocked)
            {
                Reset(held);
                return default(ReloadIntent);
            }

            if (waitForRelease)
            {
                if (!held)
                {
                    waitForRelease = false;
                }

                return default(ReloadIntent);
            }

            if (held)
            {
                if (!buttonWasPressed)
                {
                    buttonWasPressed = true;
                    buttonDownTime = now;
                }

                if (now - buttonDownTime >= threshold)
                {
                    holdRecognised = true;
                }

                return new ReloadIntent
                {
                    ReloadHeld = holdRecognised
                };
            }

            bool wasQuickTap = buttonWasPressed && !holdRecognised && now - buttonDownTime < threshold;
            buttonWasPressed = false;
            holdRecognised = false;
            return new ReloadIntent
            {
                Pump = wasQuickTap
            };
        }
    }
}

