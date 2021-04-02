using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//TODO Finish this ref https://fallout.fandom.com/wiki/Vault13.gam
//start from "LOXLEY_X"
public static class GlobalVariables
{
    //ChildKiller Reputation
    public static bool childKillerShady = false; // Has the player killed children in Shady Sands?
    public static bool childKillerHub = false; // Has the player killed children in Hub?
    public static bool childKillerAdytum = false; // Has player killed children in Adytum?
    

    public static int dogEmpathy = 0;//Dogs and player like each other. TODO find out how this is determined

    #region Number of X killed
    public static int questPeopleKilled = 0;//Number of Quest people killed.
    public static int dogsKilled = 0;//Number of dogs player has killed. Note, if this number is greater than 0, the player is a bad person.
    public static int brahminKilled = 0;//Number of Brahmin Killed
    public static int animalsKilled = 0;//Number of Animals killed
    public static int humansKilled = 0;//Number of humans killed
    public static int radscorpionsKilled = 0;//Number of Radscorpions killed.
    public static int superMutantsKilled = 0;//Number of Super Mutants killed
    public static int necropMutantsKilled = 0;//Number of Necropolis Mutants killed
    
    //Adding this here for lack of better sorting
    public static int lieutenantsDead = 0;//How many Lieutenants are dead?
    #endregion
    
    #region Location Things
    public static int mapEntranceNum = 0;//Where did the player enter the map. This exists in original but I may want to track this elsewhere
    public static int loadMapIndex = 0;//Which part of the town map is the player going to;  This exists in original but I may want to track this elsewhere
    public static Vector2 worldMapLoc = Vector2.zero;//Where the player is located on the world map. TODO Create the system for this
    public static int enteringVatsHow = 0;//How did the player enter the vats?TODO find out what options this has
    public static int worldTerrain = 0;//What is the terrain type the player is on? TODO find out the what options this has
    
    #endregion
    
    #region Time based things
    //Days until X
    public static int daysToVault13Discovered = 180;//How long before the vault is discovered?
    public static int daysToVault13Waterless = 150;//How long before the vault runs out of water? In the original this was named VAULT_WATER but for naming consistency changed
    
    //Day Tick when X happened
    public static int necroWaterChipTaken = 0;//Day tick when player took the Necropolis Water Chip
    
    //Minutes until X
    public static int countdownToDestruction = 0;//How much time is left before the bomb goes off
    #endregion
    //Does the player know about X
    public static bool necropolisKnown = false;
    
    //Has the player been to X
    public static bool necropolisVisited = false;//Has Necropolis been visited?
    public static bool motelOfDoomVisited = false;//Has the Motel of Doom been visited?
    public static bool hallOfTheDeadVisited = false;//Has the Hall of the Dead been visited?
    public static bool watershedVisited = false;//Has the Watershed been visited?
    public static bool necropolisVaultVisited = false;//Has the Necropolis Vault been visited?
    
    #region Has the player done X or has X happened?
    //ShadySands
    public static bool radscorpionSeed = false;//Is player killing radscorpions for Shady Sands?
    //Scorpion Cave
    public static int totalRadScorpions = 10;// How many RadScorpions are still around in cave TODO find out what this number should be
    
    //Raiders
    public static bool mistakenID = false;//Do the Raiders think you are Garl's dad?
    public static int totalRaiders = 24;//How many Raiders are still alive TODO find out what this number should be.
    
    //Junktown
    public static bool hiredByKillian = false;//Has Killian hired the player to trap Gizmo?
    public static bool hiredByGizmo = false;//Has Gizmo hired the player to kill Killian?
    public static bool inJail = false;//Is the player in Jail?
    public static bool bugPlanted = false;//Has the player planted the bugg Killian gave them?
    public static bool gotConfession = false;//Has the player recorded Gizmo's confession?
    
    //Hub
    public static bool hubSeed = false;//TODO find out what this is.
    public static bool withCaravan = false;//Is the player travelling with the caravas?
    
    //Boneyard
    public static bool deathclawSeed = false;//Has the player accepted the quest to find/kill the deathclaw?
    public static bool bladesHelp = false;//Will the Blades help you knock out the children of the Cathedral?
    
    //Brotherhood of steel
    public static bool joinBrotherhoodSeed = false;//Has the player accepted the quest to join the Brotherhood of Steel?
    public static bool rescueBrotherhoodSeed = false;//Has the player accepted the quest to save the Brotherhood of Steel initiate from the Hub?
    public static bool knightWarning = false;//Will the Knight need to warn player of his manners?
    
    //Necropolis
    public static bool necroWaterPumpFixed = false;//Has the Necropolis Water Pump been fixed?
    public static int signalReward = 0;//How much of a reward is Set telling Garret to give the player?
    public static int necroWaterStatus = 0;//What is the status of the water level in Necropolis?TODO find out what options this has
    
    //Cathedral
    public static bool shadowPassword = false;//Has Nicole given player the 'Shadow' password? This is cut content in the original
    public static bool followerStealthHelp = false;//Have the Followers offered to sneak player in Children of Cathedral?
    public static bool followerMachoHelp = false;//Are Followers going to attack Children of Cathedral?
    
    //Final Things
    public static int waterShedStatus = 0;//Are there super mutants in the water shed?//TODO find out what options this has other than t/f
    public static bool playerCaptured = false;//Has the player been captured by the supermutants?
    public static bool vatsBlown = false;//Have the Vats been destroyed?
    public static int vatsStatus = 0;//Not sure why this also exists but it also lets you check if vats have been blown. TODO possibly remove this.
    public static bool masterBlown = false; //has the Master been destroyed? also lol has he been blown?
    public static bool deliveredBomb = false;//Has the palyer delivered the bomb?
    public static bool bombDisarmed = false;//Is the bomb disarmed
    #endregion
    
    #region Quest Status

    public static QuestStatus findWaterChip = QuestStatus.Unstarted;//Starts on game start
    public static QuestStatus killRadscorpions = QuestStatus.Unstarted;//Starts in shady sands
    public static QuestStatus rescueTandi = QuestStatus.Unstarted;//Starts in Shady Sands. Ends in Raiders
    public static QuestStatus captureGizmo = QuestStatus.Unstarted;//In Junktown
    public static QuestStatus killKillian = QuestStatus.Unstarted;//In Junktown
    public static QuestStatus missingCaravan = QuestStatus.Unstarted;//Starts in Hub
    public static QuestStatus stealNecklace = QuestStatus.Unstarted;//In Hub
    public static QuestStatus killMerchant = QuestStatus.Unstarted;//In Hub
    public static QuestStatus killJain = QuestStatus.Unstarted;//In Hub
    public static QuestStatus killDeathclaw = QuestStatus.Unstarted;//Starts in Hub
    public static QuestStatus killSuperMutants = QuestStatus.Unstarted;//In Necropolis
    public static QuestStatus becomeInitiate = QuestStatus.Unstarted;//Starts in the Brotherhood of Steel
    public static QuestStatus findLostInitiate = QuestStatus.Unstarted;//Starts in the Brotherhood of Steel
    public static QuestStatus warnedBrotherhood = QuestStatus.Unstarted;//Starts in the Brotherhood of Steel
    public static QuestStatus romeoAndJuliet = QuestStatus.Unstarted;//Romeo and Juliet quest TODO find out more about this

    public static QuestStatus lostBrother = QuestStatus.Unstarted;//Look for the lost brother of the Blades in the Runners
    public static QuestStatus destroyFollowers = QuestStatus.Unstarted;//Deliver the bomb
    #endregion
    
    #region Has X Been Invaded?
    public static bool vaultInvaded = false;//Has the Vault been invaded?
    public static bool shadySandsInvaded = false;//Has Shady Sands been invaded?
    public static bool necropolisInvaded = false;//Has Necropolis been invaded?
    public static bool hubInvaded = false;//Has The Hub been invaded?
    public static bool junktownInvaded = false;//Has Junktown been invaded?
    public static bool brotherhoodInvaded = false;//Has The Brotherhood of Steel been invaded?
    public static bool followersInvaded = false;//Have the Followers of the Sun been invaded? Not this is cut content or just bugged content in the original.
    #endregion
    
    #region NPC Status'
    //Tandi's status
    public static int tandiStatus = 0;//Is Tandi: (0 = Free, 1 = Kidnapped, 2 = Rescued, 3 = Killed)
    public static bool tandiDead => tandiStatus == 3;
    
    //Ian from Shady Sands
    public static bool ianDead = false;//Is Ian Dead?
    //public static Location iansLocation = TODO make the locations system and add where ian is here
    
    //Seth from Shady Sands
    public static bool sethDead = false;//Is Seth dead?
    
    //Tolya The Raider
    public static bool tolyaDead = false;//Is Tolya the Raider dead?
    
    //Petrox The Raider
    public static bool petroxDead = false;//Is Petrox the Raider dead?
    
    //Hernandex from Junktown/Hub
    public static bool hernandexDead = false;//Is Hernandex dead?
    
    //Kenji the thug from Junktown/Hub
    public static bool kenjiDead = false;//Is Kenji the Thug dead?
    
    //Chris the Bounty Hunter
    public static bool chrisDead = false;//Is Chris the Bounty Hunter dead? This was HUNTER_STATUS in the original but for consistency renaming
    
    //Killian's Status
    public static bool killianDead = false;//Is Killian dead?
    
    //Gizmo's Status
    public static bool gizmoDead = false;//Is Gizmo dead?
    
    //Garl
    public static bool garlDead = false;//Is Garl dead?
    
    //Trent from the Hub and random encounter
    public static bool trentDead = false;//Is Trent dead?
    
    //Jason from the Adytum
    public static bool jasonDead = false;//Is Jason dead?
    
    //Mutants
    public static bool masterDead = false;//Is the master dead?
    #endregion

    #region Endings
    public static int tandiEnding = 0;// For Tandi's special endings. Not sure I will need this
    public static int loxleyEnding = 0;// For Loxley's special endings. I dont think this is used
    public static int maxsonEnding = 0;// For Maxson's special endings. Not sure I will need this
    
    

    #endregion

    #region TODO
/*
  VAULT_13                  :=2;       //  (67)
SHADY_SANDS               :=0;       //  (68)
RAIDERS                   :=0;       //  (69)
BURIED_VAULT              :=1;       //  (70)
JUNKTOWN                  :=0;       //  (71)
NECROPOLIS                :=0;       //  (72)
THE_HUB                   :=0;       //  (73)
BROTHERHOOD_OF_STEEL      :=0;       //  (74)
ANGELS_BONEYARD           :=0;       //  (75)
THE_GLOW                  :=0;       //  (76)
CHILDREN_OF_CATHEDRAL     :=0;       //  (77)
THE_VATS                  :=0;       //  (78)
MASTERS_LAIR              :=0;       //  (79)
BROTHERHOOD_12            :=0;       //  (80)  // BROHD12.MAP
BROTHERHOOD_34            :=0;       //  (81)  // BROHD34.MAP
BROTHERHOOD_ENTRANCE      :=0;       //  (82)  // BROHDENT.MAP
CAVES                     :=0;       //  (83)  // CAVES.MAP
CHILDREN_1                :=0;       //  (84)  // CHILDRN1.MAP
DESERT_1                  :=0;       //  (85)  // DESERT1.MAP
DESERT_2                  :=0;       //  (86)  // DESERT2.MAP
DESERT_3                  :=0;       //  (87)  // DESERT3.MAP
HALL_OF_THE_DEAD          :=0;       //  (88)  // HALLDED.MAP
HOTEL_OF_DOOM             :=0;       //  (89)  // HOTEL.MAP
JUNKTOWN_CASINO           :=0;       //  (90)  // JUNKCSNO.MAP
JUNKTOWN_ENTRANCE         :=0;       //  (91)  // JUNKENT.MAP
JUNKTOWN_KILLIAN          :=0;       //  (92)  // JUNKKILL.MAP
SHADY_SANDS_EAST          :=0;       //  (93)  // SHADYE.MAP
SHADY_SANDS_WEST          :=0;       //  (94)  // SHADYW.MAP
VAULT_13_MAP              :=0;       //  (95)  // VAULT13.MAP
BURIED_VAULT_MAP          :=0;       //  (96)  // VAULTBUR.MAP
VAULT_ENTRANCE            :=0;       //  (97)  // VAULTENT.MAP
NECROP_VAULT              :=0;       //  (98)  // VAULTNEC.MAP
WATERSHED                 :=0;       //  (99)  // WATRSHD.MAP


GANG_WAR                  :=0;       //  (128) // are the gangs gone?

BLADES_HELP               :=0;       //  (131) // will the blades help you knock out the children?
TRAIN_FOLLOWERS           :=0;       //  (132) // did the player train the followers?
FIND_AGENT                :=0;       //  (133) // find the double agent in the followers
TANGLER_DEAD              :=0;       //  (134) // is Tangler dead?
BECOME_BLADE              :=0;       //  (135) // are you a Blade?
BLADES_LEFT               :=41;      //  (136) // How many Blades are left
RIPPERS_LEFT              :=41;      //  (137) // How many Rippers are left
FIX_FARM                  :=0;       //  (138) // Fix the underground Farm
START_POWER               :=0;       //  (139) // has the power been started in the Glow?
WEAPONS_ARMED             :=0;       //  (140) // are the weapons armed in the Glow?
FOUND_DISK                :=0;       //  (141) // have you found the access code disk?
WEAPON_LOCKER             :=0;       //  (142) // are the weapon depot security systems armed?
SAVE_SINTHIA              :=0;       //  (143) // Cell baby held hostage in Junktown
SAUL_QUEST                :=0;       //  (144) // some quest which still needs to be written
TRISH_QUEST               :=0;       //  (145) // some quest not written yet
VATS_ALERT                :=0;       //  (146) // are the Vats on Alert?
VATS_COUNTDOWN            :=0;       //  (147)
FOLLOWERS_INVADED_DATE    :=90;       //  (148)
NECROPOLIS_INVADED_DATE   :=110;       //  (149)
THE_HUB_INVADED_DATE      :=140;       //  (150)
BROTHERHOOD_INVADED_DATE  :=170;       //  (151)
JUNKTOWN_INVADED_DATE     :=200;       //  (152)
SHADY_SANDS_INVADED_DATE  :=230;       //  (153)
VAULT_13_INVADED_DATE     :=500;       //  (154)
PLAYER_REPUATION          :=0;        //  (155)
BERSERKER_REPUTATION      :=0;         //   (156)
CHAMPION_REPUTATION       :=0;         //   (157)
CHILDKILLER_REPUATION     :=0;        // (158)
GOOD_MONSTER              :=0;       //  (159)
BAD_MONSTER               :=0        //   (160)
ARTIFACT_DISK             :=0;       //  (161) // Brotherhood artifact disk
FEV_DISK                  :=0;       //  (162) // describes the FEV virus
SECURITY_DISK             :=0;       //  (163) // has the security code to lower weapons
ALPHA_DISK                :=0;       //  (164) // experiment. random stuff
DELTA_DISK                :=0;       //  (165) // just another experiment
VREE_DISK                 :=0;       //  (166) // has the autopsy of the Super Mutants
HONOR_DISK                :=0;       //  (167) // has the honor code of the Brotherhood
RENT_TIME                 :=0;       //  (168) // how long the player rents the room
SAUL_STATUS               :=0;       //  (169) // What is the status of Saul?
GIZMO_STATUS              :=0;       //  (170) // How is Gizmo doing?
GANGS                     :=0;       //  (171)
GANG_BEGONE               :=0;       //    (172)
GANG_1                    :=0;      //  (173)
GANG_2                    :=0;      //  (174)
FOOLS_SCOUT               :=0;      //  (175)
FSCOUT_1                  :=0;      //  (176)
CRYPTS_SCOUT              :=0;      //  (177)
CSCOUT_1                  :=0;      //  (178)
POWER                     :=0;      //  (179)
POWER_GENERATOR           :=0;      //  (180)
GENERATOR_1               :=0;      //  (181)
GENERATOR_2               :=0;      //  (182)
GENERATOR_3               :=0;      //  (183)
PEASANTS                  :=0;      //  (184)
DOG_PHIL                  :=0;      //  (185)
DOG_1                     :=0;      //  (186)
DOG_2                     :=0;      //  (187)
WATER_THIEF               :=0;       //  (188)
NUKA_COLA_ADDICT          :=0;      //  (189)
BUFF_OUT_ADDICT           :=0;      //      (190)
MENTATS_ADDICT            :=0;      //      (191)
PSYCHO_ADDICT             :=0;      //      (192)
RADAWAY_ADDICT            :=0;      //      (193)
ALOCHOL_ADDICT            :=0;      //      (194)               // makes the names go together better
CATHEDRAL_ENEMY           :=0;       //       (195)
MORPHEUS_KNOWN            :=0;          //      196
KNOW_NIGHTKIN             :=0;          //      197
PC_WANTED                 :=0;          //      198
CRIMSON_CARAVANS_STATUS   :=0;          //      199
WATER_MERCHANTS_STATUS    :=0;          //      200
FARGO_TRADERS_STATUS      :=0;          //      201
UNDERGROUND_STATUS        :=0;          //      202
DECKER_STATUS             :=0;          //      203
CC_JOB                    :=0;          //      204
WATER_JOB                 :=0;          //      205
FARGO_JOB                 :=0;          //      206
Loxley_known              :=0;          //      207
MARK_DEATHCLAW            :=0;          //      208
MUTANT_DISK               :=0;          //      209
BROTHER_HISTORY           :=0;          //      210
SOPHIA_DISK               :=0;          //      211
MAXSON_DISK               :=0;          //      212
TANDI_ESCAPE              :=0;          //      213
RAID_LOOTING              :=0;          //      214
CVAN_DRIVER               :=0;          //      215
CVAN_GUARD                :=0;          //      216
TANDI_HEREBEFORE          :=0;          //      217
TALKED_ABOUT_TANDI        :=0;          //      218
DECKER_KNOWN              :=0;          //      219
WANTED_FOR_MURDER         :=0;          //      220
GREENE_DEAD               :=0;          //      221
WANTED_THEFT              :=0;          //      222
BROTHERHOOD_INVASION      :=0;          //      223
GLOW_POWER                :=1;          //      224
HUB_FILLER_28             :=0;          //      225
HUB_FILLER_29             :=0;          //      226
HUB_FILLER_30             :=0;          //      227
NUM_RADIO                 :=0;           //      228
RADIO_MISTAKE             :=0;           //      229
CHANNEL_SLOT_DOWN         :=0;           //      230
MASTER_FILLER_4           :=0;           //      231
MASTER_FILLER_5           :=0;           //      232
MASTER_FILLER_6           :=0;           //      233
MASTER_FILLER_7           :=0;           //      234
MASTER_FILLER_8           :=0;           //      235
MASTER_FILLER_9           :=0;           //      236
MASTER_FILLER_10          :=0;           //      237
CALM_REBELS               :=0;           //      238
CURE_JARVIS               :=0;           //      239
MAKE_ANTIDOTE             :=0;           //      240
MORPHEUS_DELIVERS_PLAYER  :=0;          //       (241)
DESTROY_VATS              :=0;           //      242
DESTROY_MASTER            :=0;           //      243
KATJA_STATUS              :=0;           //      244
ENEMY_VAULT_13            :=0;           //      245
ENEMY_SHADY_SANDS         :=0;           //      246
ENEMY_JUNKTOWN            :=0;           //      247
ENEMY_HUB                 :=0;           //      248
ENEMY_NECROPOLIS          :=0;           //      249
ENEMY_BROTHERHOOD         :=0;           //      250
ENEMY_ADYTUM              :=0;           //      251
ENEMY_RIPPERS             :=0;           //      252
ENEMY_BLADES              :=0;           //      253
ENEMY_RAIDERS             :=0;           //      254
ENEMY_CATHEDRAL           :=0;           //      255
ENEMY_FOLLOWERS           :=0;           //      256
GENERIC_FILLER_20         :=0;           //      257
WATER_CHIP_1              :=0;           //      258
WATER_CHIP_2              :=0;           //      259
WATER_CHIP_3              :=0;           //      260
WATER_CHIP_4              :=0;           //      261
WATER_CHIP_5              :=0;           //      262
WATER_CHIP_6              :=0;           //      263
WATER_CHIP_7              :=0;           //      264
WATER_CHIP_8              :=0;           //      265
WATER_CHIP_9              :=0;           //      266
WATER_CHIP_10             :=0;           //      267
WATER_CHIP_11             :=0;           //      268
WATER_CHIP_12             :=0;           //      269
WATER_CHIP_13             :=0;           //      270
WATER_CHIP_14             :=0;           //      271
WATER_CHIP_15             :=0;           //      272
DESTROY_VATS_1            :=0;          //       273
DESTROY_VATS_2            :=0;          //       274
DESTROY_VATS_3            :=0;          //       275
DESTROY_VATS_4            :=0;          //       276
DESTROY_VATS_5            :=0;          //       277
DESTROY_VATS_6            :=0;          //       278
DESTROY_VATS_7            :=0;          //       279
DESTROY_VATS_8            :=0;          //       280
DESTROY_VATS_9            :=0;          //       281
DESTROY_VATS_10           :=0;          //       282
DESTROY_VATS_11           :=0;          //       283
DESTROY_VATS_12           :=0;          //       284
DESTROY_VATS_13           :=0;          //       285
DESTROY_VATS_14           :=0;          //       286
DESTROY_VATS_15           :=0;          //       287
WATER_THIEF_1             :=0;          //       288
WATER_THIEF_2             :=0;          //       289
WATER_THIEF_3             :=0;          //       290
WATER_THIEF_4             :=0;          //       291
WATER_THIEF_5             :=0;          //       292
WATER_THIEF_6             :=0;          //       293
WATER_THIEF_7             :=0;          //       294
WATER_THIEF_8             :=0;          //       295
WATER_THIEF_9             :=0;          //       296
WATER_THIEF_10            :=0;          //       297
CALM_REBELS_1             :=0;          //       298
CALM_REBELS_2             :=0;          //       299
CALM_REBELS_3             :=0;          //       300
CALM_REBELS_4             :=0;          //       301
CALM_REBELS_5             :=0;          //       302
CALM_REBELS_6             :=0;          //       303
CALM_REBELS_7             :=0;          //       304
DESTROY_MASTER_1          :=0;          //       305
DESTROY_MASTER_2          :=0;          //       306
DESTROY_MASTER_3          :=0;          //       307
DESTROY_MASTER_4          :=0;          //       308
DESTROY_MASTER_5          :=0;          //       309
DESTROY_MASTER_6          :=0;          //       310
DESTROY_MASTER_7          :=0;          //       311
DESTROY_MASTER_8          :=0;          //       312
DESTROY_MASTER_9          :=0;          //       313
DESTROY_MASTER_10         :=0;          //       314
DESTROY_MASTER_11         :=0;          //       315
DESTROY_MASTER_12         :=0;          //       316
DESTROY_MASTER_13         :=0;          //       317
DESTROY_MASTER_14         :=0;          //       318
DESTROY_MASTER_15         :=0;          //       319
STOP_SCORPIONS_1          :=0;          //       320
STOP_SCORPIONS_2          :=0;          //       321
STOP_SCORPIONS_3          :=0;          //       322
STOP_SCORPIONS_4          :=0;          //       323
STOP_SCORPIONS_5          :=0;          //       324
STOP_SCORPIONS_6          :=0;          //       325
STOP_SCORPIONS_7          :=0;          //       326
STOP_SCORPIONS_8          :=0;          //       327
STOP_SCORPIONS_9          :=0;          //       328
STOP_SCORPIONS_10         :=0;          //       329
SAVE_TANDI_1              :=0;          //       330
SAVE_TANDI_2              :=0;          //       331
SAVE_TANDI_3              :=0;          //       332
SAVE_TANDI_4              :=0;          //       333
SAVE_TANDI_5              :=0;          //       334
SAVE_TANDI_6              :=0;          //       335
SAVE_TANDI_7              :=0;          //       336
SAVE_TANDI_8              :=0;          //       337
SAVE_TANDI_9              :=0;          //       338
SAVE_TANDI_10             :=0;          //       339
CURE_JARVIS_1             :=0;          //       340
CURE_JARVIS_2             :=0;          //       341
CURE_JARVIS_3             :=0;          //       342
CURE_JARVIS_4             :=0;          //       343
CURE_JARVIS_5             :=0;          //       344
CURE_JARVIS_6             :=0;          //       345
CURE_JARVIS_7             :=0;          //       346
CURE_JARVIS_8             :=0;          //       347
CURE_JARVIS_9             :=0;          //       348
CURE_JARVIS_10            :=0;          //       349
MAKE_ANTIDOTE_1           :=0;          //       350
MAKE_ANTIDOTE_2           :=0;          //       351
MAKE_ANTIDOTE_3           :=0;          //       352
MAKE_ANTIDOTE_4           :=0;          //       353
MAKE_ANTIDOTE_5           :=0;          //       354
MAKE_ANTIDOTE_6           :=0;          //       355
MAKE_ANTIDOTE_7           :=0;          //       356
MAKE_ANTIDOTE_8           :=0;          //       357
MAKE_ANTIDOTE_9           :=0;          //       358
MAKE_ANTIDOTE_10          :=0;          //       359
KILL_MUTANTS_1            :=0;          //       360
KILL_MUTANTS_2            :=0;          //       361
KILL_MUTANTS_3            :=0;          //       362
KILL_MUTANTS_4            :=0;          //       363
KILL_MUTANTS_5            :=0;          //       364
KILL_MUTANTS_6            :=0;          //       365
KILL_MUTANTS_7            :=0;          //       366
KILL_MUTANTS_8            :=0;          //       367
KILL_MUTANTS_9            :=0;          //       368
KILL_MUTANTS_10           :=0;          //       369
FIX_NECROP_PUMP_1         :=0;          //       370
FIX_NECROP_PUMP_2         :=0;          //       371
FIX_NECROP_PUMP_3         :=0;          //       372
FIX_NECROP_PUMP_4         :=0;          //       373
FIX_NECROP_PUMP_5         :=0;          //       374
FIX_NECROP_PUMP_6         :=0;          //       375
FIX_NECROP_PUMP_7         :=0;          //       376
FIX_NECROP_PUMP_8         :=0;          //       377
FIX_NECROP_PUMP_9         :=0;          //       378
FIX_NECROP_PUMP_10        :=0;          //       379
KILL_KILLIAN_1            :=0;          //       380
KILL_KILLIAN_2            :=0;          //       381
KILL_KILLIAN_3            :=0;          //       382
KILL_KILLIAN_4            :=0;          //       383
KILL_KILLIAN_5            :=0;          //       384
KILL_KILLIAN_6            :=0;          //       385
KILL_KILLIAN_7            :=0;          //       386
KILL_KILLIAN_8            :=0;          //       387
KILL_KILLIAN_9            :=0;          //       388
KILL_KILLIAN_10           :=0;          //       389
STOP_GIZMO_1              :=0;          //       390
STOP_GIZMO_2              :=0;          //       391
STOP_GIZMO_3              :=0;          //       392
STOP_GIZMO_4              :=0;          //       393
STOP_GIZMO_5              :=0;          //       394
STOP_GIZMO_6              :=0;          //       395
STOP_GIZMO_7              :=0;          //       396
STOP_GIZMO_8              :=0;          //       397
STOP_GIZMO_9              :=0;          //       398
STOP_GIZMO_10             :=0;          //       399
SAVE_SINTHIA_1            :=0;          //       400
SAVE_SINTHIA_2            :=0;          //       401
SAVE_SINTHIA_3            :=0;          //       402
SAVE_SINTHIA_4            :=0;          //       403
SAVE_SINTHIA_5            :=0;          //       404
SAVE_SINTHIA_6            :=0;          //       405
SAVE_SINTHIA_7            :=0;          //       406
SAVE_SINTHIA_8            :=0;          //       407
SAVE_SINTHIA_9            :=0;          //       408
SAVE_SINTHIA_10           :=0;          //       409
TRISH_QUEST_1             :=0;          //       410
TRISH_QUEST_2             :=0;          //       411
TRISH_QUEST_3             :=0;          //       412
TRISH_QUEST_4             :=0;          //       413
TRISH_QUEST_5             :=0;          //       414
TRISH_QUEST_6             :=0;          //       415
TRISH_QUEST_7             :=0;          //       416
TRISH_QUEST_8             :=0;          //       417
TRISH_QUEST_9             :=0;          //       418
TRISH_QUEST_10            :=0;          //       419
SAUL_QUEST_1              :=0;          //       420
SAUL_QUEST_2              :=0;          //       421
SAUL_QUEST_3              :=0;          //       422
SAUL_QUEST_4              :=0;          //       423
SAUL_QUEST_5              :=0;          //       424
SAUL_QUEST_6              :=0;          //       425
SAUL_QUEST_7              :=0;          //       426
SAUL_QUEST_8              :=0;          //       427
SAUL_QUEST_9              :=0;          //       428
SAUL_QUEST_10             :=0;          //       429
STEAL_NECKLACE_1          :=0;          //       430
STEAL_NECKLACE_2          :=0;          //       431
STEAL_NECKLACE_3          :=0;          //       432
STEAL_NECKLACE_4          :=0;          //       433
STEAL_NECKLACE_5          :=0;          //       434
STEAL_NECKLACE_6          :=0;          //       435
STEAL_NECKLACE_7          :=0;          //       436
STEAL_NECKLACE_8          :=0;          //       437
STEAL_NECKLACE_9          :=0;          //       438
STEAL_NECKLACE_10         :=0;          //       439
KILL_JAIN_1               :=0;          //       440
KILL_JAIN_2               :=0;          //       441
KILL_JAIN_3               :=0;          //       442
KILL_JAIN_4               :=0;          //       443
KILL_JAIN_5               :=0;          //       444
KILL_JAIN_6               :=0;          //       445
KILL_JAIN_7               :=0;          //       446
KILL_JAIN_8               :=0;          //       447
KILL_JAIN_9               :=0;          //       448
KILL_JAIN_10              :=0;          //       449
KILL_MERCHANT_1           :=0;          //       450
KILL_MERCHANT_2           :=0;          //       451
KILL_MERCHANT_3           :=0;          //       452
KILL_MERCHANT_4           :=0;          //       453
KILL_MERCHANT_5           :=0;          //       454
KILL_MERCHANT_6           :=0;          //       455
KILL_MERCHANT_7           :=0;          //       456
KILL_MERCHANT_8           :=0;          //       457
KILL_MERCHANT_9           :=0;          //       458
KILL_MERCHANT_10          :=0;          //       459
DEATHCLAW_SEED_1          :=0;          //       460
DEATHCLAW_SEED_2          :=0;          //       461
DEATHCLAW_SEED_3          :=0;          //       462
DEATHCLAW_SEED_4          :=0;          //       463
DEATHCLAW_SEED_5          :=0;          //       464
DEATHCLAW_SEED_6          :=0;          //       465
DEATHCLAW_SEED_7          :=0;          //       466
DEATHCLAW_SEED_8          :=0;          //       467
DEATHCLAW_SEED_9          :=0;          //       468
FRY_DEAD                  :=0;          //       469
MISSING_CARAVAN_1         :=0;          //       470
MISSING_CARAVAN_2         :=0;          //       471
MISSING_CARAVAN_3         :=0;          //       472
MISSING_CARAVAN_4         :=0;          //       473
MISSING_CARAVAN_5         :=0;          //       474
MISSING_CARAVAN_6         :=0;          //       475
MISSING_CARAVAN_7         :=0;          //       476
MISSING_CARAVAN_8         :=0;          //       477
MISSING_CARAVAN_9         :=0;          //       478
MISSING_CARAVAN_10        :=0;          //       479
BECOME_INITIATE_1         :=0;          //       480
BECOME_INITIATE_2         :=0;          //       481
BECOME_INITIATE_3         :=0;          //       482
BECOME_INITIATE_4         :=0;          //       483
BECOME_INITIATE_5         :=0;          //       484
BECOME_INITIATE_6         :=0;          //       485
BECOME_INITIATE_7         :=0;          //       486
BECOME_INITIATE_8         :=0;          //       487
BECOME_INITIATE_9         :=0;          //       488
BECOME_INITIATE_10        :=0;          //       489
FIND_INITIATE_1           :=0;          //       490
FIND_INITIATE_2           :=0;          //       491
FIND_INITIATE_3           :=0;          //       492
FIND_INITIATE_4           :=0;          //       493
FIND_INITIATE_5           :=0;          //       494
FIND_INITIATE_6           :=0;          //       495
FIND_INITIATE_7           :=0;          //       496
FIND_INITIATE_8           :=0;          //       497
FIND_INITIATE_9           :=0;          //       498
FIND_INITIATE_10          :=0;          //       499
ROMEO_JULIET_1            :=0;          //       500
STOP_GANGS_1              :=0;          //       501
STOP_GANGS_2              :=0;          //       502
STOP_GANGS_3              :=0;          //       503
STOP_GANGS_4              :=0;          //       504
STOP_GANGS_5              :=0;          //       505
STOP_GANGS_6              :=0;          //       506
STOP_GANGS_7              :=0;          //       507
STOP_GANGS_8              :=0;          //       508
STOP_GANGS_9              :=0;          //       509
CATCH_SPY_1               :=0;          //       510
CATCH_SPY_2               :=0;          //       511
CATCH_SPY_3               :=0;          //       512
CATCH_SPY_4               :=0;          //       513
CATCH_SPY_5               :=0;          //       514
CATCH_SPY_6               :=0;          //       515
CATCH_SPY_7               :=0;          //       516
CATCH_SPY_8               :=0;          //       517
CATCH_SPY_9               :=0;          //       518
CATCH_SPY_10              :=0;          //       519
CATCH_SPY_11              :=0;          //       520
CATCH_SPY_12              :=0;          //       521
CATCH_SPY_13              :=0;          //       522
CATCH_SPY_14              :=0;          //       523
CATCH_SPY_15              :=0;          //       524
CATCH_SPY_16              :=0;          //       525
CATCH_SPY_17              :=0;          //       526
CATCH_SPY_18              :=0;          //       527
CATCH_SPY_19              :=0;          //       528
BECOME_BLADE_1            :=0;          //       529
BECOME_BLADE_2            :=0;          //       530
BECOME_BLADE_3            :=0;          //       531
BECOME_BLADE_4            :=0;          //       532
DELIVER_PACKAGE_1         :=0;          //       533
DELIVER_PACKAGE_2         :=0;          //       534
DELIVER_PACKAGE_3         :=0;          //       535
DELIVER_PACKAGE_4         :=0;          //       536
DELIVER_PACKAGE_5         :=0;          //       537
FIX_FARM_1                :=0;          //       538
FIX_FARM_2                :=0;          //       539
FIX_FARM_3                :=0;          //       540
FIX_FARM_4                :=0;          //       541
LOST_BROTHER_1            :=0;          //       542
LOST_BROTHER_2            :=0;          //       543
LOST_BROTHER_3            :=0;          //       544
LOST_BROTHER_4            :=0;          //       545
LOST_BROTHER_5            :=0;          //       546
POWER_GLOW_1              :=0;          //       547
DISARM_TRAPS_1            :=0;          //       548
DISARM_TRAPS_2            :=0;          //       549
DISARM_TRAPS_3            :=0;          //       550
MAX_MUTANTS               :=5;          //       551
TIME_CHIP_GONE            :=0;          //       552
SET_DEAD                  :=0;          //       553
MUTANTS_GONE              :=0;          //       554
BUST_SKULZ                :=0;          //       555 // Helping Lars to bust the Skulz gang in Junktown
SHERRY_TURNS              :=0;          //       556 // Sherry gives information on the Skulz.
TRISH_STATUS		  :=0;	    //	 557 // Tells if dude has met Trish in Junktown.
MARK_V13_1                :=1;          //       558
MARK_V13_2                :=1;          //      559
MARK_V13_3                :=1;          //      560
MARK_V13_4                :=1;          //      561
MARK_V15_1                :=1;          //      562
MARK_V15_2                :=0;          //      563
MARK_V15_3                :=0;          //      564
MARK_V15_4                :=0;          //      565
MARK_SHADY_1              :=1           //      566
MARK_SHADY_2              :=0;          //      567
MARK_SHADY_3              :=0;          //      568
MARK_JUNK_1               :=1;          //      569
MARK_JUNK_2               :=0;          //      570
MARK_JUNK_3               :=0;          //      571
MARK_RAIDERS_1            :=1;          //      572
MARK_NECROP_1             :=1;          //      573
MARK_NECROP_2             :=0;          //      574
MARK_NECROP_3             :=0;          //      575
MARK_HUB_1                :=1;          //      576
MARK_HUB_2                :=0;          //      577
MARK_HUB_3                :=0;          //      578
MARK_HUB_4                :=0;          //      579
MARK_HUB_5                :=0;          //      580
MARK_HUB_6                :=0;          //      581
MARK_BROTHER_1            :=1;          //      582
MARK_BROTHER_2            :=0;          //      583
MARK_BROTHER_3            :=0;          //      584
MARK_BROTHER_4            :=0;          //      585
MARK_BROTHER_5            :=0;          //      586
MARK_BASE_1               :=1;          //      587
MARK_BASE_2               :=0;          //      588
MARK_BASE_3               :=0;          //      589
MARK_BASE_4               :=0;          //      590
MARK_BASE_5               :=0;          //      591
MARK_GLOW_1               :=1;          //      592
MARK_GLOW_2               :=0;          //      593
MARK_LA_1                 :=1;          //      594
MARK_LA_2                 :=0;          //      595
MARK_LA_3                 :=0;          //      596
MARK_LA_4                 :=0;          //      597
MARK_LA_5                 :=0;          //      598
MARK_CHILD_1              :=1;          //      599
MARK_CHILD_2              :=0;          //      600
STRANGER_STATUS           :=0;          //      (601)
GAME_DIFFICULTY           :=0;          //      602
RUNNING_BURNING_GUY       :=0;          //      603
ARADESH_STATUS            :=0;          //      604
RHOMBUS_STATUS            :=0;          //      605
KIND_TO_HAROLD            :=0;          //      606
GARRET_STATUS             :=0;          //      607
RADIO_COMPUTER_OFF        :=0;      //  608
FORCE_FIELDS_OFF          :=0;	    //	609
FIELD_COMPUTER_MODIFIED	  :=0;	    //	610
GARLS_FRIEND              :=0;          // 611
ZIMMERMAN                 :=0;          // (612) // Mikey's stuff for Boneyard.
BLADES                    :=0;          // (613)
GUN_RUNNER                :=0;          // (614)
CHEMISTRY_BOOK            :=0;          // (615)
ENEMY_REGULATOR           :=0;          // (616)
ENEMY_BLADE               :=0;          // (617)  
*/
    #endregion
    
    #region enums

    public enum QuestStatus
    {
        Unstarted,
        Started,
        Completed,
        Failed
    }
    #endregion
}
