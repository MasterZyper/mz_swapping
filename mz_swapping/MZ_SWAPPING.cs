/*
 MZ-Swapping, swaps the Player-Ped with an NPC-Ped
Copyright (C) 27.08.2019 MasterZyper 🐦
Contact: masterzyper@reloaded-server.de
You like to get a FiveM-Server? 
Visit ZapHosting*: https://zap-hosting.com/a/17444fc14f5749d607b4ca949eaf305ed50c0837

Support us on Patreon: https://www.patreon.com/gtafivemorg

For help with this Script visit: https://gta-fivem.org/

This program is free software; you can redistribute it and/or modify it under the terms of the 
GNU General Public License as published by the Free Software Foundation; either version 3 of 
the License, or (at your option) any later version.
This program is distributed in the hope that it will be useful, but WITHOUT ANY WARRANTY;
without even the implied warranty of MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. 
See the GNU General Public License for more details.
You should have received a copy of the GNU General Public License along with this program; 
if not, see <http://www.gnu.org/licenses/>.

*Affiliate-Link: Euch entstehen keine Kosten oder Nachteile. Kauf über diesen Link erwirtschaftet eine kleine prozentuale Provision für mich.
 */
using CitizenFX.Core;
using CitizenFX.Core.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace mz_swapping
{
    public class MZ_SWAPPING : BaseScript
    {
        readonly Random rand = new Random();
        int error_attempts = 0;
        private Ped GetRandomPed(bool onlyHuman = false, bool inVehicle = false)
        {
            Ped[] peds = World.GetAllPeds();
            Random rand = new Random();
            Ped ped = null;
            //Maximal 30 versuche ein gültiges Ped zu finden 
            for (int i = 0; i < 30; i++)
            {
                ped = peds[rand.Next(0, peds.Length)];
                if (!ped.IsPlayer && !ped.IsDead)
                {
                    if (onlyHuman)
                    {
                        if (ped.IsHuman)
                        {
                            if (inVehicle)
                            {
                                if (ped.IsInVehicle())
                                {
                                    return ped;
                                }
                            }
                            else
                            {
                                return ped;
                            }
                        }
                    }
                    else
                    {
                        return ped;
                    }
                }
            }
            return null;
        }
        VehicleHash[] PlanHashes = new VehicleHash[]{
            VehicleHash.Shamal,
            VehicleHash.Jet,
            VehicleHash.Besra,
            VehicleHash.CargoPlane,
            VehicleHash.Titan,
            VehicleHash.Luxor,
            VehicleHash.Luxor2,
            VehicleHash.Cuban800,
            VehicleHash.Dodo,
            VehicleHash.Duster,
            VehicleHash.Hydra,
            VehicleHash.Jet,
            VehicleHash.Mammatus,
            VehicleHash.Miljet,
            VehicleHash.Nimbus,
            VehicleHash.Stunt,
            VehicleHash.Velum,
            VehicleHash.Velum2,
            VehicleHash.Vestra
        };
        private VehicleHash GenerateRandomPlaneHash()
        {
            return PlanHashes[rand.Next(0, PlanHashes.Length)];
        }
        private async Task<Vehicle> GenerateRandomPlaneInAir(Vector2 pos)
        {
            float GroundZ = World.GetGroundHeight(pos);
            Vector3 position = new Vector3(pos.X, pos.Y, GroundZ + 150);
            Vector3 landing_position = new Vector3(pos.X + rand.Next(-5000, 5000), pos.Y + rand.Next(-5000, 5000), GroundZ + 150);
            Ped pilot = await World.CreatePed(PedHash.Chimp, position);
            Vehicle plane = await World.CreateVehicle(GenerateRandomPlaneHash(), position);
            plane.Speed = 100f;
            plane.LandingGearState = VehicleLandingGearState.Closing;
            pilot.Task.WarpIntoVehicle(plane, VehicleSeat.Driver);
            await Delay(10);
            pilot.Task.LandPlane(landing_position, landing_position, plane);
            pilot.DrivingStyle = DrivingStyle.IgnorePathing;
            return plane;
        }
        public MZ_SWAPPING()
        {
            string resource_name = API.GetCurrentResourceName();
            string resource_author = "MasterZyper";
            string swap_cmd = API.GetResourceMetadata(API.GetCurrentResourceName(), "swap_cmd", 0);

            API.RegisterCommand(swap_cmd, new Action<int, List<object>, string>(async (player, value, raw) =>
            {
                Random rand = new Random();
                Ped target_ped = null;
                while (target_ped == null)
                {
                    try
                    {
                        target_ped = GetRandomPed(true);
                        if (Game.PlayerPed.IsInVehicle())
                        {
                            VehicleSeat seat = Game.PlayerPed.SeatIndex;
                            Vehicle veh = Game.PlayerPed.CurrentVehicle;
                            float speed = veh.Speed;
                            while (!veh.IsSeatFree(seat))
                            {
                                veh.GetPedOnSeat(seat).Position = new Vector3(veh.Position.X, veh.Position.Y, veh.Position.Z + 2);
                                await Delay(0);
                            }
                            Ped cloned_player = Game.PlayerPed.Clone();
                            await Delay(0);
                            cloned_player.Task.WarpIntoVehicle(veh, seat);
                            await Delay(0);
                            if (seat == VehicleSeat.Driver)
                            {
                                veh.Speed = speed;
                                cloned_player.Task.DriveTo(veh, new Vector3(rand.Next(-2000, 2000), rand.Next(-2000, 2000), rand.Next(0, 2000)), 1000f, rand.Next(50, 100), 0);
                                cloned_player.DrivingStyle = DrivingStyle.Normal;
                                if (cloned_player.IsInPlane)
                                {
                                    Vehicle other_plane = await GenerateRandomPlaneInAir(new Vector2(veh.Position.X, veh.Position.Y));
                                    cloned_player.Task.ChaseWithPlane(other_plane, new Vector3(1000, 1000, 100));
                                    API.SetRelationshipBetweenGroups(5, (uint)API.GetHashKey("MISSION2"), (uint)API.GetHashKey("MISSION3"));
                                    API.SetRelationshipBetweenGroups(255, (uint)API.GetHashKey("MISSION3"), (uint)API.GetHashKey("MISSION2"));
                                    API.SetPedRelationshipGroupHash(cloned_player.Handle, (uint)API.GetHashKey("MISSION2"));
                                    API.SetPedRelationshipGroupHash(cloned_player.Handle, (uint)API.GetHashKey("PLAYER"));
                                    API.SetPedRelationshipGroupHash(cloned_player.Handle, (uint)API.GetHashKey("CIVMALE"));
                                    API.SetPedRelationshipGroupHash(other_plane.Driver.Handle, (uint)API.GetHashKey("CIVFEMALE"));
                                    API.SetPedRelationshipGroupHash(other_plane.Driver.Handle, (uint)API.GetHashKey("COP"));
                                    API.SetPedRelationshipGroupHash(other_plane.Driver.Handle, (uint)API.GetHashKey("ARMY"));
                                    cloned_player.CanBeShotInVehicle = true;
                                    cloned_player.DrivingStyle = DrivingStyle.IgnorePathing;
                                }
                            }
                        }
                        await Game.Player.ChangeModel(target_ped.Model);
                        Game.PlayerPed.Style.RandomizeOutfit();
                        Game.Player.WantedLevel = 0;
                        API.NetworkCopyPedBlendData(target_ped.Handle, Game.Player.Handle);
                        if (target_ped.IsInVehicle())
                        {
                            Vehicle traget_vehicle = target_ped.CurrentVehicle;
                            float speed = traget_vehicle.Speed;
                            VehicleSeat seat = target_ped.SeatIndex;
                            target_ped.Task.WarpOutOfVehicle(traget_vehicle);
                            while (!traget_vehicle.IsSeatFree(seat))
                            {
                                traget_vehicle.GetPedOnSeat(seat).Position = new Vector3(traget_vehicle.Position.X, traget_vehicle.Position.Y, traget_vehicle.Position.Z + 5);
                                await Delay(0);
                            }
                            await Delay(50);
                            Game.PlayerPed.Task.WarpIntoVehicle(traget_vehicle, seat);
                            await Delay(0);
                            Game.PlayerPed.CurrentVehicle.Speed = speed;
                            target_ped.Delete();
                        }
                        else
                        {
                            Game.PlayerPed.RelationshipGroup = target_ped.RelationshipGroup;
                            Game.PlayerPed.Sweat = target_ped.Sweat;
                            Game.PlayerPed.Weapons.Give(target_ped.Weapons.Current, target_ped.Weapons.Current.Ammo, true, true);
                            Game.PlayerPed.Position = target_ped.Position;
                            Game.PlayerPed.Rotation = target_ped.Rotation;
                            target_ped.Delete();
                        }
                        await Delay(100);
                    }
                    catch (Exception e)
                    {
                        Debug.Write(e.Message);
                        target_ped = null;
                        error_attempts++;
                        await Delay(10);
                        if (error_attempts > 10)
                        {
                            error_attempts = 0;
                            Game.PlayerPed.Position = new Vector3(rand.Next(-2000, 2000), rand.Next(-2000, 2000), 90);
                            API.PopulateNow();
                            await Delay(1000);
                        }
                    }
                }
            }), false);
            Debug.Write($"{resource_name} by {resource_author} started successfully");
        }
    }
}
