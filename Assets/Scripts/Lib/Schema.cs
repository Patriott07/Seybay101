using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        public string expiryDate; // Format: DD-MM-YYYY
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
        public string estimationTime; // 2 Day / 48 Hours (fix gabisa diganti2)
        public string passengerName; // namanya sesuai ga sama passport
        public string idPassengerCard; // nomer document pada boarding ticket
        public string departureDate; // tanggal keberangkatan

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
}
