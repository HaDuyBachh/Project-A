using UnityEngine;

namespace HaDuyBach
{

    public class ListItem : MonoBehaviour
    {
        private readonly static ListItem I = new(new Db[4] {
        // Item và description ở đây 
             /*new Db(
                new Item( Loại , index , số lượng giới hạn trên 1 ô, tốc độ thực hiện mỗi lần tấn công ),
                new string("Tên \n Mô tả")
            )*/
            //0
            new Db(
                new Item(),
                new string("")
            ),
            //1 
            new Db(
                new Item(Item.Type.pistol,1,1,1.0f,12,10,new RecoilValue(-6f,1f,0.35f,20f,24.0f)),
                new string("Súng Lục \n - Một loại súng cầm tay với sát thương thấp tầm bắn hạn chế, nhưng nhỏ gọn và ổn định.")
            ),
            //2
            new Db
            (
                new Item(Item.Type.rifle,2,1,0.1f,4,2,new RecoilValue(-2f,1f,0.10f,2.0f,20.0f)),
                new string("AK47 \n - Một loại súng tiểu liên độ có sát thương tốt nhưng chất lượng súng chỉ ở mức trung bình.")
            ),
            //3
            new Db
            (
                new Item(Item.Type.melee,3,1,1,30),
                new string("CopBaton \n - Một loại vũ khí của cảnh sát, sát thương tầm trung, tốc độ đánh trung bình.")
            ),
    });

        private class Db
        {
            public Db(Item I, string dscr)
            {
                this.item = I;
                this.dscr = dscr;
            }
            public Item item;
            public string dscr;
        }

        private readonly Db[] i;

        private ListItem(Db[] I)
        {
            i = I;
        }

        static public Item getItem(int index)
        {
            if (index <= 0) Debug.LogWarning("Đã lấy đối tượng <new Item()>");

            if (I.i[index].item.index != index)
            {
                Debug.Log("Có vẻ đây không phải là vũ khi nên " + index + "khác với thứ tự trong ListItem");
            }    

            return I.i[index].item;
        }
        static public string getDscr(int index)
        {
            return I.i[index].dscr;
        }
    }

}