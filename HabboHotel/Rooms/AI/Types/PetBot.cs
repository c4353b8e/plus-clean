namespace Plus.HabboHotel.Rooms.AI.Types
{
    using System;
    using System.Linq;
    using Core.Logging;
    using GameClients;
    using PathFinding;
    using Utilities;

    public class PetBot : BotAI
    {
        private static readonly ILogger Logger = new Logger<PetBot>();

        private int ActionTimer;
        private int EnergyTimer;
        private int SpeechTimer;

        public PetBot(int VirtualId)
        {
            SpeechTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
            ActionTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 30 + VirtualId);
            EnergyTimer = new Random((VirtualId ^ 2) + DateTime.Now.Millisecond).Next(10, 60);
        }

        private void RemovePetStatus()
        {
            var Pet = GetRoomUser();
            if (Pet != null)
            {
                foreach (var KVP in Pet.Statusses.ToList())
                {
                    if (Pet.Statusses.ContainsKey(KVP.Key))
                    {
                        Pet.Statusses.Remove(KVP.Key);
                    }
                }
            }
        }

        public override void OnSelfEnterRoom()
        {
            var nextCoord = GetRoom().GetGameMap().GetRandomWalkableSquare();
            //int randomX = RandomNumber.GenerateNewRandom(0, GetRoom().Model.MapSizeX);
            //int randomY = RandomNumber.GenerateNewRandom(0, GetRoom().Model.MapSizeY);
            if (GetRoomUser() != null)
            {
                GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
            }
        }

        public override void OnSelfLeaveRoom(bool Kicked)
        {
        }


        public override void OnUserEnterRoom(RoomUser User)
        {
            if (User.GetClient() != null && User.GetClient().GetHabbo() != null)
            {
                var Pet = GetRoomUser();
                if (Pet != null)
                {
                    if (User.GetClient().GetHabbo().Username == Pet.PetData.OwnerName)
                    {
                        var Speech = Program.GameContext.GetChatManager().GetPetLocale().GetValue("welcome.speech.pet" + Pet.PetData.Type);
                        var rSpeech = Speech[RandomNumber.GenerateRandom(0, Speech.Length - 1)];
                        Pet.Chat(rSpeech);
                    }
                }
            }
        }

        public override void OnUserLeaveRoom(GameClient Client)
        {
        }

        public override void OnUserShout(RoomUser User, string Message)
        {
        }

        public override void OnTimerTick()
        {
            var Pet = GetRoomUser();
            if (Pet == null)
            {
                return;
            }


            #region Speech

            if (SpeechTimer <= 0)
            {
                if (Pet.PetData.DBState != PetDatabaseUpdateState.NeedsInsert)
                {
                    Pet.PetData.DBState = PetDatabaseUpdateState.NeedsUpdate;
                }

                if (Pet != null)
                {
                    var RandomSpeech = new Random();
                    RemovePetStatus();

                    var Speech = Program.GameContext.GetChatManager().GetPetLocale().GetValue("speech.pet" + Pet.PetData.Type);
                    var rSpeech = Speech[RandomNumber.GenerateRandom(0, Speech.Length - 1)];

                    if (rSpeech.Length != 3)
                    {
                        Pet.Chat(rSpeech);
                    }
                    else
                    {
                        Pet.Statusses.Add(rSpeech, TextHandling.GetString(Pet.Z));
                    }
                }
                SpeechTimer = RandomNumber.GenerateNewRandom(20, 120);
            }
            else
            {
                SpeechTimer--;
            }

            #endregion

            #region Actions

            if (ActionTimer <= 0)
            {
                try
                {
                    RemovePetStatus();
                    ActionTimer = RandomNumber.GenerateRandom(15, 40 + GetRoomUser().PetData.VirtualId);
                    if (!GetRoomUser().RidingHorse)
                    {
                        // Remove Status
                        RemovePetStatus();

                        var nextCoord = GetRoom().GetGameMap().GetRandomWalkableSquare();
                        if (GetRoomUser().CanWalk)
                        {
                            GetRoomUser().MoveTo(nextCoord.X, nextCoord.Y);
                        }
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(e);
                }
            }
            else
            {
                ActionTimer--;
            }

            #endregion

            #region Energy

            if (EnergyTimer <= 0)
            {
                RemovePetStatus(); // Remove Status

                Pet.PetData.PetEnergy(true); // Add Energy

                EnergyTimer = RandomNumber.GenerateRandom(30, 120); // 2 Min Max
            }
            else
            {
                EnergyTimer--;
            }

            #endregion
        }

        #region Commands

        public override void OnUserSay(RoomUser User, string Message)
        {
            if (User == null)
            {
                return;
            }

            var Pet = GetRoomUser();
            if (Pet == null)
            {
                return;
            }

            if (Pet.PetData.DBState != PetDatabaseUpdateState.NeedsInsert)
            {
                Pet.PetData.DBState = PetDatabaseUpdateState.NeedsUpdate;
            }

            if (Message.ToLower().Equals(Pet.PetData.Name.ToLower()))
            {
                Pet.SetRot(Rotation.Calculate(Pet.X, Pet.Y, User.X, User.Y), false);
                return;
            }

            //if (!Pet.Statusses.ContainsKey("gst thr"))
            //    Pet.Statusses.Add("gst thr", TextHandling.GetString(Pet.Z));

            if (Message.ToLower().StartsWith(Pet.PetData.Name.ToLower() + " ") && User.GetClient().GetHabbo().Username.ToLower() == Pet.PetData.OwnerName.ToLower() || Message.ToLower().StartsWith(Pet.PetData.Name.ToLower() + " ") && Program.GameContext.GetChatManager().GetPetCommands().TryInvoke(Message.Substring(Pet.PetData.Name.ToLower().Length + 1)) == 8)
            {
                var Command = Message.Substring(Pet.PetData.Name.ToLower().Length + 1);

                var r = RandomNumber.GenerateRandom(1, 8); // Made Random
                if (Pet.PetData.Energy > 10 && r < 6 || Pet.PetData.Level > 15 || Program.GameContext.GetChatManager().GetPetCommands().TryInvoke(Command) == 8)
                {
                    RemovePetStatus(); // Remove Status

                    switch (Program.GameContext.GetChatManager().GetPetCommands().TryInvoke(Command))
                    {
                        // TODO - Level you can use the commands at...



                        #region free

                        case 1:
                            RemovePetStatus();

                            //int randomX = RandomNumber.GenerateNewRandom(0, GetRoom().Model.MapSizeX);
                            //int randomY = RandomNumber.GenerateNewRandom(0, GetRoom().Model.MapSizeY);
                            var nextCoord = GetRoom().GetGameMap().GetRandomWalkableSquare();
                            Pet.MoveTo(nextCoord.X, nextCoord.Y);

                            Pet.PetData.Addexperience(10); // Give XP

                            break;

                        #endregion

                        #region here

                        case 2:

                            RemovePetStatus();

                            var NewX = User.X;
                            var NewY = User.Y;

                            ActionTimer = 30; // Reset ActionTimer

                            #region Rotation

                            if (User.RotBody == 4)
                            {
                                NewY = User.Y + 1;
                            }
                            else if (User.RotBody == 0)
                            {
                                NewY = User.Y - 1;
                            }
                            else if (User.RotBody == 6)
                            {
                                NewX = User.X - 1;
                            }
                            else if (User.RotBody == 2)
                            {
                                NewX = User.X + 1;
                            }
                            else if (User.RotBody == 3)
                            {
                                NewX = User.X + 1;
                                NewY = User.Y + 1;
                            }
                            else if (User.RotBody == 1)
                            {
                                NewX = User.X + 1;
                                NewY = User.Y - 1;
                            }
                            else if (User.RotBody == 7)
                            {
                                NewX = User.X - 1;
                                NewY = User.Y - 1;
                            }
                            else if (User.RotBody == 5)
                            {
                                NewX = User.X - 1;
                                NewY = User.Y + 1;
                            }

                            #endregion

                            Pet.PetData.Addexperience(10); // Give XP

                            Pet.MoveTo(NewX, NewY);
                            break;

                        #endregion

                        #region sit

                        case 3:
                            // Remove Status
                            RemovePetStatus();

                            Pet.PetData.Addexperience(10); // Give XP

                            // Add Status
                            Pet.Statusses.Add("sit", TextHandling.GetString(Pet.Z));
                            Pet.UpdateNeeded = true;

                            ActionTimer = 25;
                            EnergyTimer = 10;
                            break;

                        #endregion

                        #region lay

                        case 4:
                            // Remove Status
                            RemovePetStatus();

                            // Add Status
                            Pet.Statusses.Add("lay", TextHandling.GetString(Pet.Z));
                            Pet.UpdateNeeded = true;

                            Pet.PetData.Addexperience(10); // Give XP

                            ActionTimer = 30;
                            EnergyTimer = 5;
                            break;

                        #endregion

                        #region dead

                        case 5:
                            // Remove Status
                            RemovePetStatus();

                            // Add Status 
                            Pet.Statusses.Add("ded", TextHandling.GetString(Pet.Z));
                            Pet.UpdateNeeded = true;

                            Pet.PetData.Addexperience(10); // Give XP

                            // Don't move to speak for a set amount of time.
                            SpeechTimer = 45;
                            ActionTimer = 30;

                            break;

                        #endregion

                        #region sleep

                        case 6:
                            // Remove Status
                            RemovePetStatus();

                            Pet.Chat("ZzzZZZzzzzZzz");
                            Pet.Statusses.Add("lay", TextHandling.GetString(Pet.Z));
                            Pet.UpdateNeeded = true;

                            Pet.PetData.Addexperience(10); // Give XP

                            // Don't move to speak for a set amount of time.
                            EnergyTimer = 5;
                            SpeechTimer = 30;
                            ActionTimer = 45;
                            break;

                        #endregion

                        #region jump

                        case 7:
                            // Remove Status
                            RemovePetStatus();

                            // Add Status 
                            Pet.Statusses.Add("jmp", TextHandling.GetString(Pet.Z));
                            Pet.UpdateNeeded = true;

                            Pet.PetData.Addexperience(10); // Give XP

                            // Don't move to speak for a set amount of time.
                            EnergyTimer = 5;
                            SpeechTimer = 10;
                            ActionTimer = 5;
                            break;

                        #endregion

                        #region breed
                        case 46:

                            break;
                        #endregion

                        default:
                            var Speech = Program.GameContext.GetChatManager().GetPetLocale().GetValue("pet.unknowncommand");

                            Pet.Chat(Speech[RandomNumber.GenerateRandom(0, Speech.Length - 1)]);
                            break;
                    }
                    Pet.PetData.PetEnergy(false); // Remove Energy
                }
                else
                {
                    RemovePetStatus(); // Remove Status

                    if (Pet.PetData.Energy < 10)
                    {
                        var Speech = Program.GameContext.GetChatManager().GetPetLocale().GetValue("pet.tired");

                        var RandomSpeech = new Random();
                        Pet.Chat(Speech[RandomNumber.GenerateRandom(0, Speech.Length - 1)]);

                        Pet.Statusses.Add("lay", TextHandling.GetString(Pet.Z));
                        Pet.UpdateNeeded = true;

                        SpeechTimer = 50;
                        ActionTimer = 45;
                        EnergyTimer = 5;
                    }
                    else
                    {
                        var Speech = Program.GameContext.GetChatManager().GetPetLocale().GetValue("pet.lazy");

                        var RandomSpeech = new Random();
                        Pet.Chat(Speech[RandomNumber.GenerateRandom(0, Speech.Length - 1)]);

                        Pet.PetData.PetEnergy(false); // Remove Energy
                    }
                }
            }
            //Pet = null;
        }

        #endregion
    }
}