using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace Schema.data
{
    [System.Serializable]
    public class NPCPassengerRuntimeData
    {
        public string npcID;
        public string passengerName; // name
        public int mutationStage; // 0 = normal,
        public PassportSchema passport;
        public BoardingPassSchema boardingPass;
        public List<LuggageItemData> luggageItems;
        public float weightOfKoper;
        public List<DialogueNode> conversationTree;
        public bool isOfferingBribe; // menawarkan suap?
        public int bribeAmount; // harga suap

        public string violationReason; // ?
    }

    [System.Serializable]
    public class PassportSchema
    {
        public string documentNumber; // Unique random
        public string ownerName; // sesuaikan sama country
        public bool sameOwnerPhoto; // keknya perlu dimofikasi (lebih ke isSamePhotoPerson)
        public string sex;
        public string bodOwner;
        public DateTime expiryDate; // Format: DD-MM-YYYY
        public string hexaCardColor; // #FF0000 tiap negara punya hexa card masing-masing
        public string countryName; // asal negara
        public string districtHome; // kota rumah

        // flag
        public bool isValid; // apakah fisikly looks ini passport yang valid (karena ada orang yang bawa passport buatan nnti ada clue bedanya bagian mana)
    }

    public enum SeatClass
    {
        Economy,
        Business,
        FirstClass,
    }

    [System.Serializable]
    public class BoardingPassSchema
    {
        public string passNumber; // nomor uniq contoh “AR-28491“ Azure rocket
        public string destination; // "Seybay Outpost";

        // public string estimationTime; // 2 Day / 48 Hours (fix gabisa diganti2)
        public string seat;
        public string passengerName; // namanya sesuai ga sama passport
        public string idPassengerCard; // nomer document pada boarding ticket
        public DateTime departureDate; // tanggal keberangkatan

        // Kelas penerbangan
        public SeatClass seatClass;

        // flag
        public bool isValid;
    }

    [System.Serializable]
    public class LuggageItemData
    {
        public string itemID;
        public string itemName;
        public Sprite itemSprite;
        public Vector2 position; // (posisi tetap ketika barang dikembalikan ke koper/ alat bantu snapping)
        public int initialZOrder; //  (posisi Z order tetap ketika dikembalikan ke koper)
        public bool isOrganic; // Pelanggaran larangan organik
        public bool isContraband; // Status barang terlarang/sita (benda terlarang)
    }

    [System.Serializable]
    public class DailyExpense
    {
        public string itemID;
        public string itemName;
        public int cost;

        [TextArea(2, 4)]
        public string effectIfBought;

        [TextArea(2, 4)]
        public string effectIfSkipped;
    }

    // ============================================================
    // 3. NORMAL DIALOGUE
    // ============================================================

    [System.Serializable]
    public class DialogueNode
    {
        // ID unik dialog
        public string dialogueID;

        // true  = Player
        // false = NPC
        public bool isPlayerDialogue;

        // Isi dialog
        [TextArea(2, 4)]
        public string textConversation;

        // True jika dialog ini memberikan
        // petunjuk tentang aturan tersembunyi
        // atau kontraband.
        public bool revealsSecretRule;
    }

    // ============================================================
    // 4. INTERROGATION ANSWER
    // ============================================================

    public enum AnswerTruth
    {
        Truth,
        Lie,
    }

    [System.Serializable]
    public class InterrogationAnswer
    {
        // ID pertanyaan yang dijawab.
        // Contoh: "ask_name", "ask_dob", "ask_destination"
        public string questionID;

        // Jawaban yang diberikan NPC kepada player
        [TextArea(2, 4)]
        public string answer;

        // Kondisi sebenarnya dari jawaban.
        // Player TIDAK mengetahui nilai ini secara langsung.
        public AnswerTruth truth;
    }

    // ============================================================
    // 5. INTERROGATION PROFILE
    // ============================================================

    [System.Serializable]
    public class InterrogationProfile
    {
        // Daftar jawaban NPC ketika diinterogasi.
        public List<InterrogationAnswer> answers;

        [Header("Moral Dilemma Event")]
        // True jika kasus ini memiliki
        // dilema moral khusus.
        public bool isMoralEvent;

        // Dialog permohonan NPC ketika
        // moral event terjadi.
        [TextArea(2, 4)]
        public string moralPleaDialog;
    }

    [System.Serializable]
    public class RuleOnDay
    {
        // ID unik rule
        public string ruleID;

        // Nama rule yang ditampilkan kepada player
        public string title;

        // Penjelasan rule kepada player
        [TextArea(2, 4)]
        public string helperText;

        // Apakah rule ini berlaku hari ini?
        public bool isActive;
    }

    [System.Serializable]
    public class LuggageItemTemplate
    {
        public string itemName;
        public Sprite itemSprite;
        public bool isOrganic;
        public bool isContraband;
    }

    [System.Serializable]
    public class StorySchema
    {
        public int Day; // Hari aktif (1 - 10)
        public List<string> preDayText;
        public List<string> onDeskText;
        public List<string> afterShiftText;
    }

    public enum Gender
    {
        Man,
        Woman,
    }

    // Format untuk menambahkan jenis obat berapapun secara bebas di Unity
    [System.Serializable]
    public struct ItemOnDay
    {
        public string name_item;
        public int price;
        public bool isShouldBuy; // wajib beli ga
        public Toggle toggleCheckbox;
        public UnityEvent onBuy,
            onNotBuy;
    }

    // ============================================================
    // 6. Arktype
    // ============================================================
    public enum Arktype
    {
        THE_ELITE,
        ORDINARY_CITIZEN,
        SICK_PERSON,
        CHILD,
        SCIENTIST,
        LOYALIST,
        WORKER,
        AUDIT_AZURA,
        THE_UNDERGROUND,
    }

    // ============================================================
    // 7. EXPRESION
    // ============================================================

    public enum Mood
    {
        Sad,
        Angry,
        Neutral,
        Anxious,
        Pleading,
        Arrogant,
        ColdShy,
        Scare,
        Sick,
        Confused,
        Taunting,
        Relieved,
    }

    public enum IssueDetail { }

    public enum Issues
    {
        Missmatch_Passport,
        Invalid_Passport,
        Missmatch_Ticket,
        Invalid_Ticket,
        Missmatch_Worker_Paper,
        Invalid_Worker_Paper,
        Invalid_Age,
        Invalid_Sertification_Paper,
        Missmatch_Sertification_Paper,
        Missmatch_Form_Spesial_Way,
        Invalid_Form_Spesial_Way,
        Missmatch_Medical_Check_Paper,
        Invalid_Medical_Check_Paper,
        Missmatch_Vaccine_Paper,
        Invalid_Vaccine_Paper,
    }

    public enum Tactic
    {
        Honest, // Jujur dan kooperatif mengikuti semua prosedur
        Pretending_BeConfused, // pura2 bingung / gapaham
        Bribe, // Mencoba menyogok dengan uang/barang berharga
        GuiltTrip, // Memanfaatkan emosi/iba (menceritakan nasib anak/keluarga)
        Intimidation, // Mengancam Aldo (mengaku kenal petinggi Azura Corp atau The Underground)
        Begging, // Memohon-mohon tanpa memberikan barang apa pun
        Force, // Memaksa/menerobos barikade secara fisik
    }

    public enum LayerSystemAudit
    {
        EMERGENCY_RULE, // aturan yang tidak bisa dilanggar siapapun
        ARKTYPE_PRIORITY, // Orang-orang yang bisa melewati rule basic
        BASIC_RULE, // aturan basic
    }

    public enum WorkerSertification
    {
        BEGINNER,
        INTERMEDIATE,
        EXPERT,
    }

    public enum RequestWorkerJob
    {
        FARMER,
        LIVESTOCK_HANDLER,
        ARSITECHTURE,
        TUNNEL_DIGGER,
        BIOMASS_HANDLER,
    }

    public enum FamilyMood
    {
        Happy, // Sehat & Tenang
        Anxious, // Cemas/Ketakutan
        Depressed, // Sedih/Putus Asa
        Rebellious, // Benci Azura Corp
    }

    [System.Serializable]
    public struct FamilyMemberStatus
    {
        public string name;
        public int starving; // 0 (Kenyang) - 100 (Kritis Kelaparan)
        public int sick; // 0 (Sehat) - 100 (Silikosis Stadium Akhir)
        public int mutasi; // 0 (Murni) - 100 (Mutasi Fisik Total)
        public FamilyMood mood;
    }

    public struct WorldState
    {
        public float temperature; // Memengaruhi biaya bahan bakar rumah
        public float pollution_level; // Memengaruhi jumlah pasien Silikosis di loket
        public float food_water_supply; // Memengaruhi harga kebutuhan pokok di malam hari
        public float unrest_level; // Memengaruhi potensi serangan/terorisme di pos
        public float azura_control; // Memengaruhi tingkat ketatnya audit & denda
    }

    public struct AldoStateGame
    {
        public int stressLevel;
        public int sickLevel;
        public int sleepyLevel;
    }
}
