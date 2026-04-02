using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ISO8583Parser
{
    public partial class MainWindow : Window
    {
        private string _currentNetwork = "VISA";
        private string _currentEncoding = "ASCII";
        private int _headerSkipBytes = 0;
        private ObservableCollection<FieldData> _parsedFields = new ObservableCollection<FieldData>();
        private ObservableCollection<FieldData> _buildFields = new ObservableCollection<FieldData>();
        private Dictionary<string, Dictionary<int, FieldDefinition>> _fieldDefinitions;
        private Dictionary<string, List<ExampleMessage>> _exampleMessages;

        // EBCDIC to ASCII conversion table
        private static readonly Dictionary<byte, byte> EBCDIC_TO_ASCII = new Dictionary<byte, byte>
        {
            {0x40, 0x20}, {0x4A, 0xA2}, {0x4B, 0x2E}, {0x4C, 0x3C}, {0x4D, 0x28}, {0x4E, 0x2B}, {0x4F, 0x7C},
            {0x50, 0x26}, {0x5A, 0x21}, {0x5B, 0x24}, {0x5C, 0x2A}, {0x5D, 0x29}, {0x5E, 0x3B}, {0x5F, 0xAC},
            {0x60, 0x2D}, {0x61, 0x2F}, {0x6A, 0xA6}, {0x6B, 0x2C}, {0x6C, 0x25}, {0x6D, 0x5F}, {0x6E, 0x3E},
            {0x6F, 0x3F}, {0x79, 0x60}, {0x7A, 0x3A}, {0x7B, 0x23}, {0x7C, 0x40}, {0x7D, 0x27}, {0x7E, 0x3D},
            {0x7F, 0x22}, {0x81, 0x61}, {0x82, 0x62}, {0x83, 0x63}, {0x84, 0x64}, {0x85, 0x65}, {0x86, 0x66},
            {0x87, 0x67}, {0x88, 0x68}, {0x89, 0x69}, {0x91, 0x6A}, {0x92, 0x6B}, {0x93, 0x6C}, {0x94, 0x6D},
            {0x95, 0x6E}, {0x96, 0x6F}, {0x97, 0x70}, {0x98, 0x71}, {0x99, 0x72}, {0xA2, 0x73}, {0xA3, 0x74},
            {0xA4, 0x75}, {0xA5, 0x76}, {0xA6, 0x77}, {0xA7, 0x78}, {0xA8, 0x79}, {0xA9, 0x7A}, {0xC1, 0x41},
            {0xC2, 0x42}, {0xC3, 0x43}, {0xC4, 0x44}, {0xC5, 0x45}, {0xC6, 0x46}, {0xC7, 0x47}, {0xC8, 0x48},
            {0xC9, 0x49}, {0xD1, 0x4A}, {0xD2, 0x4B}, {0xD3, 0x4C}, {0xD4, 0x4D}, {0xD5, 0x4E}, {0xD6, 0x4F},
            {0xD7, 0x50}, {0xD8, 0x51}, {0xD9, 0x52}, {0xE2, 0x53}, {0xE3, 0x54}, {0xE4, 0x55}, {0xE5, 0x56},
            {0xE6, 0x57}, {0xE7, 0x58}, {0xE8, 0x59}, {0xE9, 0x5A}, {0xF0, 0x30}, {0xF1, 0x31}, {0xF2, 0x32},
            {0xF3, 0x33}, {0xF4, 0x34}, {0xF5, 0x35}, {0xF6, 0x36}, {0xF7, 0x37}, {0xF8, 0x38}, {0xF9, 0x39}
        };

        public MainWindow()
        {
            InitializeComponent();
            InitializeFieldDefinitions();
            InitializeExampleMessages();
            InitializeUI();
        }

        private void InitializeFieldDefinitions()
        {
            _fieldDefinitions = new Dictionary<string, Dictionary<int, FieldDefinition>>
            {
                ["VISA"] = new Dictionary<int, FieldDefinition>
                {
                    [2] = new FieldDefinition { Name = "Primary Account Number (PAN)", Type = "LLVAR", Format = "n", MaxLen = 19, Encoding = "EBCDIC", Desc = "Card number" },
                    [3] = new FieldDefinition { Name = "Processing Code", Type = "FIXED", Format = "n", Len = 6, Encoding = "ASCII", Desc = "Transaction type" },
                    [4] = new FieldDefinition { Name = "Amount, Transaction", Type = "FIXED", Format = "n", Len = 12, Encoding = "ASCII", Desc = "Amount in minor units" },
                    [7] = new FieldDefinition { Name = "Transmission Date and Time", Type = "FIXED", Format = "n", Len = 10, Encoding = "ASCII", Desc = "MMDDhhmmss" },
                    [11] = new FieldDefinition { Name = "STAN", Type = "FIXED", Format = "n", Len = 6, Encoding = "ASCII", Desc = "System trace number" },
                    [12] = new FieldDefinition { Name = "Time, Local Transaction", Type = "FIXED", Format = "n", Len = 6, Encoding = "ASCII", Desc = "hhmmss" },
                    [13] = new FieldDefinition { Name = "Date, Local Transaction", Type = "FIXED", Format = "n", Len = 4, Encoding = "ASCII", Desc = "MMDD" },
                    [14] = new FieldDefinition { Name = "Date, Expiration", Type = "FIXED", Format = "n", Len = 4, Encoding = "ASCII", Desc = "YYMM" },
                    [18] = new FieldDefinition { Name = "Merchant Type (MCC)", Type = "FIXED", Format = "n", Len = 4, Encoding = "ASCII", Desc = "MCC code" },
                    [22] = new FieldDefinition { Name = "POS Entry Mode", Type = "FIXED", Format = "n", Len = 3, Encoding = "ASCII", Desc = "Entry mode" },
                    [25] = new FieldDefinition { Name = "POS Condition Code", Type = "FIXED", Format = "n", Len = 2, Encoding = "ASCII", Desc = "Condition" },
                    [32] = new FieldDefinition { Name = "Acquiring Institution ID", Type = "LLVAR", Format = "n", MaxLen = 11, Encoding = "ASCII", Desc = "Acquirer" },
                    [37] = new FieldDefinition { Name = "Retrieval Reference Number", Type = "FIXED", Format = "an", Len = 12, Encoding = "ASCII", Desc = "RRN" },
                    [38] = new FieldDefinition { Name = "Authorization ID Response", Type = "FIXED", Format = "an", Len = 6, Encoding = "ASCII", Desc = "Auth code" },
                    [39] = new FieldDefinition { Name = "Response Code", Type = "FIXED", Format = "an", Len = 2, Encoding = "ASCII", Desc = "Response" },
                    [41] = new FieldDefinition { Name = "Terminal ID", Type = "FIXED", Format = "ans", Len = 8, Encoding = "EBCDIC", Desc = "Terminal" },
                    [42] = new FieldDefinition { Name = "Merchant ID", Type = "FIXED", Format = "ans", Len = 15, Encoding = "EBCDIC", Desc = "Merchant" },
                    [43] = new FieldDefinition { Name = "Merchant Name/Location", Type = "FIXED", Format = "ans", Len = 40, Encoding = "EBCDIC", Desc = "Name/Location" },
                    [49] = new FieldDefinition { Name = "Currency Code", Type = "FIXED", Format = "n", Len = 3, Encoding = "ASCII", Desc = "Currency" },
                    [52] = new FieldDefinition { Name = "PIN Data", Type = "FIXED", Format = "b", Len = 16, Encoding = "BINARY", Desc = "PIN block" },
                    [55] = new FieldDefinition { Name = "ICC Data", Type = "LLLVAR", Format = "b", MaxLen = 999, Encoding = "BINARY", Desc = "EMV data" }
                },
                ["MASTERCARD"] = new Dictionary<int, FieldDefinition>
                {
                    [2] = new FieldDefinition { Name = "Primary Account Number", Type = "LLVAR", Format = "n", MaxLen = 19, Encoding = "EBCDIC", Desc = "PAN" },
                    [3] = new FieldDefinition { Name = "Processing Code", Type = "FIXED", Format = "n", Len = 6, Encoding = "ASCII", Desc = "Processing" },
                    [4] = new FieldDefinition { Name = "Amount Transaction", Type = "FIXED", Format = "n", Len = 12, Encoding = "ASCII", Desc = "Amount" },
                    [41] = new FieldDefinition { Name = "Terminal ID", Type = "FIXED", Format = "ans", Len = 8, Encoding = "EBCDIC", Desc = "Terminal" },
                    [42] = new FieldDefinition { Name = "Merchant ID", Type = "FIXED", Format = "ans", Len = 15, Encoding = "EBCDIC", Desc = "Merchant" },
                    [43] = new FieldDefinition { Name = "Name/Location", Type = "FIXED", Format = "ans", Len = 40, Encoding = "EBCDIC", Desc = "Merchant info" }
                },
                ["BKM"] = new Dictionary<int, FieldDefinition>
                {
                    [2] = new FieldDefinition { Name = "Kart Numarası", Type = "LLVAR", Format = "n", MaxLen = 19, Encoding = "EBCDIC", Desc = "PAN" },
                    [3] = new FieldDefinition { Name = "İşlem Kodu", Type = "FIXED", Format = "n", Len = 6, Encoding = "ASCII", Desc = "İşlem" },
                    [4] = new FieldDefinition { Name = "İşlem Tutarı", Type = "FIXED", Format = "n", Len = 12, Encoding = "ASCII", Desc = "Tutar" },
                    [41] = new FieldDefinition { Name = "Terminal ID", Type = "FIXED", Format = "ans", Len = 8, Encoding = "EBCDIC", Desc = "Terminal" },
                    [42] = new FieldDefinition { Name = "Üye İşyeri No", Type = "FIXED", Format = "ans", Len = 15, Encoding = "EBCDIC", Desc = "İşyeri" },
                    [43] = new FieldDefinition { Name = "İşyeri Adı/Yer", Type = "FIXED", Format = "ans", Len = 40, Encoding = "EBCDIC", Desc = "Ad/Lokasyon" }
                }
            };
        }

        private void InitializeExampleMessages()
        {
            _exampleMessages = new Dictionary<string, List<ExampleMessage>>
            {
                ["VISA"] = new List<ExampleMessage>
                {
                    new ExampleMessage 
                    { 
                        Name = "ASCII - Auth Request (0100)", 
                        Message = "0100723405412AC180040000000000000016453212345678901000000000000010000021513453012345613453002152512541105100081234560012345678901TERM0001123456789012345TEST MERCHANT",
                        Encoding = "ASCII",
                        HeaderSkip = 0
                    },
                    new ExampleMessage 
                    { 
                        Name = "EBCDIC - Oracle Dump (0200)", 
                        Message = "0200D50000000894223000000000000000000000000010F4F7F6F1F3F4F0F0F0F0F0F0F0F0F3F5000000000000010000E3C5D9D4F0F0F0F1",
                        Encoding = "EBCDIC",
                        HeaderSkip = 0
                    },
                    new ExampleMessage 
                    { 
                        Name = "EBCDIC - Oracle Dump with Header", 
                        Message = "00D5000016010200D50000000894223000000000000000000000000010F4F7F6F1F3F4F0F0F0F0F0F0F0F0F3F5000000000000010000E3C5D9D4F0F0F0F1",
                        Encoding = "EBCDIC",
                        HeaderSkip = 6
                    }
                },
                ["MASTERCARD"] = new List<ExampleMessage>
                {
                    new ExampleMessage 
                    { 
                        Name = "ASCII - Purchase (0200)", 
                        Message = "02004000000000000000161234567890123456000000050000TERM0001",
                        Encoding = "ASCII",
                        HeaderSkip = 0
                    }
                },
                ["BKM"] = new List<ExampleMessage>
                {
                    new ExampleMessage 
                    { 
                        Name = "ASCII - Satış (0200)", 
                        Message = "02004000000000000000161234567890123456000000020000TERM0001",
                        Encoding = "ASCII",
                        HeaderSkip = 0
                    }
                }
            };
        }

        private void InitializeUI()
        {
            ParsedFieldsList.ItemsSource = _parsedFields;
            BuildFieldsList.ItemsSource = _buildFields;
            
            UpdateNetworkButtonStyles();
            UpdateEncodingButtonStyles();
            UpdateExampleMessages();
            UpdateFieldComboBox();
        }

        private void NetworkButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            _currentNetwork = button.Tag.ToString();
            UpdateNetworkButtonStyles();
            UpdateExampleMessages();
            UpdateFieldComboBox();
            UpdateStatus($"{_currentNetwork} network seçildi");
        }

        private void EncodingButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            _currentEncoding = button.Tag.ToString();
            UpdateEncodingButtonStyles();
            UpdateStatus($"{_currentEncoding} encoding seçildi");
        }

        private void HeaderSkip_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            _headerSkipBytes = (int)e.NewValue;
            if (TxtHeaderSkipValue != null)
                TxtHeaderSkipValue.Text = _headerSkipBytes.ToString();
        }

        private void UpdateNetworkButtonStyles()
        {
            var activeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#667EEA"));
            var inactiveColor = new SolidColorBrush(Colors.White);
            var activeForeground = new SolidColorBrush(Colors.White);
            var inactiveForeground = new SolidColorBrush(Colors.Black);

            BtnVisa.Background = _currentNetwork == "VISA" ? activeColor : inactiveColor;
            BtnVisa.Foreground = _currentNetwork == "VISA" ? activeForeground : inactiveForeground;

            BtnMastercard.Background = _currentNetwork == "MASTERCARD" ? activeColor : inactiveColor;
            BtnMastercard.Foreground = _currentNetwork == "MASTERCARD" ? activeForeground : inactiveForeground;

            BtnBkm.Background = _currentNetwork == "BKM" ? activeColor : inactiveColor;
            BtnBkm.Foreground = _currentNetwork == "BKM" ? activeForeground : inactiveForeground;
        }

        private void UpdateEncodingButtonStyles()
        {
            var activeColor = new SolidColorBrush((Color)ColorConverter.ConvertFromString("#28a745"));
            var inactiveColor = new SolidColorBrush(Colors.White);
            var activeForeground = new SolidColorBrush(Colors.White);
            var inactiveForeground = new SolidColorBrush(Colors.Black);

            BtnAscii.Background = _currentEncoding == "ASCII" ? activeColor : inactiveColor;
            BtnAscii.Foreground = _currentEncoding == "ASCII" ? activeForeground : inactiveForeground;

            BtnEbcdic.Background = _currentEncoding == "EBCDIC" ? activeColor : inactiveColor;
            BtnEbcdic.Foreground = _currentEncoding == "EBCDIC" ? activeForeground : inactiveForeground;
        }

        private void UpdateExampleMessages()
        {
            CmbExamples.Items.Clear();
            CmbExamples.Items.Add("-- Örnek Mesaj Seçin --");
            
            if (_exampleMessages.ContainsKey(_currentNetwork))
            {
                foreach (var example in _exampleMessages[_currentNetwork])
                {
                    CmbExamples.Items.Add(example.Name);
                }
            }
            
            CmbExamples.SelectedIndex = 0;
        }

        private void UpdateFieldComboBox()
        {
            CmbAddField.Items.Clear();
            CmbAddField.Items.Add("-- Field Seçin --");

            if (_fieldDefinitions.ContainsKey(_currentNetwork))
            {
                foreach (var field in _fieldDefinitions[_currentNetwork].OrderBy(f => f.Key))
                {
                    string encoding = field.Value.Encoding == "EBCDIC" ? " [EBCDIC]" : "";
                    CmbAddField.Items.Add($"Field {field.Key} - {field.Value.Name}{encoding}");
                }
            }

            CmbAddField.SelectedIndex = 0;
        }

        private void ParseMessage_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string rawMessage = TxtRawMessage.Text.Trim().Replace(" ", "").Replace("\n", "").Replace("\r", "");
                
                if (string.IsNullOrEmpty(rawMessage) || rawMessage.Length < 20)
                {
                    MessageBox.Show("Mesaj çok kısa! Minimum 20 karakter gerekli.", "Hata", 
                        MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                // Skip header bytes
                if (_headerSkipBytes > 0 && rawMessage.Length > _headerSkipBytes * 2)
                {
                    rawMessage = rawMessage.Substring(_headerSkipBytes * 2);
                }

                // MTI parse
                string mti = rawMessage.Substring(0, 4);
                TxtMti.Text = mti;

                // Bitmap parse (hex to binary)
                string bitmapHex = rawMessage.Substring(4, 16);
                string bitmapBin = HexToBinary(bitmapHex);
                TxtBitmap.Text = bitmapBin;

                // Check for secondary bitmap
                bool hasSecondaryBitmap = bitmapBin[0] == '1';
                int bitmapLength = hasSecondaryBitmap ? 32 : 16;
                
                if (hasSecondaryBitmap && rawMessage.Length > 20 + bitmapLength)
                {
                    string secondaryBitmapHex = rawMessage.Substring(20, 16);
                    bitmapBin += HexToBinary(secondaryBitmapHex);
                }

                // Parse fields
                var fieldDefs = _fieldDefinitions[_currentNetwork];
                _parsedFields.Clear();
                int position = 4 + bitmapLength;

                for (int i = 1; i <= (hasSecondaryBitmap ? 128 : 64); i++)
                {
                    if (i <= bitmapBin.Length && bitmapBin[i - 1] == '1')
                    {
                        if (!fieldDefs.ContainsKey(i))
                            continue;

                        var fieldDef = fieldDefs[i];
                        string fieldValue = "";
                        string fieldValueHex = "";
                        int fieldLength = 0;
                        string fieldEncoding = fieldDef.Encoding;

                        if (fieldDef.Type == "FIXED")
                        {
                            fieldLength = fieldDef.Len;
                            
                            if (fieldEncoding == "EBCDIC" && _currentEncoding == "EBCDIC")
                            {
                                int hexLen = fieldLength * 2;
                                if (position + hexLen <= rawMessage.Length)
                                {
                                    fieldValueHex = rawMessage.Substring(position, hexLen);
                                    fieldValue = EbcdicToAscii(fieldValueHex);
                                    position += hexLen;
                                }
                            }
                            else if (fieldEncoding == "BINARY" || fieldDef.Format == "b")
                            {
                                if (position + fieldLength <= rawMessage.Length)
                                {
                                    fieldValueHex = rawMessage.Substring(position, fieldLength);
                                    fieldValue = $"[HEX: {fieldValueHex}]";
                                    position += fieldLength;
                                }
                            }
                            else
                            {
                                if (position + fieldLength <= rawMessage.Length)
                                {
                                    fieldValue = rawMessage.Substring(position, fieldLength);
                                    position += fieldLength;
                                }
                            }
                        }
                        else if (fieldDef.Type == "LLVAR")
                        {
                            if (position + 2 <= rawMessage.Length)
                            {
                                int lenIndicator;
                                string lenBytes = rawMessage.Substring(position, 2);
                                
                                // Try hex length for EBCDIC fields
                                if (fieldEncoding == "EBCDIC" && _currentEncoding == "EBCDIC" && 
                                    System.Text.RegularExpressions.Regex.IsMatch(lenBytes, "^[0-9A-F]{2}$", 
                                    System.Text.RegularExpressions.RegexOptions.IgnoreCase))
                                {
                                    int hexLen = Convert.ToInt32(lenBytes, 16);
                                    if (hexLen >= 1 && hexLen <= 99)
                                    {
                                        lenIndicator = hexLen;
                                    }
                                    else
                                    {
                                        lenIndicator = int.Parse(lenBytes);
                                    }
                                }
                                else
                                {
                                    lenIndicator = int.Parse(lenBytes);
                                }
                                
                                position += 2;
                                
                                if (fieldEncoding == "EBCDIC" && _currentEncoding == "EBCDIC")
                                {
                                    int hexDataLen = lenIndicator * 2;
                                    if (position + hexDataLen <= rawMessage.Length)
                                    {
                                        fieldValueHex = rawMessage.Substring(position, hexDataLen);
                                        fieldValue = EbcdicToAscii(fieldValueHex);
                                        position += hexDataLen;
                                        fieldLength = lenIndicator;
                                    }
                                }
                                else
                                {
                                    if (position + lenIndicator <= rawMessage.Length)
                                    {
                                        fieldValue = rawMessage.Substring(position, lenIndicator);
                                        position += lenIndicator;
                                        fieldLength = lenIndicator;
                                    }
                                }
                            }
                        }
                        else if (fieldDef.Type == "LLLVAR")
                        {
                            if (position + 3 <= rawMessage.Length)
                            {
                                int lenIndicator = int.Parse(rawMessage.Substring(position, 3));
                                position += 3;
                                
                                if (fieldEncoding == "BINARY" || fieldDef.Format == "b")
                                {
                                    if (position + lenIndicator <= rawMessage.Length)
                                    {
                                        fieldValueHex = rawMessage.Substring(position, lenIndicator);
                                        fieldValue = $"[HEX: {fieldValueHex}]";
                                        position += lenIndicator;
                                        fieldLength = lenIndicator;
                                    }
                                }
                                else
                                {
                                    if (position + lenIndicator <= rawMessage.Length)
                                    {
                                        fieldValue = rawMessage.Substring(position, lenIndicator);
                                        position += lenIndicator;
                                        fieldLength = lenIndicator;
                                    }
                                }
                            }
                        }

                        _parsedFields.Add(new FieldData
                        {
                            FieldNumber = $"Field {i}",
                            FieldNumberInt = i,
                            FieldName = fieldDef.Name,
                            FieldValue = fieldValue,
                            FieldValueHex = fieldValueHex,
                            FieldType = $"{fieldDef.Type} ({fieldDef.Format})",
                            FieldEncoding = fieldEncoding,
                            FieldDescription = fieldDef.Desc
                        });
                    }
                }

                TxtFieldCount.Text = _parsedFields.Count.ToString();
                TxtMessageLength.Text = rawMessage.Length.ToString();

                UpdateStatus($"✓ Mesaj başarıyla parse edildi! {_parsedFields.Count} field bulundu.");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Parse hatası: {ex.Message}\n\n{ex.StackTrace}", "Hata", 
                    MessageBoxButton.OK, MessageBoxImage.Error);
                UpdateStatus($"✗ Parse hatası");
            }
        }

        private void ExampleMessage_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CmbExamples.SelectedIndex <= 0)
                return;

            string selectedName = CmbExamples.SelectedItem.ToString();
            var example = _exampleMessages[_currentNetwork].FirstOrDefault(ex => ex.Name == selectedName);
            
            if (example != null)
            {
                TxtRawMessage.Text = example.Message;
                _currentEncoding = example.Encoding;
                _headerSkipBytes = example.HeaderSkip;
                
                UpdateEncodingButtonStyles();
                SliderHeaderSkip.Value = example.HeaderSkip;
                
                UpdateStatus($"Örnek mesaj yüklendi: {selectedName}");
            }
        }

        private void ClearAll_Click(object sender, RoutedEventArgs e)
        {
            TxtRawMessage.Clear();
            TxtMti.Text = "----";
            TxtBitmap.Text = "";
            TxtFieldCount.Text = "0";
            TxtMessageLength.Text = "0";
            _parsedFields.Clear();
            UpdateStatus("Tümü temizlendi");
        }

        private void UpdateStatus(string message)
        {
            TxtStatus.Text = message;
        }

        private string HexToBinary(string hex)
        {
            return string.Join("", hex.Select(c => 
                Convert.ToString(Convert.ToInt32(c.ToString(), 16), 2).PadLeft(4, '0')
            ));
        }

        private string EbcdicToAscii(string hexString)
        {
            var result = new StringBuilder();
            for (int i = 0; i < hexString.Length; i += 2)
            {
                byte ebcdicByte = Convert.ToByte(hexString.Substring(i, 2), 16);
                if (EBCDIC_TO_ASCII.ContainsKey(ebcdicByte))
                {
                    result.Append((char)EBCDIC_TO_ASCII[ebcdicByte]);
                }
                else
                {
                    result.Append('?');
                }
            }
            return result.ToString();
        }
    }

    public class FieldDefinition
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string Format { get; set; }
        public int Len { get; set; }
        public int MaxLen { get; set; }
        public string Encoding { get; set; }
        public string Desc { get; set; }
    }

    public class FieldData : INotifyPropertyChanged
    {
        private string _fieldValue;

        public string FieldNumber { get; set; }
        public int FieldNumberInt { get; set; }
        public string FieldName { get; set; }
        public string FieldType { get; set; }
        public string FieldEncoding { get; set; }
        public string FieldDescription { get; set; }
        public string FieldValueHex { get; set; }
        
        public string FieldValue
        {
            get => _fieldValue;
            set
            {
                _fieldValue = value;
                OnPropertyChanged(nameof(FieldValue));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    public class ExampleMessage
    {
        public string Name { get; set; }
        public string Message { get; set; }
        public string Encoding { get; set; }
        public int HeaderSkip { get; set; }
    }
}
