using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NeuroOCR
{
    class Neuro
    {
        private TableOCR table;
        public String name;
        public Neuro(String str) {
            name = str;
            table = new TableOCR("ocr\\"+name+".ocr");
        }
        public void Clever(TableOCR img, bool isTrue) {
            if (isTrue)
            {
                table.Add(img);
            }
            else {
                table.Delete(img);
            }
            table.Save();
        }
        public OCRPair Work(TableOCR img) {
            return (new OCRPair(table.Multiple(img), name));
        }
        public void Flush() {
            table.Flush();
        }
        public void Hard()
        {
            table.Hard();
        }
        public void Clear() {
            table.Clear();
            table.Save();
        }
    }
}
