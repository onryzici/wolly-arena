# Brotato araştırması ve Woolly denge revizyonu

18 Eylül 2026. Kapsam: standart tek oyunculu koşu; Endless ayrı ele alındı. Resmî Steam açıklaması temel oyun döngüsünü doğruluyor; ayrıntılı formüller topluluk Wiki'sinden. Wiki sayfalarındaki boss HP rakamları birbiriyle tutarlı değil; bu nedenle o rakamlar oyuna kopyalanmadı.

## Brotato nasıl bitiyor?

Standart koşu 20 dalga. Süreler 20 saniyeden başlayıp her dalga 5 saniye artarak 9. dalgada 60 saniyeye ulaşıyor; 9–19 arası 60, final 90 saniye. Dalga sonunda sandık ödülleri, kazanılmış seviye seçimleri ve mağaza geliyor. [Dalgalar](https://brotato.wiki.spellsandguns.com/Waves)

20. dalgada bütün bossları öldürmek koşuyu hemen bitiriyor. Bosslar yaşasa bile süreyi sağ tamamlamak da zafer. Danger 0–4 tek boss, Danger 5 iki boss kullanıyor. Bu, saldırı ve hayatta kalma odaklı iki farklı kazanma yolu sunuyor. [Predator](https://brotato.wiki.spellsandguns.com/Predator), [Invoker](https://brotato.wiki.spellsandguns.com/Invoker)

Endless seçilirse koşu 20'den sonra devam ediyor; 20. dalgada bossu erken öldürmek süreyi bitirmiyor. 20'yi geçen koşu sonradan ölse de galibiyet sayılıyor. 20 sonrasında hasadın bileşik büyümesi duruyor ve hasat her dalga %20 azalıyor. Bizde bu revizyonda Endless eklenmedi. [Endless](https://brotato.wiki.spellsandguns.com/Endless_Mode)

## Güç artışı nasıl kontrol ediliyor?

- En fazla altı silah; otomatik ateş ve dalgalar arası alışveriş temel döngü. [Resmî açıklama](https://store.steampowered.com/app/1942280/Brotato/)
- Mağazada dört teklif var. Yenileme ücretli ve aynı mağazada tekrarlandıkça pahalanıyor. Fiyatın dalga bileşeni `base + wave + 0.1 × base × wave`; nadirlik zamanla açılıyor. Aynı silahın eş seviyeleri birleştirilebiliyor. Bu oyunda tüm fiyatları aynen almak doğru olmaz: düşman yoğunlukları farklı. [Mağaza](https://brotato.wiki.spellsandguns.com/Shop)
- XP eşikleri kare biçiminde büyüyor; Wiki tablosunun ilk eşikleri 16, 25, 36, 49. Sayfadaki seviye numaralandırması tutarsız olduğundan formülü doğrudan aktarmak yerine bu artış ilkesi kullanıldı. Her seviye dalga sonunda bir stat seçimi veriyor. [XP](https://brotato.wiki.spellsandguns.com/Experience)
- Düşük kaliteli seviye seçeneklerinde hasar %5, saldırı hızı %5, hareket %3, zırh 1 gibi küçük artışlar var; daha yüksek nadirlikte miktarlar büyüyor. Bizde başlangıçtan itibaren tüm seçimlerin büyük olması bedelsiz gücü artırıyordu. [Yükseltmeler](https://brotato.wiki.spellsandguns.com/Upgrades)
- Hasat hem para hem XP üretip dalga sonunda %5 büyüyebiliyor. Bunu bizim daha küçük ekonomiye aynen taşımadık; bizde hasat yalnız para vermeye devam ediyor. [Hasat](https://brotato.wiki.spellsandguns.com/Harvesting)
- Brotato'da iyileştiren yiyecekler var. Bizde can düşüşünün kaldırılması kullanıcının istediği özel tasarım kararı; Brotato'nun kuralı değil. [Tüketilebilirler](https://brotato.wiki.spellsandguns.com/Consumable)
- Düşmanlar yalnızca HP ile zorlaşmıyor: yaklaşan, uzaktan atan, hücum eden ve destek veren tipler var. Bizdeki sürekli sendeleme kilidi önce giderildi. Yeni düşman tipleri bu değişikliğin kapsamına eklenmedi. [Düşmanlar](https://brotato.wiki.spellsandguns.com/Enemies)

## Mevcut oyundaki sorun

Eski kodda her öldürme 3 altın; her 18 öldürmede garantili sandık ve diğer öldürmelerde %1,8 sandık olasılığı vardı. İlk dalga sandığı 14 altın + 3 XP + 5 can veriyordu. Ayrıca %12 ihtimalle 8 can düşüyordu. İlk XP eşikleri 5, 8, 11, 14 idi.

**Varsayımsal** ilk dalgada 40 öldürme için: beklenen sandık `2 + 38 × 0.018 = 2.684`; altın `120 + 2.684 × 14 + 5 hasat = 162.576`. Beklenen iyileşme kapasitesi `40 × .12 × 8 + 2.684 × 5 = 51.82`; bu, gerçekten iyileşen miktar değildir, can doluluğunu hesaba katmaz. Sandık XP'si hariç bile 40 XP dört seviye seçimi veriyordu.

Ayrı bir sorun: her isabet 0,16 saniyelik geri tepmeyi yeniliyor ve saldırıyı en az 0,22 saniye ileri atıyordu. 0,14 saniyelik seri tüfek isabetleri düşmanın hem yürümesini hem saldırmasını sürekli durdurabiliyordu. Dolayısıyla yalnız düşman HP'sini yükseltmek yeterli değildi.

## Uygulanan ilk denge

| Alan | Yeni davranış |
|---|---|
| Can ödülleri | Normal/boss can düşüşü yok; sandık can veya XP vermiyor |
| Altın | Düşen para 1 altın; ilk üç dalga %100, sonra dalga başına 1,5 puan azalarak %75 tabanına iniyor |
| Normal sandık | %1 olasılık, dalga başına en fazla 1; `6 + floor(wave/2)` altın |
| Boss ödülü | Ayrı garantili sandık, `10 + wave` altın |
| XP | Mevcut seviye için `n = level−1`; eşik `16 + 6n + floor(n²/2)` |
| Fiyat | `ceil(base × tier × 1.35) + 2 × wave` |
| Yenileme | `4 + wave + rerolls × max(3, floor(wave/3))` |
| Seviye seçimi | Can 8, zırh 1, yenilenme 1, hasat 3, kritik %3, hareket %4, hasar/saldırı hızı %5 |
| Bazı pasifler | Yelek canı 20→12, bot hareketi %12→%8, dürbün kritiği %10→%7, tetik/barut %15→%10, plaka zırhı 4→3 |
| Normal düşman | Eski HP tabanına ilk beş dalga ×1,2; sonrasında dalga başına +0,14; temas hasarı +2 |
| Sendeleme | Normal düşmanda en fazla 0,45 saniyede bir; bossta 0,9 saniyede bir saldırı kesintisi. Diğer isabetler hasar ve görsel tepki vermeyi sürdürür |
| Boss HP | 5/10/15/20: 1.800 / 6.300 / 13.800 / 24.300 |
| Final | 90 saniye; boss öldüğünde veya süre sağ tamamlandığında zafer |

Dalgalar 1–19 kendi 30–44 saniyelik temposunu koruyor. Ara bossların öldürülmesi gerekiyor; süre dolunca sıradan düşmanların gelmesi artık durmuyor. Böylece zamanın dolması ücretsiz, baskısız boss düellosuna dönüşmüyor. Finalde erken boss zaferi ayrı kural.

Yenilenme statı, maksimum can artışının mevcut etkisi ve sonraki dalgaya geçişte 10 can iyileşmesi korundu. Altın dalga sonunda toplanmaya devam ediyor. Eski kayıtlar silinmedi; önceden kazanılmış yüksek seviyeler ve stat bonusları geri alınmadığı için yeni ekonominin değerlendirmesi **yeni koşuda** yapılmalı. Katalogdaki pasif değerleri mevcut kayıttaki eşyalara da uygulanır.

## Çevrimdışı ölçüm ve sınırlar

Üretimdeki C# mağaza/XP/kayıt kodu doğrudan .NET altında çalıştırıldı; Unity Editor açılmadı. `python3 tools/validate_equipment_offline.py` runtime ve Editor C# kaynaklarını derler, 101 kontrolü çalıştırır. Sonuçlar `WoollyArenaTest/Logs/equipment-offline-review.txt` içinde.

40 öldürmeli ilk dalgada yeni beklenen altın `40 + (1−.99^40) × 6 + 5 = 46.99`: yaklaşık %71 daha az gelir. XP iki seviye seçimi verir. İlk mağazada en ucuzdan alıp yenileyen algoritmanın 1.000 farklı tohumdaki sonuçları:

| Varsayımsal ilk dalga öldürmeleri | Alışveriş aralığı | Ortalama |
|---|---:|---:|
| 20 | 1–2 | 1,01 |
| 40 | 2–3 | 2,04 |
| 60 | 2–4 | 3,02 |

Ayrıca üç sabit öldürme senaryosu, her biri 100 farklı tohumla 19 dalga boyunca çalıştırıldı. Bunlar oyun oynayan botlar değildir: öldürmeler girdi, seviye seçimi rastgele, mağaza tercihi silah öncelikli ve en fazla bir yenileme. Final öncesi seviye sırasıyla 11 / 13 / 15, pasif sayısı ortalama 9 / 14,6 / 20,8 çıktı. İdeal tek hedef DPS yaklaşık 759 / 1.020 / 1.224; final bossunun nominal dayanma süresi yaklaşık 32 / 24 / 20 saniye. Tüm saçmaların isabet ettiği, menzil/duvar/kaçınma kaybı olmadığı varsayılır; gerçek öldürme süreleri değildir.

Bu testler fiyat/XP sınırları, kayıt bütünlüğü ve final koşullarını doğrular; hareketsiz kazanmanın tamamen bittiğini veya aktif oynanışın adil olduğunu kanıtlamaz. Unity Play, tam koşu ve telefon testi yapılmadı. Sonraki görsel/oynanış kontrolünde aynı tohumlarla hareketsiz ve aktif oyun; dalga başına öldürme, alınan hasar, harcanan altın, silah sayısı ve boss süresi karşılaştırılmalı. Ölçüm öncesinde ikinci bir toplu stat artırımı yapılmamalı.
