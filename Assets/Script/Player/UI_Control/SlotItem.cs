namespace HaDuyBach
{
    public class SlotItem
    {
        public int number;
        public Item item;
        public SlotItem()
        {
            item = new Item();
            number = 0;
        }
        public bool empty()
        {
            return number == 0;
        }
        public bool Add(Item I, ref int n)
        {
            //Không nhận idex == 0
            if (I.index == 0) return false;

            bool kt = false;
            if (empty()) item = I;
            if (number + n >= I.limitPerSlot)
            {
                if (I.limitPerSlot - number > 0) kt = true;
                n -= I.limitPerSlot - number;
                number = I.limitPerSlot;
            }
            else
            {
                number += n;
                n = 0;
                kt = true;
            }
            return kt;
        }
    }

}
