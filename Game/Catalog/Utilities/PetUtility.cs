﻿namespace Plus.Game.Catalog.Utilities
{
    using System;
    using Plus.Utilities;
    using Rooms.AI;

    public static class PetUtility
    {
        public static bool CheckPetName(string name)
        {
            if (name.Length < 1 || name.Length > 16)
            {
                return false;
            }

            if (!HabboUtilities.IsValidAlphaNumeric(name))
            {
                return false;
            }

            return true;
        }

        public static Pet CreatePet(int userId, string name, int type, string race, string colour)
        {
            var pet = new Pet(0, userId, 0, name, type, race, colour, 0, 100, 100, 0, UnixUtilities.GetNow(), 0, 0, 0.0, 0, 0, 0, -1, "-1");

            using (var dbClient = Program.DatabaseManager.GetQueryReactor())
            {
                dbClient.SetQuery("INSERT INTO bots (user_id,name, ai_type) VALUES (" + pet.OwnerId + ",@" + pet.PetId + "name, 'pet')");
                dbClient.AddParameter(pet.PetId + "name", pet.Name);
                pet.PetId = Convert.ToInt32(dbClient.InsertQuery());

                dbClient.SetQuery("INSERT INTO bots_petdata (id,type,race,color,experience,energy,createstamp) VALUES (" + pet.PetId + ", " + pet.Type + ",@" + pet.PetId + "race,@" + pet.PetId + "color,0,100,UNIX_TIMESTAMP())");
                dbClient.AddParameter(pet.PetId + "race", pet.Race);
                dbClient.AddParameter(pet.PetId + "color", pet.Color);
                dbClient.RunQuery();
            }
            return pet;
        }
    }
}
