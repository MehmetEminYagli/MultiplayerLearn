using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using UnityEngine;
using Unity.Netcode;

public class PlayerNetwork : NetworkBehaviour
{


    //yeni custom dataile çalışan sistem
    private NetworkVariable<MyCustomData> randomNumber = new NetworkVariable<MyCustomData>(
        new MyCustomData
        {
            _int = 56,
            _bool = true,
            
        }, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);

    //NetworkVariable'yi custom data tipleri ile yapma ve INetworkSerializable Kullanımı
    public struct MyCustomData : INetworkSerializable
    //bu kısım böyle çalışmaz. burada istemci işlenen verilerin server üzerinden gideceğine karar veremediği için hata alırsın bu sorunu çözmek için sturct'ı INetworkSerializable yapman lazım.
    {
        public int _int;
        public bool _bool;
        public FixedString128Bytes message;//uyarı mesajı yada bir text göndericeğini düşün o işlerde yarabilir

        public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
        {
            //şimdi verileri buradan gönderip alabilirsin hata almazsın
            serializer.SerializeValue(ref _int);
            serializer.SerializeValue(ref _bool);
            serializer.SerializeValue(ref message);
        }
    }

    //custom data ile çalışan sistem

    public override void OnNetworkSpawn()
    {
        randomNumber.OnValueChanged += (MyCustomData previousValue, MyCustomData newValue) =>
        {
            Debug.Log(OwnerClientId + " " + newValue._int + "; " + newValue._bool + " " + newValue.message);
        };
    }


    #region 
    //eski sadece değer ile çalışan sistem
    //private NetworkVariable<int> randomNumber = new NetworkVariable<int>(1, NetworkVariableReadPermission.Everyone, NetworkVariableWritePermission.Owner);
    //burası mesela her oyuncunun puanını her oyuncu güncel olarak görebilmesi gerekiyor ya o işe yarayan  = den sonra parantez içinde olan
    //kısım everyvone her oyuncu okuyabilir. ikinci kısım herkes kendi sahip olduğu değeri güncelleyebilir yazabilir.


    //böylelikle bu veriyi sürekli çağırıp sistemi boştan yere meşgul etmezsiniz.
    //ağ balantılı nesneler ile çalılırken şöyle yapmanız daha doğru olur
    //public override void OnNetworkSpawn()
    //{
    //    randomNumber.OnValueChanged += (int previousValue, int newValue) =>
    //    {
    //        Debug.Log(OwnerClientId + " randomNumber: " + randomNumber.Value);
    //    };
    //}
    #endregion


    void Update()
    {

        if (!IsOwner) return;

        //customdata ile
        if (Input.GetKeyDown(KeyCode.T))
        {
            randomNumber.Value = new MyCustomData
            {
                _int = 10,
                _bool = false,
                message = "hello world :D",
            };
        }


        //eski
        //if (Input.GetKeyDown(KeyCode.T))
        //{
        //    randomNumber.Value = Random.Range(0, 100);
        //}


        Vector3 moveDir = new Vector3(0, 0, 0);

        if (Input.GetKey(KeyCode.W)) moveDir.z = +1f;
        if (Input.GetKey(KeyCode.S)) moveDir.z = -1f;
        if (Input.GetKey(KeyCode.A)) moveDir.x = -1f;
        if (Input.GetKey(KeyCode.D)) moveDir.x = +1f;

        float moveSpeed = 3f;
        transform.position += moveDir * moveSpeed * Time.deltaTime;
    }
}
