namespace NeuroOCR
{
    public struct OCRPair {
        public int w;
        public string c;
        public OCRPair(int a, string b) {
            w = a;
            c = b;
        }
        public static OCRPair operator +(OCRPair a, OCRPair b) {
            if (b.w > a.w)
            {
                return b;
            }
            else {
                return a;
            }
        }
    }
}
