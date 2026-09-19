# Survivor arayüzü ve bölgesel düşman grupları

19 Eylül 2026. Mevcut 20 silahlı sürüm üzerine uygulanır.

## Arayüz

- Seviye atlama artık mağaza yerleşimini kullanmaz. Dört büyük kart; nadirlik çerçevesi, stat simgesi, büyük `+bonus`, mevcut/yeni değer ve kısa etki açıklaması taşır. Bekleyen seçim sayısı, otomatik karakter gelişimi, altın ve yenileme ücreti ayrı gösterilir.
- Önizleme gerçek stat hesabını kullanır; kritik/zırh/hız sınırları ve yüksek saldırı hızının azalan getirisi değerlerde görünür. Seçim ve yenileme mevcut kayıt/ekonomi akışından geçer.
- Savaş HUD'ı arka plan kutusu kullanmaz: solda can değeri ve ince can çizgisi, merkezde dalga/süre, sağda para ve yatay XP çizgisi vardır. Boss için ayrı ince can çizgisi açılır. Okunurluk için yazılarda ince koyu kontur kullanılır. Karakter üstündeki tekrar eden ad/can/şarjör göstergesi hayatta kalma modunda gizlenir.
- Parlak mavi düğmelerin yerini düz koyu yüzeyler, yeşil vurgu ve daha sade Sen yazı tipi alır. Joystick ve dash aynı paleti kullanır; menülerde savaş kontrolleri gizlenir. Sen fontu mevcut proje kaynağından alınmıştır; OFL lisansı yanında tutulur.

## AI davranışları

| Rol | Davranış |
|---|---|
| Takipçi | Oyuncuya küçük yan açıyla yaklaşır |
| Kuşatıcı | Uzakta sağ/sol kanat hedefi seçer, yakında doğrudan darbeye girer |
| Yol kesici | Oyuncunun ölçülen hareketine göre önünü kesmeye çalışır; tahmin en çok 2,4 m |
| Menzilli | 4,2 m altında geri çekilir; açık atışta 4,2–7,2 m mesafeyi korur; siper varsa açı değiştirir |

Hedefler NavMesh üzerinde doğrulanır; ulaşılamayan taktik konumda oyuncunun erişilebilir bölgesine dönülür. Yol kararları düşman başına yaklaşık 0,32–0,53 saniyede bir dağıtılır. Mevcut darbe, sendeleme ve sabit hedefli saldırı uyarıları korunur. Ek hasar veya can artışı bu güncellemenin AI değişikliği değildir.

## Bölgesel spawn

Sekiz çevre bölgesi dönüşümlü kullanılır. Normal grup büyüklüğü dalgaya göre 3 → 5, grup aralığı 2,4 → yaklaşık 1,17 saniyedir. Dalga açılışı ve takviyeler birden fazla bölgeye bölünür. Her hücrede tek renk, dolan dairesel uyarı yaklaşık 0,95 saniye önceden görünür; grup üyeleri 0,08 saniye arayla belirir. Önceki çarpı simgeleri kaldırılmıştır.

Planlama sırasında oyuncuya 4,25 m'den yakın, engelli, dolu veya yolu olmayan hücre reddedilir. Doğma anında 3,5 m güvenlik mesafesi ve boşluk tekrar kontrol edilir. Canlı ve bekleyen düşmanlar ortak dalga sınırına dahildir; en fazla 24 bekleyen işaret ve 60 canlı düşman vardır. Duraklatma süreyi dondurur; dalga sonu bütün bekleyen doğmaları iptal eder.

## Doğrulama

`tools/validate_equipment_offline.py`: runtime/Editor derlemesi ve **268 model kontrolü**. Yeni kontroller yan kuşatma, kestirme hedefi, menzil koruma, spawn sınırları ve üç karakterin tüm stat önizlemelerini kapsar.

`Woolly > Review Survivor UI And Squads`: gerçek UI seçim/yenileme düğmeleri, joystick üzerinde EventSystem dokunma girdisi, 16:9 / 20:9 / 4:3 render'ları; farklı bölgelerden grup doğması, duraklatma, nüfus sınırı, canlı kuşatma hedefi, gerçek NavMesh geri çekilmesi ve normal bir dalganın ödül ekranına ulaşması.

Yerel çıktılar `WoollyArenaTest/Logs/survivor-runtime-review.txt` ve `survivor-*.png`. Entegrasyon dalgasında oyuncu hasar koruması kullanılır; bu kontrol zorluğu kazanma testi değildir. Telefon performansı ve insanın oynadığı tam koşu doğrulanmadı. Arsenal notlarındaki önceki süreler, bu yeni spawn temposunun denge ölçümü olarak kullanılmamalıdır.

Arka plansız HUD revizyonundan sonraki tam sahne kontrolü hatasız geçti. Ayrıca `Logs/arsenal-pressure-review.txt` içinde yeni AI/spawn temposuyla ayrı baskı kontrolü tamamlandı:

| Kontrol | Son ölçüm |
|---|---|
| Altı kademe-IV seri tüfek, +%300 ham hız, +%100 hasar, +25 kritik; sabit final boss hedefi | 17,35 sn'de öldü; beş saniye sınamasını yaşadı |
| 16. dalga, altı kademe-III seri tüfek, +%120 ham hız, +%60 hasar, +10 kritik; 100 HP ve ek zırh yok; hareketsiz | 14,04 sn'de yenildi |
| Aynı build ve 35 saniyelik dalga; gerçek joystick girdisi ve dash kullanan otomatik kaçış rotası | Dalga temizliğine 47 HP ile ulaştı; 167,68 m hareket |

Son iki baskı kontrolünde açılıştaki normal bir saniyelik koruma dışında test hasar koruması yoktur. Bunlar tek bir otomatik kontrolün ölçümleridir; bütün build'ler veya tam koşu dengesi için garanti değildir. Mobil dokunma girdisi Editor'de sınanmıştır, fiziksel telefonda değil.
