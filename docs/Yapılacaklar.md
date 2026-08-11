# Yapılacaklar

- **Kullanıcı Yönetimi** 
    * UserName ve Name alanları açılacak
    * Girişler UserName üzerinden yapılacak

- **Eski Veriler** 
    * Pasif teknelere gerek var mı?
    * Tüm Kişiler ve Araçlar aktarılacak mı?
    * Cekek giriş bilgileri hangi yıllar olacak?
    * Db reset işlemi (eski verileri silme, Identity 1 den başlatma)
    * Eski tablolardan aktarım scriptleri hazırlanacak

- **Raporlar**
    * CekekTakip Raporu yapılacak, 
    * Güvenlik rolü rapor yetkisi olmayacak
    * Rapor için veri tabanında view oluşacak rapor viewden çekecek
    * CekekTakipleri tablosuna kişi ve araç bilgileri join yapılacak
    * Raporda her sütun için filtreleme olacak, metin ve sayısal filtreleme operatörlerini destekleyecek
    * Filtreler isim verilerek kaydedilebilecek
    * Excel export özellikli olacak