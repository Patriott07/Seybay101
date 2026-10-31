**Tech Plan** 

**Purpose:** Dokumen ini menjadi panduan seluruh programmer yang terjun langsung ke dalam engine. Dokumen ini mencakup basic information, Rules, Do and Don’t dan Schema data pada permainan. Dokumen ini dirancang agar tiap programmer memiliki aturan,visi dan style yang sama.

**Basic Information**  
**Game Engine :** Unity v. 6000.1.7f1  
**Language Program :** C\#Sharp  
**Paradigm :** OOP & Unity   
**Language Naming Convension :** English  
**Target Platform:** PC (Windows), Fixed Resolution 1920x1080 (16:9 Aspect Ratio).   
**UI Management:** Unity Canvas   
**Save System** : JSON native  
**Data Content Delivered**: Scriptable Object Unity and JSON native  
**Github :** https\://github.com/Patriott07/Seybay101

**Getting Started**

1. git clone project to local computer 

 https\://github.com/Patriott07/Seybay101

2. Open Unity Hub  
3. Open Project with folder \-\> to location git clone before  
4. Waiting Unity generate file Build, Library,etc.  
5. Read Rules  
6. Read Naming Convention  
7. Read Design Pattern   
8. Read Project Structure  
9. Read Package integrate in this project  
10. Read Schema data in game   
11. Get Ready with Agent

**Rules**

1. **Jangan males commit code.**  
2. **Nulis commit yang deskriptif, jelas dan mudah dipahami sesama programmer.**  
3. **Buat Branch masing-masing tiap anggota.**  
4. **Ikuti Naming Confension sesuai kesepakatan**  
5. **Pake Design Pattern yang udah dipilih**  
6. **Setiap fitur yang selesai coba bikin docs sederhana dengan format .md**  
7. **Jangan Edit Scene utama. buat fitur nya di scene sendiri**  
8. **Flow aman : Branch mu \-\> ngoding \-\> beres \-\> git merge ke main \-\> git push. kalo udah git push balik lagi ke branch masing-masing**  
9. **Naming file yang bener guys.**

**Naming Convention (English)**  
1\. \_nameVariable (untuk penamaan private variabel)  
2.nameVariabel (untuk publik variabel)  
3\. nameMethod (untuk penulisan fungsi private)  
4\. NameMethod (untuk penulisan fungsi public)

**Design Pattern** 

1. Manual Reference (General Script)  
2. Unity Events Subs/Unsub (General Script)  
3. Singleton (For Manager Script) 

**Folder Projects**  
\|-- Assets  
    \\ 2D  
       **– \\ Character                                 (**Menyimpan asset karakter)  
       **– \\ Background                             (**Menyimpan asset background)  
       **– \\ Sprite                                        (**Menyimpan asset object)  
       **– \\ Env                                            (**Menyimpan asset environtment object)  
    \\ Animation  
       **– \\ Animation Controller               (**Menyimpan seluruh Controller logic game/ui)  
       **– \\ Animation Clip                         (**Menyimpan seluruh Clip animasi game/ui)  
    \\ UI  
      **– \\ Elements                                   (**Menyimpan asset icon, asset-non bg)  
      **– \\ Background                              (**Menyimpan asset background)  
    **\\ Font                                                (**Menyimpan asset Font yang digunakan)  
    \\ Audio  
       **– \\ Music                                        (**Menyimpan asset music)  
       **– \\ Sfx                                             (**Menyimpan asset sound effect)  
       **– \\ Ambience                                 (**Menyimpan asset ambience)  
       **– \\ Dialogue                                   (**Menyimpan asset dialogue)  
    \\ Scene  
       **– \\ UI                                               (**Menyimpan scene ui terpisah, kalau mau)  
       **– \\ Game                                        (**Menyimpan scene gameplay)  
       **– \\ Prototype                  (**Menyimpan scene prototipe / fungsi yang sedang dicoba buat)  
    **\\ Prefab                                             (**Menyimpan Prefab)  
    \\ Script  
       **– \\ Docs                                         (**Menyimpan dokumentasi tiap script yang dibuat)  
       **– \\ UI                                              (**Menyimpan script yang berhubungan dengan UI)  
       **– \\ Game                                        (**Menyimpan script yang gameplay)  
       **– \\ System                                     (**Menyimpan script system in game)  
       **– \\ Camera                                     (**Menyimpan script camera)  
       **– \\ Scriptable Object                    (**Menyimpan script data gameplay)  
            **–\\ Data                                      (**Menyimpan data json gameplay)  
    \\ Polishing  
       **– \\ Post Processing                     (**Menyimpan post processing asset)  
       **– \\ Vfx                                            (**Menyimpan vfx prefab/texture)  
     **\\ Agent Helper                              (**Menyimpan dokumentasi context dan helper for agent)  
       .gitignore  
     

**Packages** 

| Name  | Version | Description | Lisence |
| :---- | :---- | :---- | :---- |
| PlayerPrefs Editor | 1.2.5 | Membantu untuk visualisasi dan management PlayerPrefs menjadi lebih mudah | Free |
| DOTween (HOTween v2) | 1.2.825 | Membantu membuat animasi menjadi lebih mudah | Free  |
| TextMesh Pro | Built-in unity | Membantu membuat visualisasi text lebih baik dan menjaga resolusi text tetap terbaca dengan baik | Free |
| Newtonsoft Json | 2.0.2 | Flexible JSON serializer for converting between .NET objects and JSONLINQ to JSON for manually reading and writing JSONWrite indented, easy-to-read JSONConvert JSON to and from XML | Free |

**How To Use Agent?**

**Data Structure & Schemas** 

**Schema Person**  
Menampung seluruh profil penumpang, dokumen visual, kondisi mutasi, dan isi koper yang muncul di loket 

| \[CreateAssetMenu(fileName \= "NPC\_Data", menuName \= "Seybay/NPC Passenger Data")\] public class NPCPassengerData : ScriptableObject {     \[Header("Identity")\]     public string npcID;     public string passengerName;     public int mutationStage; // 0 \= Normal, 1 \= Silikasi, 2 \= Plasma, 3 \= Umbral (random)          \[Header("Documents")\]     public PassportSchema passport; // reference class Passport     public BoardingPassSchema boardingPass; // reference class Boarding Pass/ticket          \[Header("Luggage & Contraband")\]     public List\<LuggageItemData\> luggageItems; // barang dikoper     public float weightOfKoper; // berat barang di koper          \[Header("Dialogue & Dilemma")\]     public List\<DialogueNode\> conversationTree;      public bool isOfferingBribe; // (nawarin suap)     public int bribeAmount; // harga suap          \[Header("Validation Answer")\]     public DecisionType correctAction; // Approve, Reject }  |
| :---- |
|  |

**Schema Document Passport \&Boarding Pass Schema**   
Mengelola data teks dan stempel pada dokumen yang ditarik/dibandingkan di atas meja loket. 

| \[System.Serializable\] public class PassportSchema {     public string documentNumber; // Unique random      public string ownerName; // sesuaikan sama country     public Sprite ownerPhoto; // keknya perlu dimofikasi (lebih ke isSamePhotoPerson)     public string expiryDate; // Format: DD-MM-YYYY     public string hexaCardColor; // \#FF0000 tiap negara punya hexa card masing-masing      public string countryName; // asal negara        // flag      public bool isValid; // apakah fisikly looks ini passport yang valid (karena ada orang yang bawa passport buatan nnti ada clue bedanya bagian mana) } \[System.Serializable\] public class BoardingPassSchema {     public string passNumber; // nomor uniq contoh “AR-28491“ Azure rocket     public string passengerName; // namanya sesuai ga sama passport     public string idPassengerCard; // nomer document pada passport     public string departureDate; // tanggal keberangkatan      public string seatClass; // Economy, Business, FIrst Class     // flag     public bool isValid;     }  |
| :---- |
|  |

**Schema Barang Koper**  
Mengatur benda-benda di dalam koper yang bisa diinspeksi, di-drag.

| \[System.Serializable\] public class LuggageItemData {     public string itemID;     public string itemName;     public Sprite itemSprite;     public Vector2 position;  // (posisi tetap ketika barang dikembalikan ke koper/ alat bantu snapping)     public int initialZOrder; //  (posisi Z order tetap ketika dikembalikan ke koper)     public bool isOrganic;        // Pelanggaran larangan organik     public bool isContraband;     // Status barang terlarang/sita (benda terlarang) }  |
| :---- |

**Schema Daily Rule & Day Progression Schema**   
Mengatur rotasi hari (Day 1 \- Day 10), aturan harian baru (*Rulebook*), batas waktu shift, dan lonjakan harga tiket di malam hari (*Surge Pricing*) 

| \[CreateAssetMenu(fileName \= "Day\_Config", menuName \= "Seybay/Daily Rule Config")\] public class DailyRuleConfig : ScriptableObject {     public int dayNumber; // 1 \- 10     public string dateDisplay; // Contoh: "3, Augustus 2045"     public float shiftDurationInSeconds; // Timer shift harian (60 \= 1 minute reallife)     public string shiftTextEnded; // contoh : Shift Ended at Schedule time / SHift Ended because i feel     sick / Shift Ended because of chaos          \[Header("Surge Pricing Economy")\]     public int ticketPriceToday; // Harga tiket yang makin lama makin mahal      public int dailyLivingCost;  // Biaya makan wajib     public int childMedicineCost; // Biaya obat harian anak          \[Header("Rulebook Requirements")\]     // Day one    public List\<RuleOnDay\> rules; // kumpulan aturan     public bool checkingValidPassport;      public bool allowMutationStage2;     public bool banOrganicItems;     public bool requireHazmatSeal;          \[Header("NPC Pool")\]     public List\<NPCPassengerData\> dailyPassengerQueue; // Antrean NPC non-generated / alias story } \[System.Serializable\] public class RuleOnDay Schema {     public bool isActive;     public title; // Checking valid Passport      public text helper; // bantuan text penjelasan untuk pemain memahami aturan baru} |
| :---- |

**Schema Chat & Introgasi**  
Mengatur panel *Chat / Inbox* yang ada pada tab mockup kanan atas untuk interogasi penumpang atau komunikasi radio dengan staf 

| \[System.Serializable\] public class DialogueNode {     public string dialogueID; public bool isPlayerDialogue ; // kalo true nnati buble dikanan kalo false buble dikiri     \[TextArea(2, 4)\] public string textConversation;  // text pesan      } \[System.Serializable\] public class InterrogationProfile {     // Daftar jawaban NPC ketika diinterogasi.     public List\<InterrogationAnswer\> answers;      public string fakeName;           // Nama bohong jika isLying \= true     public string fakeDOB;            // Tanggal bohong jika isLying \= true          \[Header("Moral Dilemma Event")\]     public bool isMoralEvent;             // True jika kasus khusus (misal: anak terpisah)     \[TextArea(2, 4)\]     public string moralPleaDialog;    // Kalimat memohon saat diinterogasi } \[Serializable\] public class InterrogationAnswer {     // ID pertanyaan yang dijawab.     // Contoh: "ask\_name", "ask\_dob", "ask\_destination"     public string questionID;     // Jawaban yang diberikan NPC kepada player     \[TextArea(2, 4)\]     public string answer;     // Kondisi sebenarnya dari jawaban.     // Player TIDAK mengetahui nilai ini secara langsung.     public bool truth; }  |
| :---- |

**Schema Save Data System**  
Struktur data JSON runtime yang menyimpan pencapaian ekonomi, status audit, dan inventaris tiket untuk evaluasi ending di Hari ke-10. 

| \[System.Serializable\] public class GameSaveState {     public int currentDay;          // Hari aktif (1 \- 10\)     public int playerCash;       // Saldo uang saat ini     public int purchasedTickets;    // Jumlah tiket evakuasi terbeli (0 \- 4\)    public bool purhaceTicketForAldo;     public bool purhaceTicketForNasya;     public bool purhaceTicketForVirly;     public bool purhaceTicketForKraisa;              \[Header("Ending Check")\]     public int calculatedEndingID;  // Ending 0, 1, 2, atau 3 }  |
| :---- |

**Schema Story**

| \[System.Serializable\] public class StorySchema {     public int Day;          // Hari aktif (1 \- 10\)     public List\<string\> preDayText;     public List\<string\> onDeskText;     public List\<string\> afterShiftText;  }  |
| :---- |

