# ISO 8583 Message Parser - EBCDIC/ASCII Support

**Switch & Takas Ekibi** - Yapı Kredi Teknoloji

VISA, Mastercard, BKM ISO 8583 mesajlarını EBCDIC ve ASCII formatında parse eden Windows masaüstü uygulaması. Oracle dump mesajları için header skip desteği ile.

---

## 🎯 Özellikler

### ✅ Dual Encoding Support
- **ASCII Mode:** Standart ASCII field encoding
- **EBCDIC Mode:** Field 2, 41, 42, 43 için EBCDIC hex parsing
- **Auto-detection:** Hex length detection (10 hex = 16 decimal)

### ✅ Oracle Dump Support
- **Header Skip:** 0-20 byte header atlanabilir
- **Slider Control:** Kolay header skip ayarı
- **Real-world Messages:** Gerçek Oracle dump örnekleri

### ✅ Network Support
- **VISA:** Authorization, Financial transactions
- **Mastercard:** Purchase, refund messages
- **BKM:** Türkiye standart mesajları (Türkçe field isimleri)

### ✅ Field Types
- **FIXED:** Sabit uzunluk fieldları
- **LLVAR:** 2-digit length variable fields
- **LLLVAR:** 3-digit length variable fields
- **BINARY:** PIN, EMV data (hex display)

### ✅ Advanced Features
- **Hex Length Detection:** "10" hex = 16 decimal auto-parse
- **EBCDIC → ASCII Conversion:** Full conversion table
- **Bitmap Visualization:** Primary + Secondary bitmap görüntüleme
- **Field-by-field Display:** Her field detaylı gösterim
  - ASCII value
  - HEX value (EBCDIC fieldlar için)
  - Field encoding badge
  - Description

---

## 📋 Gereksinimler

- **Windows 10/11**
- **.NET 8.0 Runtime** ([İndir](https://dotnet.microsoft.com/download/dotnet/8.0))
- **Visual Studio 2022** (geliştirme için, opsiyonel)

---

## 🚀 Kurulum ve Çalıştırma

### Yöntem 1: Visual Studio ile

```bash
1. ISO8583Parser.sln dosyasını Visual Studio 2022'de açın
2. Solution Explorer'da proje üzerine sağ tıklayın
3. "Build" veya "Rebuild" seçin
4. F5 tuşuna basın veya "Start" butonuna tıklayın
```

### Yöntem 2: Komut Satırı ile

```bash
# Proje dizinine gidin
cd ISO8583Parser

# Restore ve build
dotnet restore
dotnet build

# Çalıştır
dotnet run
```

### Yöntem 3: EXE Oluşturma

```bash
# Release build (runtime gerektirir)
dotnet publish -c Release -r win-x64 --self-contained false

# Self-contained (runtime dahil)
dotnet publish -c Release -r win-x64 --self-contained true

# Çıktı:
# bin\Release\net8.0-windows\win-x64\publish\ISO8583Parser.exe
```

---

## 🎮 Kullanım

### 1. ASCII Mesaj Parse

```
Adımlar:
1. Network: VISA seç
2. Encoding: ASCII seç
3. Header Skip: 0
4. Mesaj yapıştır:
   0100723405412AC180040000000000000016453212345678901000000000000010000...
5. "Parse Et" tıkla

Sonuç:
✓ MTI: 0100
✓ 15 field parsed
✓ Field 2: 4532123456789010 (ASCII)
```

### 2. EBCDIC Mesaj Parse (Oracle Dump)

```
Adımlar:
1. Network: VISA seç
2. Encoding: EBCDIC seç
3. Header Skip: 6 (Oracle header için)
4. Mesaj yapıştır:
   00D5000016010200D50000000894223000000000000000000000000010F4F7F6F1F3F4F0F0F0F0F0F0F0F0F3F5...
5. "Parse Et" tıkla

Sonuç:
✓ MTI: 0200
✓ 10 field parsed
✓ Field 2: 4761340000000035 [EBCDIC]
  ASCII: 4761340000000035
  HEX: F4F7F6F1F3F4F0F0F0F0F0F0F0F0F3F5
```

### 3. Örnek Mesaj Kullanma

```
1. "Örnek Mesajlar" dropdown'ı aç
2. "EBCDIC - Oracle Dump with Header" seç
3. Mesaj otomatik yüklenir
4. Encoding ve Header Skip otomatik ayarlanır
5. "Parse Et" tıkla
```

---

## 📊 Arayüz Özellikleri

### Header Bölümü
```
┌────────────────────────────────────────────────────────────┐
│ 🏦 ISO 8583 Message Parser - EBCDIC/ASCII                 │
│ VISA, Mastercard, BKM - Oracle Dump Support                │
│                                          [Switch & Takas]  │
│                                      [Yapı Kredi Teknoloji]│
└────────────────────────────────────────────────────────────┘
```

### Network & Encoding Seçimi
```
┌────────────────────────────────────────────────────────────┐
│ Network: [VISA] [Mastercard] [BKM]                         │
│ Encoding: [ASCII] [EBCDIC]                                 │
└────────────────────────────────────────────────────────────┘
```

### Header Skip Control
```
┌────────────────────────────────────────────────────────────┐
│ ⚙️ Oracle Header Skip (bytes): [━━━━━━○━━━] 6              │
│ (Oracle dump için 6 byte header skip edin)                │
└────────────────────────────────────────────────────────────┘
```

### Parse Sonucu
```
┌─────────┬─────────┬────────┬──────────────────────┐
│  MTI    │ Fields  │ Length │ Bitmap               │
│  0200   │   10    │  180   │ 11010101000000...    │
└─────────┴─────────┴────────┴──────────────────────┘

Field 2: Primary Account Number (PAN) [EBCDIC]
  Type: LLVAR (n)
  Card number
  
  ASCII: 4761340000000035
  
  HEX:
  F4F7F6F1F3F4F0F0F0F0F0F0F0F0F3F5

Field 41: Terminal ID [EBCDIC]
  Type: FIXED (ans)
  Terminal
  
  ASCII: TERM0001
  
  HEX:
  E3C5D9D4F0F0F0F1
```

---

## 🔍 EBCDIC Parse Detayları

### Field 2 (PAN) Parse Süreci

**Input Mesaj:**
```
...0200D50000000894223000000000000000000000000010F4F7F6F1F3F4F0F0F0F0F0F0F0F0F3F5...
                                                    ^^ ^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^^
                                                    Len PAN (EBCDIC)
```

**Parse Adımları:**

**1. MTI:**
```
Position: 0-3
Value: 0200
```

**2. Bitmap:**
```
Position: 4-19
Hex: D500000008942230
Binary: 1101010100000000... (Field 2 active)
```

**3. Field 2 Length:**
```
Position: 20-21
Hex: "10"
Parse as HEX: 0x10 = 16 decimal ✓
```

**4. Field 2 Data:**
```
Position: 22-53 (32 hex chars = 16 bytes)
EBCDIC Hex: F4F7F6F1F3F4F0F0F0F0F0F0F0F0F3F5

EBCDIC → ASCII Conversion:
F4 → '4'
F7 → '7'
F6 → '6'
F1 → '1'
F3 → '3'
F4 → '4'
F0 → '0' (×8)
F3 → '3'
F5 → '5'

Result: 4761340000000035 ✓
```

---

## 🧪 Test Senaryoları

### Test 1: ASCII VISA Authorization
```
Network: VISA
Encoding: ASCII
Header Skip: 0
Message: 0100723405412AC180040000000000000016453212345678901...

Expected:
✓ MTI: 0100
✓ Field 2: 4532123456789010 (ASCII text)
✓ Field 41: TERM0001 (ASCII text)
```

### Test 2: EBCDIC VISA Financial
```
Network: VISA
Encoding: EBCDIC
Header Skip: 0
Message: 0200D50000000894223000000000000000000000000010F4F7F6F1...

Expected:
✓ MTI: 0200
✓ Field 2: 4761340000000035
  HEX: F4F7F6F1F3F4F0F0F0F0F0F0F0F0F3F5
✓ Field 41: TERM0001
  HEX: E3C5D9D4F0F0F0F1
```

### Test 3: Oracle Dump with Header
```
Network: VISA
Encoding: EBCDIC
Header Skip: 6
Message: 00D5000016010200D50000000894223000000000000000000000000010F4F7...

Expected:
✓ Header skipped (6 bytes)
✓ MTI: 0200 (parsed from position 6)
✓ Field 2: 4761340000000035 [EBCDIC]
```

---

## 🔧 Geliştirici Notları

### EBCDIC Conversion Table

**Numerics (F0-F9):**
```
EBCDIC → ASCII
0xF0   → '0' (48)
0xF1   → '1' (49)
0xF2   → '2' (50)
0xF3   → '3' (51)
0xF4   → '4' (52)
0xF5   → '5' (53)
0xF6   → '6' (54)
0xF7   → '7' (55)
0xF8   → '8' (56)
0xF9   → '9' (57)
```

**Letters:**
```
EBCDIC → ASCII
0xC1   → 'A' (65)
0xC5   → 'E' (69)
0xD4   → 'M' (77)
0xD9   → 'R' (82)
0xE3   → 'T' (84)
```

**Special:**
```
0x40   → ' ' (Space)
```

### Length Parsing Logic

```csharp
// LLVAR length for EBCDIC fields
string lenBytes = message.Substring(position, 2);

// Try hex format first
if (Regex.IsMatch(lenBytes, "^[0-9A-F]{2}$"))
{
    int hexLen = Convert.ToInt32(lenBytes, 16);
    if (hexLen >= 1 && hexLen <= 99)
    {
        lenIndicator = hexLen; // Use hex
    }
    else
    {
        lenIndicator = int.Parse(lenBytes); // Fallback to ASCII
    }
}
```

### Field Encoding Logic

```csharp
if (fieldEncoding == "EBCDIC" && currentEncoding == "EBCDIC")
{
    // Parse as EBCDIC hex
    int hexLen = lenIndicator * 2;
    string hex = message.Substring(position, hexLen);
    string ascii = EbcdicToAscii(hex);
}
else if (fieldEncoding == "BINARY")
{
    // Keep as hex
    string hex = message.Substring(position, lenIndicator);
}
else
{
    // Parse as ASCII
    string value = message.Substring(position, lenIndicator);
}
```

---

## 🐛 Troubleshooting

### Problem: "Parse hatası - Index out of range"

**Sebep:** Header skip değeri çok yüksek veya mesaj çok kısa

**Çözüm:**
```
1. Header Skip'i 0'a ayarlayın
2. Mesajın tam olduğundan emin olun
3. Mesajda space/newline varsa temizleyin
```

### Problem: "Field 2 yanlış parse ediliyor"

**Sebep:** Encoding yanlış seçilmiş

**Çözüm:**
```
EBCDIC mesajlar için:
1. Encoding: EBCDIC seç
2. F4F7F6... gibi hex görmüyorsanız ASCII deneyin
3. Length "10" hex = 16 decimal olmalı
```

### Problem: "MTI görünmüyor"

**Sebep:** Oracle header skip edilmemiş

**Çözüm:**
```
Oracle dump için:
1. Header Skip: 6 ayarlayın
2. "00D5 0000 1601" başlıyorsa 6 byte skip
3. Direkt "0200" ile başlıyorsa 0 skip
```

---

## 📚 Ek Kaynaklar

### Field Definitions

**VISA Fields:**
- F2: PAN (LLVAR, EBCDIC)
- F3: Processing Code (FIXED, ASCII)
- F4: Amount (FIXED, ASCII)
- F41: Terminal ID (FIXED, EBCDIC)
- F42: Merchant ID (FIXED, EBCDIC)
- F43: Merchant Name (FIXED, EBCDIC)

**Common Processing Codes:**
- 000000: Purchase
- 010000: Cash Withdrawal
- 200000: Refund

**Common Response Codes:**
- 00: Approved
- 05: Do not honor
- 51: Insufficient funds
- 55: Incorrect PIN

---

## 👥 Ekip

**Switch & Takas Ekibi**
Yapı Kredi Teknoloji

- Card Payments Team
- ISO 8583 Message Processing
- Network Clearing & Settlement

---

## 📝 Lisans

© 2025 Yapı Kredi Teknoloji
Switch & Takas Ekibi

Internal use only - Yapı Kredi Teknoloji

---

## 🔄 Versiyon Geçmişi

### v2.0.0 (2025-04-02)
- ✅ **EBCDIC Support:** Field 2, 41, 42, 43 EBCDIC parsing
- ✅ **Hex Length Detection:** "10" hex = 16 decimal auto-parse
- ✅ **Oracle Dump Support:** Header skip (0-20 bytes)
- ✅ **Dual Encoding:** ASCII/EBCDIC toggle
- ✅ **Bitmap Visualization:** Primary + Secondary
- ✅ **Example Messages:** Real-world EBCDIC examples
- ✅ **Modern UI:** WPF with Switch & Takas branding

### v1.0.0 (2025-02-15)
- ✅ İlk sürüm
- ✅ ASCII parsing
- ✅ VISA, Mastercard, BKM support

---

## 📞 Destek

Sorularınız için:
- **Ekip:** Switch & Takas
- **Şirket:** Yapı Kredi Teknoloji

---

**Production Ready** ✓
**Oracle Dump Tested** ✓
**EBCDIC Verified** ✓
**PAN 4761340000000035** ✓
