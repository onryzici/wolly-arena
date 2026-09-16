# Woolly Arena

Yatay mobil ekran için Unity arena shooter prototipi.

## Açılış
Unity Hub ile `WoollyArenaTest` klasörünü açın. Unity sürümü: **6000.5.6f1**. Başlangıç sahnesi: `Assets/Woolly/Scenes/Lobby.unity`.

Lobide **OYNA** test arenasına geçer; arenadaki **LOBİ** düğmesi geri döner. Mağaza, görev ve sosyal menüler prototip bilgi panelleridir; gerçek satın alma veya çevrimiçi servis içermez.

## Kontroller
- Mobil: sol kontrol hareket/koşu; sağ düğmeye dokunma veya basılı tutma karakterin baktığı yöne ateş eder. Sağ düğmeyi sürüklemek yön değiştirmez.
- Masaüstü: WASD, Shift, fare ile nişan/ateş, R ile doldurma.
- Sekiz mermi; şarjör boşaldığında otomatik doldurma.

Dört düşman NavMesh üzerinde farklı saldırı konumlarına ilerler; siper, görüş hattı ve birbirleriyle mesafeyi dikkate alır. Mermi izleri, namlu parlaması, çarpma parçacıkları, ayak izi ve toz efektleri havuzlanır.

## iOS
IL2CPP / ARM64 / Metal, yatay yön, iOS 15+. `Lobby` ve `TrainingArena` sahnelerini build listesine ekleyin. `Builds/iOS` çıktısını Xcode ile derleyin; kendi Apple geliştirme takımınızı seçin. Derleme çıktıları ve imzalama dosyaları repoya dahil değildir.

## Kaynaklar
- Düzenlenebilir karakter: `art/woolly/Woolly_Unity_AnimationSource_v4.blend`.
- UI parçaları: kullanıcı tarafından sağlanan GUI Pro-SuperCasual paketi. Paket varlıkları kendi lisanslarına tabidir.
- Lilita One ve Liberation Sans fontlarının lisansları ilgili Assets klasörlerindedir.
- Lobi düzeni: `Assets/Woolly/Editor/LobbySetup.cs`; kaydedilmiş Canvas sahnede düzenlenebilir.
