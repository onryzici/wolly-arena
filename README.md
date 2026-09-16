# Woolly Arena

Yatay mobil ekran için Unity arena shooter prototipi.

## Açılış
Unity Hub ile `WoollyArenaTest` klasörünü açın. Unity sürümü: **6000.5.6f1**. Başlangıç sahnesi: `Assets/Woolly/Scenes/Lobby.unity`.

Lobide **OYNA** test arenasına geçer; arenadaki **LOBİ** düğmesi geri döner. Mağaza, görev ve sosyal menüler prototip bilgi panelleridir; gerçek satın alma veya çevrimiçi servis içermez.

## Kontroller
- Mobil: sol kontrol hareket/koşu; sağ düğmeye dokunma veya basılı tutma karakterin baktığı yöne ateş eder. Sağ düğmeyi sürüklemek yön değiştirmez.
- Masaüstü: WASD, Shift, fare ile nişan/ateş, R ile doldurma, **Space ile dodge/takla**.
- Mobil: sağdaki çift ok düğmesi dodge yapar. Hareket varsa o yöne, dururken karakterin baktığı yöne kaçılır. Düğmenin halkası ve süre göstergesi yeniden hazır olmayı gösterir.
- Sekiz mermi; şarjör boşaldığında otomatik doldurma.

Dört düşman NavMesh üzerinde farklı saldırı konumlarına ilerler; siper, görüş hattı ve birbirleriyle mesafeyi dikkate alır. Mermi izleri, namlu parlaması, çarpma parçacıkları, ayak izi ve toz efektleri havuzlanır.

İsabet alan karakter kısa süre açık renkte parlar ve darbe yönünde sendeler. Düşman 0,16 saniyede yaklaşık 0,38 m, oyuncu 0,16 m geri itilir; siperler ve gezinme sınırları bu mesafeyi kısaltır. `EnemyAgent.hitPushDistance` ve `ArenaPlayer.hitPushDistance` ile mesafe, `HitReaction` üzerinden parlama/sendeleme süresi ayarlanabilir. Koruma süresindeki veya hasar vermeyen vuruşlar tepki oluşturmaz.

`Woolly > Review Hit Reactions` gerçek atış, geri itme, siper ve NavMesh sınırı, peş peşe isabet, koruma, ölüm ve yeniden doğma kontrollerini çalıştırır. Sonuç `Logs/hit-reaction-review.txt` dosyasına yazılır; ardından temiz arena Play modunda açılır.

Dodge 0,52 saniyede 2,25 m ilerler; bekleme süresi başlangıçtan itibaren 1,1 saniyedir. İlk 0,28 saniyede hasar alınmaz, son toparlanma anlarında tekrar hasar alınabilir. Takla sırasında ateş kesilir, şarjör doldurma devam eder; karakter siperlerin içinden geçmez. `DodgeAbility` üzerindeki mesafe, süre, bekleme ve koruma alanları ayarlanabilir. `Animations/Dodge Roll.anim`, çömelme, diz/kolları toplama, omuz üzerinden dönme ve ayağa kalkma pozları içeren bir iskelet animasyonudur; model ölçeği değiştirilmez. `Woolly > Build Dodge Animation` düzenlenebilir klibi yeniden üretir. `Woolly > Review Dodge` klavye, mobil çoklu dokunma, yön/mesafe, siper, koruma ve silah etkileşimlerini Play modunda doğrular; sonuç `Logs/dodge-review.txt` dosyasındadır.

## iOS
IL2CPP / ARM64 / Metal, yatay yön, iOS 15+. `Lobby` ve `TrainingArena` sahnelerini build listesine ekleyin. `Builds/iOS` çıktısını Xcode ile derleyin; kendi Apple geliştirme takımınızı seçin. Derleme çıktıları ve imzalama dosyaları repoya dahil değildir.

## Kaynaklar
- Düzenlenebilir karakter: `art/woolly/Woolly_Unity_AnimationSource_v4.blend`.
- UI parçaları: kullanıcı tarafından sağlanan GUI Pro-SuperCasual paketi. Paket varlıkları kendi lisanslarına tabidir.
- Lobi, paketin `1_Lobby` ekranından yatay ekrana uyarlanmıştır. Kullanılan PNG parçaları `Assets/Woolly/UI/Lobby/SuperCasual` klasöründedir.
- Lobi arka planı: `Assets/Woolly/UI/Lobby/Canyon Lobby.png`, yerleşik imagegen ile üretilmiş yatay kanyon panoraması. Tam üretim promptu yanındaki `Canyon Lobby provenance.txt` dosyasındadır. Shader görselin oranını korur; 16:9, 20:9 ve 4:3 kadrajları doğrulanır.
- [Sen ExtraBold](https://github.com/philatype/Sen), Lilita One ve Liberation Sans fontlarının lisansları ilgili Assets klasörlerindedir.
- Lobi düzeni: `Assets/Woolly/Editor/LobbySetup.cs`; kaydedilmiş Canvas sahnede düzenlenebilir. `Woolly > Rebuild Lobby` yalnızca lobiyi yeniden oluşturur.
- Görsel kontrol: `WoollyArena.Editor.LobbySetup.BuildAndCapture`, 16:9, 20:9 ve 4:3 önizlemelerini `Logs` klasörüne kaydeder. `WoollyArena.Editor.LobbyFlowReview.Run`, ayarlar/ses düğmelerini ve lobi → dört düşmanlı arena → lobi akışını kontrol edip lobiyi Play modunda açık bırakır.
