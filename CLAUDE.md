# CLAUDE.md

Bu dosya, Claude Code'un (claude.ai/code) bu repository'de kod üzerinde çalışırken rehberlik sağlar.

## Proje Genel Bakış

Windows Standalone için Unity 6 (6000.0.60f1) 2D oyun projesi. Şu anda kapsamlı plugin altyapısı önceden yapılandırılmış erken şablon aşamasında.

## Build ve Geliştirme

Bu bir Unity projesidir - komut satırı build scriptleri yoktur. Unity Editor'de açın veya şunu kullanın:
```bash
# Komut satırı build (Unity kurulumu gerektirir)
Unity -projectPath . -buildWindowsPlayer Build/Game.exe -quit -batchmode
```

Unity Test Framework `com.unity.test-framework` üzerinden kullanılabilir. Testleri Unity Editor'ün Test Runner penceresinden çalıştırın.

Hot Reload, play modunda hızlı iterasyon için yapılandırılmıştır (`Packages/com.singularitygroup.hotreload/`).

## Mimari

### Temel Sistemler
- **Render Pipeline**: `Assets/Settings/` içinde yapılandırılmış 2D Renderer ile Universal Render Pipeline (URP)
- **Input System**: `Assets/InputSystem_Actions.inputactions` içinde önceden yapılandırılmış aksiyonlarla yeni Input System
  - Oyuncu aksiyonları: Move, Look, Attack, Interact, Crouch, Jump, Sprint, Previous, Next
  - Kontrol şemaları: Keyboard&Mouse, Gamepad, Touch, Joystick, XR

### Entegre Edilmiş Pluginler (`Assets/Plugins/`)

| Plugin | Namespace | Amaç |
|--------|-----------|------|
| Feel | `MoreMountains.Tools` | Oyun hissi, geri bildirimler, görsel efektler, kamera sarsıntısı |
| NiceVibrations | `Lofelt.NiceVibrations` | Mobil haptik geri bildirim |
| Text Animator | `Febucci.UI.Core` | Diyalog/UI için animasyonlu metin efektleri |
| AllIn1SpriteShader | - | Gelişmiş sprite render efektleri |

### Kod Konumları
- `Assets/Scripts/` - Özel oyun kodu (buraya implement edin)
- `Assets/Scenes/SampleScene.unity` - Ana oyun sahnesi
- `Assets/Prefabs/` - Yeniden kullanılabilir oyun objeleri
- `Assets/Sprites/` - 2D grafikler
- `Assets/Sounds/Musics/` - Ses dosyaları

### Temel Bağımlılıklar (UPM)
- Input System 1.14.2
- URP 17.0.4
- Timeline 1.8.9
- Visual Scripting 1.9.7

## Kurallar

- Birincil odağımız performanstır; bellek, CPU ve GPU kullanımını optimize etmeye özel bir vurgu yapmanı istiyorum.
- Sen bir Senior Unity Game Developersın, oyunda yaptığın her yeniliği buna göre yapmanı istiyorum.
- C# scriptleri `Assets/Scripts/` klasörüne koyun
- Eski input sistemini kullan.
- Feel framework geri bildirimler sağlar - oyun hissi için `MMF_Player` componentlerini kullanın
- URP 2D Renderer yapılandırılmıştır - 2D ışıklar ve sprite materyalleri kullanın
